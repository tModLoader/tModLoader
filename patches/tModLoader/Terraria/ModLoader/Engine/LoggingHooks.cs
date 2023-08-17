using Microsoft.CodeAnalysis;
using MonoMod.RuntimeDetour;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Engine;

internal static class LoggingHooks
{
	internal static void Init()
	{
		FixBrokenConsolePipeError();
		PrettifyStackTraceSources();
		HookWebRequests();
		HookProcessStart();
	}

	private static Hook writeFileNativeHook;
	private delegate int orig_WriteFileNative(IntPtr hFile, ReadOnlySpan<byte> bytes, bool useFileAPIs);
	private delegate int hook_WriteFileNative(orig_WriteFileNative orig, IntPtr hFile, ReadOnlySpan<byte> bytes, bool useFileAPIs);
	private static void FixBrokenConsolePipeError()
	{ // #2925
		if (!OperatingSystem.IsWindows())
			return;

		// add 0xE9 (ERROR_PIPE_NOT_CONNECTED) to the 'ignored' errors in
		// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Console/src/System/ConsolePal.Windows.cs#L1213

		var consoleStreamType = typeof(Console).Assembly.GetType("System.ConsolePal").GetNestedType("WindowsConsoleStream", BindingFlags.NonPublic);
		var m = consoleStreamType.GetMethod("WriteFileNative", BindingFlags.Static | BindingFlags.NonPublic);
		writeFileNativeHook = new Hook(m, new hook_WriteFileNative((orig, hFile, bytes, useFileAPIs) => {
			int ret = orig(hFile, bytes, useFileAPIs);
			if (ret == 0xE9) // ERROR_PIPE_NOT_CONNECTED
				return 0; // success

			return ret;
		}));
	}

	private static Hook processStartHook;
	private static void HookProcessStart()
	{
		processStartHook = new Hook(typeof(Process).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance), new Func<Func<Process, bool>, Process, bool>((orig, self) => {
			Logging.tML.Debug($"Process.Start (UseShellExecute = {self.StartInfo.UseShellExecute}): \"{self.StartInfo.FileName}\" {self.StartInfo.Arguments}");
			return orig(self);
		}));
	}

	private delegate void orig_StackTrace_CaptureStackTrace(StackTrace self, int skipFrames, bool fNeedFileInfo, Exception e);

	private delegate void hook_StackTrace_CaptureStackTrace(orig_StackTrace_CaptureStackTrace orig, StackTrace self, int skipFrames, bool fNeedFileInfo, Exception e);

	private static void Hook_StackTrace_CaptureStackTrace(orig_StackTrace_CaptureStackTrace orig, StackTrace self, int skipFrames, bool fNeedFileInfo, Exception e) {
		// avoid including the hook frames in manually captured stack traces. Note that 2 frames are from the hook, and the System.Diagnostics frame is normally trimmed by CalculateFramesToSkip in StackTrace.CoreCLR.cs
		//
		//    at Hook<System.Void Terraria.ModLoader.Engine.LoggingHooks::Hook_StackTrace_CaptureStackTrace(Terraria.ModLoader.Engine.LoggingHooks+orig_StackTrace_CaptureStackTrace,System.Diagnostics.StackTrace,System.Int32,System.Boolean,System.Exception)>(StackTrace , Int32 , Boolean , Exception )
		//    at SyncProxy<System.Void System.Diagnostics.StackTrace:CaptureStackTrace(System.Int32, System.Boolean, System.Exception) > (StackTrace, Int32, Boolean, Exception)
		//    at System.Diagnostics.StackTrace..ctor(Boolean fNeedFileInfo)
		int skipHookFrames = e == null ? 3 : 0;

		orig(self, skipFrames + skipHookFrames, fNeedFileInfo, e);

		if (fNeedFileInfo)
			Logging.PrettifyStackTraceSources(self.GetFrames());
	}

	private static Hook stackTrace_CaptureStackTrace;
	private static void PrettifyStackTraceSources() {
		stackTrace_CaptureStackTrace = new Hook(typeof(StackTrace).GetMethod("CaptureStackTrace", BindingFlags.NonPublic | BindingFlags.Instance)!,
			new hook_StackTrace_CaptureStackTrace(Hook_StackTrace_CaptureStackTrace));
	}

	private delegate ValueTask<HttpResponseMessage> orig_SendAsyncCore(object self, HttpRequestMessage request, Uri? proxyUri, bool async, bool doRequestAuth, bool isProxyConnect, CancellationToken cancellationToken);

	private delegate ValueTask<HttpResponseMessage> hook_SendAsyncCore(orig_SendAsyncCore orig, object self, HttpRequestMessage request, Uri? proxyUri, bool async, bool doRequestAuth, bool isProxyConnect, CancellationToken cancellationToken);

	private static Hook httpSendAsyncHook;
	/// <summary>
	/// Attempt to hook the .NET internal methods to log when requests are sent to web addresses.
	/// Use the right internal methods to capture redirects
	/// </summary>
	private static void HookWebRequests()
	{
		try {
			// .NET 6
			var sendAsyncCoreMethodInfo = typeof(HttpClient).Assembly
				.GetType("System.Net.Http.HttpConnectionPoolManager")
				?.GetMethod("SendAsyncCore", BindingFlags.Public | BindingFlags.Instance);

			if (sendAsyncCoreMethodInfo != null) {
				httpSendAsyncHook = new Hook(sendAsyncCoreMethodInfo, new hook_SendAsyncCore((orig, self, request, proxyUri, async, doRequestAuth, isProxyConnect, cancellationToken) => {
					if (IncludeURIInRequestLogging(request.RequestUri))
						Logging.tML.Debug($"Web Request: {request.RequestUri}");

					return orig(self, request, proxyUri, async, doRequestAuth, isProxyConnect, cancellationToken);
				}));
				return;
			}
		}
		catch {
		}

		Logging.tML.Warn("HttpWebRequest send/submit method not found");
	}

	private static bool IncludeURIInRequestLogging(Uri uri)
	{
		if (uri.IsLoopback && uri.LocalPath.Contains("game_")) // SteelSeries SDK
			return false;

		return true;
	}
}
