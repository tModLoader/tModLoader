using Ionic.Zip;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Terraria.ModLoader.Core
{
	internal static class LogArchiver
	{
		internal static void ArchiveLogs() {
			foreach (var logFile in Directory.GetFiles(Logging.LogDir, "*.old"))
				Archive(logFile, Path.GetFileNameWithoutExtension(logFile));

			DeleteOldArchives();
		}

		private static void Archive(string logFile, string entryName) {
			var time = File.GetCreationTime(logFile);
			int n = 1;

			var pattern = new Regex($"{time:yyyy-MM-dd}-(\\d+)\\.zip");
			var existingLogs = Directory.GetFiles(Logging.LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).ToList();
			if (existingLogs.Count > 0)
				n = existingLogs.Select(s => int.Parse(pattern.Match(Path.GetFileName(s)).Groups[1].Value)).Max() + 1;

			using (var zip = new ZipFile(Path.Combine(Logging.LogDir, $"{time:yyyy-MM-dd}-{n}.zip"), Encoding.UTF8)) {
				using (var stream = File.OpenRead(logFile)) {
					zip.AddEntry(entryName, stream);
					zip.Save();
				}
			}

			File.Delete(logFile);
		}

		private const int MAX_LOGS = 20;
		private static void DeleteOldArchives() {
			var pattern = new Regex(".*\\.zip");
			var existingLogs = Directory.GetFiles(Logging.LogDir).Where(s => pattern.IsMatch(Path.GetFileName(s))).OrderBy(File.GetCreationTime).ToList();
			foreach (var f in existingLogs.Take(existingLogs.Count - MAX_LOGS)) {
				try {
					File.Delete(f);
				}
				catch (IOException) { }
			}
		}
	}
}
