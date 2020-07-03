using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Terraria.ModLoader.Core
{
	/// <summary>
	/// Log archiving is performed after log initialization in a separate class to avoid loading Ionic.Zip before logging initialises and it can be patched
	/// Some CLRs will load all required assemblies when the class is entered, not necessarily just the method, so you've got to watch out
	/// </summary>
	internal static class LogArchiver
	{
		private const int MAX_LOGS = 20;

		/// <summary>
		/// Attempt archiving logs.
		/// </summary>
		internal static void ArchiveLogs()
		{
			SetupLogDirs();
			MoveOldLogs();
			DeleteOldArchives();
		}

		private static IEnumerable<string> GetLogs()
		{
			try {
				return Directory.GetFiles(Logging.LogDir, "*.zip").AsEnumerable();
			}
			catch (Exception e) {
				// Intermediate problem, try logging
				Logging.tML.Error(e);
			}
			return new string[0];
		}

		private static IEnumerable<string> GetOldLogs()
		{
			try {
				return Directory.GetFiles(Logging.LogDir, "*.old*").AsEnumerable();
			}
			catch (Exception e) {
				// Intermediate problem, try logging
				Logging.tML.Error(e);
			}
			return new string[0];
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

		private static void MoveOldLogs()
		{
			foreach (string log in GetLogs()) {
				try {
					File.Move(log, Path.Combine(Logging.LogArchiveDir, Path.GetFileName(log)));
				}
				catch (Exception e) {
					Logging.tML.Error(e);
				}
			}

			foreach (string log in GetOldLogs()) {
				Archive(log, Path.GetFileNameWithoutExtension(log));
			}
		}

		private static void Archive(string logFile, string entryName)
		{

			DateTime time;
			try {
				time = File.GetCreationTime(logFile);
			}
			catch (Exception e) {
				Logging.tML.Error(e);
				return;
			}
			int n = 1;

			var pattern = new Regex($"{time:yyyy-MM-dd}-(\\d+)\\.zip");
			string[] existingLogs = new string[0];
			try {
				existingLogs = Directory.GetFiles(Logging.LogArchiveDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToArray();
			}
			catch (Exception e) {
				Logging.tML.Error(e);
				return;
			}

			if (existingLogs.Length > 0)
				n = existingLogs.Select(s => int.Parse(pattern.Match(Path.GetFileName(s)).Groups[1].Value)).Max() + 1;

			try {
				using (var zip = new ZipFile(Path.Combine(Logging.LogArchiveDir, $"{time:yyyy-MM-dd}-{n}.zip"), Encoding.UTF8)) {
					using (var stream = File.OpenRead(logFile)) {
						zip.AddEntry(entryName, stream);
						zip.Save();
					}
				}

				File.Delete(logFile);
			}
			catch (Exception e) {
				// Problem either in File.OpenRead, zip.Save or File.Delete IO ops
				Logging.tML.Error(e);
			}
		}

		private static void DeleteOldArchives()
		{
			var existingLogs = GetLogs().OrderBy(File.GetCreationTime).ToList();

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
}