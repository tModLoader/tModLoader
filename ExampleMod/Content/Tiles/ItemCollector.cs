using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ModLoader.Container;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ItemCollector : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;

			TileObjectData newTile = TileObjectData.newTile;
			newTile.CopyFrom(TileObjectData.Style2x2);
			newTile.Width = 2;
			newTile.Height = 2;
			newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, newTile.Width, 0);
			newTile.Origin = new Point16(0, 1);
			newTile.CoordinateHeights = new[] { 16, 16 };
			newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ItemCollectorTE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Item Collector");
			AddMapEntry(new Color(190, 230, 190), name);
		}

		public override bool RightClick(int i, int j) {
			if (!TileEntityUtils.TryGetTileEntity(i, j, out ItemCollectorTE te)) return false;

			ItemStorage storage = te.GetItemStorage();
			Main.LocalPlayer.LootAll(storage);

			return true;
		}

		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Items.Placeable.ItemCollector>();
			Main.LocalPlayer.cursorItemIconEnabled = true;

			Main.LocalPlayer.noThrow = 2;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 32, 32, ModContent.ItemType<Items.Placeable.ItemCollector>());
			ModContent.GetInstance<ItemCollectorTE>().Kill(i, j);
		}
	}
}