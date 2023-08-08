using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;
using Terraria.Social;

namespace Terraria.Utilities;

public static partial class FileUtilities
{
	public static void CopyExtended(string source, string destination, bool cloud, bool overwriteAlways, bool overwriteOld = true)
	{
		bool overwrite = DetermineIfShouldOverwrite(overwriteAlways, overwriteOld, source, destination);
		if (!overwrite && File.Exists(destination))
			return;

		if (!cloud) {
			try {
				File.Copy(source, destination, overwrite);
			}
			catch (IOException ex) {
				if (ex.GetType() != typeof(IOException))
					throw;

				// TER-827 - fallback for random File.Copy failures on Win11
				using (var inputstream = File.OpenRead(source))
				using (var outputstream = File.Create(destination))
					inputstream.CopyTo(outputstream);
			}
			return;
		}

		// Sanitize the paths for Steam calls
		string cloudPath = Social.Steam.CoreSocialModule.GetCloudSaveLocation();
		destination = ConvertToRelativePath(cloudPath, destination);
		source = ConvertToRelativePath(cloudPath, source);

		if (SocialAPI.Cloud != null && (overwrite || !SocialAPI.Cloud.HasFile(destination))) {
			var bytes = SocialAPI.Cloud.Read(source);
			SocialAPI.Cloud.Write(destination, bytes);
		}
	}

	public static void CopyFolderEXT(string sourcePath, string destinationPath, bool isCloud = false, Regex excludeFilter = null, bool overwriteAlways = false, bool overwriteOld = false)
	{
		Directory.CreateDirectory(destinationPath);
		string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
		for (int i = 0; i < directories.Length; i++) {
			string relativePath = ConvertToRelativePath(sourcePath, directories[i]);
			if (excludeFilter != null && excludeFilter.IsMatch(relativePath))
				continue;

			Directory.CreateDirectory(directories[i].Replace(sourcePath, destinationPath));
		}

		directories = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
		// Assumes each file is on average 0.5 MB and is moved at 15 MB/s.  - Solxan.
		ModLoader.Logging.tML.Info($"Number of files to Copy: {directories.Length}. Estimated time for HDD @15 MB/s: {directories.Length / 30} seconds");
		foreach (string obj in directories) {
			string relativePath = ConvertToRelativePath(sourcePath, obj);
			if (excludeFilter != null && excludeFilter.IsMatch(relativePath))
				continue;

			CopyExtended(obj, obj.Replace(sourcePath, destinationPath), isCloud, overwriteAlways, overwriteOld);
		}
	}

	/// <summary>
	/// Converts the full 'path' to remove the base path component.
	/// Example: C://My Documents//Help Me I'm Hungry.txt is full 'path'
	///		basePath is C://My Documents
	///		Thus returns 'Help Me I'm Hungry.txt'
	/// </summary>
	public static string ConvertToRelativePath(string basePath, string fullPath)
	{
		if (!fullPath.StartsWith(basePath)) {
			ModLoader.Logging.tML.Debug($"string {fullPath} does not contain string {basePath}. Is this correct?");
			return fullPath;
		}

		return fullPath.Substring(basePath.Length + 1);
	}

	/// <summary>
	/// DEtermines if should overwrite the file at Destination with the file at Source
	/// </summary>
	private static bool DetermineIfShouldOverwrite(bool overwriteAlways, bool overwriteOld, string source, string destination)
	{
		if (overwriteAlways)
			return true;

		// doesn't really matter
		if (!File.Exists(destination))
			return overwriteAlways;

		// If file exists, and we aren't going to overwrite old versions
		if (!overwriteOld)
			return false;

		var srcFile = File.GetLastWriteTimeUtc(source);
		var dstFile = File.GetLastWriteTimeUtc(destination);

		return dstFile < srcFile;
	}

	// TODO: Do we need to do extra work for .plr files that have been renamed? Is that valid?
	// TODO: We could probably support cloud players as well, if we tried.
	// Vanilla and 1.3 paths are defaults, 1.4 TML paths are relative to current savepath.
	public static (string path, string message, int stabilityLevel)[] GetAlternateSavePathFiles(string folderName)
	{
		return new (string path, string message, int stabilityLevel)[] {
			(path: Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), $"{folderName}"), "Click to copy \"{0}\" over from Terraria", 0),
			(path: Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), "ModLoader", $"{folderName}"), "Click to copy \"{0}\" over from 1.3 tModLoader", 0),
			(path: Path.Combine(Main.SavePath, "..", Program.ReleaseFolder, $"{folderName}"), "Click to copy \"{0}\" over from stable", 1),
			(path: Path.Combine(Main.SavePath, "..", Program.PreviewFolder, $"{folderName}"), "Click to copy \"{0}\" over from preview", 2),
			(path: Path.Combine(Main.SavePath, "..", Program.DevFolder, $"{folderName}"), "Click to copy \"{0}\" over from dev", 3),
			(path: Path.Combine(Main.SavePath, "..", Program.Legacy143Folder, $"{folderName}"), "Click to copy \"{0}\" over from 1.4.3-Legacy", 0),
		};
	}

	internal static bool WriteTagCompound(string path, bool isCloud, TagCompound tag)
	{
		var stream = new MemoryStream();
		TagIO.ToStream(tag, stream);

		var data = stream.ToArray();
		var fileName = Path.GetFileName(path);

		if (data[0] != 0x1F || data[1] != 0x8B) {
			Write(path + ".corr", data, data.Length, isCloud);
			throw new IOException($"Detected Corrupted Save Stream attempt.\nAborting to avoid {fileName} corruption.\nYour last successful save will be kept. ERROR: Stream Missing NBT Header.");
		}

		// Attempt 1: Write
		Write(path, data, data.Length, isCloud);
		if (Enumerable.SequenceEqual(ReadAllBytes(path, isCloud), data))
			return true;

		// Attempt 2: Write
		ModLoader.Logging.tML.Warn($"Detected failed save for {fileName}. Re-attempting after 2 seconds");
		System.Threading.Thread.Sleep(2000);

		Write(path, data, data.Length, isCloud);
		if (!Enumerable.SequenceEqual(ReadAllBytes(path, isCloud), data))
			throw new IOException($"Unable to save current progress.\nAborting to avoid {fileName} corruption.\nYour last successful save will be kept. ERROR: Stream Missing NBT Header.");

		return true;
	}
}