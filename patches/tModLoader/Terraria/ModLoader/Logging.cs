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
using Terraria.ModLoader.UI;

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
	private static readonly Regex statusGeneratingWorld = new(@"\d+\.\d% - (.+?) - \d+\.\d%$");

	public static string LogPath { get; private set; }

	/// <summary> Available for logging when Mod.Logging is not available, such as field initialization </summary>
	public static ILog PublicLogger { get; } = LogManager.GetLogger("PublicLogger");

	internal static ILog Terraria { get; } = LogManager.GetLogger("Terraria");
	internal static ILog tML { get; } = LogManager.GetLogger("tML");
	internal static ILog FNA { get; } = LogManager.GetLogger("FNA");

	internal static void Init(LogFile logFile)
	{
		LegacyCleanups();

		if (Program.LaunchParameters.ContainsKey("-build"))
			return;

		// This is the first file we attempt to use.
		Utils.TryCreatingDirectory(LogDir);
		try {
			InitLogPaths(logFile);
			ConfigureAppenders(logFile);
			// Force-update log4net file's creation date, because log4net does not do that.
			TryUpdatingFileCreationDate(LogPath);
		}
		catch (Exception e) {
			ErrorReporting.FatalExit("Failed to init logging", e);
		}
	}

	internal static void LogStartup(bool dedServ)
	{
		if (Program.LaunchParameters.ContainsKey("-build"))
			return;

		tML.InfoFormat("Starting tModLoader {0} {1} built {2}", dedServ ? "server" : "client", BuildInfo.BuildIdentifier, $"{BuildInfo.BuildDate:g}");
		tML.InfoFormat("Log date: {0}", DateTime.Now.ToString("d"));
		tML.InfoFormat("Running on {0} (v{1}) {2} {3} {4}", ReLogic.OS.Platform.Current.Type, Environment.OSVersion.Version, System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture, FrameworkVersion.Framework, FrameworkVersion.Version);
		tML.InfoFormat("CPU: {0} processors. RAM: {1}", Environment.ProcessorCount, UIMemoryBar.SizeSuffix(UIMemoryBar.GetTotalMemory()));
		tML.InfoFormat("FrameworkDescription: {0}", RuntimeInformation.FrameworkDescription);
		tML.InfoFormat("Executable: {0}", Assembly.GetEntryAssembly().Location);
		tML.InfoFormat("Working Directory: {0}", Path.GetFullPath(Directory.GetCurrentDirectory()));

		string args = string.Join(' ', Environment.GetCommandLineArgs().Skip(1));

		if (!string.IsNullOrEmpty(args) || Program.LaunchParameters.Any()) {
			tML.InfoFormat("Launch Parameters: {0}", args);
			tML.InfoFormat("Parsed Launch Parameters: {0}", string.Join(' ', Program.LaunchParameters.Select(p => ($"{p.Key} {p.Value}").Trim())));
		}

		DumpEnvVars();

		string stackLimit = Environment.GetEnvironmentVariable("COMPlus_DefaultStackSize");

		if (!string.IsNullOrEmpty(stackLimit))
			tML.InfoFormat("Override Default Thread Stack Size Limit: {0}", stackLimit);

		foreach (string line in initWarnings)
			tML.Warn(line);

		AppDomain.CurrentDomain.UnhandledException += (s, args) => tML.Error("Unhandled Exception", args.ExceptionObject as Exception);
		LogFirstChanceExceptions();
		AssemblyResolving.Init();
		LoggingHooks.Init();
		LogArchiver.ArchiveLogs();
		NativeExceptionHandling.Init();
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
			File = LogPath,
			AppendToFile = false,
			Encoding = encoding,
			Layout = layout,
		};

		fileAppender.ActivateOptions();
		appenders.Add(fileAppender);

		BasicConfigurator.Configure(appenders.ToArray());
	}

	private static void InitLogPaths(LogFile logFile)
	{
		// Launch.log is for the current run, so don't mark as old. Only needed for startup issues
		// environment-client.log and environment-sever.log are old, will be replaced with new one during Logging.LogStartup
		// terrariasteamclient.log is a log for the client that will be replaced with a new one during later tML Startup.

		// the environment log file name is derived from the free log name we get below, so if this process logs to client2.log, env vars will dump to environment-client2.log
		// We could pasa a log file name launch arg to TerrariaSteamClient, but in practice collisions should rarely if ever happen, due to steam not allowing parallel launches.

		var mainLogName = logFile.ToString().ToLowerInvariant();
		var baseLogNames = new List<string> { mainLogName };

		if (logFile != LogFile.TerrariaSteamClient)
			baseLogNames.Add("environment-" + mainLogName);
		if (logFile == LogFile.Client)
			baseLogNames.Add(LogFile.TerrariaSteamClient.ToString().ToLowerInvariant());

		var logFileName = GetFreeLogFileName(baseLogNames, roll: logFile != LogFile.TerrariaSteamClient);
		LogPath = Path.Combine(LogDir, logFileName);

		// potential race condition, unless we Touch the file we're about to use. Unlikely to be an issue in practice.
	}

	private static string GetFreeLogFileName(List<string> baseLogNames, bool roll)
	{
		var baseLogName = baseLogNames[0];
		var pattern = new Regex($"(?:{string.Join('|', baseLogNames)})(\\d*)\\.log$");
		var existingLogs = Directory.GetFiles(LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();

		if (!existingLogs.All(CanOpen)) {
			int n = existingLogs.Select(s => {
				string tok = pattern.Match(Path.GetFileName(s)).Groups[1].Value;

				return tok.Length == 0 ? 1 : int.Parse(tok);
			}).Max();

			return $"{baseLogName}{n + 1}.log";
		}

		if (roll) {
			RenameToOld(existingLogs);
		}
		else if (existingLogs.Any()) {
			var logNames = existingLogs.Select(s => Path.GetFileName(s));
			initWarnings.Add($"Old log files found which should have already been archived. The {baseLogName}.log will be overwritten. [{string.Join(", ", logNames)}]");
		}

		return $"{baseLogName}.log";
	}

	private static void RenameToOld(List<string> existingLogs) {
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

	[ThreadStatic]
	private static string lastStatusLogged; // Needs to be ThreadStatic so competing threads don't spam logs, such as during world gen saving.
	internal static void LogStatusChange(string newStatusText)
	{
		lastStatusLogged ??= string.Empty;

		// Trim numbers and percentage to reduce log spam
		string newBase = newStatusText;

		if (statusRegex.Match(newStatusText) is { Success: true } statusMatchNew) {
			newBase = statusMatchNew.Groups[1].Value;
		}

		if (WorldGen.generatingWorld) {
			// 21.2% - Adding more grass - 90.3%
			if (statusGeneratingWorld.Match(newStatusText) is { Success: true } statusGenMatchNew) {
				newBase = statusGenMatchNew.Groups[1].Value;
			}
			if (WorldGen.drunkWorldGenText && !Main.dedServ)
				newBase = "Random Numbers (Drunk World)";
		}

		if (newBase != lastStatusLogged && newBase.Length > 0) {
			LogManager.GetLogger("StatusText").Info(newBase);
			lastStatusLogged = newBase;
		}
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
			string fileName = $"environment-{Path.GetFileName(LogPath)}";
			string filePath = Path.Combine(LogDir, fileName);

			TryUpdatingFileCreationDate(filePath);

			using var f = File.OpenWrite(filePath);
			using var w = new StreamWriter(f);
			foreach (var key in Environment.GetEnvironmentVariables().Keys) {
				w.WriteLine($"{key}={Environment.GetEnvironmentVariable((string)key)}");
			}
		}
		catch (Exception e) {
			tML.Error("Failed to dump env vars", e);
		}
	}

	private static void TryUpdatingFileCreationDate(string filePath)
	{
		if (File.Exists(filePath)) {
			using var _ = new QuietExceptionHandle();

			try { File.SetCreationTime(filePath, DateTime.Now); }
			catch { }
		}
	}

	private static readonly string[] autoRemovedFiles = {
		"environment-",
	};

	// Removes files that shouldn't have ever existed.
	private static void LegacyCleanups()
	{
		using var _ = new QuietExceptionHandle();

		foreach (string filePath in autoRemovedFiles) {
			string fullPath = Path.Combine(LogDir, filePath);

			if (File.Exists(fullPath)) {
				try { File.Delete(fullPath); }
				catch { }
			}
		}
	}
}
