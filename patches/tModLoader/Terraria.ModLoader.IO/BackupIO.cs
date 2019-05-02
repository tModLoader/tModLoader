using Ionic.Zip;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;

namespace Terraria.ModLoader
{
	internal class BackupIO
	{
		public static class World
		{
			public static readonly string WorldDir = Path.Combine(Main.SavePath, "Worlds");
			public static readonly string WorldBackupDir = Path.Combine(WorldDir, "Backups");

			private static string WorldPath => Main.worldPathName;
			private static string WorldName => Path.GetFileNameWithoutExtension(WorldPath);
			private static string TodaysBackup => $"{DateTime.Now:yyyy-MM-dd}-{WorldName}.zip";
			private static bool IsArchiveOlder(DateTime time, TimeSpan thresholdAge) => (DateTime.Now - time) > thresholdAge;
			private static DateTime GetTime(string file) => Convert.ToDateTime(file.Substring(0, 10));

			internal static void ArchiveWorld() {
				Task.Factory.StartNew(() => {
					Directory.CreateDirectory(WorldBackupDir);
					DeleteOldArchives();

					using (var zip = new ZipFile(Path.Combine(WorldBackupDir, TodaysBackup), Encoding.UTF8)) {
						zip.AddFile(Main.worldPathName, "");
						var tWldFile = Path.Combine(WorldDir, WorldName + ".twld");
						if (File.Exists(tWldFile)) zip.AddFile(tWldFile, "");
						zip.Save();
					}
				});
			}

			internal static void DeleteOldArchives() {
				if (File.Exists(Path.Combine(WorldBackupDir, TodaysBackup))) {
					DeleteArchive(Path.Combine(WorldBackupDir, TodaysBackup));
				}

				var archives = new DirectoryInfo(WorldBackupDir).GetFiles($"*{WorldName}*.zip", SearchOption.TopDirectoryOnly)
					.OrderBy(f => GetTime(f.Name))
					.ToArray();

				FileInfo previous = null;

				foreach (var archived in archives) {
					if (previous == null) {
						previous = archived;
						continue;
					}

					var time = GetTime(archived.Name);
					var freshness =
						IsArchiveOlder(time, TimeSpan.FromDays(30))
						? 30 : IsArchiveOlder(time, TimeSpan.FromDays(7))
						? 7 : 1;

					if ((time - GetTime(previous.Name)).Days < freshness) {
						DeleteArchive(previous.FullName);
					}
					previous = archived;
				}
			}

			internal static void DeleteArchive(string path) {
				try {
					File.Delete(path);
				}
				catch (Exception e) {
					Logging.tML.Error("Problem deleting old world archive", e);
				}
			}
		}

		public static class Player
		{
			public static readonly string PlayerDir = Path.Combine(Main.SavePath, "Players");
			public static readonly string PlayerBackupDir = Path.Combine(PlayerDir, "Backups");

			private static string TodaysBackup(string name) => $"{DateTime.Now:yyyy-MM-dd}-{name}.zip";
			private static bool IsArchiveOlder(DateTime time, TimeSpan thresholdAge) => (DateTime.Now - time) > thresholdAge;
			private static DateTime GetTime(string file) => Convert.ToDateTime(file.Substring(0, 10));

			internal static void ArchivePlayer(PlayerFileData fileData) {
				if (fileData.customDataFail != null) return;

				Task.Factory.StartNew(() => {
					var name = fileData.Name;
					Directory.CreateDirectory(PlayerBackupDir);
					DeleteOldArchives(fileData);

					using (var zip = new ZipFile(Path.Combine(PlayerBackupDir, TodaysBackup(name)), Encoding.UTF8)) {
						zip.AddFile(Path.Combine(PlayerDir, name + ".plr"), "");
						var tPlrFile = Path.Combine(PlayerDir, name + ".tplr");
						if (File.Exists(tPlrFile)) zip.AddFile(tPlrFile, "");
						var plrDir = Path.Combine(PlayerDir, name);
						if (Directory.Exists(plrDir)) zip.AddFiles(Directory.GetFiles(plrDir), false, name);
						zip.Save();
					}
				});
			}

			internal static void DeleteOldArchives(PlayerFileData fileData) {
				var name = fileData.Name;
				var bakupPath = Path.Combine(PlayerBackupDir, TodaysBackup(name));
				if (File.Exists(bakupPath)) {
					DeleteArchive(bakupPath);
				}

				var archives = new DirectoryInfo(PlayerBackupDir).GetFiles($"*{name}*.zip", SearchOption.TopDirectoryOnly)
					.OrderBy(f => GetTime(f.Name))
					.ToArray();

				FileInfo previous = null;

				foreach (var archived in archives) {
					if (previous == null) {
						previous = archived;
						continue;
					}

					var time = GetTime(archived.Name);
					var freshness =
						IsArchiveOlder(time, TimeSpan.FromDays(30))
						? 30 : IsArchiveOlder(time, TimeSpan.FromDays(7))
						? 7 : 1;

					if ((time - GetTime(previous.Name)).Days < freshness) {
						DeleteArchive(previous.FullName);
					}
					previous = archived;
				}
			}

			internal static void DeleteArchive(string path) {
				try {
					File.Delete(path);
				}
				catch (Exception e) {
					Logging.tML.Error("Problem deleting old player archive", e);
				}
			}
		}
	}
}




