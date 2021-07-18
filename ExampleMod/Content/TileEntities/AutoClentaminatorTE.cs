using System;
using System.Collections.Generic;
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
	public class AutoClentaminatorTE : ModTileEntity, IItemStorage
	{
		private class AutoClentaminatorItemStorage : ItemStorage
		{
			public AutoClentaminatorItemStorage() : base(1) {
			}

			public override bool IsItemValid(int slot, Item item) {
				return Solutions.ContainsKey(item.type);
			}

			public override int GetSlotSize(int slot, Item item) {
				return 50;
			}
		}

		private enum ConversionTypes
		{
			Pure = 0,
			Corrupt = 1,
			Hallow = 2,
			Mushroom = 3,
			Crimson = 4
		}

		private const int Range = 48;
		private const int Speed = 600;

		private static readonly Dictionary<int, ConversionTypes> Solutions = new Dictionary<int, ConversionTypes> {
			{ ItemID.GreenSolution, ConversionTypes.Pure },
			{ ItemID.RedSolution, ConversionTypes.Crimson },
			{ ItemID.PurpleSolution, ConversionTypes.Corrupt },
			{ ItemID.BlueSolution, ConversionTypes.Hallow },
			{ ItemID.DarkBlueSolution, ConversionTypes.Mushroom }
		};

		private int timer;
		private AutoClentaminatorItemStorage itemStorage;
		public float cleansingProgress;
		private float cleansingDelta;
		public int currentType;

		public AutoClentaminatorTE() {
			itemStorage = new AutoClentaminatorItemStorage();
		}

		public override void Update() {
			if (++timer >= Speed) {
				timer = 0;

				currentType = itemStorage[0].type;
				if (itemStorage.ModifyStackSize(this, 0, -1)) {
					cleansingDelta = 0.02f;

					SoundEngine.PlaySound(SoundID.NPCDeath59.WithVolume(0.4f), (Position.X * 16) + 24, (Position.Y * 16) + 24);
				}
			}

			cleansingProgress += cleansingDelta;
			if (cleansingProgress > 1f) {
				cleansingDelta = 0f;
				cleansingProgress = 0f;
			}
			else if (cleansingProgress > 0f) {
				// todo: clean this up
				Vector2 center = new Vector2((Position.X * 16f) + 24f, (Position.Y * 16) + 24f);
				float f = Range * 16 * cleansingProgress;
				f *= f;

				for (int y = (int)(-Range * cleansingProgress) + 1; y < Range * cleansingProgress; y++) {
					for (int x = (int)(-Range * cleansingProgress) + 1; x < Range * cleansingProgress; x++) {
						Vector2 point = new Vector2((x * 16f) + 8f, (y * 16) + 8f);
						if (Math.Abs(Vector2.DistanceSquared(center, point) - f) < 4f) continue;

						WorldGen.Convert(Position.X + x, Position.Y + y, (int)Solutions[currentType], 0);
					}
				}
			}
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.IsActive && tile.type == ModContent.TileType<AutoClentaminator>() && tile.frameX == 0 && tile.frameY == 0;
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
			itemStorage.DropItems(this, new Rectangle(Position.X * 16, Position.Y * 16, 48, 48));
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