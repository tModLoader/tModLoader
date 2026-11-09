using ExampleMod.Content.Biomes;
using ExampleMod.Content.Liquids;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
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
		public override bool? AllowFishing(int liquidType, Player player, Projectile bobber) {
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
			if ((liquidType == LiquidID.Lava || liquidType == ModContent.LiquidType<ExampleComplexLiquid>()) && player.onFire) {
				return false;
			}
			return null;
		}

		// EvaporatesInHell allows us to specify which liquids evaporate and doesn't evaporate in the underworld under certain condition (or not).
		// This allows us to do things such as prevent water from evaporating and allowing other liquids to evaporate
		public override bool? CanWaterEvaporateInUnderworld() {
			// In this small example, water is only allowed to evaporate if it's pre hardmode
			return !Main.hardMode;
		}

		// In this example, we change how lava and water should interact before making a LiquidMerge.
		// If the EoC is defeated and the water and lava come in contact, there is 50% chance it will spawn an obsidian pillar instead of doing a regular tile merge
		public override bool PreMerge(int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid) {
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

			// This second example here make it so Lava and Honey does not merge at all if they are in the underworld, allowing them to co-exist next to each other
			// if (type == LiquidID.Lava && otherLiquid == LiquidID.Honey ||
			//	type == LiquidID.Honey && otherLiquid == LiquidID.Lava && liquidY > Main.UnderworldLayer) {
			//	// By returning false, it effectively prevent the liquid from doing any sort of merging and make it so that LiquidMerge does not run in this case
			//	return false;
			//}
			return base.PreMerge(liquidX, liquidY, tileX, tileY, type, otherLiquid);
		}

		//Like modded liquids, we can also change the merge type of a vanilla liquid.
		//Here we set the merge between lava and shimmer to be orichalcum ore.
		// We also make so that lava and water have a 50/50 chance to produce either obsidian or obsidian brick
		public override int? Merge(int type, int otherLiquid) {
			// Due to the nature of how liquids work, we need to check both way for the interaction, as it depend on which liquid flow first in the other liquid.
			// In this case, we are checking if lava and shimmer collide, then make orichalcum
			if ((type == LiquidID.Lava && otherLiquid == LiquidID.Shimmer) || (type == LiquidID.Shimmer && otherLiquid == LiquidID.Lava)) {
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
		//Normally Lava are true, but here we set them to false depending on where the liquid itself is located or if the player is in a specific biome. 
		public override bool? BlocksTilePlacement(Player player, int i, int j, int type) {
			// In this case, we check if the liquid is water and if the player is in the Example Surface Biome
			if (type == LiquidID.Water && player.InModBiome<ExampleSurfaceBiome>()) {
				// Then we return true to not allow placement in water if the player is in Example Surface Biome
				return true;
			}

			// In this case, we check if the liquid is lava and if it's located in the underworld 
			if (type == LiquidID.Lava && j > Main.maxTilesY - 200) {
				// Then we return false if the lava is located in the underworld so we can place tile on it
				return false;
			}
			return null;
		}

		//ChecksForDrowning allows which liquid the player can drown in.
		//Normally water and honey will only let you drown, but here we make shimmer allow the player to drown in, and make water breathable.
		public override bool CanPlayerDrown(Player player, int type) {
			if (type == LiquidID.Water && player.armor[0].type == ItemID.IronHelmet) {
				return false;
			}

			return base.CanPlayerDrown(player, type);
		}

		// In this example, we make it so that shimmer cannot be pumped if the moonlord was not defeated in the world
		public override bool OnPump(int inLiquidType, int inX, int inY, int outX, int outY) {
			// We check if the current liquid type is shimmer, if it is then we can check if the moon was not defeated.
			if (inLiquidType == LiquidID.Shimmer && !NPC.downedMoonlord) {
				// If this is the case, then we cannot pump shimmer until the moon lord has been defeated
				return false;
			}
			return base.OnPump(inLiquidType, inX, inY, outX, outY);
		}

		//Here, we make lava have no liquid movement at all
		//Normally, returning false has no velocity applied to the player at all as liquid movement is responsible for adding velocity to position.
		//To fix this, we copy the default movement when outside of liquids and have it run when in lava.
		public override bool PlayerMovement(Player player, int type, bool fallThrough, bool ignorePlats) {
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

		//related to PlayerLiquidMovement hook, we make lava also ignore item physics when the item is falling in the liquid
		public override void ItemCollision(WorldItem item, int type, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed) {
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

		//Lastly, and very easily, projectiles don't have default dry behaviour, so all we have to do for lava to not react to projectiles is to return false;
		public override bool ProjectileMovement(Projectile projectile, int type, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough) {
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
