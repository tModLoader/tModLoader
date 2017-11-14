using System.IO;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

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
		public const int DefaultVolcanoCooldown = 10000; // At least 3 min of daytime between volcanoes
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

		public override TagCompound Save()
		{
			var downed = new List<string>();
			if (downedAbomination) downed.Add("abomination");
			if (downedPuritySpirit) downed.Add("puritySpirit");

			return new TagCompound {
				{"downed", downed}
			};
		}

		public override void Load(TagCompound tag)
		{
			var downed = tag.GetList<string>("downed");
			downedAbomination = downed.Contains("abomination");
			downedPuritySpirit = downed.Contains("puritySpirit");
		}

		public override void LoadLegacy(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			if (loadVersion == 0)
			{
				BitsByte flags = reader.ReadByte();
				downedAbomination = flags[0];
				downedPuritySpirit = flags[1];
			}
			else
			{
				ErrorLogger.Log("ExampleMod: Unknown loadVersion: " + loadVersion);
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			BitsByte flags = new BitsByte();
			flags[0] = downedAbomination;
			flags[1] = downedPuritySpirit;
			writer.Write(flags);

			//If you prefer, you can use the BitsByte constructor approach as well.
			//writer.Write(saveVersion);
			//BitsByte flags = new BitsByte(downedAbomination, downedPuritySpirit);
			//writer.Write(flags);

			// This is another way to do the same thing, but with bitmasks and the bitwise OR assignment operator (the |=)
			// Note that 1 and 2 here are bit masks. The next values in the pattern are 4,8,16,32,64,128. If you require more than 8 flags, make another byte.
			//writer.Write(saveVersion);
			//byte flags = 0;
			//if (downedAbomination)
			//{
			//	flags |= 1;
			//}
			//if (downedPuritySpirit)
			//{
			//	flags |= 2;
			//}
			//writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			downedAbomination = flags[0];
			downedPuritySpirit = flags[1];
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			if (ShiniesIndex != -1)
			{
				tasks.Insert(ShiniesIndex + 1, new PassLegacy("Example Mod Ores", delegate (GenerationProgress progress)
				{
					progress.Message = "Example Mod Ores";

					for (int k = 0; k < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 6E-05); k++)
					{
						WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY), (double)WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), mod.TileType("ExampleBlock"), false, 0f, 0f, false, true);
					}
				}));
			}

			int TrapsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Traps"));
			if (TrapsIndex != -1)
			{
				tasks.Insert(TrapsIndex + 1, new PassLegacy("Example Mod Traps", delegate (GenerationProgress progress)
				{
					progress.Message = "Example Mod Traps";
					// Computers are fast, so WorldGen code sometimes looks stupid.
					// Here, we want to place a bunch of tiles in the world, so we just repeat until success. It might be useful to keep track of attempts and check for attempts > maxattempts so you don't have infinite loops. 
					// The WorldGen.PlaceTile method returns a bool, but it is useless. Instead, we check the tile after calling it and if it is the desired tile, we know we succeeded.
					for (int k = 0; k < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 6E-05); k++)
					{
						bool placeSuccessful = false;
						Tile tile;
						int tileToPlace = mod.TileType<Tiles.ExampleCutTileTile>();
						while (!placeSuccessful)
						{
							int x = WorldGen.genRand.Next(0, Main.maxTilesX);
							int y = WorldGen.genRand.Next(0, Main.maxTilesY);
							WorldGen.PlaceTile(x, y, tileToPlace);
							tile = Main.tile[x, y];
							placeSuccessful = tile.active() && tile.type == tileToPlace;
						}
					}
				}));
			}

			int LivingTreesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Living Trees"));
			if (LivingTreesIndex != -1)
			{
				tasks.Insert(LivingTreesIndex + 1, new PassLegacy("Post Terrain", delegate (GenerationProgress progress)
				{
					progress.Message = "What is it Lassie, did Timmy fall down a well?";
					MakeWells();
				}));
			}
		}

		private void MakeWells()
		{
			float widthScale = (Main.maxTilesX / 4200f);
			int numberToGenerate = WorldGen.genRand.Next(1, (int)(2f * widthScale));
			for (int k = 0; k < numberToGenerate; k++)
			{
				bool success = false;
				int attempts = 0;
				while (!success)
				{
					attempts++;
					if (attempts > 1000)
					{
						success = true;
						continue;
					}
					int i = WorldGen.genRand.Next(300, Main.maxTilesX - 300);
					if (i <= Main.maxTilesX / 2 - 50 || i >= Main.maxTilesX / 2 + 50)
					{
						int j = 0;
						while (!Main.tile[i, j].active() && (double)j < Main.worldSurface)
						{
							j++;
						}
						if (Main.tile[i, j].type == TileID.Dirt)
						{
							j--;
							if (j > 150)
							{
								bool placementOK = true;
								for (int l = i - 4; l < i + 4; l++)
								{
									for (int m = j - 6; m < j + 20; m++)
									{
										if (Main.tile[l, m].active())
										{
											int type = (int)Main.tile[l, m].type;
											if (type == TileID.BlueDungeonBrick || type == TileID.GreenDungeonBrick || type == TileID.PinkDungeonBrick || type == TileID.Cloud || type == TileID.RainCloud)
											{
												placementOK = false;
											}
										}
									}
								}
								if (placementOK)
								{
									success = PlaceWell(i, j);
								}
							}
						}
					}
				}
			}
		}

		int[,] wellshape = new int[,]
		{
			{0,0,3,1,4,0,0 },
			{0,3,1,1,1,4,0 },
			{3,1,1,1,1,1,4 },
			{5,5,5,6,5,5,5 },
			{5,5,5,6,5,5,5 },
			{5,5,5,6,5,5,5 },
			{2,1,5,6,5,1,2 },
			{1,1,5,5,5,1,1 },
			{1,1,5,5,5,1,1 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,5,5,5,1,0 },
			{0,1,1,1,1,1,0 },
		};

		int[,] wellshapeWall = new int[,]
		{
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
		};

		int[,] wellshapeWater = new int[,]
		{
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,0,0,0,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,1,1,1,0,0 },
			{0,0,0,0,0,0,0 },
		};

		public bool PlaceWell(int i, int j)
		{
			if (!WorldGen.SolidTile(i, j + 1))
			{
				return false;
			}
			if (Main.tile[i, j].active())
			{
				return false;
			}
			if (j < 150)
			{
				return false;
			}

			for (int y = 0; y < wellshape.GetLength(0); y++)
			{
				for (int x = 0; x < wellshape.GetLength(1); x++)
				{
					int k = i - 3 + x;
					int l = j - 6 + y;
					if (WorldGen.InWorld(k, l, 30))
					{
						Tile tile = Framing.GetTileSafely(k, l);
						switch (wellshape[y, x])
						{
							case 1:
								tile.type = TileID.RedBrick;
								tile.active(true);
								break;
							case 2:
								tile.type = TileID.RedBrick;
								tile.active(true);
								tile.halfBrick(true);
								break;
							case 3:
								tile.type = TileID.RedBrick;
								tile.active(true);
								tile.slope(2);
								break;
							case 4:
								tile.type = TileID.RedBrick;
								tile.active(true);
								tile.slope(1);
								break;
							case 5:
								tile.active(false);
								break;
							case 6:
								tile.type = TileID.Rope;
								tile.active(true);
								break;
						}
						switch (wellshapeWall[y, x])
						{
							case 1:
								tile.wall = WallID.RedBrick;
								break;
						}
						switch (wellshapeWater[y, x])
						{
							case 1:
								tile.liquid = 255;
								break;
						}
					}
				}
			}
			return true;
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
				// If you look at the sprite for Chests by extracting Tiles_21.xnb, you'll see that the 12th chest is the Ice Chest. Since we are counting from 0, this is where 11 comes from. 36 comes from the width of each tile including padding. 
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
			ExamplePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>(mod);
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
					string key = "Mods.ExampleMod.VolcanoWarning";
					Color messageColor = Color.Orange;
					if (Main.netMode == 2) // Server
					{
						NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
					}
					else if (Main.netMode == 0) // Single Player
					{
						Main.NewText(Language.GetTextValue(key), messageColor);
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
								Main.projectile[projectile].Name = "Volcanic Rubble";
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

		// In ExampleMod, we use PostDrawTiles to draw the TEScoreBoard area. PostDrawTiles draws before players, npc, and projectiles, so it works well.
		public override void PostDrawTiles()
		{
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			Rectangle screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
			screenRect.Inflate(Tiles.TEScoreBoard.drawBorderWidth, Tiles.TEScoreBoard.drawBorderWidth);
			int scoreBoardType = mod.TileEntityType<Tiles.TEScoreBoard>();
			foreach (var item in TileEntity.ByID)
			{
				if (item.Value.type == scoreBoardType)
				{
					var scoreBoard = item.Value as Tiles.TEScoreBoard;
					Rectangle scoreBoardArea = scoreBoard.GetPlayArea();
					// We only want to draw while the area is visible. 
					if (screenRect.Intersects(scoreBoardArea))
					{
						scoreBoardArea.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
						DrawBorderedRect(Main.spriteBatch, Color.LightBlue * 0.1f, Color.Blue * 0.3f, scoreBoardArea.TopLeft(), scoreBoardArea.Size(), Tiles.TEScoreBoard.drawBorderWidth);
					}
				}
			}
			Main.spriteBatch.End();
		}

		// A helper method that draws a bordered rectangle. 
		public static void DrawBorderedRect(SpriteBatch spriteBatch, Color color, Color borderColor, Vector2 position, Vector2 size, int borderWidth)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), color);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X - borderWidth, (int)position.Y - borderWidth, (int)size.X + borderWidth * 2, borderWidth), borderColor);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X - borderWidth, (int)position.Y + (int)size.Y, (int)size.X + borderWidth * 2, borderWidth), borderColor);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X - borderWidth, (int)position.Y, (int)borderWidth, (int)size.Y), borderColor);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X + (int)size.X, (int)position.Y, (int)borderWidth, (int)size.Y), borderColor);
		}
	}
}
