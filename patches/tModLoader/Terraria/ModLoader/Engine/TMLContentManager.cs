using Microsoft.Xna.Framework.Content;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Terraria.ModLoader.Engine
{
	internal class TMLContentManager : ContentManager
	{
		private readonly TMLContentManager alternateContentManager;
		private readonly HashSet<string> ExistingImages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		private int loadedAssets = 0;

		public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory, TMLContentManager alternateContentManager) : base(serviceProvider, rootDirectory) {
			TryFixFileCasings(rootDirectory);

			this.alternateContentManager = alternateContentManager;

			//Fill cache for ImageExists() lookup.
			void CacheImagePaths(string path) {
				string basePath = Path.Combine(path, "Images");

				foreach (string file in Directory.EnumerateFiles(basePath, "*.xnb", SearchOption.AllDirectories)) {
					ExistingImages.Add(Path.GetFileNameWithoutExtension(file.Remove(0, basePath.Length + 1)));
				}
			}

			CacheImagePaths(rootDirectory);

			if (alternateContentManager != null)
				CacheImagePaths(alternateContentManager.RootDirectory);
		}

		protected override Stream OpenStream(string assetName) {
			if (!assetName.StartsWith("tmod:")) {
				if (alternateContentManager != null && File.Exists(Path.Combine(alternateContentManager.RootDirectory, assetName + ".xnb"))) { 
					try {
						return alternateContentManager.OpenStream(assetName);
					}
					catch {}
				}
				return base.OpenStream(assetName);
			}

			if (!assetName.EndsWith(".xnb"))
				assetName += ".xnb";

			return ModContent.OpenRead(assetName);
		}

		public override T Load<T>(string assetName) {

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
		public bool TryGetPath(string asset, out string result) {
			if (alternateContentManager != null && alternateContentManager.TryGetPath(asset, out result)) {
				return true;
			}

			string path = Path.Combine(RootDirectory, asset);

			result = File.Exists(path) ? path : null;

			return result != null;
		}

		public bool ImageExists(string assetName) => ExistingImages.Contains(assetName);

		private static void TryFixFileCasings(string rootDirectory) {
			// Windows stale resource file case workaround
			if (!Platform.IsWindows) {
				return;
			}

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
				"Images/Projectile_618.xnb"
			};

			foreach (string problematicAsset in problematicAssets) {
				string expectedName = Path.GetFileName(problematicAsset);
				string expectedFullPath = Path.Combine(rootDirectory, problematicAsset);
				var faultyAssetInfo = new FileInfo(Path.Combine(rootDirectory, problematicAsset));

				// faultyAssetInfo.Name should be the system file cased name, but this is not the case
				// don't rely to that either way to be sure
				if (!faultyAssetInfo.Exists) {
					continue;
				}

				// This assetInfo is correct cased (but only the name, need recursive if you want full case,
				// nothing more is needed in this case though
				var assetInfo = faultyAssetInfo.Directory.EnumerateFileSystemInfos(faultyAssetInfo.Name).First();
				string realName = assetInfo.Name;

				if (expectedName == realName) {
					continue;
				}

				// The asset is wrongfully cased, fix that,
				// changing a vanilla file name is something to log for sure
				string relativeRealPath = Path.GetRelativePath(rootDirectory, assetInfo.FullName);

				Logging.tML.Info($"Found vanilla asset with wrong case, renaming: (from {rootDirectory}) {relativeRealPath} -> {problematicAsset}");
				// Programmatically move with different case works
				File.Move(assetInfo.FullName, expectedFullPath);
			}
		}
	}
}