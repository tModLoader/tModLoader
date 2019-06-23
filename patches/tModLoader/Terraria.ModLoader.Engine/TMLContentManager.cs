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
	}
}