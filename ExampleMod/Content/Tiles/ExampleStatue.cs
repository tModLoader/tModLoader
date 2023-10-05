using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ExampleStatue : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.addTile(Type);

			DustType = 11;

			AddMapEntry(new Color(144, 148, 144), CreateMapEntryName());
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			var source = WorldGen.GetItemSource_FromTileBreak(i, j);

			Item.NewItem(source, i * 16, j * 16, 32, 48, ModContent.ItemType<Items.Placeable.ExampleStatue>());
		}

		// This hook allows you to make anything happen when this statue is powered by wiring.
		// In this example, powering the statue either spawns a random coin with a 95% chance, or, with a 5% chance - a goldfish.
		public override void HitWire(int i, int j) {
			// Find the coordinates of top left tile square through math
			int y = j - Main.tile[i, j].TileFrameY / 18;
			int x = i - Main.tile[i, j].TileFrameX / 18;

			const int TileWidth = 2;
			const int TileHeight = 3;

			for (int yy = y; yy < y + TileHeight; yy++) {
				for (int xx = x; xx < x + TileWidth; xx++) {
					Wiring.SkipWire(xx, yy);
				}
			}

			// Calculcate the center of this tile to use as an entity spawning position.
			// Note that we use 0.65 for height because even though the statue takes 3 blocks, its appearance is shorter.
			float spawnX = (x + TileWidth * 0.5f) * 16;
			float spawnY = (y + TileHeight * 0.65f) * 16;

			// This example shows both item spawning code and npc spawning code, you can use whichever code suits your mod
			// There is a 95% chance for item spawn and a 5% chance for npc spawn
			// If you want to make a item spawning statue, see below.

			var entitySource = new EntitySource_TileUpdate(x, y, context: "ExampleStatue");

			if (Main.rand.NextFloat() < .95f) {
				if (Wiring.CheckMech(x, y, 60) && Item.MechSpawn(spawnX, spawnY, ItemID.SilverCoin) && Item.MechSpawn(spawnX, spawnY, ItemID.GoldCoin) && Item.MechSpawn(spawnX, spawnY, ItemID.PlatinumCoin)) {
					int id = ItemID.SilverCoin;

					if (Main.rand.NextBool(100)) {
						id++;

						if (Main.rand.NextBool(100)) {
							id++;
						}
					}

					Item.NewItem(entitySource, (int)spawnX, (int)spawnY - 20, 0, 0, id, 1, false, 0, false);
				}
			}
			else {
				// If you want to make an NPC spawning statue, see below.
				int npcIndex = -1;

				// 30 is the time before it can be used again. NPC.MechSpawn checks nearby for other spawns to prevent too many spawns. 3 in immediate vicinity, 6 nearby, 10 in world.
				int spawnedNpcId = NPCID.Goldfish;

				if (Wiring.CheckMech(x, y, 30) && NPC.MechSpawn(spawnX, spawnY, spawnedNpcId)) {
					npcIndex = NPC.NewNPC(entitySource, (int)spawnX, (int)spawnY - 12, spawnedNpcId);
				}

				if (npcIndex >= 0) {
					var npc = Main.npc[npcIndex];

					npc.value = 0f;
					npc.npcSlots = 0f;
					// Prevents Loot if NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue and !Main.HardMode or NPCID.Sets.StatueSpawnedDropRarity != -1 and NextFloat() >= NPCID.Sets.StatueSpawnedDropRarity or killed by traps.
					// Prevents CatchNPC
					npc.SpawnedFromStatue = true;
				}
			}
		}
	}
}