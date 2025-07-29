using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.ModLoader;

public static partial class Logging
{
	public readonly ref struct QuietExceptionHandle
	{
		public QuietExceptionHandle() { quietExceptionCount++; }
		public readonly void Dispose() => quietExceptionCount--;
	}

	[ThreadStatic]
	private static int quietExceptionCount;
	private static readonly HashSet<string> pastExceptions = new();
	private static readonly HashSet<string> ignoreTypes = new() {
		"ReLogic.Peripherals.RGB.DeviceInitializationException",
		"System.Threading.Tasks.TaskCanceledException",
	};
	private static readonly HashSet<string> ignoreSources = new() {
		"MP3Sharp",
	};
	private static readonly List<string> ignoreContents = new() {
		"System.Console.set_OutputEncoding", // when the game is launched without a console handle (client outside dev environment)
		"Terraria.ModLoader.Core.ModCompile",
		"Delegate.CreateDelegateNoSecurityCheck",
		"MethodBase.GetMethodBody",
		"System.Int32.Parse..Terraria.Main.DedServ_PostModLoad", // bad user input in ded-serv console
		"Convert.ToInt32..Terraria.Main.DedServ_PostModLoad", // bad user input in ded-serv console
		"Terraria.Net.Sockets.TcpSocket.Terraria.Net.Sockets.ISocket.AsyncSend", // client disconnects from server
		"System.Diagnostics.Process.Kill", // attempt to kill non-started process when joining server
		"UwUPnP", // UPnP does a lot of trial and error
		"System.Threading.CancellationTokenSource.Cancel", // an operation (task) was deliberately cancelled
		"System.Net.Http.HttpConnectionPool.AddHttp11ConnectionAsync", // Async connection errors thrown on the thread pool. These get bounced back to the caller continuation and can be logged there
		"ReLogic.Peripherals.RGB.SteelSeries.GameSenseConnection._sendMsg",
	};
	// There are a couple of annoying messages that happen during cancellation of asynchronous downloads, and they have no other useful info to suppress by
	private static readonly List<string> ignoreMessages = new() {
		"A blocking operation was interrupted by a call to WSACancelBlockingCall", // c#.net abort for downloads
		"The request was aborted: The request was canceled.", // System.Net.ConnectStream.IOError
		"Object name: 'System.Net.Sockets.Socket'.", // System.Net.Sockets.Socket.BeginReceive
		"Object name: 'System.Net.Sockets.NetworkStream'",// System.Net.Sockets.NetworkStream.UnsafeBeginWrite
		"This operation cannot be performed on a completed asynchronous result object.", // System.Net.ContextAwareResult.get_ContextCopy()
		"Object name: 'SslStream'.", // System.Net.Security.SslState.InternalEndProcessAuthentication
		"Unable to load DLL 'Microsoft.DiaSymReader.Native.x86.dll'", // Roslyn
	};
	private static readonly List<string> ignoreThrowingMethods = new() {
		"MonoMod.Utils.Interop.Unix.DlError", // MonoMod trying to find the right version of libdl and falling back on DLLNotFoundException
		"System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException", // connection lost during socket operation
		"Terraria.Lighting.doColors_Mode", // vanilla lighting which bug randomly happens
		"System.Threading.CancellationToken.Throw", // an operation (task) was deliberately cancelled
	};

	private static Exception previousException;

	public static void IgnoreExceptionSource(string source)
	{
		ignoreSources.Add(source);
	}

	public static void IgnoreExceptionContents(string source)
	{
		if (!ignoreContents.Contains(source))
			ignoreContents.Add(source);
	}

	internal static void ResetPastExceptions()
	{
		pastExceptions.Clear();
	}

	private static void LogFirstChanceExceptions()
	{
		AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
	}

	private static void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs args)
	{
		if (quietExceptionCount > 0)
			return;

		using var _ = new QuietExceptionHandle();

		bool oom = args.Exception is OutOfMemoryException;
		if (oom) {
			TryFreeingMemory();
			Logging.tML.Info($"tModLoader RAM usage during OutOfMemoryException: {UIMemoryBar.SizeSuffix(Process.GetCurrentProcess().PrivateMemorySize64)}");
		}

		try {
			if (!oom) {
				if (args.Exception == previousException
				|| args.Exception is ThreadAbortException
				|| ignoreTypes.Contains(args.Exception.GetType().FullName)
				|| ignoreSources.Contains(args.Exception.Source)
				|| ignoreMessages.Any(str => args.Exception.Message?.Contains(str) ?? false)
				|| ignoreThrowingMethods.Any(str => args.Exception.StackTrace?.Contains(str) ?? false)) {
					return;
				}
			}

			var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
			var traceString = stackTrace.ToString();

			if (!oom && ignoreContents.Any(s => MatchContents(traceString, s)))
				return;

			traceString = stackTrace.ToString();
			traceString = traceString[traceString.IndexOf('\n')..];

			string exString = args.Exception.GetType() + ": " + args.Exception.Message + traceString;

			lock (pastExceptions) {
				if (!pastExceptions.Add(exString))
					return;
			}

			tML.Warn(Language.GetTextValue("tModLoader.RuntimeErrorSilentlyCaughtException") + '\n' + exString);

			previousException = args.Exception;

			string msg = args.Exception.Message + " " + Language.GetTextValue("tModLoader.RuntimeErrorSeeLogsForFullTrace", Path.GetFileName(LogPath));
			// Solxan: We are using Program.SavePathShared == null as a flag to indicate Main CCtor can't run. 
			if (Program.SavePathShared == null || Main.dedServ) { // TODO, sometimes console write fails on unix clients. Hopefully it doesn't happen on servers? System.IO.IOException: Input/output error at System.ConsolePal.Write
				Console.ForegroundColor = ConsoleColor.DarkMagenta;
				Console.WriteLine(msg);
				Console.ResetColor();
			}
			// Solxan: We are using Program.SavePathShared == null as a flag to indicate ModCompile CCtor can't run. 
			else if (Program.SavePathShared != null && ModCompile.activelyModding && !Main.gameMenu) {
				AddChatMessage(msg);
			}

			if (oom) {
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.OutOfMemory"));
			}

			if (args.Exception.Data.Contains("dump") && args.Exception.Data["dump"] is string opt) {
				args.Exception.Data.Remove("dump");
				CrashDump.WriteException(opt switch { "full" => CrashDump.Options.WithFullMemory, _ => CrashDump.Options.Normal });
			}
		}
		catch (Exception e) {
			tML.Warn("FirstChanceExceptionHandler exception", e);
		}
	}

	private static bool MatchContents(ReadOnlySpan<char> traceString, ReadOnlySpan<char> contentPattern)
	{
		while (true) {
			int sep = contentPattern.IndexOf("..");
			var m = sep >= 0 ? contentPattern[..sep] : contentPattern;
			int f = traceString.IndexOf(m);
			if (f < 0)
				return false;

			if (sep < 0)
				return true;

			traceString = traceString[(f + m.Length)..];
			contentPattern = contentPattern[(sep + 2)..];
		}
	}
}
