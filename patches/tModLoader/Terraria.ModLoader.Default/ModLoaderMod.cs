using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader.Default
{
	internal class ModLoaderMod : Mod
	{
		private static bool texturesLoaded;
		private static Texture2D mysteryItemTexture;
		private static Texture2D startBagTexture;
		private static Texture2D mysteryTileTexture;

		public override string Name => "ModLoader";

		internal ModLoaderMod()
		{
			Side = ModSide.NoSync;
		}

		public override void Load()
		{
			LoadTextures();
			AddTexture("MysteryItem", mysteryItemTexture);
			AddTexture("StartBag", startBagTexture);
			AddTexture("MysteryTile", mysteryTileTexture);
			AddItem("MysteryItem", new MysteryItem(), "ModLoader/MysteryItem");
			AddItemInfo("MysteryGlobalItemInfo", new MysteryGlobalItemInfo());
			AddItem("StartBag", new StartBag(), "ModLoader/StartBag");
			AddItem("AprilFools", new AprilFools(), "Terraria/Item_3389");
			AddTile("MysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTile("PendingMysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTileEntity("MysteryTileEntity", new MysteryTileEntity());
			AddPlayer("MysteryPlayer", new MysteryPlayer());
			AddModWorld("MysteryWorld", new MysteryWorld());
			AddModWorld("MysteryTilesWorld", new MysteryTilesWorld());
			AddCommand("HelpCommand", new HelpCommand());
			AddCommand("ModlistCommand", new ModlistCommand());
		}

		private static void LoadTextures()
		{
			if (Main.dedServ || texturesLoaded)
			{
				return;
			}
			mysteryItemTexture = ReadTexture("MysteryItem");
			startBagTexture = ReadTexture("StartBag");
			mysteryTileTexture = ReadTexture("MysteryTile");
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
