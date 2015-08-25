using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

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
			ModLoader.AddTexture("ModLoader/MysteryItem", mysteryItemTexture);
			AddItem("MysteryItem", new MysteryItem(), "ModLoader/MysteryItem");
		}

		private static void LoadTextures()
		{
			if (texturesLoaded)
			{
				return;
			}
			//this is what happens when you're lazy
			byte[] bytes = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 26, 0, 0, 0, 26, 8, 6, 0, 0, 0, 169, 74, 76, 206, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 6, 98, 75, 71, 68, 0, 255, 0, 255, 0, 255, 160, 189, 167, 147, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 11, 19, 0, 0, 11, 19, 1, 0, 154, 156, 24, 0, 0, 0, 7, 116, 73, 77, 69, 7, 223, 7, 18, 20, 14, 24, 241, 76, 249, 42, 0, 0, 0, 29, 105, 84, 88, 116, 67, 111, 109, 109, 101, 110, 116, 0, 0, 0, 0, 0, 67, 114, 101, 97, 116, 101, 100, 32, 119, 105, 116, 104, 32, 71, 73, 77, 80, 100, 46, 101, 7, 0, 0, 0, 124, 73, 68, 65, 84, 72, 199, 237, 86, 209, 10, 192, 32, 8, 236, 98, 31, 126, 127, 238, 30, 134, 32, 130, 36, 49, 156, 27, 19, 132, 48, 45, 175, 51, 11, 36, 101, 20, 200, 28, 69, 114, 232, 128, 100, 42, 64, 228, 58, 0, 0, 41, 127, 93, 183, 12, 209, 180, 153, 90, 189, 219, 94, 143, 232, 223, 168, 255, 61, 138, 238, 133, 183, 219, 74, 234, 141, 104, 55, 211, 108, 124, 125, 49, 0, 72, 169, 247, 143, 196, 207, 215, 115, 180, 203, 149, 102, 189, 234, 234, 207, 33, 90, 189, 51, 30, 177, 71, 18, 33, 123, 15, 71, 125, 59, 195, 46, 71, 237, 16, 225, 115, 255, 186, 19, 56, 249, 89, 51, 83, 14, 228, 129, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				mysteryItemTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
			}
			texturesLoaded = true;
		}
	}
}
