﻿using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.Engine;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	public static class Logging
	{
		public static readonly string LogDir = "tModLoader-Logs";
		public static readonly string LogArchiveDir = Path.Combine(LogDir, "Old");

		// BOM-less UTF8 encoding. Unfortunately, silly Discord, the application we send and get sent logs through 100 times a day,
		// doesn't support previewing of UTF-8 text files if they have a byte-order-mark. Never going to be fixed, it seems.
		// -- Mirsario.
		private static readonly Encoding Encoding = new UTF8Encoding(false);
		private static readonly List<string> InitWarnings = new List<string>();

		public static string LogPath { get; private set; }

		/// <summary> Available for logging when Mod.Logging is not available, such as field initialization </summary>
		public static ILog PublicLogger { get; } = LogManager.GetLogger("PublicLogger");

		internal static ILog Terraria { get; } = LogManager.GetLogger("Terraria");
		internal static ILog tML { get; } = LogManager.GetLogger("tML");

		internal static void Init(bool dedServ) {
			if (Program.LaunchParameters.ContainsKey("-build"))
				return;

			// This is the first file we attempt to use.
			Utils.TryCreatingDirectory(LogDir);

			ConfigureAppenders(dedServ);
		}

		internal static void LogStartup(bool dedServ) {
			tML.InfoFormat("Starting tModLoader {0} {1} built {2}", dedServ ? "server" : "client", BuildInfo.BuildIdentifier, $"{BuildInfo.BuildDate:g}");
			tML.InfoFormat("Log date: {0}", DateTime.Now.ToString("d"));
			tML.InfoFormat("Running on {0} {1} {2} {3}", ReLogic.OS.Platform.Current.Type, System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture, FrameworkVersion.Framework, FrameworkVersion.Version);
			tML.InfoFormat("Executable: {0}", Assembly.GetEntryAssembly().Location);
			tML.InfoFormat("Working Directory: {0}", Path.GetFullPath(Directory.GetCurrentDirectory()));

			string args = string.Join(' ', Environment.GetCommandLineArgs().Skip(1));
			if (!string.IsNullOrEmpty(args)) {
				tML.InfoFormat("Launch Parameters: {0}", args);
				tML.InfoFormat("Parsed Launch Parameters: {0}", string.Join(' ', Program.LaunchParameters.Select(p => ($"{p.Key} {p.Value}").Trim())));
			}

			string stackLimit = Environment.GetEnvironmentVariable("COMPlus_DefaultStackSize");
			if (!string.IsNullOrEmpty(stackLimit))
				tML.InfoFormat("Override Default Thread Stack Size Limit: {0}", stackLimit);

			if (ModCompile.DeveloperMode)
				tML.Info("Developer mode enabled");

			foreach (var line in InitWarnings)
				tML.Warn(line);

				AppDomain.CurrentDomain.UnhandledException += (s, args) => tML.Error("Unhandled Exception", args.ExceptionObject as Exception);
			LogFirstChanceExceptions();
			AssemblyResolving.Init();
			LoggingHooks.Init();
			LogArchiver.ArchiveLogs();
			if (!dedServ)
				GraphicsChangeTracker.RedirectLogs();
		}

		private static void ConfigureAppenders(bool dedServ) {
			var layout = new PatternLayout {
				ConversionPattern = "[%d{HH:mm:ss}] [%t/%level] [%logger]: %m%n"
			};
			layout.ActivateOptions();

			var appenders = new List<IAppender>();
			if (!dedServ) {
				appenders.Add(new ConsoleAppender {
					Name = "ConsoleAppender",
					Layout = layout
				});
			}
			appenders.Add(new DebugAppender {
				Name = "DebugAppender",
				Layout = layout
			});

			var fileAppender = new FileAppender {
				Name = "FileAppender",
				File = LogPath = Path.Combine(LogDir, GetNewLogFile(dedServ ? "server" : "client")),
				AppendToFile = false,
				Encoding = Encoding,
				Layout = layout
			};
			fileAppender.ActivateOptions();
			appenders.Add(fileAppender);

			BasicConfigurator.Configure(appenders.ToArray());
		}

		private static string GetNewLogFile(string baseName) {
			var pattern = new Regex($"{baseName}(\\d*)\\.log$");
			var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();

			if (!existingLogs.All(CanOpen)) {
				int n = existingLogs.Select(s => {
					var tok = pattern.Match(Path.GetFileName(s)).Groups[1].Value;
					return tok.Length == 0 ? 1 : int.Parse(tok);
				}).Max();
				return $"{baseName}{n + 1}.log";
			}

			foreach (var existingLog in existingLogs.OrderBy(File.GetCreationTime)) {
				var oldExt = ".old";
				int n = 0;
				while (File.Exists(existingLog + oldExt))
					oldExt = $".old{++n}";

				try {
					File.Move(existingLog, existingLog + oldExt);
				}
				catch (IOException e) {
					InitWarnings.Add($"Move failed during log initialization: {existingLog} -> {Path.GetFileName(existingLog)}{oldExt}\n{e}");
				}
			}

			return $"{baseName}.log";
		}

		private static bool CanOpen(string fileName) {
			try {
				using (new FileStream(fileName, FileMode.Append)) ;
				return true;
			}
			catch (IOException) {
				return false;
			}
		}

		private static void LogFirstChanceExceptions() {
			AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
		}

		private static HashSet<string> pastExceptions = new HashSet<string>();
		internal static void ResetPastExceptions() => pastExceptions.Clear();

		private static HashSet<string> ignoreSources = new HashSet<string> {
			"MP3Sharp"
		};
		public static void IgnoreExceptionSource(string source) => ignoreSources.Add(source);

		private static List<string> ignoreContents = new List<string> {
			"System.Console.set_OutputEncoding", // when the game is launched without a console handle (client outside dev environment)
			"Terraria.ModLoader.Core.ModCompile",
			"Delegate.CreateDelegateNoSecurityCheck",
			"MethodBase.GetMethodBody",
			"Terraria.Net.Sockets.TcpSocket.Terraria.Net.Sockets.ISocket.AsyncSend", // client disconnects from server
			"System.Diagnostics.Process.Kill", // attempt to kill non-started process when joining server
			"Terraria.ModLoader.Core.AssemblyManager.CecilAssemblyResolver.Resolve",
			"Terraria.ModLoader.Engine.TMLContentManager.OpenStream" // TML content manager delegating to vanilla dir
		};

		// there are a couple of annoying messages that happen during cancellation of asynchronous downloads
		// that have no other useful info to suppress by
		private static List<string> ignoreMessages = new List<string> {
			"A blocking operation was interrupted by a call to WSACancelBlockingCall", // c#.net abort for downloads
			"The request was aborted: The request was canceled.", // System.Net.ConnectStream.IOError
			"Object name: 'System.Net.Sockets.Socket'.", // System.Net.Sockets.Socket.BeginReceive
			"Object name: 'System.Net.Sockets.NetworkStream'",// System.Net.Sockets.NetworkStream.UnsafeBeginWrite
			"This operation cannot be performed on a completed asynchronous result object.", // System.Net.ContextAwareResult.get_ContextCopy()
			"Object name: 'SslStream'.", // System.Net.Security.SslState.InternalEndProcessAuthentication
			"Unable to load DLL 'Microsoft.DiaSymReader.Native.x86.dll'" // Roslyn
		};

		private static List<string> ignoreThrowingMethods = new List<string> {
			"at Terraria.Lighting.doColors_Mode", // vanilla lighting which bug randomly happens
			"System.Threading.CancellationToken.Throw", // an operation (task) was deliberately cancelled
		};

		public static void IgnoreExceptionContents(string source) {
			if (!ignoreContents.Contains(source))
				ignoreContents.Add(source);
		}

		private static ThreadLocal<bool> handlerActive = new ThreadLocal<bool>(() => false);
		private static Exception previousException;

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
					if (args.Exception == previousException ||
						args.Exception is ThreadAbortException ||
						ignoreSources.Contains(args.Exception.Source) ||
						ignoreMessages.Any(str => args.Exception.Message?.Contains(str) ?? false) ||
						ignoreThrowingMethods.Any(str => args.Exception.StackTrace?.Contains(str) ?? false))
						return;
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

				if (!Main.dedServ && ModCompile.activelyModding && !Main.gameMenu)
					AddChatMessage(msg);
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

		// Separated method to avoid triggering Main's static constructor

		private static void AddChatMessage(string msg) {
			AddChatMessage(msg, Color.OrangeRed);
		}

		private static void AddChatMessage(string msg, Color color) {
			if (Main.gameMenu)
				return;

			float soundVolume = Main.soundVolume;
			Main.soundVolume = 0f;
			Main.NewText(msg, color);
			Main.soundVolume = soundVolume;
		}

		private static void TryFreeingMemory() {
			// In case of OOM, unload the Main.tile array and do immediate garbage collection.
			// If we don't do this, there will be a big chance that this method will fail to even quit the game, due to another OOM exception being thrown.

			Main.tile = new Tilemap(0, 0);
			GC.Collect();
		}

		private static Regex statusRegex = new Regex(@"(.+?)[: \d]*%$");
		internal static void LogStatusChange(string oldStatusText, string newStatusText) {
			// trim numbers and percentage to reduce log spam
			var oldBase = statusRegex.Match(oldStatusText).Groups[1].Value;
			var newBase = statusRegex.Match(newStatusText).Groups[1].Value;
			if (newBase != oldBase && newBase.Length > 0)
				LogManager.GetLogger("StatusText").Info(newBase);
		}

		internal static void ServerConsoleLine(string msg) => ServerConsoleLine(msg, Level.Info);
		internal static void ServerConsoleLine(string msg, Level level, Exception ex = null, ILog log = null) {
			if (level == Level.Warn)
				Console.ForegroundColor = ConsoleColor.Yellow;
			else if (level == Level.Error)
				Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine(msg);
			Console.ResetColor();

			(log ?? Terraria).Logger.Log(null, level, msg, ex);
		}

		internal static readonly FieldInfo f_fileName =
			typeof(StackFrame).GetField("_fileName", BindingFlags.Instance | BindingFlags.NonPublic) ??
			typeof(StackFrame).GetField("strFileName", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly Assembly TerrariaAssembly = Assembly.GetExecutingAssembly();

		public static void PrettifyStackTraceSources(StackFrame[] frames) {
			if (frames == null)
				return;

			foreach (var frame in frames) {
				string filename = frame.GetFileName();
				var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
				if (filename == null || assembly == null)
					continue;

				string trim;
				if (AssemblyManager.GetAssemblyOwner(assembly, out var modName))
					trim = modName;
				else if (assembly == TerrariaAssembly)
					trim = "tModLoader";
				else
					continue;

				int idx = filename.LastIndexOf(trim, StringComparison.InvariantCultureIgnoreCase);
				if (idx > 0) {
					filename = filename.Substring(idx);
					f_fileName.SetValue(frame, filename);
				}
			}
		}
	}
}
