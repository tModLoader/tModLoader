using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class ExampleStatue : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Statue");
			AddMapEntry(new Color(144, 148, 144), name);
			dustType = 11;
			disableSmartCursor = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 32, 48, ItemType<ExampleStatueItem>());
		}

		public override void HitWire(int i, int j) {
			// Find the coordinates of top left tile square through math
			int y = j - Main.tile[i, j].frameY / 18;
			int x = i - Main.tile[i, j].frameX / 18;

			Wiring.SkipWire(x, y);
			Wiring.SkipWire(x, y + 1);
			Wiring.SkipWire(x, y + 2);
			Wiring.SkipWire(x + 1, y);
			Wiring.SkipWire(x + 1, y + 1);
			Wiring.SkipWire(x + 1, y + 2);

			// We add 16 to x to spawn right between the 2 tiles. We also want to right on the ground in the y direction.
			int spawnX = x * 16 + 16;
			int spawnY = (y + 3) * 16;

			if (Main.rand.NextFloat() < .95f) // this is 95% chance for item spawn, 5% chance for npc spawn
			{
				// If you want to make a NPC spawning statue, see below.
				if (Wiring.CheckMech(x, y, 60) && Item.MechSpawn(spawnX, spawnY, ItemID.SilverCoin) && Item.MechSpawn(spawnX, spawnY, ItemID.GoldCoin) && Item.MechSpawn(spawnX, spawnY, ItemID.PlatinumCoin)) {
					int id = ItemID.SilverCoin;
					if (Main.rand.NextBool(100)) {
						id++;
						if (Main.rand.NextBool(100)) {
							id++;
						}
					}
					Item.NewItem(spawnX, spawnY - 20, 0, 0, id, 1, false, 0, false);
				}
			}
			else {
				// If you want to make a NPC spawning statue, see below.
				int npcIndex = -1;
				// 30 is the time before it can be used again. NPC.MechSpawn checks nearby for other spawns to prevent too many spawns. 3 in immediate vicinity, 6 nearby, 10 in world.
				if (Wiring.CheckMech(x, y, 30) && NPC.MechSpawn((float)spawnX, (float)spawnY, NPCID.Goldfish)) {
					npcIndex = NPC.NewNPC(spawnX, spawnY - 12, NPCID.Goldfish);
				}
				if (npcIndex >= 0) {
					Main.npc[npcIndex].value = 0f;
					Main.npc[npcIndex].npcSlots = 0f;
					// Prevents Loot if NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue and !Main.HardMode or NPCID.Sets.StatueSpawnedDropRarity != -1 and NextFloat() >= NPCID.Sets.StatueSpawnedDropRarity or killed by traps.
					// Prevents CatchNPC
					Main.npc[npcIndex].SpawnedFromStatue = true;
				}
			}
		}
	}

	public class ExampleStatueItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Golden Fish Statue");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.ArmorStatue);
			item.createTile = TileType<ExampleStatue>();
			item.placeStyle = 0;
		}
	}

	public class ExampleStatueModWorld : ModWorld
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
			int ResetIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Reset"));
			if (ResetIndex != -1) {
				tasks.Insert(ResetIndex + 1, new PassLegacy("Example Mod Statue Setup", delegate (GenerationProgress progress) {
					progress.Message = "Adding ExampleMod Statue";

					// Not necessary, just a precaution.
					if (WorldGen.statueList.Any(point => point.X == TileType<ExampleStatue>())) {
						return;
					}
					// Make space in the statueList array, and then add a Point16 of (TileID, PlaceStyle)
					Array.Resize(ref WorldGen.statueList, WorldGen.statueList.Length + 1);
					for (int i = WorldGen.statueList.Length - 1; i < WorldGen.statueList.Length; i++) {
						WorldGen.statueList[i] = new Point16(TileType<ExampleStatue>(), 0);
						// Do this if you want the statue to spawn with wire and pressure plate
						// WorldGen.StatuesWithTraps.Add(i);
					}
				}));
			}
		}
	}
}
