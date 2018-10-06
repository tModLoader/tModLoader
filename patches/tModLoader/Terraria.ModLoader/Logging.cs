using Ionic.Zip;
using log4net;
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
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public static class Logging
	{
		public static readonly string LogDir = Path.Combine(Main.SavePath, "Logs");
		public static string LogPath { get; private set; }

		internal static ILog Terraria { get; } = LogManager.GetLogger("Terraria");
		internal static ILog tML { get; } = LogManager.GetLogger("tML");

		internal static void Init()
		{
			if (Program.LaunchParameters.ContainsKey("-build"))
				return;

			if (!Directory.Exists(LogDir))
				Directory.CreateDirectory(LogDir);

			var layout = new PatternLayout {
				ConversionPattern = "[%d{HH:mm:ss}] [%t/%level] [%logger]: %m%n"
			};
			layout.ActivateOptions();

			var appenders = new List<IAppender>();
#if CLIENT
			string side = "client";
			appenders.Add(new ConsoleAppender {
				Name = "ConsoleAppender",
				Layout = layout
			});
#else
			string side = "server";
#endif
			appenders.Add(new DebugAppender {
				Name = "DebugAppender",
				Layout = layout
			});
			var fileAppender = new FileAppender {
				Name = "FileAppender",
				File = LogPath = Path.Combine(LogDir, RollLogs(side)),
				AppendToFile = false,
				Encoding = Encoding.UTF8,
				Layout = layout
			};
			fileAppender.ActivateOptions();
			appenders.Add(fileAppender);

			BasicConfigurator.Configure(appenders.ToArray());

			tML.InfoFormat("Starting {0}{1} {2}", ModLoader.versionedName, ModLoader.compressedPlatformRepresentation, side);
			tML.InfoFormat("Executable: {0}", Assembly.GetEntryAssembly().Location);
			tML.InfoFormat("Working Directory: {0}", Path.GetFullPath(Directory.GetCurrentDirectory()));
			tML.InfoFormat("Launch Parameters: {0}", string.Join(" ", Program.LaunchParameters.Select(p => (p.Key + " " + p.Value).Trim())));

			HookModuleLoad();
			AppDomain.CurrentDomain.UnhandledException += (s, args) => tML.Error("Unhandled Exception", args.ExceptionObject as Exception);
		}

		private static string RollLogs(string baseName)
		{
			var pattern = new Regex($"{baseName}(\\d*)\\.log");
			var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();

			if (!existingLogs.All(CanOpen))
			{
				int n = existingLogs.Select(s =>
				{
					var tok = pattern.Match(Path.GetFileName(s)).Groups[1].Value;
					return tok.Length == 0 ? 1 : int.Parse(tok);
				}).Max();
				return $"{baseName}{n + 1}.log";
			}

			foreach (var existingLog in existingLogs.OrderBy(File.GetCreationTime))
				Archive(existingLog);

			DeleteOldArchives();

			return $"{baseName}.log";
		}

		private static bool CanOpen(string fileName)
		{
			try
			{
				using (new FileStream(fileName, FileMode.Append));
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		private static void Archive(string logPath)
		{
			var time = File.GetCreationTime(logPath);
			int n = 1;

			var pattern = new Regex($"{time:yyyy-MM-dd}-(\\d+)\\.zip");
			var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();
			if (existingLogs.Count > 0)
				n = existingLogs.Select(s => int.Parse(pattern.Match(Path.GetFileName(s)).Groups[1].Value)).Max() + 1;

			using (var zip = new ZipFile(Path.Combine(LogDir, $"{time:yyyy-MM-dd}-{n}.zip"))) {
				zip.AddFile(logPath, "");
				zip.Save();
			}

			File.Delete(logPath);
		}

		private const int MAX_LOGS = 20;
		private static void DeleteOldArchives()
		{
			var pattern = new Regex(".*\\.zip");
			var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).OrderBy(File.GetCreationTime).ToList();
			foreach (var f in existingLogs.Take(existingLogs.Count - MAX_LOGS))
			{
				try
				{
					File.Delete(f);
				}
				catch (IOException)
				{}
			}
		}

		private static void HookModuleLoad() {
            var f = typeof(AppDomain).GetField("_AssemblyResolve", BindingFlags.Instance | BindingFlags.NonPublic);
            var a = (ResolveEventHandler)f.GetValue(AppDomain.CurrentDomain);
            f.SetValue(AppDomain.CurrentDomain, null);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                tML.DebugFormat("Assembly Resolve: {0} -> {1}", args.RequestingAssembly, args.Name);
                return null;
            };
            AppDomain.CurrentDomain.AssemblyResolve += a;
        }

		internal static void LogFirstChanceExceptions(bool enabled)
		{
			if (enabled)
				AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
			else
				AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionHandler;
		}

		private static HashSet<string> pastExceptions = new HashSet<string>();
		private static void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs args)
		{
			if (!pastExceptions.Add(args.Exception.ToString()))
				return;
			if (args.Exception.Source == "MP3Sharp")
				return;

			var stackTrace = new StackTrace(true).ToString();
			if (stackTrace.Contains("Terraria.ModLoader.ModCompile"))
				return;

			var msg = args.Exception.Message + " " + Language.GetTextValue("tModLoader.RuntimeErrorSeeLogsForFullTrace", Path.GetFileName(LogPath));
#if CLIENT
			float soundVolume = Main.soundVolume;
			Main.soundVolume = 0f;
			Main.NewText(msg, Microsoft.Xna.Framework.Color.OrangeRed);
			Main.soundVolume = soundVolume;
#else
			Console.ForegroundColor = ConsoleColor.DarkMagenta;
			Console.WriteLine(msg);
			Console.ResetColor();
#endif

			tML.Warn(Language.GetTextValue("tModLoader.RuntimeErrorSilentlyCaughtException"), args.Exception);
			tML.Warn(stackTrace);
		}

		private static Regex statusRegex = new Regex(@"(.+?)[: \d]*%$");
		internal static void LogStatusChange(string oldStatusText, string newStatusText)
		{
			// trim numbers and percentage to reduce log spam
			var oldBase = statusRegex.Match(oldStatusText).Groups[1].Value;
			var newBase = statusRegex.Match(newStatusText).Groups[1].Value;
			if (newBase != oldBase && newBase.Length > 0)
				LogManager.GetLogger("StatusText").Info(newBase);
		}

		internal static void ServerConsoleLine(string msg) => ServerConsoleLine(msg, Level.Info);
		internal static void ServerConsoleLine(string msg, Level level)
		{
			Console.WriteLine(msg);
			Terraria.Logger.Log(null, level, msg, null);
		}
	}
}
