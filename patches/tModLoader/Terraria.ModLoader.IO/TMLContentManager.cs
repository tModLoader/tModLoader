using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Terraria.ModLoader.IO
{
	public class TMLContentManager : ContentManager
	{
		public TMLContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory) { }

		protected override Stream OpenStream(string assetName) {
			if (!assetName.StartsWith("tmod:"))
				return base.OpenStream(assetName);

			ModContent.SplitName(assetName.Substring(5).Replace('\\', '/'), out var modName, out var entryPath);
			if (!entryPath.EndsWith(".xnb"))
				entryPath += ".xnb";

			return ModLoader.GetMod(modName).GetFileSteam(entryPath);
		}
	}
}