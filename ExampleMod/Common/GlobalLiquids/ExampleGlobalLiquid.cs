using ExampleMod.Content.Biomes;
using ExampleMod.Content.Liquids;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalLiquids
{
	//The GlobalLiquid class can do everything ModLiquid can do, and more.
	//Allowing the editing of both vanilla and other modded liquids.
	//Here we do a range of different edits to vanilla liquids to show how to use this global class.
	public class ExampleGlobalLiquid : GlobalLiquid
	{
		//AllowFishingInLiquid allow to control the fact you can fish in a liquid at a global level instead of a ModLiquid level
		public override bool? AllowFishingInLiquid(int liquidType, Player player, Projectile bobber) {
			// We check if we allow fishing in shimmer only if the EoC has been defeated
			if (liquidType == LiquidID.Shimmer && NPC.downedBoss1) {
				// Returning true allow fishing in the liquid
				return true;
			}
			// We check if the bobber is the golden rod bobber and if the EoC has been defeated
			if (liquidType == LiquidID.Water && bobber.type == ProjectileID.BobberGolden && NPC.downedBoss1) {
				// Returning false block fishing in the liquid
				return false;
			}
			// We check if the player is on fire and if the liquid is lava
			if ((liquidType == LiquidID.Lava || liquidType == ModContent.LiquidType<ExampleComplexLiquid>()) && player.onFire) {
				return false;
			}
			return null;
		}

		//When fishing in water, extra water dusts are produced when the bobber splashes.
		//This hook/method is for doing for other liquids, providing extra flare when fishing in a liquid
		//Here we make shimmer emit the npc shimmer particle when fished in
		public override bool OnFishingBobberSplash(Projectile proj, int type) {
			if (type == LiquidID.Shimmer) {
				ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
					PositionInWorld = proj.position
				});
			}
			return true;
		}

		// EvaporatesInHell allows us to specify which liquids evaportate and doesn't evaporate in the underworld under certain condition (or not).
		// This allows us to do things such as prevent water from evaporating and allowing other liquids to evaporate
		public override bool? CanLiquidEvaporate(int i, int j, int type) {
			// In this first example, we make it so honey just evaporate in the underworld without any extra condition
			if (type == LiquidID.Honey) {
				return true;
			}

			// In this second example, we disable water evaporation in the underworld if the wall of flesh as been defeated
			if (type == LiquidID.Water && Main.hardMode) {
				return false;
			}

			// In this third example, we make water evaporate if it's essentially in space
			if (type == LiquidID.Water && j < 200) {
				return true;
			}

			// In this fourth example, we make lava evaporate if it's standing on a block of ice
			if (type == LiquidID.Lava && WorldGen.InWorld(i, j + 1)) {
				Tile tile = Main.tile[i, j + 1];
				if (tile.TileType == TileID.IceBlock && tile.HasTile) {
					return true;
				}
			}

			return null;
		}

		//The LiquidFallDelay hook/method is used for changing the falldelay of a liquid.
		//Here we change the delay of water to be 10 (maximum delay time)
		public override int? LiquidFallDelay(int type) {
			// In this first example, we make the maximum falldelay for all liquids if the moonlord is active in the world
			if (NPC.AnyNPCs(NPCID.MoonLordCore)) {
				return 10;
			}

			// In this second example, we make the maximum falldelay of water at all time 
			if (type == LiquidID.Water) {
				return 10;
			}
			return null;
		}

		// In this example, we change how lava and water should interact before making a LiquidMerge.
		// If the EoC is defeated and the water and lava come in contact, there is 50% chance it will spawn an obsidian pillar instead of doing a regular tile merge
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid) {
			// We start by checking if lava and water are the 2 liquid colliding together
			if (type == LiquidID.Water && otherLiquid == LiquidID.Lava ||
			    type == LiquidID.Lava && otherLiquid == LiquidID.Water) {
				// Then we check if the Eye of Cthulhu has been defeated and run a Main.rand.NextBool() to determine if we generate an obsdian pillar or not
				if (NPC.downedBoss1 && Main.rand.NextBool()) {
					// If the check pass, we start making the pillar
					for (int i = tileY; i > tileY - 10; i--) {
						// We need to check if the pillar is within the bound of the world, if it isn't get out of the loop
						if (!WorldGen.InWorld(tileX, i)) {
							break;
						}

						Tile tile = Main.tile[tileX, i];
						tile.HasTile = true;
						tile.TileType = TileID.Obsidian;
						WorldGen.SquareTileFrame(tileX, i);
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							NetMessage.SendTileSquare(-1, tileX, i, TileChangeType.None);
						}
					}
					// Then return false to prevent LiquidMerge from executing
					return false;
				}
			}
			return base.PreLiquidMerge(liquidX, liquidY, tileX, tileY, type, otherLiquid);
		}

		//Like modded liquids, we can also change the merge type of a vanilla liquid.
		//Here we set the merge between lava and shimmer to be orichalcum ore.
		// We also make so that lava and water have a 50/50 chance to produce either obsidian or obsidian brick
		public override int? LiquidMerge(int type, int otherLiquid) {
			// Due to the nature of how liquids work, we need to check both way for the interaction, as it depend on which liquid flow first in the other liquid.
			// In this case, we are checking if lava and shimmer collide, then make orichalcum
			if ((type == LiquidID.Lava && otherLiquid == LiquidID.Shimmer) || (type == LiquidID.Shimmer && otherLiquid == LiquidID.Lava)) {
				return TileID.Orichalcum;
			}

			// In this case, we check for lava and water colliding.
			if (type == LiquidID.Water && otherLiquid == LiquidID.Lava ||
			    type == LiquidID.Lava && otherLiquid == LiquidID.Water) {
				// Then we check if the eye of cthulhu as been defeated
				if (NPC.downedBoss1) {
					// And finally we do use Main.rand.NextBool() to determine if obsidian (true) or Ancient Obsidian Brick (false will generate)
					return Main.rand.NextBool() ? TileID.Obsidian : TileID.AncientObsidianBrick;
				}
			}
			return null;
		}

		//BlocksTilePlacement allows us to specify which liquids block placing tiles over them.
		//Normally Lava and water are true, but here we set them to false depending on where the liquid itself is located or if the player is in a specific biome. 
		public override bool? BlocksTilePlacement(Player player, int i, int j, int type) {
			// In this case, we check if the liquid is water and if the player is in the Example Surface Biome
			if (type == LiquidID.Water && player.InModBiome<ExampleSurfaceBiome>()) {
				// Then we return true to not allow placement in water if the player is in Example Surface Biome
				return true;
			}

			// In this case, we check if the liquid is lava and if it's located in the underworld 
			if (type == LiquidID.Lava && i > Main.maxTilesY - 200) {
				// Then we return true if the lava is located in the underworld
				return true;
			}

			// TODO: Make an example about where a liquid is in a specific structure boundary
			return null;
		}

		//Here, using OnPlayerSplash, we can make players emit extra particles when entering or exiting a liquid (such as shimmer in this example)
		//Here we make shimmer emit the NPC shimmer particle when both entering and exiting
		public override bool OnPlayerSplash(Player player, int type, bool isEnter) {
			if (type == LiquidID.Shimmer) {
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
					PositionInWorld = player.position + new Vector2(player.width / 2, player.height / 2),
				}, player.whoAmI);
			}
			return true;
		}

		//ChecksForDrowning allows which liquid the player can drown in.
		//Normally water and honey will only let you drown, but here we make shimmer allow the player to drown in, and make water breathable.
		public override void CanPlayerDrown(Player player, int type, ref bool isDrowning) {
			if (type == LiquidID.Water && player.armor[0].type == ItemID.IronHelmet) {
				isDrowning = false;
			}
		}


		//Here using OnPump we make shimmer un-pump-able, returning false, skipping the liquid movement logic
		public override bool OnPump(int inLiquidType, int inX, int inY, int outX, int outY) {
			if (inY > Main.UnderworldLayer && outY < Main.UnderworldLayer) {
				return false;

			}

			if (inLiquidType == LiquidID.Shimmer) {
				return false;
			}
			return true;
		}

		//Here, we make lava have no liquid movement at all
		//Normally, returning false has no velocity applied to the player at all as liquid movement is responsible for adding velocity to position.
		//To fix this, we copy the default movement when outside of liquids and have it run when in lava.
		public override bool PlayerLiquidMovement(Player player, int type, bool fallThrough, bool ignorePlats) {
			if (type == LiquidID.Lava) {
				player.DryCollision(fallThrough, ignorePlats);
				if (player.mount.Active && player.mount.IsConsideredASlimeMount && player.velocity.Y != 0f && !player.SlimeDontHyperJump) {
					Vector2 vel = player.velocity;
					player.velocity.X = 0f;
					player.DryCollision(fallThrough, ignorePlats);
					player.velocity.X = vel.X;
				}
				if (player.mount.Active && player.mount.Type == 43 && player.velocity.Y != 0f) {
					Vector2 vel = player.velocity;
					player.velocity.X = 0f;
					player.DryCollision(fallThrough, ignorePlats);
					player.velocity.X = vel.X;
				}
				return false;
			}
			return true;
		}

		//PlayerLiquidMovement isn't enough to make the player fully ignore lava movement, so we have to also reset the gravity of the player as well
		//Here we copy over the settings when gravity, maxfallspeed, jump height and speed into this method, resetting their values to before the player being in a liquid.
		public override void PlayerGravityModifier(Player player, int type, ref float gravity, ref float maxFallSpeed, ref int jumpHeight, ref float jumpSpeed) {
			if (type == LiquidID.Lava) {
				maxFallSpeed = 10f;
				gravity = Player.defaultGravity;
				jumpHeight = 15;
				jumpSpeed = 5.01f;
				if (player.PortalPhysicsEnabled) {
					maxFallSpeed = 35f;
				}
			}
		}

		//related to PlayerLiquidMovement hook, we make lava also ignore item physics when the item is falling in the liquid
		public override void ItemLiquidCollision(WorldItem item, int type, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed) {
			if (type == LiquidID.Lava) {
				//we replicate the gravity with the values before liquid effects the references
				gravity = 0.1f;
				maxFallSpeed = 7f;
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					int x = (int)(item.position.X + (float)(item.width / 2)) / 16;
					int y = (int)(item.position.Y + (float)(item.height / 2)) / 16;
					if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY && !Main.sectionManager.TileLoaded(x, y)) {
						gravity = 0f;
						item.velocity.X = 0f;
						item.velocity.Y = 0f;
					}
				}
				//the default for wetVelocity has the item move at 0.5x speed,
				//but looking further into Item.UpdateItem shows that wetVelocity is just the normal velocity but multiplied by a multilier.
				//So setting it to the item's velocity makes sure that velocity is kept.
				wetVelocity = item.velocity;
			}
		}

		//Again related to PlayerLiquidMovement hook
		//Here we replace gavity and maxFallSpeed with a replication of the gravity calculations before liquid calulations are called
		//This allows npcs in lava to have normal gravity and not be effected by liquids
		//To see how lava is prevented from multiplying velocity, please see Common.GlobalNPCs.ExampleGlobalLiquidNPC.SetDefaults
		public override void NPCGravityModifier(NPC npc, int type, ref float gravity, ref float maxFallSpeed) {
			if (type == LiquidID.Lava) {
				maxFallSpeed = 10f;
				gravity = 0.3f;
				if (!npc.GravityIgnoresType) {
					if (type == 258) {
						gravity = 0.1f;
						if (npc.velocity.Y > 3f) {
							npc.velocity.Y = 3f;
						}
					}
					else if (type == 425 && npc.ai[2] == 1f) {
						gravity = 0.1f;
					}
					else if ((type == 576 || type == 577) && npc.ai[0] > 0f && npc.ai[1] == 2f) {
						gravity = 0.45f;
						if (npc.velocity.Y > 32f) {
							npc.velocity.Y = 32f;
						}
					}
					else if (type == 427 && npc.ai[2] == 1f) {
						gravity = 0.1f;
						if (npc.velocity.Y > 4f) {
							npc.velocity.Y = 4f;
						}
					}
					else if (type == 426) {
						gravity = 0.1f;
						if (npc.velocity.Y > 3f) {
							npc.velocity.Y = 3f;
						}
					}
					else if (type == 541) {
						gravity = 0f;
					}
					else if (npc.aiStyle == 7 && npc.ai[0] == 25f) {
						gravity = 0f;
					}
				}
				if (!npc.GravityIgnoresSpace) {
					float worldSize = (float)Main.maxTilesX / 4200f;
					worldSize *= worldSize;
					float height = (float)((double)(npc.position.Y / 16f - (60f + 10f * worldSize)) / (Main.worldSurface / 6.0));
					if ((double)height < 0.25) {
						height = 0.25f;
					}
					if (height > 1f) {
						height = 1f;
					}
					gravity *= height;
				}
			}
		}

		//Lastly, and very easily, projectiles don't have default dry behaviour, so all we have to do for lava to not react to projectiles is to return false;
		public override bool ProjectileLiquidMovement(Projectile projectile, int type, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough) {
			if (type == LiquidID.Lava) {
				return false;
			}
			return true;
		}

		//AnimateLiquid in a GlobalLiquid returns a boolean
		//using this boolean, we can toggle the normal vanilla animating code or not
		//with that, we disable the shimmer's animation, forcing it to be stuck on the first frame
		public override bool AnimateLiquid(int type, GameTime gameTime, ref int frame, ref float frameState) {
			if (type == LiquidID.Shimmer) {
				return false;
			}
			return true;
		}
	}
}
