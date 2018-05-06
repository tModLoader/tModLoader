using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Default.Patreon;

namespace Terraria.ModLoader.Default
{
	internal class ModLoaderMod : Mod
	{
		private static bool texturesLoaded;
		private static Texture2D mysteryItemTexture;
		private static Texture2D startBagTexture;
		private static Texture2D mysteryTileTexture;

		public override string Name => "ModLoader";
		public override Version Version => ModLoader.version;
		public override Version tModLoaderVersion => ModLoader.version;

		internal ModLoaderMod()
		{
			Side = ModSide.NoSync;
			DisplayName = "tModLoader";
		}

		public override void Load()
		{
			LoadTextures();
			AddTexture("MysteryItem", mysteryItemTexture);
			AddTexture("StartBag", startBagTexture);
			AddTexture("MysteryTile", mysteryTileTexture);
			AddItem("MysteryItem", new MysteryItem());
			AddGlobalItem("MysteryGlobalItem", new MysteryGlobalItem());
			AddItem("StartBag", new StartBag());
			AddItem("AprilFools", new AprilFools());
			AddTile("MysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTile("PendingMysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTileEntity("MysteryTileEntity", new MysteryTileEntity());
			AddPlayer("MysteryPlayer", new MysteryPlayer());
			AddModWorld("MysteryWorld", new MysteryWorld());
			AddModWorld("MysteryTilesWorld", new MysteryTilesWorld());
			AddCommand("HelpCommand", new HelpCommand());
			AddCommand("ModlistCommand", new ModlistCommand());
			AddPatreon();
		}

		private void AddPatreon()
		{
			AddPatreonItemAndEquipType(new toplayz_Head(), "toplayz", EquipType.Head);
			AddPatreonItemAndEquipType(new toplayz_Body(), "toplayz", EquipType.Body);
			AddPatreonItemAndEquipType(new toplayz_Legs(), "toplayz", EquipType.Legs);
			AddPatreonItemAndEquipType(new KittyKitCatCat_Head(), "KittyKitCatCat", EquipType.Head);
			AddPatreonItemAndEquipType(new KittyKitCatCat_Body(), "KittyKitCatCat", EquipType.Body);
			AddPatreonItemAndEquipType(new KittyKitCatCat_Legs(), "KittyKitCatCat", EquipType.Legs);
			AddPatreonItemAndEquipType(new PotyBlank_Head(), "PotyBlank", EquipType.Head);
			AddPatreonItemAndEquipType(new PotyBlank_Body(), "PotyBlank", EquipType.Body);
			AddPatreonItemAndEquipType(new PotyBlank_Legs(), "PotyBlank", EquipType.Legs);
			AddPatreonItemAndEquipType(new Dinidini_Head(), "dinidini", EquipType.Head);
			AddPatreonItemAndEquipType(new Dinidini_Body(), "dinidini", EquipType.Body);
			AddPatreonItemAndEquipType(new Dinidini_Legs(), "dinidini", EquipType.Legs);
		}

		private void AddPatreonItemAndEquipType(ModItem item, string name, EquipType equipType)
		{
			if (!Main.dedServ)
			{
				AddTexture($"Patreon.{name}_{equipType}", ReadTexture($"Patreon.{name}_{equipType}"));
				AddTexture($"Patreon.{name}_{equipType}_{equipType}", ReadTexture($"Patreon.{name}_{equipType}_{equipType}"));
				if (equipType == EquipType.Body)
					AddTexture($"Patreon.{name}_{equipType}_Arms", ReadTexture($"Patreon.{name}_{equipType}_Arms"));
			}
			AddItem($"{name}_{equipType}", item);
			AddEquipTexture(item, equipType, item.Name, item.Texture + '_' + equipType, item.Texture + "_Arms", item.Texture + "_FemaleBody");
		}

		private static void LoadTextures()
		{
			if (Main.dedServ)
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
