using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader;

public static partial class Logging
{
	public enum LogFile
	{
		Client,
		Server,
		TerrariaSteamClient
	}

	public static readonly string LogDir = "tModLoader-Logs";
	public static readonly string LogArchiveDir = Path.Combine(LogDir, "Old");

	// BOM-less UTF8 encoding. Unfortunately, silly Discord, the application we send and get sent logs through 100 times a day,
	// doesn't support previewing of UTF-8 text files if they have a byte-order-mark. Never going to be fixed, it seems.
	// -- Mirsario.
	private static readonly Encoding encoding = new UTF8Encoding(false);
	private static readonly List<string> initWarnings = new();
	private static readonly Regex statusRegex = new(@"(.+?)[: \d]*%$");

	public static string LogPath { get; private set; }

	/// <summary> Available for logging when Mod.Logging is not available, such as field initialization </summary>
	public static ILog PublicLogger { get; } = LogManager.GetLogger("PublicLogger");

	internal static ILog Terraria { get; } = LogManager.GetLogger("Terraria");
	internal static ILog tML { get; } = LogManager.GetLogger("tML");
	internal static ILog FNA { get; } = LogManager.GetLogger("FNA");

	internal static void Init(LogFile logFile)
	{
		if (Program.LaunchParameters.ContainsKey("-build"))
			return;

		// This is the first file we attempt to use.
		Utils.TryCreatingDirectory(LogDir);
		ConfigureAppenders(logFile);
	}

	internal static void LogStartup(bool dedServ)
	{
		tML.InfoFormat("Starting tModLoader {0} {1} built {2}", dedServ ? "server" : "client", BuildInfo.BuildIdentifier, $"{BuildInfo.BuildDate:g}");
		tML.InfoFormat("Log date: {0}", DateTime.Now.ToString("d"));
		tML.InfoFormat("Running on {0} (v{1}) {2} {3} {4}", ReLogic.OS.Platform.Current.Type, Environment.OSVersion.Version, System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture, FrameworkVersion.Framework, FrameworkVersion.Version);
		tML.InfoFormat("FrameworkDescription: {0}", RuntimeInformation.FrameworkDescription);
		tML.InfoFormat("Executable: {0}", Assembly.GetEntryAssembly().Location);
		tML.InfoFormat("Working Directory: {0}", Path.GetFullPath(Directory.GetCurrentDirectory()));

		string args = string.Join(' ', Environment.GetCommandLineArgs().Skip(1));

		if (!string.IsNullOrEmpty(args)) {
			tML.InfoFormat("Launch Parameters: {0}", args);
			tML.InfoFormat("Parsed Launch Parameters: {0}", string.Join(' ', Program.LaunchParameters.Select(p => ($"{p.Key} {p.Value}").Trim())));
		}

		DumpEnvVars();

		string stackLimit = Environment.GetEnvironmentVariable("COMPlus_DefaultStackSize");

		if (!string.IsNullOrEmpty(stackLimit))
			tML.InfoFormat("Override Default Thread Stack Size Limit: {0}", stackLimit);

		if (ModCompile.DeveloperMode)
			tML.Info("Developer mode enabled");

		foreach (string line in initWarnings)
			tML.Warn(line);

		AppDomain.CurrentDomain.UnhandledException += (s, args) => tML.Error("Unhandled Exception", args.ExceptionObject as Exception);
		LogFirstChanceExceptions();
		AssemblyResolving.Init();
		LoggingHooks.Init();
		LogArchiver.ArchiveLogs();
		
		if (!dedServ)
			FNALogging.RedirectLogs();
	}

	private static void ConfigureAppenders(LogFile logFile)
	{
		var layout = new PatternLayout {
			ConversionPattern = "[%d{HH:mm:ss.fff}] [%t/%level] [%logger]: %m%n"
		};
		
		layout.ActivateOptions();

		var appenders = new List<IAppender>();
		if (logFile == LogFile.Client) {
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
			File = LogPath = Path.Combine(LogDir, GetNewLogFile(logFile.ToString().ToLowerInvariant())),
			AppendToFile = false,
			Encoding = encoding,
			Layout = layout
		};

		fileAppender.ActivateOptions();
		appenders.Add(fileAppender);

		BasicConfigurator.Configure(appenders.ToArray());
	}

	private static string GetNewLogFile(string baseName)
	{
		var pattern = new Regex($"{baseName}(\\d*)\\.log$");
		var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();

		if (!existingLogs.All(CanOpen)) {
			int n = existingLogs.Select(s => {
				string tok = pattern.Match(Path.GetFileName(s)).Groups[1].Value;
				
				return tok.Length == 0 ? 1 : int.Parse(tok);
			}).Max();
			
			return $"{baseName}{n + 1}.log";
		}

		foreach (string existingLog in existingLogs.OrderBy(File.GetCreationTime)) {
			string oldExt = ".old";
			int n = 0;

			while (File.Exists(existingLog + oldExt)) {
				oldExt = $".old{++n}";
			}

			try {
				File.Move(existingLog, existingLog + oldExt);
			}
			catch (IOException e) {
				initWarnings.Add($"Move failed during log initialization: {existingLog} -> {Path.GetFileName(existingLog)}{oldExt}\n{e}");
			}
		}

		return $"{baseName}.log";
	}

	private static bool CanOpen(string fileName)
	{
		try {
			using (new FileStream(fileName, FileMode.Append)) { };
			
			return true;
		}
		catch (IOException) {
			return false;
		}
	}

	// Separated method to avoid triggering Main's static constructor
	private static void AddChatMessage(string msg)
	{
		AddChatMessage(msg, Color.OrangeRed);
	}

	private static void AddChatMessage(string msg, Color color)
	{
		if (Main.gameMenu)
			return;

		float soundVolume = Main.soundVolume;
		Main.soundVolume = 0f;
		Main.NewText(msg, color);
		Main.soundVolume = soundVolume;
	}
	
	internal static void LogStatusChange(string oldStatusText, string newStatusText)
	{
		// Trim numbers and percentage to reduce log spam
		string oldBase = statusRegex.Match(oldStatusText).Groups[1].Value;
		string newBase = statusRegex.Match(newStatusText).Groups[1].Value;

		if (newBase != oldBase && newBase.Length > 0)
			LogManager.GetLogger("StatusText").Info(newBase);
	}
	
	internal static void ServerConsoleLine(string msg)
		=> ServerConsoleLine(msg, Level.Info);
	
	internal static void ServerConsoleLine(string msg, Level level, Exception ex = null, ILog log = null)
	{
		if (level == Level.Warn)
			Console.ForegroundColor = ConsoleColor.Yellow;
		else if (level == Level.Error)
			Console.ForegroundColor = ConsoleColor.Red;

		Console.WriteLine(msg);
		Console.ResetColor();

		(log ?? Terraria).Logger.Log(null, level, msg, ex);
	}

	private static void DumpEnvVars()
	{
		try {
			using var f = File.OpenWrite(Path.Combine(LogDir, "environment.log"));
			using var w = new StreamWriter(f);
			foreach (var key in Environment.GetEnvironmentVariables().Keys) {
				w.WriteLine($"{key}={Environment.GetEnvironmentVariable((string)key)}");
			}
		}
		catch (Exception e) {
			tML.Error("Failed to dump env vars", e);
		}
	}
}
