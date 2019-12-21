using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Terraria.ModLoader.Engine
{
	internal class TMLContentManager : ContentManager
	{
		public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory) { }

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

			return base.Load<T>(assetName);
		}
	}
}