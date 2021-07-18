using System.Linq;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Container;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.TileEntities
{
	public class ItemCollectorTE : ModTileEntity, IItemStorage
	{
		private static int[] IgnoredItems = { ItemID.Heart, ItemID.CandyApple, ItemID.CandyCane, ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum };

		private const int Range = 10 * 16;
		private const int Speed = 30;

		private int timer;
		private ItemStorage itemStorage;

		public ItemCollectorTE() {
			itemStorage = new ItemStorage(18);
		}

		public override void Update() {
			if (++timer >= Speed) {
				timer = 0;
				Vector2 center = new Vector2((Position.X * 16) + 16, (Position.Y * 16) + 16);

				for (int i = 0; i < Main.maxItems; i++) {
					ref Item item = ref Main.item[i];

					if (!item.active || item.IsAir) continue;
					if (item.IsACoin || IgnoredItems.Contains(item.type) || ItemID.Sets.NebulaPickup[item.type]) continue;

					if (Vector2.DistanceSquared(item.Center, center) > Range * Range) continue;

					item.noGrabDelay = 0;
					itemStorage.InsertItem(this, ref item);
				}

				int index = Chest.FindChest(Position.X - 2, Position.Y);
				if (index == -1) index = Chest.FindChest(Position.X + 2, Position.Y);
				if (index == -1) return;

				foreach (Item item in itemStorage) {
					if (!item.active || item.IsAir) continue;

					PlaceItemInChest(index, item);
				}
			}
		}

		// Adapted from ChestUI.TryPlacingInChest
		private static void PlaceItemInChest(int index, Item item) {
			var chestinv = Main.chest[index].item;

			if (item.maxStack > 1) {
				for (int i = 0; i < chestinv.Length; i++) {
					if (chestinv[i].stack >= chestinv[i].maxStack || !item.IsTheSameAs(chestinv[i]))
						continue;

					int num = item.stack;
					if (item.stack + chestinv[i].stack > chestinv[i].maxStack)
						num = chestinv[i].maxStack - chestinv[i].stack;

					item.stack -= num;
					chestinv[i].stack += num;
					SoundEngine.PlaySound(7);
					if (item.stack <= 0) {
						item.SetDefaults();
						NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, index, i);

						break;
					}

					if (chestinv[i].type == ItemID.None) {
						chestinv[i] = item.Clone();
						item.SetDefaults();
					}

					NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, index, i);
				}
			}

			if (item.stack > 0) {
				for (int j = 0; j < chestinv.Length; j++) {
					if (chestinv[j].stack != 0)
						continue;

					SoundEngine.PlaySound(7);
					chestinv[j] = item.Clone();
					item.SetDefaults();
					NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, index, j);

					break;
				}
			}
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.IsActive && tile.type == ModContent.TileType<ItemCollector>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j - 1, Type);
				return -1;
			}

			return Place(i, j - 1);
		}

		public override void OnKill() {
			itemStorage.DropItems(this, new Rectangle(Position.X * 16, Position.Y * 16, 32, 32));
		}

		public override TagCompound Save() {
			return itemStorage.Save();
		}

		public override void Load(TagCompound tag) {
			itemStorage.Load(tag);
		}

		public ItemStorage GetItemStorage() => itemStorage;
	}
}