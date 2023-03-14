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
