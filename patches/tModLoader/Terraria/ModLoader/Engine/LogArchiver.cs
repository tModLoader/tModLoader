using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Engine;

/// <summary>
/// Log archiving is performed after log initialization in a separate class to avoid loading Ionic.Zip before logging initialises and it can be patched
/// Some CLRs will load all required assemblies when the class is entered, not necessarily just the method, so you've got to watch out
/// </summary>
internal static class LogArchiver
{
	private const int MAX_LOGS = 20;

#if NETCORE
	// Register support for encodings not present on .NET Core (necessary to port from .NET Framework)
	static LogArchiver() {
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}
#endif

	/// <summary>
	/// Attempt archiving logs.
	/// </summary>
	internal static void ArchiveLogs()
	{
		SetupLogDirs();
		MoveZipsToArchiveDir();
		Archive();
		DeleteOldArchives();
	}

	private static IEnumerable<string> GetArchivedLogs()
	{
		try {
			return Directory.EnumerateFiles(Logging.LogArchiveDir, "*.zip");
		}
		catch (Exception e) {
			Logging.tML.Error(e);
			return Enumerable.Empty<string>();
		}
	}

	private static IEnumerable<string> GetOldLogs()
	{
		try {
			return Directory.EnumerateFiles(Logging.LogDir, "*.old*");
		}
		catch (Exception e) {
			Logging.tML.Error(e);
			return Enumerable.Empty<string>();
		}
	}

	private static void SetupLogDirs()
	{
		try {
			Directory.CreateDirectory(Logging.LogArchiveDir);
		}
		catch (Exception e) {
			// Intermediate issues, try logging
			Logging.tML.Error(e);
			return;
		}
	}

	private static void MoveZipsToArchiveDir()
	{
		// this should only need to happen once, when someone upgrades from a tML version which used the Logs directory for zips, to one which stores zips in Old
		// if logs have been archived in the past, and somehow there's new zips in the log dir, just delete them
		bool justdelete = GetArchivedLogs().Any();
		foreach (var path in Directory.EnumerateFiles(Logging.LogDir, "*.zip")) {
			try {
				if (justdelete)
					File.Delete(path);
				else
					File.Move(path, Path.Combine(Logging.LogArchiveDir, Path.GetFileName(path)));
			}
			catch (Exception e) {
				Logging.tML.Error(e);
			}
		}
	}

	private static void Archive()
	{
		var logFiles = GetOldLogs().ToList();
		if (!logFiles.Any())
			return;

		// Get the Creation Time to use for the Zip.
		DateTime time;
		try {
			time = logFiles.Select(File.GetCreationTime).Min();
		}
		catch (Exception e) {
			Logging.tML.Error(e);
			return;
		}
		int n = 1;

		// Check if other Zips exist already for today
		var pattern = new Regex($"{time:yyyy-MM-dd}-(\\d+)\\.zip");
		string[] existingLogArchives = new string[0];
		try {
			existingLogArchives = Directory.GetFiles(Logging.LogArchiveDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToArray();
		}
		catch (Exception e) {
			Logging.tML.Error(e);
			return;
		}

		// Calculate what the new index should be if already exists for today
		if (existingLogArchives.Length > 0)
			n = existingLogArchives.Select(s => int.Parse(pattern.Match(Path.GetFileName(s)).Groups[1].Value)).Max() + 1;

		// Initiate the ZIP.
		try {
			using (var zip = new ZipFile(Path.Combine(Logging.LogArchiveDir, $"{time:yyyy-MM-dd}-{n}.zip"), Encoding.UTF8)) {
				foreach (var logFile in logFiles) {
					// Omit '.old', but not '.old9001', as doing the latter would result in duplicate names.
					string entryName = Path.GetExtension(logFile) == ".old"
						? Path.GetFileNameWithoutExtension(logFile)
						: Path.GetFileName(logFile);

					using (var stream = File.OpenRead(logFile)) {
						if (stream.Length > 10_000_000) {
							// Some users have enormous log files for unknown reasons. Techinically 4GB is the limit for regular zip files, but 10MB seems reasonable.
							Logging.tML.Warn($"{logFile} exceeds 10MB, it will be truncated for the logs archive.");
							zip.AddEntry(entryName, stream.ReadBytes(10_000_000));
						}
						else {
							zip.AddEntry(entryName, stream);
						}
						zip.Save();
					}

					File.Delete(logFile);
				}
			}
		}
		catch (Exception e) {
			// Problem either in File.OpenRead, zip.Save or File.Delete IO ops
			Logging.tML.Error(e);
		}
	}

	private static void DeleteOldArchives()
	{
		var existingLogs = GetArchivedLogs().OrderBy(File.GetCreationTime).ToList();

		foreach (string f in existingLogs.Take(existingLogs.Count - MAX_LOGS)) {
			try {
				File.Delete(f);
			}
			catch (Exception e) {
				Logging.tML.Error(e);
			}
		}
	}
}