using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace tModLoader.BuildTasks;

public static class SavePathLocator
{
	public const string PreviewFolder = "tModLoader-preview";
	public const string ReleaseFolder = "tModLoader";
	public const string DevFolder = "tModLoader-dev";

	public static string FindSavePath(TaskLoggingHelper logger, string tmlDllPath, string assemblyName)
	{
		string saveFolder = FindSaveFolder(logger, tmlDllPath);

		return Path.Combine(saveFolder, "Mods", Path.ChangeExtension(assemblyName, ".tmod"));
	}

	public static string FindSaveFolder(TaskLoggingHelper logger, string tmlDllPath)
	{
		logger.LogMessage(MessageImportance.Normal, "Searching for save path...");

		string tmlSteamPath = Path.GetDirectoryName(tmlDllPath) ?? throw new Exception($"Getting directory of {nameof(tmlDllPath)} failed");

		string fileFolder = GetBuildPurpose(logger, tmlDllPath) switch {
			BuildPurpose.Stable => ReleaseFolder,
			BuildPurpose.Dev => DevFolder,
			BuildPurpose.Preview => PreviewFolder,
			_ => throw new ArgumentOutOfRangeException(),
		};

		if (File.Exists(Path.Combine(tmlSteamPath, "savehere.txt"))) {
			string path = Path.Combine(tmlSteamPath, fileFolder);
			logger.LogMessage(MessageImportance.Normal, $"Found savehere.txt, saving at {path}");
			return path;
		}

		string savePath = GetStoragePath("Terraria");
		savePath = Path.Combine(savePath, fileFolder);

		logger.LogMessage(MessageImportance.Low, $"Saving at {savePath}");
		return savePath;
	}

	private static string GetStoragePath(string subfolder) => Path.Combine(GetStoragePath(), subfolder);

	private static string GetStoragePath()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games");
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			string? environmentVariable = Environment.GetEnvironmentVariable("HOME");
			if (string.IsNullOrEmpty(environmentVariable))
				return ".";

			return environmentVariable + "/Library/Application Support";
		}

		string? text = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
		if (!string.IsNullOrEmpty(text)) {
			return text;
		}

		text = Environment.GetEnvironmentVariable("HOME");
		if (string.IsNullOrEmpty(text))
			return ".";

		text += "/.local/share";

		return text;
	}

	private static BuildPurpose GetBuildPurpose(TaskLoggingHelper logger, string tmlDllPath)
	{
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(tmlDllPath);
		string tmlInfoVersion = versionInfo.ProductVersion;

		if (string.IsNullOrEmpty(tmlInfoVersion)) {
			logger.LogWarning("Couldn't get tModLoader's informational version, defaulting to 'Stable' build.");
			return BuildPurpose.Stable;
		}

		logger.LogMessage(MessageImportance.Low, $"tML Informational Version: {tmlInfoVersion}");

		string[] parts = tmlInfoVersion.Substring(tmlInfoVersion.IndexOf('+') + 1).Split('|');
		if (parts.Length > 3) {
			if (Enum.TryParse(parts[3], true, out BuildPurpose purpose))
				return purpose;
		}

		logger.LogWarning("Couldn't parse the build purpose, defaulting to 'Stable' build.");
		return BuildPurpose.Stable;
	}

	public static Version GetTmlVersion(string tmlDllPath)
	{
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(tmlDllPath);
		string tmlVersion = versionInfo.ProductVersion;
		tmlVersion = tmlVersion.Substring(tmlVersion.IndexOf("+", StringComparison.Ordinal) + 1);
		tmlVersion = tmlVersion.Split('|')[0];
		return Version.Parse(tmlVersion);
	}
}

public enum BuildPurpose
{
	Dev, // Personal Builds
	Preview, // Monthly preview builds from CI that modders develop against for compatibility
	Stable, // The 'stable' builds from CI that players are expected to play on.
}