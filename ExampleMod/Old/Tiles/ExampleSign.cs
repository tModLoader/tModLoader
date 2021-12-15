using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Tiles
{
	public class ExampleSign : ModTile
	{
		public override void SetDefaults()
		{
			// Credits to Dark;Light for finding this flag
			// Keep in mind that the max amount of signs is 1000 (the size of the tileSign array)
			// The Main.tileSign flag will do the following:
			//  *Automatically manages the sign for the specified tile
			//   -Adds a right-click to the tile to bring up an edit sign window
			//   -Allows editing of the sign text
			//   -Saves and loads sign data to world file
			Main.tileSign[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			// Use a 2x2 style as our foundation
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

			// Allow hanging from ceilings
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.StyleHorizontal = true;
			TileObjectData.newAlternate.AnchorAlternateTiles = new int[] { 124 };
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorLeft = AnchorData.Empty;
			TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(1);

			// Allow attaching to a solid object that is to the left of the sign
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.StyleHorizontal = true;
			TileObjectData.newAlternate.AnchorAlternateTiles = new int[] { 124 };
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(2);

			// Allow attaching to a solid object that is to the right of the sign
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.StyleHorizontal = true;
			TileObjectData.newAlternate.AnchorAlternateTiles = new int[] { 124 };
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(3);

			// Allow attaching to a wall behind the sign
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.StyleHorizontal = true;
			TileObjectData.newAlternate.AnchorAlternateTiles = new int[] { 124 };
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(4);

			// Allow attaching sign to the ground
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.StyleHorizontal = true;
			TileObjectData.newAlternate.AnchorAlternateTiles = new int[] { 124 };
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.addAlternate(5);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Example Sign");
			AddMapEntry(new Color(200, 200, 200), name);
			dustType = mod.DustType("Sparkle");
			disableSmartCursor = true;
			adjTiles = new int[] { Type };
		}

		public override bool HasSmartInteract()
		{
			return true;
		}

		public override void PlaceInWorld(int i, int j, Terraria.Item item)
		{
			Main.sign[Sign.ReadSign(i, j, true)].text = "Hello, I am a sign!";
		}

		public override bool NewRightClick(int i, int j)
		{
			var sign = Main.sign[Sign.ReadSign(i, j, true)];

			Main.NewText("You right-clicked on a sign!", Color.Yellow);
			Main.NewText($"Sign Position: X:{sign.x}, Y:{sign.y}", Color.Yellow);
			Main.NewText(Main.sign[Sign.ReadSign(i, j, true)].text);
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Terraria.Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("ExampleSign"));
			Sign.KillSign(i, j);
		}
	}
}