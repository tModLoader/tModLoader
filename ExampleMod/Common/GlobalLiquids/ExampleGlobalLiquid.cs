using Microsoft.Xna.Framework;
using Terraria;
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
		//AllowFishingInShimmer does exactly as it's name suggests. 
		//As this mod adds loot to the shimmer fishing pool, we need to make sure that we can actually fish in shimmer, and not get the shimmer debuff.
		//Please see ExampleFishingPlayer to see modded liquid fishing pool examples
		public override bool AllowFishingInShimmer() {
			return true;
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
			Main.NewText("test");
			return true;
		}

		//EvaporatesInHell allows us to specify which liquids evaportate and doesn't evaporate in the underworld.
		//This allows us to do things such as prevent water from evaporating and allowing other liquids to evaporate
		public override bool? EvaporatesInHell(int i, int j, int type) {
			if (type == LiquidID.Shimmer) {
				return true;
			}
			if (type == LiquidID.Water) {
				return false;
			}
			return null;
		}

		//The LiquidFallDelay hook/method is used for changing the falldelay of a liquid.
		//Here we change the delay of water to be 10 (maximum delay time)
		public override int? LiquidFallDelay(int type) {
			if (type == LiquidID.Water) {
				return 10;
			}
			return null;
		}

		//Like modded liquids, we can also change the merge type of a vanilla liquid.
		//Here we set the merge between lava and shimmer to be orichalcum ore.
		public override int? LiquidMerge(int i, int j, int type, int otherLiquid) {
			if (type == LiquidID.Lava) {
				if (type == LiquidID.Shimmer) {
					return TileID.Orichalcum;
				}
			}
			return null;
		}

		//BlocksTilePlacement allows us to specify which liquids block placing tiles over them.
		//Normally Lava is true, but here we set it to false, allowing the player to be able to place tiles over the liquid.
		public override bool? BlocksTilePlacement(Player player, int i, int j, int type) {
			if (type == LiquidID.Lava) {
				return false;
			}
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
		public override bool? ChecksForDrowning(int type) {
			if (type == LiquidID.Water) {
				return false;
			}
			if (type == LiquidID.Shimmer) {
				return true;
			}
			return null;
		}

		//When drowning, we can prevent the emitting of breath bubbles when submerged for both NPCs and Players
		//We make shimmer and water not emit breath bubbles from the player's mouth
		public override bool? AllowEmitBreathBubbles(int type) {
			if (type == LiquidID.Shimmer || type == LiquidID.Water) {
				return false;
			}
			return null;
		}

		//Here using OnPump we make shimmer un-pump-able, returning false, skipping the liquid movement logic
		public override bool OnPump(int inLiquidType, int inX, int inY, int outX, int outY) {
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
			maxFallSpeed = 10f;
			gravity = Player.defaultGravity;
			jumpHeight = 15;
			jumpSpeed = 5.01f;
			if (player.PortalPhysicsEnabled) {
				maxFallSpeed = 35f;
			}
		}

		//related to the PlayerLiquidMovement hook, we fix lava displaying the incorrect MPH number when moving through it
		//Since lava does not slow us down, we set its multiplier to 1x
		public override void StopWatchMPHMultiplier(int type, ref float multiplier) {
			if (type == LiquidID.Lava) {
				multiplier = 1f;
			}
		}

		//related to PlayerLiquidMovement hook, we make lava also ignore item physics when the item is falling in the liquid
		public override void ItemLiquidCollision(Item item, int type, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed) {
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
