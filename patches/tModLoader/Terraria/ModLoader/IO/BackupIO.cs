using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Zlib;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader;

internal static class BackupIO
{
	// at the end of WorldGen.generateWorld:
	//  BackupIO.archiveLock = true;
	// in WorldGen.do_worldGenCallBack after WorldFile.saveWorld:
	//  BackupIO.archiveLock = false;
	public static bool archiveLock = false;

	private static readonly Regex dateRegex = new(@"\d+-\d\d*-\d\d*", RegexOptions.Compiled);

	private static bool IsArchiveOlder(DateTime time, TimeSpan thresholdAge) => (DateTime.Now - time) > thresholdAge;

	private static string GetArchiveName(string name, bool isCloudSave) => name + (isCloudSave ? "-cloud" : "");

	private static string TodaysBackup(string name, bool isCloudSave) => $"{DateTime.Now:yyyy-MM-dd}-{GetArchiveName(name, isCloudSave)}.zip";

	private static bool TryGetTime(string file, out DateTime result)
	{
		var match = dateRegex.Match(file);

		result = default;

		return match.Success && DateTime.TryParse(match.Value, out result);
	}

	/// <summary>
	/// Run a given archiving task, which will archive to a backup .zip file
	/// Zip entries added will be compressed
	/// </summary>
	private static void RunArchiving(Action<ZipFile, bool, string> saveAction, bool isCloudSave, string dir, string name, string path)
	{
		try {
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
		catch (Exception e) {
			Logging.tML.Error("A problem occurred when trying to create a backup file.", e);
		}
	}

	/// <summary>
	/// Adds a new entry to the archive .zip file
	/// Will use the best compression level using Deflate
	/// Some files are already compressed and will not be compressed further
	/// </summary>
	private static void AddZipEntry(this ZipFile zip, string path, bool isCloud = false)
	{
		zip.CompressionMethod = CompressionMethod.Deflate;
		zip.CompressionLevel = CompressionLevel.BestCompression;
		zip.Comment = $"Archived on ${DateTime.Now} by tModLoader";

		if (!isCloud && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory) {
			zip.AddFiles(Directory.GetFiles(path), false, Path.GetFileNameWithoutExtension(path));
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
	private static void DeleteOldArchives(string dir, bool isCloudSave, string name)
	{
		string path = Path.Combine(dir, TodaysBackup(name, isCloudSave));

		if (File.Exists(path)) {
			DeleteArchive(path);
		}

		(FileInfo file, DateTime date)[] archives = new DirectoryInfo(dir)
			.GetFiles($"*{GetArchiveName(name, isCloudSave)}*.zip", SearchOption.TopDirectoryOnly)
			.Select(f => (f, TryGetTime(f.Name, out var date) ? date : default))
			.Where(tuple => tuple.Item2 != default) // Ignore files with non-parsable filenames/dates.
			.OrderBy(tuple => tuple.Item2)
			.ToArray();

		(FileInfo file, DateTime date)? previous = null;

		foreach (var archived in archives) {
			if (!previous.HasValue) {
				previous = archived;
				continue;
			}

			int freshness;

			if (IsArchiveOlder(archived.date, TimeSpan.FromDays(30))) {
				freshness = 30;
			}
			else if (IsArchiveOlder(archived.date, TimeSpan.FromDays(7))) {
				freshness = 7;
			}
			else {
				freshness = 1;
			}

			if ((archived.date - previous.Value.date).Days < freshness) {
				DeleteArchive(previous.Value.file.FullName);
			}

			previous = archived;
		}
	}

	private static void DeleteArchive(string path)
	{
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

		private static void WriteArchive(ZipFile zip, bool isCloudSave, string path)
		{
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
		private static void WriteArchive(ZipFile zip, bool isCloudSave, string path)
		{
			// Write .plr and .tplr files
			if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);
			path = Path.ChangeExtension(path, ".tplr");
			if (FileUtilities.Exists(path, isCloudSave)) zip.AddZipEntry(path, isCloudSave);

			// Write other files, such as tmap files to the zip
			if (isCloudSave) WriteCloudFiles(zip, path);
			else WriteLocalFiles(zip, path);
		}

		/// <summary>
		/// Write cloud files, which will get the relevant part of the path and write map &amp; tmap files
		/// </summary>
		private static void WriteCloudFiles(ZipFile zip, string path)
		{
			// Path is still equal to local path
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
		/// Write local files, which simply writes the entire player dir
		/// </summary>
		private static void WriteLocalFiles(ZipFile zip, string path)
		{
			// Write map files from plr dir
			var plrDir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
			if (Directory.Exists(plrDir)) zip.AddZipEntry(plrDir);
		}
	}
}
