using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Container;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class AutoClentaminator : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;

			TileObjectData newTile = TileObjectData.newTile;
			newTile.CopyFrom(TileObjectData.Style3x3);
			newTile.Width = 3;
			newTile.Height = 3;
			newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, newTile.Width, 0);
			newTile.Origin = new Point16(1, 2);
			newTile.CoordinateHeights = new[] { 16, 16, 18 };
			newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AutoClentaminatorTE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Elemental Purge");
			AddMapEntry(new Color(190, 230, 190), name);
			dustType = 11;
		}

		public override bool RightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			int left = i - ((tile.frameX % 54) / 18);
			int top = j - ((tile.frameY % 54) / 18);

			int index = ModContent.GetInstance<AutoClentaminatorTE>().Find(left, top);
			if (index == -1) return false;

			AutoClentaminatorTE te = (AutoClentaminatorTE)TileEntity.ByID[index];
			ItemHandler handler = te.GetItemHandler();

			Item item = new Item(ItemID.PurpleSolution) { stack = 5 };
			handler.InsertItem(0, ref item, true);
			Main.NewText($"Inserted {5 - item.stack} items");
			Item itemInSlot = handler.GetItemInSlot(0);
			Main.NewText($"Currently has {itemInSlot.Name} x{itemInSlot.stack}");

			return true;
		}

		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Items.Placeable.AutoClentaminator>();
			Main.LocalPlayer.cursorItemIconEnabled = true;

			Main.LocalPlayer.noThrow = 2;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 48, 48, ModContent.ItemType<Items.Placeable.AutoClentaminator>());
			ModContent.GetInstance<AutoClentaminatorTE>().Kill(i, j);
		}
	}
}