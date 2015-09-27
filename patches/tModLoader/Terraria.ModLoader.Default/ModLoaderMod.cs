using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

#if CLIENT
using Properties;
#endif
namespace Terraria.ModLoader.Default
{
	public class ModLoaderMod : Mod
	{
		private static bool texturesLoaded = false;
		private static Texture2D mysteryItemTexture;

		public ModLoaderMod()
		{
			file = "ModLoader";
		}

		public override void SetModInfo(out string name, ref ModProperties properties)
		{
			name = "ModLoader";
		}

		public override void Load()
		{
			LoadTextures();
			AddTexture("MysteryItem", mysteryItemTexture);
			AddItem("MysteryItem", new MysteryItem(), FileName("MysteryItem"));
		}

		private static void LoadTextures()
		{
#if CLIENT
			if (texturesLoaded)
			{
				return;
			}
            byte[] data;
            using(MemoryStream stream = new MemoryStream())
            {
                Resources.MysteryItem.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                data = stream.ToArray();
            }
			using (MemoryStream stream = new MemoryStream(data))
			{
				mysteryItemTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
			}
			texturesLoaded = true;
#endif
		}
	}
}
