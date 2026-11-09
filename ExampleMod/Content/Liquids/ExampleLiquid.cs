using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		//SetStaticDefaults are the defaults added when the game initially loads.
		//Here we set a few settings that this liquid will have.
		public override void SetStaticDefaults() {
			//This is the viscosity of the liquid, only used visually.
			//Lava usually has this set to 200, while honey has this set to 240. All other liquids set this to 0 by default.
			LiquidRenderer.VISCOSITY_MASK[Type] = 200;

			//This is the length (in amount of tiles) the liquid will visually have when flowing/falling downwards or if there is a slope underneath.
			LiquidRenderer.WATERFALL_LENGTH[Type] = 20;

			//This is the opacity of the liquid. How well you can see objects in the liquid.
			//DefaultOpacity is a vanilla array containing the definitions of each liquid type's default opacity for just white and color lighting modes
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.95f;

			//For the Waves Quality setting, when set to Medium, waves are set to be the same distance no matter the liquid type.
			//To do this, the game applied a multiplier to make them all consistant between liquids. Here we set our own multiplier to make the waves the same distance as honey and lava.
			LiquidID.Sets.LiquidRippleMultiplier[Type] = 0.3f;

			//This is used to specify what dust is used when splashing in this liquid.
			//SplashDustType is -1 normally, which prevents the liquid from doing any splash dust
			SplashDustType = ModContent.DustType<ExampleLiquidSplash>();

			//This is used to specify what sound is played when an entity enters a liquid
			SplashSound = SoundID.SplashWeak;

			LiquidID.Sets.FallDelay[Type] = 2; //The delay (in ticks) when this liquid is falling

			LiquidID.Sets.CheckForDrowning[Type] = true; //If the player/npcs can drown in this liquid
			LiquidID.Sets.AllowEmitBreathBubbles[Type] = false; //Bubbles will come out of the player/npc's mouth normally when drowning, here we can stop that by setting it to false.

			//These multipliers are used in the default mod liquid player movement.
			//Here we make our liquid slow the player down by half what honey would allow the player to move at.

			//Heres the defaults for each liquid:
			//Water/Lava/Regular modded liquid = 0.5f
			//Honey = 0.25f
			//Shimmer = 0.375f
			LiquidID.Sets.PlayerMovementMultiplier[Type] = 0.125f; //For modders who don't want to reimplement the entire player movement for this liquid, this property is for them
			LiquidID.Sets.StopWatchMPHMultiplier[Type] = 0.125f; //We set stopwatch to the same multiplier as we don't want a difference between whats felt and what the player can read their movement as.
			LiquidID.Sets.NPCMovementMultiplierDefault[Type] = 0.125f; //NPCs have a similar modifier but as a field, here we set the default value as some other NPCs set this multiplier to 0. We set this to PlayerMovementMultiplier as we need them to all be the same.
			LiquidID.Sets.ProjectileMovementMultiplier[Type] = 0.125f; //Simiarly to Players, Projectiles have this property for easy editing of a projectile velocity multiplier without needing to reimplement all of the projectile liquid movement code.

			LiquidID.Sets.FishingPoolMinimumSize[Type] = 150; // We set the amount of tile required before it's a valid fishing pool, here we set it to 150 so it's double of what normally water would require

			//For more dangerous liquids, we may want to have our liquid call On(Player/NPC/Projectile)Collision whenever an entity touches the liquid, rather than when an entity splashes in a liquid
			//For this we use similar collision calculations as lava using this boolean.
			//By default, this is disabled
			LiquidID.Sets.UsesLavaCollisionForWet[Type] = true;

			//This ID set controls what items classify as a sponge when trying to suck up this liquid
			//Here we remove the Ultra Absorbant sponge, Allow the Lava Absorbant sponge and the staff of regrowth to suck up this liquid
			LiquidID.Sets.CanBeAbsorbedBy[Type].Remove(ItemID.UltraAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.LavaAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.StaffofRegrowth); //Here is an example of turning a regular item into a sponge thats capable of sucking up our liquid

			// Here we set the gravity modifier for NPC in a way that will make any NPC affected by gravity float instead of sinking
			LiquidID.Sets.WetNPCGravityModifier[Type] = -0.5f;
			// And here we make it so the max fall (or rather, max upward speed?) is set to be slower than usual (like honey)
			LiquidID.Sets.WetNPCMaxFallSpeedModifier[Type] = 4f;

			// We set in here the various player modifier when in a liquid
			LiquidID.Sets.WetPlayerGravityModifier[Type] = 0.9f;
			LiquidID.Sets.WetPlayerMaxFallSpeedModifier[Type] = 20f;
			LiquidID.Sets.WetPlayerJumpHeightModifier[Type] = 60;
			LiquidID.Sets.WetPlayerJumpSpeedModifier[Type] = 10.1f;

			//We can add a map entry to our liquid, by doing so we can show where our liquid is on the map.
			//Unlike vanilla, we can also add a map entry name, which will display a name if the liquid is being selected on the map.
			AddMapEntry(new Color(200, 200, 200), DisplayName);

			VanillaFallbackOnModDeletion = (ushort)LiquidID.Water;

			//Please see ExampleFishingPlayer to see modded liquid fishing pool examples
		}

		//Here with Merge, we are able to decide when the liquid generates with a different tile.
		//Using the adjacentLiquid param, we are able to select which liquid that collides with ours creates a specific tile.
		public override int Merge(int adjacentLiquid) {
			if (adjacentLiquid == LiquidID.Water) {
				return TileID.TeamBlockBlue; //When the liquid collides with water. Blue team block is created
			}
			else if (adjacentLiquid == LiquidID.Lava) {
				return TileID.TeamBlockRed; //When the liquid collides with lava. Red team block is created
			}
			else if (adjacentLiquid == LiquidID.Honey) {
				return TileID.TeamBlockYellow; //When the liquid collides with honey. Yellow team block is created
			}
			else if (adjacentLiquid == LiquidID.Shimmer) {
				return TileID.TeamBlockPink; //When the liquid collides with shimmer. Pink team block is created
			}
			//The base return is what the liquid generates by default. This is useful for when this liquid collides with another modded liquid that this liquid has no support for.
			//usually by default, this method will return TIleID.Stone, and generates a stone tile if it cannot recognise any predetermined tile type to generate with
			return TileID.TeamBlockWhite;

			//NOTE: for custom collisions/tile creation, please see PreLiquidMerge to determine whether the liquid should do its normal tile merging,
			//or if you want to do other effects when this liquid merges with another liquid.
		}

		//MergeSound is played only on clients, and serves as editing the sound of when a liquid merges with another liquid.
		//Here we set a few custom sounds to play when this liquid merges with other liquids.
		public override void MergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound) {
			collisionSound = SoundID.Shatter; //by default, we set the sound to be the glass shattering sound when merging with a liquid.
			if (otherLiquid == LiquidID.Water) {
				collisionSound = SoundID.Item2; //...but if the liquid being merged with is water, then we play Item 2 (Eating sound)
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				collisionSound = SoundID.MaxMana; //...but if the liquid being merged with is shimmer, then we play the maximum mana sound
			}
		}

		//ChooseWaterfallStyle allows for the selection of what waterfall style this liquid chooses when next to a slope.
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<ExampleLiquidfall>().Slot;
		}

		//ModifyLight allows the liquid to emit light similarly to any tile or wall.
		//You can use this to emit light similarly to lava or shimmer.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			//Here we make the liquid just emit a bright white light by setting R, G, and B to 1.
			r = 1f;
			g = 1f;
			b = 1f;
		}

		//Here using ModifyNearbyTiles we do something similar to lava
		//by changing nearby tiles from one to another, not only transformaing grasses into their dirts, but also moss into stone and sand into glass
		public override void ModifyNearbyTiles(int i, int j, int liquidX, int liquidY) {
			Tile tile = Main.tile[i, j];
			//Grass and mud into dirt
			if (tile.TileType == TileID.Grass || tile.TileType == TileID.CorruptGrass || tile.TileType == TileID.HallowedGrass || tile.TileType == TileID.CrimsonGrass || tile.TileType == TileID.GolfGrass || tile.TileType == TileID.GolfGrassHallowed || tile.TileType == TileID.Mud) {
				tile.TileType = TileID.Dirt;
				WorldGen.SquareTileFrame(i, j);
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, i, j, 3);
				}
			}
			//jungle grass into mud
			else if (tile.TileType == TileID.JungleGrass || tile.TileType == TileID.MushroomGrass || tile.TileType == TileID.CorruptJungleGrass || tile.TileType == TileID.CrimsonJungleGrass) {
				tile.TileType = TileID.Mud;
				WorldGen.SquareTileFrame(i, j);
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, i, j, 3);
				}
			}
			//Ash grass into ash
			else if (tile.TileType == TileID.AshGrass) {
				tile.TileType = TileID.Ash;
				WorldGen.SquareTileFrame(i, j);
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, i, j, 3);
				}
			}
			//Non-glowing moss into stone
			else if (tile.TileType == TileID.BrownMoss || tile.TileType == TileID.BlueMoss || tile.TileType == TileID.GreenMoss || tile.TileType == TileID.RedMoss) {
				tile.TileType = TileID.Stone;
				WorldGen.SquareTileFrame(i, j);
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, i, j, 3);
				}
			}
			//Sand into glass
			else if (tile.TileType == TileID.Sand || tile.TileType == TileID.Ebonsand || tile.TileType == TileID.Crimsand || tile.TileType == TileID.Pearlsand) {
				tile.TileType = TileID.Glass;
				WorldGen.SquareTileFrame(i, j);
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, i, j, 3);
				}
			}
		}

		//Here we use the OnNPCCollision and OnPlayerCollision hooks to apply effects to entities when they enter this liquid.
		//NOTE: UsesLavaCollisionForWet property is important for this hook.
		//If true, this will effect entities even if they are slightly touching
		//If false, it will only effect the entity when almost fully submerged
		//Firstly, we apply the dryad's ward debuff to NPCs
		public override void OnNPCCollision(NPC npc) {
			//Make sure that the NPC can take damage, and the game is not a player on a server
			if (!npc.dontTakeDamage && Main.netMode != NetmodeID.MultiplayerClient) {
				//we apply the debuff for 4 seconds
				npc.AddBuff(BuffID.DryadsWardDebuff, 60 * 4);
			}
		}

		//Secondly, we apply the 2nd tier of Well Fed for 30 seconds
		// And thirdly, we remove the onFire debuff if the player have it
		public override void OnPlayerCollision(Player player) {
			//No conditions needed for our liquid
			//Shimmer and honey also don't have any other conditions outside of already not shimmering
			player.AddBuff(BuffID.WellFed2, 60 * 30);
			//Here we check if the player is either on fire or is afflicted with hellfire
			if (player.onFire || player.onFire3) {
				// If they are, we iterate trough all the buff slot the player have and remove them if they are found
				for (int i = 0; i < Player.MaxBuffs; i++) {
					int buffType = player.buffType[i];
					if (buffType == BuffID.OnFire || buffType == BuffID.OnFire3) {
						player.DelBuff(i);
					}
				}
			}
		}

		//Here we animate our liquid separately from other liquids in the game.
		//We animate it similarly to how it's normally animated, except the animation is twice as slow
		public override void AnimateLiquid(GameTime gameTime, ref int frame, ref float frameState) {
			float frameSpeed = Main.windSpeedCurrent * (25f / 2); //The code is exactly 1:1 with how vanilla animates it's liquids, except here the 2 is here to animate it half as slow
			frameSpeed = ((!(frameSpeed < 0f)) ? (frameSpeed + 5f) : (frameSpeed - 5f));
			frameSpeed = MathF.Abs(frameSpeed);
			frameState += frameSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (frameState < 0f)
				frameState += 16f;

			frameState %= 16f;

			frame = (int)frameState;
		}

		//These methods provide a way to edit the waves produced by a liquid
		//both modify the waves to be slightly larger and offset slightly less when moving through
		public override void NPCRippleModifier(NPC npc, ref float rippleStrength, ref float rippleOffset) {
			if (!npc.wet)
				rippleOffset = -1f;

			float factor = ((float)(int)npc.wetCount / 9f);

			factor = ((float)(int)npc.wetCount / 9f);
			rippleStrength += 0.25f * factor;

			rippleStrength *= 0.5f;
		}

		public override void PlayerRippleModifier(Player player, ref float rippleStrength, ref float rippleOffset) {
			if (!player.wet)
				rippleOffset = -1f;

			float factor = ((float)(int)player.wetCount / 9f);

			factor = ((float)(int)player.wetCount / 9f);
			rippleStrength += 0.5f * factor;

			rippleStrength *= 0.75f;
		}



		//The following contains all logic related to modifying the movement of entities in this liquid, making Players, Items, NPCs and Projectiles move slower while in this liquid
		//Here we replicate normal liquid movement behaviour using PlayerMovement
		public override bool PlayerMovement(Player player, bool fallThrough, bool ignorePlats) {
			int height = (!player.onTrack) ? player.height : (player.height - 20);
			Vector2 velocity = player.velocity;
			player.velocity = Collision.TileCollision(player.position, player.velocity, player.width, height, fallThrough, ignorePlats, (int)player.gravDir);
			Vector2 vector2 = player.velocity * LiquidID.Sets.PlayerMovementMultiplier[Type]; //We reuse the PlayerMovementMultiplier here for it to serve the same purpose
			if (player.velocity.X != velocity.X) {
				vector2.X = player.velocity.X;
			}
			if (player.velocity.Y != velocity.Y) {
				vector2.Y = player.velocity.Y;
			}
			player.position += vector2;
			player.TryFloatingInFluid();
			return false; //We return false as we do not want the normal liquid movement to execute after this hook/method
		}

		//related above, we use this method/hook to make items move at half the speed that they would when in honey
		public override void ItemCollision(WorldItem item, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed) {
			gravity = 0.02f;
			maxFallSpeed = 1f;
			wetVelocity = item.velocity * 0.125f;

			//The following has this liquid delete items of the Blue rarity similar to how lava deletes items of the white rarity
			//We put this here as liquid movement is called just before lava deletion (Item.CheckLavaDeath)
			if (!item.beingGrabbed) {
				if (item.playerIndexTheItemIsReservedFor == Main.myPlayer && item.inner.rare == ItemRarityID.Blue && item.type >= ItemID.None && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[item.type]) {
					//item.active = false;
					item.type = ItemID.None;
					item.stack = 0;
					if (Main.netMode != NetmodeID.SinglePlayer) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI);
					}
				}
			}

			//Here we detect if the item is a regular sink and destroy if that is the case, just to show a message.
			if (item.type == ItemID.WoodenSink) {
				item.TurnToAir();
				CombatText.NewText(item.Hitbox, Color.White, "Let that sink in!", true);
			}
		}

		//lastly, we reimplement the projectile movement in liquids using the ProjectileMovement
		//This hook is very similar and different to PlayerMovement, returning a bool and only having wetVelocity as a referenced parameter
		public override bool ProjectileMovement(Projectile projectile, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough) {
			Vector2 vector = projectile.velocity;
			projectile.velocity = Collision.TileCollision(collisionPosition, projectile.velocity, Width, Height, fallThrough, fallThrough);
			wetVelocity = projectile.velocity * LiquidID.Sets.ProjectileMovementMultiplier[Type]; //We reuse the ProjectileMovementMultiplier here for it to serve the same purpose
			if (projectile.velocity.X != vector.X) {
				wetVelocity.X = projectile.velocity.X;
			}
			if (projectile.velocity.Y != vector.Y) {
				wetVelocity.Y = projectile.velocity.Y;
			}
			return false; //We return false as we do not want the normal liquid movement to execute after this hook/method
		}

		// We use this hook to make so that the liquid can only be pumped up to 15 tiles away from it's original location
		public override bool OnPump(int inX, int inY, int outX, int outY) {
			// We start by converting our coordinates into Vector
			Vector2 inPosition = new Vector2(inX, inY);
			Vector2 outPosition = new Vector2(outX, outY);

			// We then calculate the distance between the 2 vector
			double distance = inPosition.Distance(outPosition);

			// Then we check if the distance between them is higher than 15, if is then you can pump the liquid to that location
			if (distance > 15.0) {
				return true;
			}

			// Here we have another example where if the moon lord has been defeated you can move the liquid using pumps;
			// if (NPC.downedMoonlord) {
			//	 return true;
			// }

			// otherwise you cannot pump it at all
			return false;
		}
	}
}
