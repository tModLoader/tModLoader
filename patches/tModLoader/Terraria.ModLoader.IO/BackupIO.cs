using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.IO;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader
{
	internal static class BackupIO
	{
		// at the end of WorldGen.generateWorld:
		//  BackupIO.archiveLock = true;
		// in WorldGen.do_worldGenCallBack after WorldFile.saveWorld:
		//  BackupIO.archiveLock = false;
		public static bool archiveLock = false;
		private static bool IsArchiveOlder(DateTime time, TimeSpan thresholdAge) => (DateTime.Now - time) > thresholdAge;
		private static string GetArchiveName(string name, bool isCloudSave) => name + (isCloudSave ? "-cloud" : "");
		private static string TodaysBackup(string name, bool isCloudSave) => $"{DateTime.Now:yyyy-MM-dd}-{GetArchiveName(name, isCloudSave)}.zip";
		private static DateTime GetTime(string file) => Convert.ToDateTime(file.Substring(0, 10));

		/// <summary>
		/// Run a given archiving task, which will archive to a backup .zip file
		/// Zip entries added will be compressed
		/// </summary>
		private static void RunArchiving(Action<ZipFile, bool, string> saveAction, bool isCloudSave, string dir, string name, string path) {
			Directory.CreateDirectory(dir);
			DeleteOldArchives(dir, isCloudSave, name);

			using (var zip = new ZipFile(Path.Combine(dir, TodaysBackup(name, isCloudSave)), Encoding.UTF8)) {
				// use zip64 extensions if necessary for huge files
				zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
				zip.ZipErrorAction = ZipErrorAction.Throw;
				saveAction(zip, isCloudSave, path);
				zip.Save();
			}
		}

		/// <summary>
		/// Adds a new entry to the archive .zip file
		/// Will use the best compression level using BZip2 technique
		/// Some files are already compressed and will not be compressed further
		/// </summary>
		private static void AddZipEntry(this ZipFile zip, string path, bool isCloud = false) {
			zip.CompressionMethod = CompressionMethod.BZip2;
			zip.CompressionLevel = CompressionLevel.BestCompression;
			zip.Comment = $"Archived on ${DateTime.Now} by tModLoader";

			if ((Path.IsPathRooted(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
				|| isCloud && !Path.HasExtension(path)) {
				if (isCloud) {
					//Require manual because there's no easy way to filter files here
					Logging.tML.Warn("Failed to add directory to archive from cloud: Directory through cloud not supported");
				}
				else zip.AddFiles(Directory.GetFiles(path), false, Path.GetFileNameWithoutExtension(path));
			}
			else {
				if (isCloud) zip.AddEntry(Path.GetFileName(path), FileUtilities.ReadAllBytes(path, true));
				else zip.AddFile(path, "");
			}
		}

		/// <summary>
		/// Will delete old archive files
		/// Algorithm details:
		/// - One backup per day for the last week
		/// - One backup per week for the last month
		/// - One backup per month for all time
		/// </summary>
		private static void DeleteOldArchives(string dir, bool isCloudSave, string name) {
			var path = Path.Combine(dir, TodaysBackup(name, isCloudSave));
			if (File.Exists(path)) {
				DeleteArchive(path);
			}

			var archives = new DirectoryInfo(dir).GetFiles($"*{GetArchiveName(name, isCloudSave)}*.zip", SearchOption.TopDirectoryOnly)
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

		private static void DeleteArchive(string path) {
			try {
				File.Delete(path);
			}
			catch (Exception e) {
				Logging.tML.Error("Problem deleting old archive file", e);
			}
		}

		/// <summary>
		/// Responsible for archiving world backups
		/// </summary>
		public static class World
		{
			public static readonly string WorldDir = Path.Combine(Main.SavePath, "Worlds");
			public static readonly string WorldBackupDir = Path.Combine(WorldDir, "Backups");

			internal static void ArchiveWorld(string path, bool isCloudSave)
				=> RunArchiving(WriteArchive, isCloudSave, WorldBackupDir, Path.GetFileNameWithoutExtension(path), path);

			private static void WriteArchive(ZipFile zip, bool isCloudSave, string path) {
				if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);
				path = Path.ChangeExtension(path, ".twld");
				if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);
			}
		}

		/// <summary>
		/// Responsible for archiving player backups
		/// </summary>
		public static class Player
		{
			public static readonly string PlayerDir = Path.Combine(Main.SavePath, "Players");
			public static readonly string PlayerBackupDir = Path.Combine(PlayerDir, "Backups");

			public static void ArchivePlayer(string path, bool isCloudSave)
				=> RunArchiving(WriteArchive, isCloudSave, PlayerBackupDir, Path.GetFileNameWithoutExtension(path), path);

			/// <summary>
			/// Write the archive. Writes the .plr and .tplr files, then writes the player directory
			/// </summary>
			private static void WriteArchive(ZipFile zip, bool isCloudSave, string path) {
				// Write .plr and .tplr files
				if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);
				path = Path.ChangeExtension(path, ".tplr");
				if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);

				if (isCloudSave) WriteCloud(zip, path);
				else WriteLocal(zip, path);
			}

			/// <summary>
			/// Writes for cloud files
			/// </summary>
			private static void WriteCloud(ZipFile zip, string path) {
				var name = Path.GetFileNameWithoutExtension(path);
				path = Path.ChangeExtension(path, "");
				path = path.Substring(0, path.Length - 1);
				// Write map files from plr dir in cloud
				var cloudFiles = SocialAPI.Cloud.GetFiles().Where(p => p.StartsWith(path, StringComparison.CurrentCultureIgnoreCase)
								 && (p.EndsWith(".map", StringComparison.CurrentCultureIgnoreCase) || p.EndsWith(".tmap", StringComparison.CurrentCultureIgnoreCase)));

				foreach (string cloudPath in cloudFiles)
					zip.AddEntry($"{name}/{Path.GetFileName(cloudPath)}", FileUtilities.ReadAllBytes(cloudPath, true));
			}

			/// <summary>
			/// Writes for local files
			/// </summary>
			private static void WriteLocal(ZipFile zip, string path) {
				// Write map files from plr dir
				var plrDir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				if (Directory.Exists(plrDir)) zip.AddZipEntry(plrDir);
			}
		}
	}
}
