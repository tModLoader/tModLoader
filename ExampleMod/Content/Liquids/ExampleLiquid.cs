using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
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
			SlopeOpacity = 1f; //Slope opacity is mainly for liquids near slopes when in retro or trippy lighting modes or when LiquidSlopeFix setting is disabled
			LiquidfallOpacityMultiplier = 0.5f; //Here we make the liquidfalls of this liquid draw at a 0.5x multiplier, making them seem much thicker
			//To change the old liquid rendering opacity, please see the RetroDrawEffects override.

			//For the Waves Quality setting, when set to Medium, waves are set to be the same distance no matter the liquid type.
			//To do this, the game applied a multiplier to make them all consistant between liquids. Here we set our own multiplier to make the waves the same distance.
			WaterRippleMultiplier = 0.3f;

			//This is used to specify what dust is used when splashing in this liquid.
			//SplashDustType is -1 normally, which prevents the liquid from doing any splash dust
			SplashDustType = ModContent.DustType<ExampleLiquidSplash>();

			//This is used to specify what sound is played when an entity enters a liquid
			SplashSound = SoundID.SplashWeak;

			FallDelay = 2; //The delay (in ticks) when this liquid is falling

			ChecksForDrowning = true; //If the player/npcs can drown in this liquid
			AllowEmitBreathBubbles = false; //Bubbles will come out of the player/npc's mouth normally when drowning, here we can stop that by setting it to false.

			//These multipliers are used in the default mod liquid player movement.
			//Here we make our liquid slow the player down by half what honey would allow the player to move at.

			//Heres the defaults for each liquid:
			//Water/Lava/Regular modded liquid = 0.5f
			//Honey = 0.25f
			//Shimmer = 0.375f
			PlayerMovementMultiplier = 0.125f; //For modders who don't want to reimplement the entire player movement for this liquid, this property is for them
			StopWatchMPHMultiplier = PlayerMovementMultiplier; //We set stopwatch to the same multiplier as we don't want a difference between whats felt and what the player can read their movement as.
			NPCMovementMultiplierDefault = PlayerMovementMultiplier; //NPCs have a similar modifier but as a field, here we set the default value as some other NPCs set this multiplier to 0. We set this to PlayerMovementMultiplier as we need them to all be the same.
			ProjectileMovementMultiplier = PlayerMovementMultiplier; //Simiarly to Players, Projectiles have this property for easy editing of a projectile velocity multiplier without needing to reimplement all of the projectile liquid movement code.

			FishingPoolSizeMultiplier = 2f; //The multiplier used for calculating the size of a fishing pool of this liquid. Here, each liquid tile counts as 2 for every tile in a fished pool.

			//For more dangerous liquids, we may want to have our liquid call On(Player/NPC/Projectile)Collision whenever an entity touches the liquid, rather than when an entity splashes in a liquid
			//For this we use similar collision calculations as lava using this boolean.
			//By default, this is disabled
			UsesLavaCollisionForWet = true;

			//Here we allow the extinguishing of the OnFire debuffs for both players and NPCs using this property
			ExtinguishesOnFireDebuffs = false;

			//This ID set controls what items classify as a sponge when trying to suck up this liquid
			//Here we remove the Ultra Absorbant sponge, Allow the Lava Absorbant sponge and the staff of regrowth to suck up this liquid
			LiquidID.Sets.CanBeAbsorbedBy[Type].Remove(ItemID.UltraAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.LavaAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.StaffofRegrowth); //Here is an example of turning a regular item into a sponge thats capable of sucking up our liquid

			//We can add a map entry to our liquid, by doing so we can show where our liquid is on the map.
			//Unlike vanilla, we can also add a map entry name, which will display a name if the liquid is being selected on the map.
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());

			//Please see ExampleFishingPlayer to see modded liquid fishing pool examples
		}

		//Here with LiquidMerge, we are able to decide when the liquid generates with a different tile.
		//Using the otherLiquid param, we are able to select which liquid that collides with ours creates a specific tile.
		public override int LiquidMerge(int i, int j, int otherLiquid) {
			if (otherLiquid == LiquidID.Water) {
				return TileID.TeamBlockBlue; //When the liquid collides with water. Blue team block is created
			}
			else if (otherLiquid == LiquidID.Lava) {
				return TileID.TeamBlockRed; //When the liquid collides with lava. Red team block is created
			}
			else if (otherLiquid == LiquidID.Honey) {
				return TileID.TeamBlockYellow; //When the liquid collides with honey. Yellow team block is created
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				return TileID.TeamBlockPink; //When the liquid collides with shimmer. Pink team block is created
			}
			//The base return is what the liquid generates by default. This is useful for when this liquid collides with another modded liquid that this liquid has no support for.
			//usually by default, this method will return TIleID.Stone, and generates a stone tile if it cannot recognise any predetermined tile type to generate with
			return TileID.TeamBlockWhite;

			//NOTE: for custom collisions/tile creation, please see PreLiquidMerge to determine whether the liquid should do its normal tile merging,
			//or if you want to do other effects when this liquid merges with another liquid.
		}

		//LiquidMergeSound is played only on clients, and serves as editing the sound of when a liquid merges with another liquid.
		//Here we set a few custom sounds to play when this liquid merges with other liquids.
		public override void LiquidMergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound) {
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

		//LiquidLightMaskMode is how the game decides what lightMaskMode to use when this liquid is over a tile
		//We set this to none, this is due to the liquid emitting light, needing no special lightMaskMode for its interaction with light.
		//LightMaskMode is how light interacts with the liquid or tile when hitting it. i.e. honey makes light slowly turn yellow when passing through
		public override LightMaskMode LiquidLightMaskMode(int i, int j) {
			return LightMaskMode.None;
		}

		//ModifyLight allows the liquid to emit light similarly to any tile or wall.
		//You can use this to emit light similarly to lava or shimmer.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			//Here we make the liquid just emit a bright white light by setting R, G, and B to 1.
			r = 1f;
			g = 1f;
			b = 1f;
		}

		//Using EvaporatesInHell, we are able to choose whether this liquid evaporates in hell, based on a condition.
		//For custom evaporation, use the UpdateLiquid hook.
		public override bool EvaporatesInHell(int i, int j) {
			//Here, our liquid in the bottom half of the underworld evaporates, while in the upper half does not
			if (j > Main.maxTilesY - 100) {
				return true;
			}
			return false;
		}

		//Using RetroDrawEffects, we can do stuff only during the rendering of liquids in the retro lighting style.
		//Here we set the opacity we want during retro lighting so that its consistant with the opacity of the liquid when not in non-retro lighting
		//NOTE: Despite being having RETRO in the name, this also applies to the "Trippy" Lighting style as well.
		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			drawData.liquidAlphaMultiplier *= 1.8f;
			if (drawData.liquidAlphaMultiplier > 1f) {
				drawData.liquidAlphaMultiplier = 1f;
			}
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
		public override void OnPlayerCollision(Player player) {
			//No conditions needed for our liquid
			//Shimmer and honey also don't have any other conditions outside of already not shimmering
			player.AddBuff(BuffID.WellFed2, 60 * 30, false, false);
		}

		//Here we animate our liquid seperately from other liquids in the game.
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
		//Here we replicate normal liquid movement behaviour using PlayerLiquidMovement
		public override bool PlayerLiquidMovement(Player player, bool fallThrough, bool ignorePlats) {
			int height = (!player.onTrack) ? player.height : (player.height - 20);
			Vector2 velocity = player.velocity;
			player.velocity = Collision.TileCollision(player.position, player.velocity, player.width, height, fallThrough, ignorePlats, (int)player.gravDir);
			Vector2 vector2 = player.velocity * PlayerMovementMultiplier; //We reuse the PlayerMovementMultiplier here for it to serve the same purpose
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

		//Liquid movement is effected in 2 parts for players
		//Liquid velocity and gravity modifiers are seperate from the rest of the movement, we modify them here in PlayerGravityModifier
		public override void PlayerGravityModifier(Player player, ref float gravity, ref float maxFallSpeed, ref int jumpHeight, ref float jumpSpeed) {
			//These values are half of what honey applies to the player
			gravity = 0.05f;
			maxFallSpeed = 1.5f;
			//In other liquids, the jump speed and height is increased to simulate "swimming"
			//We don't want that for our liquid so we don't modify the other hook/method parameters
		}

		//related above, we use this method/hook to make items move at half the speed that they would when in honey
		public override void ItemLiquidCollision(Item item, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed) {
			gravity = 0.02f;
			maxFallSpeed = 1f;
			wetVelocity = item.velocity * 0.125f;

			//The following has this liquid delete items of the Blue rarity similar to how lava deletes items of the white rarity
			//We put this here as liquid movement is called just before lava deletion (Item.CheckLavaDeath)
			if (!item.beingGrabbed) {
				if (item.playerIndexTheItemIsReservedFor == Main.myPlayer && item.rare == ItemRarityID.Blue && item.type >= ItemID.None && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[item.type]) {
					item.active = false;
					item.type = ItemID.None;
					item.stack = 0;
					if (Main.netMode != NetmodeID.SinglePlayer) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI);
					}
				}
			}
		}

		//Like above, we set the parameters 'gravity' and 'maxFallSpeed' to be half of what honey would set them to.
		public override void NPCGravityModifier(NPC npc, ref float gravity, ref float maxFallSpeed) {
			gravity = 0.05f;
			maxFallSpeed = 1f;
			//You may notice that NPCLiquidMovement doesn't have a wetVelocity param, set the NPCMovementMultiplierDefault property in SetStaticDefaults to change the wet velocity multiplier for NPCs
			//The property is seperate because of NPCs indivdually setting the multiplier value based on type (DD2 npcs set it to 1f to ignore liquid movement)
		}

		//lastly, we reimplement the projectile movement in liquids using the ProjectileLiquidMovement
		//This hook is very similar and different to PlayerLiquidMovement, returning a bool and only having wetVelocity as a referenced parameter
		public override bool ProjectileLiquidMovement(Projectile projectile, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough) {
			Vector2 vector = projectile.velocity;
			projectile.velocity = Collision.TileCollision(collisionPosition, projectile.velocity, Width, Height, fallThrough, fallThrough);
			wetVelocity = projectile.velocity * ProjectileMovementMultiplier; //We reuse the ProjectileMovementMultiplier here for it to serve the same purpose
			if (projectile.velocity.X != vector.X) {
				wetVelocity.X = projectile.velocity.X;
			}
			if (projectile.velocity.Y != vector.Y) {
				wetVelocity.Y = projectile.velocity.Y;
			}
			return false; //We return false as we do not want the normal liquid movement to execute after this hook/method
		}



		//The following contains all the logic for what this liquid does when being entered and exited by different entities.
		//Each hook/method is used to execute what would happen when something enters/exits a liquid.
		//There is a hook for the following
		// * Players
		// * NPCs
		// * Projectiles
		// * Items
		//Each hook has a "isEnter" parameter, which is true whenever entity is entering the liquid and false when exiting.
		//This is usually used to do different effects when entering a liquid rather than exiting one

		//The commeneted code is adapted from vanilla's splash code for honey. Uncomment the code to make this liquid have it's splash act like honey's splash instead.
		//public override bool OnPlayerSplash(Player player, bool isEnter) {
		//	for (int i = 0; i < 20; i++) {
		//		int dust = Dust.NewDust(new Vector2(player.position.X - 6f, player.position.Y + (player.height / 2) - 8f), player.width + 12, 24, SplashDustType);
		//		Main.dust[dust].velocity.Y -= 1f;
		//		Main.dust[dust].velocity.X *= 2.5f;
		//		Main.dust[dust].scale = 1.3f;
		//		Main.dust[dust].alpha = 100;
		//		Main.dust[dust].noGravity = true;
		//	}
		//	SoundEngine.PlaySound(SplashSound, player.position);
		//	return false;
		//}

		//public override bool OnNPCSplash(NPC npc, bool isEnter) {
		//	for (int i = 0; i < 10; i++) {
		//		int dust = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (npc.height / 2) - 8f), npc.width + 12, 24, SplashDustType);
		//		Main.dust[dust].velocity.Y -= 1f;
		//		Main.dust[dust].velocity.X *= 2.5f;
		//		Main.dust[dust].scale = 1.3f;
		//		Main.dust[dust].alpha = 100;
		//		Main.dust[dust].noGravity = true;
		//	}
		//	//only play the sound if the npc isnt a slime, mouse, tortoise, or if it has no gravity
		//	if (npc.aiStyle != NPCAIStyleID.Slime &&
		//			npc.type != NPCID.BlueSlime && npc.type != NPCID.MotherSlime && npc.type != NPCID.IceSlime && npc.type != NPCID.LavaSlime &&
		//			npc.type != NPCID.Mouse &&
		//			npc.aiStyle != NPCAIStyleID.GiantTortoise &&
		//			!npc.noGravity) {
		//		SoundEngine.PlaySound(SplashSound, npc.position);
		//	}
		//	return false;
		//}

		//public override bool OnProjectileSplash(Projectile proj, bool isEnter) {
		//	for (int i = 0; i < 10; i++) {
		//		int dust = Dust.NewDust(new Vector2(proj.position.X - 6f, proj.position.Y + (proj.height / 2) - 8f), proj.width + 12, 24, SplashDustType);
		//		Main.dust[dust].velocity.Y -= 1f;
		//		Main.dust[dust].velocity.X *= 2.5f;
		//		Main.dust[dust].scale = 1.3f;
		//		Main.dust[dust].alpha = 100;
		//		Main.dust[dust].noGravity = true;
		//	}
		//	SoundEngine.PlaySound(SplashSound, proj.position);
		//	return false;
		//}

		//public override bool OnItemSplash(Item item, bool isEnter) {
		//	for (int i = 0; i < 5; i++) {
		//		int dust = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (item.height / 2) - 8f), item.width + 12, 24, SplashDustType);
		//		Main.dust[dust].velocity.Y -= 1f;
		//		Main.dust[dust].velocity.X *= 2.5f;
		//		Main.dust[dust].scale = 1.3f;
		//		Main.dust[dust].alpha = 100;
		//		Main.dust[dust].noGravity = true;
		//	}
		//	SoundEngine.PlaySound(SplashSound, item.position);
		//	return false;
		//}
	}
}
