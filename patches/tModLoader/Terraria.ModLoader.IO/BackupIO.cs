using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.IO;

namespace Terraria.ModLoader
{
	internal static class BackupIO
	{
		private static bool IsArchiveOlder(DateTime time, TimeSpan thresholdAge) => (DateTime.Now - time) > thresholdAge;
		private static string TodaysBackup(string name) => $"{DateTime.Now:yyyy-MM-dd}-{name}.zip";
		private static DateTime GetTime(string file) => Convert.ToDateTime(file.Substring(0, 10));

		private static Task CreateArchiveTask(Action<ZipFile> saveAction, string dir, string name, ReaderWriterLockSlim @lock) {
			return Task.Factory.StartNew(() => {
				Directory.CreateDirectory(dir);
				DeleteOldArchives(dir, name, @lock);
				using (var zip = new ZipFile(Path.Combine(dir, TodaysBackup(name)), Encoding.UTF8)) {
					if (@lock.TryEnterWriteLock(TimeSpan.FromSeconds(2))) {
						try {
							// use zip64 extensions if necessary for huge files
							zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
							zip.ZipErrorAction = ZipErrorAction.Throw;
							saveAction(zip);
							zip.Save();
						}
						finally {
							@lock.ExitWriteLock();
						}
					}
					else {
						Logging.tML.Error($"Could not establish a write lock for new archive file [{name}]");
					}
				}
			});
		}

		private static void AddZipEntry(this ZipFile zip, string path) {
			// Use BZip2 compression technique for more effective compression than deflate
			// We can achieve up to 70%  or more compression on large world files
			// (If possible, LZMA compression may be preferred)
			zip.CompressionMethod = CompressionMethod.BZip2;
			zip.CompressionLevel = CompressionLevel.BestCompression;
			zip.Comment = $"Archived on ${DateTime.Now} by tModLoader";

			if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
				zip.AddFiles(Directory.GetFiles(path), false, Path.GetFileNameWithoutExtension(path));
			else
				zip.AddFile(path, "");
		}

		private static void DeleteOldArchives(string dir, string name, ReaderWriterLockSlim @lock) {
			var path = Path.Combine(dir, TodaysBackup(name));
			if (File.Exists(path)) {
				DeleteArchive(path, @lock);
			}

			var archives = new DirectoryInfo(dir).GetFiles($"*{name}*.zip", SearchOption.TopDirectoryOnly)
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
					DeleteArchive(previous.FullName, @lock);
				}
				previous = archived;
			}
		}

		private static void DeleteArchive(string path, ReaderWriterLockSlim @lock) {
			if (!@lock.TryEnterWriteLock(TimeSpan.FromSeconds(2)))
				Logging.tML.Error("Could not establish a write lock for old archive file");

			try {
				File.Delete(path);
			}
			catch (Exception e) {
				Logging.tML.Error("Problem deleting old archive file", e);
			}
			finally {
				@lock.ExitWriteLock();
			}
		}

		public static class World
		{
			public static readonly string WorldDir = Path.Combine(Main.SavePath, "Worlds");
			public static readonly string WorldBackupDir = Path.Combine(WorldDir, "Backups");
			public static string WorldPath => Main.worldPathName;
			public static string WorldName => Path.GetFileNameWithoutExtension(WorldPath);
			private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

			internal static void ArchiveWorld() {
				CreateArchiveTask(WriteArchive, WorldBackupDir, WorldName, _lock);
			}

			private static void WriteArchive(ZipFile zip) {
				zip.AddZipEntry(WorldPath);
				var tWldFile = Path.Combine(WorldDir, WorldName + ".twld");
				if (File.Exists(tWldFile)) zip.AddZipEntry(tWldFile);
			}
		}

		public static class Player
		{
			public static readonly string PlayerDir = Path.Combine(Main.SavePath, "Players");
			public static readonly string PlayerBackupDir = Path.Combine(PlayerDir, "Backups");
			private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

			internal static void ArchivePlayer(PlayerFileData fileData) {
				if (fileData.customDataFail != null) return;
				CreateArchiveTask(zip => WriteArchive(zip, fileData.Name), PlayerBackupDir, fileData.Name, _lock);
			}

			private static void WriteArchive(ZipFile zip, string name) {
				zip.AddZipEntry(Path.Combine(PlayerDir, name + ".plr"));
				var tPlrFile = Path.Combine(PlayerDir, name + ".tplr");
				if (File.Exists(tPlrFile)) zip.AddZipEntry(tPlrFile);
				var plrDir = Path.Combine(PlayerDir, name);
				if (Directory.Exists(plrDir)) zip.AddZipEntry(plrDir);
			}
		}
	}
}
