using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Container;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.TileEntities
{
	public class ExampleContainerTE : ModTileEntity, IItemHandler
	{
		private enum ConversionTypes
		{
			Pure = 0,
			Corrupt = 1,
			Hallow = 2,
			Mushroom = 3,
			Crimson = 4
		}

		private const int range = 48;
		private const int speed = 30;

		private int timer;
		private ItemHandler ItemHandler;

		public ExampleContainerTE() {
			ItemHandler = new ItemHandler();
			ItemHandler.IsItemValid += (slot, item) => item.type == ItemID.PurificationPowder || item.type == ItemID.ViciousPowder || item.type == ItemID.VilePowder;
			ItemHandler.GetSlotSize += slot => 50;
		}

		public override void Update() {
			if (++timer >= speed) {
				timer = 0;

				int type = ItemHandler.GetItemInSlot(0).type;
				if (ItemHandler.Shrink(0, 1)) {
					int i = Position.X + 1 + Main.rand.Next(-range, range + 1);
					int j = Position.Y + 1 + Main.rand.Next(-range, range + 1);

					WorldGen.Convert(i, j, (int)ConversionTypes.Corrupt, 2);
				}
			}
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && tile.type == ModContent.TileType<ExampleContainer>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i - 1, j - 2, Type);
				return -1;
			}

			return Place(i - 1, j - 2);
		}

		public override void OnKill() {
			ItemHandler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 48));
		}

		public override TagCompound Save() {
			return ItemHandler.Save();
		}

		public override void Load(TagCompound tag) {
			ItemHandler.Load(tag);
		}

		public ItemHandler GetItemHandler() => ItemHandler;
	}
}