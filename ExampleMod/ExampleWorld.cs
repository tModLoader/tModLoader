using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using System;

namespace ExampleMod
{
	public class ExampleWorld : ModWorld
	{
		private const int saveVersion = 0;
		public static bool downedAbomination = false;
		public static bool downedPuritySpirit = false;
		public const int VolcanoProjectiles = 30;
		public const float VolcanoAngleSpread = 170;
		public const int DefaultVolcanoTremorTime = 200; // ~ 3 seconds
		public const int DefaultVolcanoCountdown = 300; // 5 seconds
		public const int DefaultVolcanoCooldown = 10000; // At least 3 min of daytime between volcanos
		public const int VolcanoChance = 10000; // Chance each tick of Volcano if cooldown exhausted.
		public int VolcanoCountdown;
		public int VolcanoCooldown = DefaultVolcanoCooldown;
		public int VolcanoTremorTime;
		public static int exampleTiles = 0;

		public override void Initialize()
		{
			downedAbomination = false;
			downedPuritySpirit = false;
			VolcanoCountdown = 0;
			VolcanoTremorTime = 0;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			byte flags = 0;
			if (downedAbomination)
			{
				flags |= 1;
			}
			if (downedPuritySpirit)
			{
				flags |= 2;
			}
			writer.Write(flags);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			reader.ReadInt32();
			byte flags = reader.ReadByte();
			downedAbomination = ((flags & 1) == 1);
			downedPuritySpirit = ((flags & 2) == 2);
		}

		public override void SendCustomData(BinaryWriter writer)
		{
			byte flags = 0;
			if (downedAbomination)
			{
				flags |= 1;
			}
			if (downedPuritySpirit)
			{
				flags |= 2;
			}
			writer.Write(flags);
		}

		public override void ReceiveCustomData(BinaryReader reader)
		{
			byte flags = reader.ReadByte();
			downedAbomination = ((flags & 1) == 1);
			downedPuritySpirit = ((flags & 2) == 2);
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			if (ShiniesIndex == -1)
			{
				// Shinies pass removed by some other mod.
				return;
			}
			tasks.Insert(ShiniesIndex + 1, new PassLegacy("Example Mod Ores", delegate (GenerationProgress progress)
			{
				progress.Message = "Example Mod Ores";

				for (int k = 0; k < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 6E-05); k++)
				{
					WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY), (double)WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), mod.TileType("ExampleBlock"), false, 0f, 0f, false, true);
				}
			}));
		}

		public override void PostWorldGen()
		{
			for (int i = 0; i < Main.maxTilesX; i++)
			{
				Main.tile[i, Main.maxTilesY / 2].type = TileID.Chlorophyte;
			}
			int num = NPC.NewNPC((Main.spawnTileX + 5) * 16, Main.spawnTileY * 16, mod.NPCType("Example Person"), 0, 0f, 0f, 0f, 0f, 255);
			Main.npc[num].homeTileX = Main.spawnTileX + 5;
			Main.npc[num].homeTileY = Main.spawnTileY;
			Main.npc[num].direction = 1;
			Main.npc[num].homeless = true;
			// Place some items in Ice Chests
			int[] itemsToPlaceInWaterChests = new int[] { mod.ItemType("CarKey"), mod.ItemType("ExampleLightPet"), ItemID.PinkJellyfishJar };
			int itemsToPlaceInWaterChestsChoice = 0;
			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				if (chest != null && Main.tile[chest.x, chest.y].type == TileID.Containers && Main.tile[chest.x, chest.y].frameX == 11 * 36)
				{
					for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
					{
						if (chest.item[inventoryIndex].type == 0)
						{
							chest.item[inventoryIndex].SetDefaults(itemsToPlaceInWaterChests[itemsToPlaceInWaterChestsChoice]);
							itemsToPlaceInWaterChestsChoice = (itemsToPlaceInWaterChestsChoice + 1) % itemsToPlaceInWaterChests.Length;
							break;
						}
					}
				}
			}
		}

		public override void ResetNearbyTileEffects()
		{
			ExamplePlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<ExamplePlayer>(mod);
			modPlayer.voidMonolith = false;
			exampleTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts)
		{
			exampleTiles = tileCounts[mod.TileType("ExampleBlock")];
		}

		public override void PostUpdate()
		{
			if (Main.dayTime && VolcanoCountdown == 0)
			{
				if (VolcanoCooldown > 0)
				{
					VolcanoCooldown--;
				}
				if (VolcanoCooldown <= 0 && Main.rand.Next(VolcanoChance) == 0)
				{
					string message = "Did you hear something....A Volcano! Find Cover!";
					Color messageColor = Color.Orange;
					if (Main.netMode == 2) // Server
					{
						NetMessage.SendData(25, -1, -1, message, 255, messageColor.R, messageColor.G, messageColor.B, 0);
					}
					else if (Main.netMode == 0) // Single Player
					{
						Main.NewText(message, messageColor.R, messageColor.G, messageColor.B);
					}
					VolcanoCountdown = DefaultVolcanoCountdown;
					VolcanoCooldown = DefaultVolcanoCooldown;
				}
			}
			if (VolcanoCountdown > 0)
			{
				VolcanoCountdown--;
				if (VolcanoCountdown == 0)
				{
					VolcanoTremorTime = DefaultVolcanoTremorTime;
					// Since PostUpdate only happens in single and server, we need to inform the clients to shake if this is a server
					if (Main.netMode == 2)
					{
						var netMessage = mod.GetPacket();
						netMessage.Write((byte)ExampleModMessageType.SetTremorTime);
						netMessage.Write(VolcanoTremorTime);
						netMessage.Send();
					}
					for (int playerIndex = 0; playerIndex < 255; playerIndex++)
					{
						if (Main.player[playerIndex].active)
						{
							Player player = Main.player[playerIndex];
							int speed = 12;
							float spawnX = Main.rand.Next(1000) - 500 + player.Center.X;
							float spawnY = -1000 + player.Center.Y;
							Vector2 baseSpawn = new Vector2(spawnX, spawnY);
							Vector2 baseVelocity = player.Center - baseSpawn;
							baseVelocity.Normalize();
							baseVelocity = baseVelocity * speed;
							List<int> identities = new List<int>();
							for (int i = 0; i < VolcanoProjectiles; i++)
							{
								Vector2 spawn = baseSpawn;
								spawn.X = spawn.X + i * 30 - (VolcanoProjectiles * 15);
								Vector2 velocity = baseVelocity;
								velocity = baseVelocity.RotatedBy(MathHelper.ToRadians(-VolcanoAngleSpread / 2 + (VolcanoAngleSpread * i / (float)VolcanoProjectiles)));
								velocity.X = velocity.X + 3 * Main.rand.NextFloat() - 1.5f;
								int projectile = Projectile.NewProjectile(spawn.X, spawn.Y, velocity.X, velocity.Y, Main.rand.Next(ProjectileID.MolotovFire, ProjectileID.MolotovFire3 + 1), 10, 10f, Main.myPlayer, 0f, 0f);
								Main.projectile[projectile].hostile = true;
								Main.projectile[projectile].name = "Volcanic Rubble";
								identities.Add(Main.projectile[projectile].identity);
							}
							if (Main.netMode == 2)
							{
								var netMessage = mod.GetPacket();
								netMessage.Write((byte)ExampleModMessageType.VolcanicRubbleMultiplayerFix);
								netMessage.Write(identities.Count);
								for (int i = 0; i < identities.Count; i++)
								{
									netMessage.Write(identities[i]);
								}
								netMessage.Send();
							}
						}
					}
				}
			}
		}
	}
}
