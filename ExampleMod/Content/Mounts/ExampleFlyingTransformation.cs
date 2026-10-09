using ExampleMod.Content.Buffs;
using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Mounts
{
	public class ExampleFlyingTransformation : ModMount {
		public override void SetStaticDefaults() {

			// Sets and properties that are for transformation mounts
			MountID.Sets.IsTransformationMount[Type] = true; // Designates this as a transformation mount.
			MountID.Sets.PlayerIsHidden[Type] = true; // Used by other drawing things to know that this mount hides the player. Settings this does NOT actually hide the player. (See below for that)

			MountData.delegations = new Mount.MountDelegatesData {
				MouthPosition = DelegateMethods.Mount.NoPosition, // Set the position of the mount's mouth. In this case, none.
				HandPosition = DelegateMethods.Mount.NoPosition, // Set the position of the mount's hands. In this case, none.
				PlayerSize = ExampleFlyingTransformationPlayerSize, // Set a custom size for the hitbox.
				DashDust = ExampleFlyingTransformationDashDust // Change the behavior of dust while dashing.
			};

			// Sets that specific for this mount
			MountID.Sets.CanDash[Type] = true; // This mount can dash with normal dashes.
			MountID.Sets.HoverIgnoresFatigue[Type] = true; // This mount has infinite flight.
			MountID.Sets.DontHoldItems[Type] = true; // This mount does not hold items.
			MountID.Sets.DontDismountWhenCCed[Type] = true; // Being "crowed controlled" (frozen, stoned, webbed, etc.) does not dismount the player.
			MountData.dismountsOnItemUse = true; // The player will be dismounted if they use items.

			// Misc
			MountData.spawnDust = ModContent.DustType<Sparkle>();
			MountData.spawnDustNoGravity = true;
			MountData.buff = ModContent.BuffType<ExampleFlyingTransformationBuff>();

			// Movement
			MountData.flightTimeMax = 320; 
			MountData.fatigueMax = 320; // If MountID.Sets.HoverIgnoresFatigue is set to true, this value will be ignored and the mount will have infinite flight.
			MountData.fallDamage = 0f;
			MountData.usesHover = true; // This mount flies.
			MountData.blockExtraJumps = true; // This mount cannot use double jumps.
			MountData.dashSpeed = 4.5f;
			MountData.runSpeed = MountData.dashSpeed;
			MountData.acceleration = 0.2f;
			MountData.jumpHeight = 8;
			MountData.jumpSpeed = 5f;

			// Frame data and player offsets
			MountData.totalFrames = 5; // The total number of frames in the sprite.
			MountData.playerYOffsets = Enumerable.Repeat(0, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
			MountData.xOffset = 0; // The offset for the hitbox. The size of the hitbox is set in ExampleFlyingTransformationPlayerSize
			MountData.yOffset = -1;
			MountData.playerHeadOffset = -24;
			MountData.bodyFrame = 0; // Which body frame the player uses while mounted. Since this mount makes the player invisible, it doesn't matter here.

			// This mount's frames are set up as frame 0 being the standing frame and 1-5 being the flying frames.

			// Standing
			MountData.standingFrameStart = 0; // Start on frame 0.
			MountData.standingFrameCount = 1; // A total of 1 frame for the standing animation.
			MountData.standingFrameDelay = 8; // The delay between frames (if there were more than one frame).
			// Dashing
			MountData.dashingFrameStart = 1;
			MountData.dashingFrameCount = 4;
			MountData.dashingFrameDelay = 2;
			// Flying
			MountData.flyingFrameStart = 1;
			MountData.flyingFrameCount = 4;
			MountData.flyingFrameDelay = 6;
			// Idle
			MountData.idleFrameStart = 0;
			MountData.idleFrameCount = 1;
			MountData.idleFrameDelay = 8;
			MountData.idleFrameLoop = true; // Loop the idle animation (if there were more than one frame).
			// Running
			MountData.runningFrameStart = MountData.flyingFrameStart;
			MountData.runningFrameCount = MountData.flyingFrameCount;
			MountData.runningFrameDelay = MountData.flyingFrameDelay * 4; // The running animation plays a lot faster than the flying animation, so slow it down here.
			// In Air
			MountData.inAirFrameStart = MountData.flyingFrameStart;
			MountData.inAirFrameCount = MountData.flyingFrameCount;
			MountData.inAirFrameDelay = MountData.flyingFrameDelay;
			// Swimming
			MountData.swimFrameStart = MountData.flyingFrameStart;
			MountData.swimFrameCount = MountData.flyingFrameCount;
			MountData.swimFrameDelay = MountData.flyingFrameDelay;
			if (Main.netMode != NetmodeID.Server) {
				MountData.textureWidth = MountData.frontTexture.Width();
				MountData.textureHeight = MountData.frontTexture.Height();
			}
		}

		/// <summary>
		/// This delegate sets the size of the player's hitbox.
		/// <br/> To offset the hitbox, set MountData.xOffset and MountData.yOffset
		/// </summary>
		public static bool ExampleFlyingTransformationPlayerSize(Player player, out Vector2? size) {
			size = new Vector2(14f, 14f);
			return true;
		}

		/// <summary>
		/// This delegate changes how dust behaves wile the mount is dashing.
		/// </summary>
		public static Dust ExampleFlyingTransformationDashDust(Player player, int currentDustCount, Dust dust) {
			if (currentDustCount % 2 == 0) {
				dust.active = false;
			}
			else {
				dust.position = Main.rand.NextVector2FromRectangle(player.Hitbox);
				dust.scale *= 0.75f;
			}

			return dust;
		}

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			// This mount has the wings as the backTexture (drawType 0) and the head as the frontTexture (drawType 2)
			// Let's rotate the head of the mount, but not the wings.
			if (drawType == 2) {
				// Rotate the head of the mount in the Y direction the player is moving.
				rotation = drawPlayer.velocity.Y * drawPlayer.direction * 0.1f;
				rotation = Utils.Clamp(rotation, -0.5f, 0.5f) - MathHelper.PiOver4 * drawPlayer.direction;
			}

			return true;
		}

		public override void UpdateEffects(Player player) {
			// Spawn some dusts while active.
			player.IsAllowedToHoldItems = false;
			player.noItems = true;
			bool flag = Main.rand.NextBool(15);
			if ((int)Main.timeForVisualEffects % 2 == 0 && (flag || (float)(Main.rand.Next(6) + 1) < player.velocity.Length())) {
				Dust dust = Dust.NewDustDirect(player.Center, 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 100, default, 0.5f);
				dust.position = player.Center + new Vector2(0f, -2f);
				if (flag)
					dust.velocity *= 0.4f;
				else
					dust.velocity *= 0.04f * player.velocity.Length();

				dust.velocity += player.velocity * 0.3f;
				dust.position += player.velocity * 0.7f;
				dust.position += (Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat() * 2f;
				dust.noGravity = true;
				dust.noLight = true;
			}
		}

		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			
			Lighting.AddLight(mountedPlayer.Center, new Vector3(0.33f, 0.33f, 0.33f)); // Emit some light.

			return base.UpdateFrame(mountedPlayer, state, velocity);
		}

		// Here is where we change the player's draw info to hide the entire player.
		// Setting drawInfo.hideEntirePlayer = true in ModPlayer.ModifyDrawInfo() doesn't work because it runs too late.
		public override void ModifyPlayerDrawInfo(ref PlayerDrawSet drawInfo) {
			drawInfo.hideEntirePlayer = true;
			drawInfo.weaponDrawOrder = WeaponDrawOrder.BehindBackArm;
		}
	}
}
