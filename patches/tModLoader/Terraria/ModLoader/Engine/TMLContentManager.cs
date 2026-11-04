using Microsoft.Xna.Framework.Content;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Terraria.ModLoader.Engine;

internal class TMLContentManager : ContentManager
{
	internal readonly TMLContentManager overrideContentManager;
	private readonly HashSet<string> ExistingImages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

	private int loadedAssets = 0;

	public IEnumerable<string> RootDirectories {
		get {
			if (overrideContentManager != null)
				yield return overrideContentManager.RootDirectory;

			yield return RootDirectory;
		}
	}

	public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory, TMLContentManager overrideContentManager) : base(serviceProvider, rootDirectory)
	{
		//Solxan for discussion: this only runs if tModLoader has a Content/Images folder based on the Main.tml.cs call site.
		if (overrideContentManager != null) // Only need to check vanilla content directory. 
			TryFixFileCasings(rootDirectory);

		this.overrideContentManager = overrideContentManager;

		//Fill cache for ImageExists() lookup.
		void CacheImagePaths(string path) {
			string basePath = Path.Combine(path, "Images");

			foreach (string file in Directory.EnumerateFiles(basePath, "*.xnb", SearchOption.AllDirectories)) {
				ExistingImages.Add(Path.GetFileNameWithoutExtension(file.Remove(0, basePath.Length + 1)));
			}
		}

		CacheImagePaths(rootDirectory);

		if (overrideContentManager != null)
			CacheImagePaths(overrideContentManager.RootDirectory);
	}

	protected override Stream OpenStream(string assetName)
	{
		if (!assetName.StartsWith("tmod:")) {
			if (overrideContentManager != null && File.Exists(Path.Combine(overrideContentManager.RootDirectory, assetName + ".xnb"))) {
				try {
					using var _ = new Logging.QuietExceptionHandle();
					return overrideContentManager.OpenStream(assetName);
				}
				catch {}
			}
			return base.OpenStream(assetName);
		}

		if (!assetName.EndsWith(".xnb"))
			assetName += ".xnb";

		return ModContent.OpenRead(assetName);
	}

	public override T Load<T>(string assetName)
	{

		// default Load implementation is just ReadAsset with a cache. Don't cache tML assets, because then we'd have to clear the cache on mod loading.
		// Mods use Mod.GetFont/GetEffect rather than ContentManager.Load directly anyway, so Load should only be called once per mod load by tML.
		if (assetName.StartsWith("tmod:"))
			return ReadAsset<T>(assetName, null);

		loadedAssets++;
		if (loadedAssets % 1000 == 0)
			Logging.Terraria.Info($"Loaded {loadedAssets} vanilla assets");

		return base.Load<T>(assetName);
	}

	/// <summary> Returns a path to the provided relative asset path, prioritizing overrides in the alternate content manager. Throws exceptions on failure. </summary>
	public string GetPath(string asset) => TryGetPath(asset, out string result) ? result : throw new FileNotFoundException($"Unable to find asset '{asset}'.");
	/// <summary> Safely attempts to get a path to the provided relative asset path, prioritizing overrides in the alternate content manager. </summary>
	public bool TryGetPath(string asset, out string result)
	{
		if (overrideContentManager != null && overrideContentManager.TryGetPath(asset, out result)) {
			return true;
		}

		string path = Path.Combine(RootDirectory, asset);

		result = File.Exists(path) ? path : null;

		return result != null;
	}

	public bool ImageExists(string assetName) => ExistingImages.Contains(assetName);

	private static void TryFixFileCasings(string rootDirectory)
	{
		// The file listed below will be checked and fixed for case on disk
		// this method does not work on UNC paths (don't think remote path Terraria
		// installs will be present in a long time, but good to keep this logged)
		// and will only find/change FILE case, not all the directory tree.
		// A full implementation for search of actual name can be found at:
		// https://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
		string[] problematicAssets = {
			"Images/NPC_517.xnb",
			"Images/Gore_240.xnb",
			"Images/Projectile_179.xnb",
			"Images/Projectile_189.xnb",
			"Images/Projectile_618.xnb",
			"Images/Tiles_650.xnb",
			"Images/Item_2648.xnb"
		};

		bool? thisOsIsMoveProtected = null;

		foreach (string problematicAsset in problematicAssets) {
			if (ValidateFileCasing(rootDirectory, problematicAsset, out string actualFullPath))
				continue;

			if (thisOsIsMoveProtected == null)
				thisOsIsMoveProtected = IsThisOSMoveProtectedForCaseChange();

			// The asset is wrongfully cased, fix that,
			// changing a vanilla file name is something to log for sure
			string relativeActualPath = Path.GetRelativePath(rootDirectory, actualFullPath);

			Logging.tML.Info($"Found vanilla asset with wrong case, renaming: (from {rootDirectory}) {relativeActualPath} -> {problematicAsset}");

			string expectedFullPath = Path.Combine(rootDirectory, problematicAsset);

			// Programmatically & defensively move to force case change
			if (thisOsIsMoveProtected is true) {
				File.Move(actualFullPath, actualFullPath + ".1");
				File.Move(actualFullPath + ".1", expectedFullPath);
			}
			else {
				File.Move(actualFullPath, expectedFullPath);
			}
		}
	}

	/// <summary>
	/// In OS environments with both case insensitive and case sensitive you either allow or block having two files with different casing.
	/// For example, Windows blocks NTFS and exFAT folders from having both client.log and Client.log normally.
	/// However NTFS is case-sensitive itself so you can change how it handles in WSL using tools like https://learn.microsoft.com/en-us/windows/wsl/case-sensitivity
	/// But exFAT is strictly case-insensitive.
	/// Where this gets messy is if you try to 'Move' a file on a particular OS to force a capitalization rename.
	/// If the OS is blocking, and it copies before deleting, then a singular 'move' is not enough to change capitalization. 
	/// </summary>
	/// <returns>True if the OS blocks using move directly to rename files to fix casing</returns>
	private static bool IsThisOSMoveProtectedForCaseChange()
	{
		string canaryFile = "LaunchUtils/canaryfile.txt";
		bool moveCaseProtected = false;

		var fileStream = File.Create(canaryFile);
		fileStream.Close();

		try {
			File.Move(canaryFile, "LaunchUtils/cAnAryfile.txt");
		}
		catch (Exception) {
			moveCaseProtected = true;
		}
		finally {
			File.Delete(canaryFile);
		}
		return moveCaseProtected;
	}

	/// <summary>
	/// Returns False if the File in Root Directory does not match the expected casing 
	/// <param name="actualFullPath"></param>
	/// <returns></returns>
	private static bool ValidateFileCasing(string rootDirectory, string fileToCheck, out string asFoundFullPath)
	{
		var fileInfo = new FileInfo(Path.Combine(rootDirectory, fileToCheck));
		string expectedName = Path.GetFileName(fileToCheck);

		asFoundFullPath = fileInfo.FullName;

		// There are 4 possible states
		//		File is found using FileInfo.Exists && the FileSystemInfo for it has the same casing (happy-path)
		//		File is found using FileInfo.Exists && the FileSystemInfo for it has different casing (case insenstive read)
		//		File is NOT found using FileInfo.Exists && file is NOT on the user system (missing file)
		//		File is NOT found using FileInfo.Exists && file has the wrong casing (case sensitive read)

		FileSystemInfo fileSystemInfo;

		if (fileInfo.Exists) {
			// likely case insensitive read OR happy-path 
			fileSystemInfo = fileInfo.Directory.EnumerateFileSystemInfos(fileInfo.Name).First();

			// Happy-path short-circuit
			if (fileSystemInfo.Name == expectedName)
				return true;
		}
		else {
			// Case sensitive read OR missing
			fileSystemInfo = fileInfo.Directory.EnumerateFileSystemInfos().FirstOrDefault(p => p.Name.Equals(expectedName, StringComparison.InvariantCultureIgnoreCase));

			// file is missing short-circuit
			if (fileSystemInfo == null) {
				Logging.tML.Info($"An expected vanilla asset is missing: (from {rootDirectory}) {fileToCheck}");
				return true;
			}
		}

		asFoundFullPath = fileSystemInfo.FullName;
		return false;
	}
}