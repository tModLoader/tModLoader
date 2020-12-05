using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.Engine
{
	internal class TMLContentManager : ContentManager
	{
		private readonly TMLContentManager alternateContentManager;
		private readonly HashSet<string> ExistingImages = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		private int loadedAssets = 0;

		public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory, TMLContentManager alternateContentManager) : base(serviceProvider, rootDirectory) {
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
	}
}