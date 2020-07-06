using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent; //This lets us access methods (like ItemType) from ModContent without having to type its name.

namespace ExampleMod.Content.Tiles.Furniture
{
	public class ExampleWorkbench : ModTile
	{
		public override void SetDefaults() {
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = new[] { 18 };
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Example Workbench");
			AddMapEntry(new Color(200, 200, 200), name);

			dustType = DustType<Dusts.Sparkle>();
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.WorkBenches };
		}

		public override void NumDust(int x, int y, bool fail, ref int num) => num = fail ? 1 : 3;

		public override void KillMultiTile(int x, int y, int frameX, int frameY) {
			Item.NewItem(x * 16, y * 16, 32, 16, ItemType<Items.Placeable.Furniture.ExampleWorkbench>());
		}
	}
}