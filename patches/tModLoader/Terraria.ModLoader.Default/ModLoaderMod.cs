using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Terraria.ModLoader.Default
{
	public class ModLoaderMod : Mod
	{
		private static bool texturesLoaded = false;
		private static Texture2D mysteryItemTexture;
		private static Texture2D startBagTexture;

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
			AddTexture("StartBag", startBagTexture);
			AddItem("MysteryItem", new MysteryItem(), FileName("MysteryItem"));
			AddItem("StartBag", new StartBag(), FileName("StartBag"));
			AddPlayer("MysteryPlayer", new MysteryPlayer());
		}

		private static void LoadTextures()
		{
			if (Main.dedServ || texturesLoaded)
			{
				return;
			}
			mysteryItemTexture = ReadTexture("MysteryItem");
			startBagTexture = ReadTexture("StartBag");
			texturesLoaded = true;
		}

		private static Texture2D ReadTexture(string file)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("Terraria.ModLoader.Default." + file + ".png");
			return Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
		}
	}
}
