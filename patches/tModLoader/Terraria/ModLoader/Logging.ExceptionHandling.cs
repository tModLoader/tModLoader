using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	public static partial class Logging
	{
		private static readonly ThreadLocal<bool> handlerActive = new(() => false);
		private static readonly HashSet<string> pastExceptions = new();
		private static readonly HashSet<string> ignoreSources = new() {
			"MP3Sharp",
		};
		private static readonly List<string> ignoreContents = new() {
			"System.Console.set_OutputEncoding", // when the game is launched without a console handle (client outside dev environment)
			"Terraria.ModLoader.Core.ModCompile",
			"Delegate.CreateDelegateNoSecurityCheck",
			"MethodBase.GetMethodBody",
			"Terraria.Net.Sockets.TcpSocket.Terraria.Net.Sockets.ISocket.AsyncSend", // client disconnects from server
			"System.Diagnostics.Process.Kill", // attempt to kill non-started process when joining server
			"Terraria.ModLoader.Core.AssemblyManager.CecilAssemblyResolver.Resolve",
			"Terraria.ModLoader.Engine.TMLContentManager.OpenStream", // TML content manager delegating to vanilla dir
			"UwUPnP", // UPnP does a lot of trial and error
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
			"at Terraria.Lighting.doColors_Mode", // vanilla lighting which bug randomly happens
			"System.Threading.CancellationToken.Throw", // an operation (task) was deliberately cancelled
		};
		
		private static Exception previousException;

		public static void IgnoreExceptionSource(string source) {
			ignoreSources.Add(source);
		}

		public static void IgnoreExceptionContents(string source) {
			if (!ignoreContents.Contains(source))
				ignoreContents.Add(source);
		}

		internal static void ResetPastExceptions() {
			pastExceptions.Clear();
		}

		private static void LogFirstChanceExceptions() {
			AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
		}

		private static void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs args) {
			if (handlerActive.Value)
				return;

			bool oom = args.Exception is OutOfMemoryException;

			if (oom) {
				TryFreeingMemory();
			}

			try {
				handlerActive.Value = true;

				if (!oom) {
					if (args.Exception == previousException
					|| args.Exception is ThreadAbortException
					|| ignoreSources.Contains(args.Exception.Source)
					|| ignoreMessages.Any(str => args.Exception.Message?.Contains(str) ?? false)
					|| ignoreThrowingMethods.Any(str => args.Exception.StackTrace?.Contains(str) ?? false)) {
						return;
					}
				}

				var stackTrace = new StackTrace(true);

				PrettifyStackTraceSources(stackTrace.GetFrames());

				string traceString = stackTrace.ToString();

				if (!oom && ignoreContents.Any(traceString.Contains))
					return;

				traceString = traceString.Substring(traceString.IndexOf('\n'));

				string exString = args.Exception.GetType() + ": " + args.Exception.Message + traceString;

				lock (pastExceptions) {
					if (!pastExceptions.Add(exString))
						return;
				}

				previousException = args.Exception;
				string msg = args.Exception.Message + " " + Language.GetTextValue("tModLoader.RuntimeErrorSeeLogsForFullTrace", Path.GetFileName(LogPath));

				if (!Main.dedServ && ModCompile.activelyModding && !Main.gameMenu) {
					AddChatMessage(msg);
				}
				else {
					Console.ForegroundColor = ConsoleColor.DarkMagenta;
					Console.WriteLine(msg);
					Console.ResetColor();
				}

				tML.Warn(Language.GetTextValue("tModLoader.RuntimeErrorSilentlyCaughtException") + '\n' + exString);

				if (oom) {
					string error = Language.GetTextValue("tModLoader.OutOfMemory");
					Logging.tML.Fatal(error);
					Interface.MessageBoxShow(error);
					Environment.Exit(1);
				}
			}
			catch (Exception e) {
				tML.Warn("FirstChanceExceptionHandler exception", e);
			}
			finally {
				handlerActive.Value = false;
			}
		}
	}
}
