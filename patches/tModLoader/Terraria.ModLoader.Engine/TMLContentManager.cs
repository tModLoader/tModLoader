using System;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Terraria.ModLoader.Engine
{
	internal class TMLContentManager : ContentManager
	{
		private TMLContentManager alternateContentManager;
		public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory, TMLContentManager alternateContentManager) : base(serviceProvider, rootDirectory) {
			this.alternateContentManager = alternateContentManager;
		}

		protected override Stream OpenStream(string assetName) {
			if (!assetName.StartsWith("tmod:"))
				return base.OpenStream(assetName);

			if (!assetName.EndsWith(".xnb"))
				assetName += ".xnb";

			return ModContent.OpenRead(assetName);
		}

		public override T Load<T>(string assetName) {
			// default Load implementation is just ReadAsset with a cache. Don't cache tML assets, because then we'd have to clear the cache on mod loading.
			// Mods use Mod.GetFont/GetEffect rather than ContentManager.Load directly anyway, so Load should only be called once per mod load by tML.
			if (assetName.StartsWith("tmod:"))
				return ReadAsset<T>(assetName, null);

			// Prioritize loading from alternateContentManager since it contains 1.3.5 assets, assets that are missing or different resolutions in 1.4
			if (alternateContentManager != null && File.Exists(Path.Combine(alternateContentManager.RootDirectory, assetName + ".xnb")))
				return alternateContentManager.Load<T>(assetName); // Funky approach because RootDirectory is enforced and can't change on the fly.

			return base.Load<T>(assetName);
		}

		public bool ImageExists(string assetName)
		{
			return File.Exists(Path.Combine(RootDirectory, "Image", assetName + ".xnb")) || alternateContentManager != null && File.Exists(Path.Combine(alternateContentManager.RootDirectory, "Image", assetName + ".xnb"));
		}
	}
}