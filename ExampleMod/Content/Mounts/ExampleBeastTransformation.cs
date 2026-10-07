using ExampleMod.Content.Buffs;
using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Mounts
{
	public class ExampleBeastTransformation : ModMount {
		public override void SetStaticDefaults() {

			// Sets and properties that are for transformation mounts
			MountID.Sets.IsTransformationMount[Type] = true; // Designates this as a transformation mount.
			MountID.Sets.PlayerIsHidden[Type] = true; // Used by other drawing things to know that this mount hides the player. Settings this does NOT actually hide the player. (See below for that)

			MountData.delegations = new Mount.MountDelegatesData {
				MouthPosition = ExampleBeastTransformationMouthPosition, // Sets the position of the items.
				HandPosition = DelegateMethods.Mount.NoPosition // You might expect this mount to change the hand position, but it actually uses the mouth position for items.
			};

			// Sets that specific for this mount
			MountID.Sets.CanUseHooks[Type] = true; // Grappling hooks can be used while using this mount.

			// Misc
			MountData.spawnDust = ModContent.DustType<Sparkle>();
			MountData.buff = ModContent.BuffType<ExampleBeastTransformationBuff>();

			// Movement
			MountData.flightTimeMax = 0;
			MountData.fallDamage = 0.1f;
			MountData.runSpeed = 4.5f;
			MountData.dashSpeed = 7.5f;
			MountData.acceleration = 0.15f;
			MountData.jumpHeight = 15;
			MountData.jumpSpeed = 6.01f;

			// Frame data and player offsets
			MountData.totalFrames = 21; // The total number of frames in the sprite.
			MountData.playerYOffsets = Enumerable.Repeat(0, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
			MountData.xOffset = 4; // The offset for the hitbox.
			MountData.yOffset = -4;
			MountData.bodyFrame = 0; // Which body frame the player uses while mounted. Since this mount makes the player invisible, it doesn't matter here.

			// Standing
			MountData.standingFrameStart = 0; // Start on frame 0.
			MountData.standingFrameCount = 1; // A total of 1 frame for the standing animation.
			MountData.standingFrameDelay = 12; // The delay between frames (if there were more than one frame).
			// Running
			MountData.runningFrameStart = 8;
			MountData.runningFrameCount = 7;
			MountData.runningFrameDelay = 20;
			// In Air (falling)
			MountData.inAirFrameStart = 5;
			MountData.inAirFrameCount = 2;
			MountData.inAirFrameDelay = 6;
			// Flying
			MountData.flyingFrameStart = 16;
			MountData.flyingFrameCount = 5;
			MountData.flyingFrameDelay = 6;
			// Idle
			MountData.idleFrameCount = 0;
			MountData.idleFrameDelay = 0;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = false;
			// Swimming
			MountData.swimFrameCount = MountData.inAirFrameCount;
			MountData.swimFrameDelay = MountData.inAirFrameDelay;
			MountData.swimFrameStart = MountData.inAirFrameStart;
			if (Main.netMode != NetmodeID.Server) {
				MountData.textureWidth = MountData.frontTexture.Width();
				MountData.textureHeight = MountData.frontTexture.Height();
			}
		}

		/// <summary>
		/// Sets the position of the items.
		/// </summary>
		public static bool ExampleBeastTransformationMouthPosition(Player player, out Vector2? position) {
			Vector2 spinningpoint = new Vector2(player.direction * 24, player.gravDir * -12f);
			position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) + spinningpoint.RotatedBy(player.fullRotation);
			return true;
		}

		public override void SetMount(Player player, ref bool skipDust) {
			// Assign the container that says whether this mount is allowed to fly.
			player.mount._mountSpecificData = new ExampleBeastTransformationSelectiveFlyingMountData();
		}

		public override void JumpSpeed(Player mountedPlayer, ref float jumpSeed, float xVelocity) {
			/*
			if (mountedPlayer.wingsLogic > 0) {
				WingStats wingStats = mountedPlayer.GetWingStats(mountedPlayer.wingsLogic);
				jumpSeed = wingStats.AccRunSpeedOverride / 1.5f;
			}
			*/
		}

		public override void JumpHeight(Player mountedPlayer, ref int jumpHeight, float xVelocity) {
			//Main.NewText($"jumpHeight {jumpHeight}");
		}

		// Mounts that have flightTimeMax > 0 can automatically fly.
		// This mount can only fly if the player if the has wings.
		public override bool? CanFly(Player player) {
			return ((ExampleBeastTransformationSelectiveFlyingMountData)player.mount._mountSpecificData).allowedToFly;
		}

		public override void UpdateAfterEquips(Player player) {
			bool hasWings = player.wingsLogic > 0;
			// This mount can only fly if the player if the has wings.
			((ExampleBeastTransformationSelectiveFlyingMountData)player.mount._mountSpecificData).allowedToFly = hasWings;
			if (hasWings && player.empressBrooch) {
				player.mount._flyTime = player.wingTimeMax; // Infinite flight if the Soaring Insignia is equipped.
			}
		}

		public override void ResetFlightTime(Player player, ref int flightTime) {
			flightTime = player.wingTimeMax; // Set the max flight time to the same the player's.
		}


		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			int yFrame = 0;
			// Animate the arm
			// drawType 3 is the frontTextureExtra
			if (drawType == 3) {
				if (drawPlayer.itemAnimation > 0) {
					Rectangle bodyFrame = drawPlayer.bodyFrame;
					int bodyYFrame = bodyFrame.Y / bodyFrame.Height;
					int useStyle = drawPlayer.lastVisualizedSelectedItem.useStyle;
					yFrame = Utils.Clamp(bodyYFrame, 1, 4);
					if (useStyle == ItemUseStyleID.Guitar && drawPlayer.itemAnimation > drawPlayer.itemAnimationMax / 2)
						yFrame = 3;

					if (useStyle == ItemUseStyleID.EatFood || useStyle == ItemUseStyleID.DrinkLiquid || useStyle == ItemUseStyleID.HoldUp || useStyle == ItemUseStyleID.RaiseLamp)
						yFrame = 2;

					if (useStyle == ItemUseStyleID.GolfPlay || useStyle == ItemUseStyleID.MowTheLawn)
						yFrame = 3;
				}
				else {
					yFrame = drawPlayer.lastVisualizedSelectedItem.holdStyle switch {
						1 or 6 => 3,
						2 => 2,
						_ => drawPlayer.mount._frame,
					};
				}
			}
			else {
				yFrame = drawPlayer.mount._frame;
			}
			int totalMountFrames = drawPlayer.mount._data.totalFrames;
			int totalMountTextureHeight = drawPlayer.mount._data.textureHeight;
			int mountFrameHeight = totalMountTextureHeight / totalMountFrames;
			frame.Y = mountFrameHeight * yFrame;
			return true;
		}

		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {

			if (state != 0)
				mountedPlayer.mount._idleTime = 0;

			if (mountedPlayer.isDisplayDollOrInanimate)
				mountedPlayer.mount._idleTime = 0;

			var selectiveFlyingMountData = (ExampleBeastTransformationSelectiveFlyingMountData)mountedPlayer.mount._mountSpecificData;

			// Flying in water
			if (state == 4) { 
				// Change it to either In the air, flying or in the air, not flying.
				state = (selectiveFlyingMountData.allowedToFly ? 3 : 2);
			}

			bool inTheAir = state == 2 || state == 3;
			int wingType = ((mountedPlayer.wings > 0) ? mountedPlayer.wings : mountedPlayer.wingsLogic);
			if (inTheAir && selectiveFlyingMountData.allowedToFly && ((wingType > 0 && !ArmorIDs.Wing.Sets.AlwaysAnimated[wingType]) || mountedPlayer.ShouldDrawWingsThatAreAlwaysAnimated(ignoreMounts: true))) {
				state = 3; // If the player has wings equipped or can fly by some other means, change the state to in the air, flying.
			}
			else if (state == 3) {
				state = 2; // If the player cannot fly, change the state to in the air, not flying.
			}

			switch (state)
				{
				case 0: // Idle
					mountedPlayer.mount._frame = mountedPlayer.mount._data.standingFrameStart;
					break;
				case 1: // Walking/Running
					UpdateFrame_Walking(mountedPlayer, velocity);
					break;
				case 2: // In the air, not flying
					UpdateFrame_Falling(mountedPlayer, velocity);
					break;
				case 3: { // In the air, flying
						mountedPlayer.mount._frameCounter += 1f;
						int flyingFrameDelay = mountedPlayer.mount._data.flyingFrameDelay;
						if (mountedPlayer.mount._flyTime > 0)
							flyingFrameDelay -= 2;

						if (mountedPlayer.mount._frameCounter > (float)flyingFrameDelay) {
							mountedPlayer.mount._frameCounter -= flyingFrameDelay;
							mountedPlayer.mount._frame++;
						}

						if (mountedPlayer.mount._frame < mountedPlayer.mount._data.flyingFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.flyingFrameStart + mountedPlayer.mount._data.flyingFrameCount)
							mountedPlayer.mount._frame = mountedPlayer.mount._data.flyingFrameStart;
						break;
					}
			}

			return false;
		}

		private static void UpdateFrame_Walking(Player mountedPlayer, Vector2 velocity) {
			float absVelX = Math.Abs(velocity.X);

			mountedPlayer.mount._frameCounter += absVelX;
			if (absVelX >= 0f) {
				if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.runningFrameDelay) {
					mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.runningFrameDelay;
					mountedPlayer.mount._frame++;
				}

				if (mountedPlayer.mount._frame < mountedPlayer.mount._data.runningFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount)
					mountedPlayer.mount._frame = mountedPlayer.mount._data.runningFrameStart;
			}
			else {
				if (mountedPlayer.mount._frameCounter < 0f) {
					mountedPlayer.mount._frameCounter += mountedPlayer.mount._data.runningFrameDelay;
					mountedPlayer.mount._frame--;
				}

				if (mountedPlayer.mount._frame < mountedPlayer.mount._data.runningFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount)
					mountedPlayer.mount._frame = mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount - 1;
			}
		}

		private static void UpdateFrame_Falling(Player mountedPlayer, Vector2 velocity) {
			mountedPlayer.mount._frameCounter += 1f;
			if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.inAirFrameDelay) {
				mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.inAirFrameDelay;
				mountedPlayer.mount._frame++;
			}

			if (mountedPlayer.mount._frame < mountedPlayer.mount._data.inAirFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.inAirFrameStart + mountedPlayer.mount._data.inAirFrameCount)
				mountedPlayer.mount._frame = mountedPlayer.mount._data.inAirFrameStart;

			if (mountedPlayer.grappling[0] >= 0) {
				if (velocity.Length() > 0.01f) {
					if (mountedPlayer.mount._frame == mountedPlayer.mount._data.inAirFrameStart + mountedPlayer.mount._data.inAirFrameCount - 1)
						mountedPlayer.mount._frameCounter = 0f;
				}
				else {
					mountedPlayer.mount._frame = 7;
				}
			}
			else if (velocity.Y < 0f) {
				if (mountedPlayer.mount._frame == mountedPlayer.mount._data.inAirFrameStart + mountedPlayer.mount._data.inAirFrameCount - 1)
					mountedPlayer.mount._frameCounter = 0f;
			}
			else {
				mountedPlayer.mount._frame = 7;
			}
		}

		private class ExampleBeastTransformationSelectiveFlyingMountData
		{
			public bool showFlyingFrames;
			public bool allowedToFly;

			public ExampleBeastTransformationSelectiveFlyingMountData() {
				showFlyingFrames = false;
				allowedToFly = false;
			}
		}

		// Here is where we change the player's draw info to hide the entire player.
		// ModPlayer.ModifyDrawInfo() doesn't work because it runs too late.
		public override void ModifyPlayerDrawInfo(ref PlayerDrawSet drawInfo) {
			drawInfo.hideEntirePlayerExceptHelmetsAndFaceAccessories = true;
			drawInfo.weaponDrawOrder = WeaponDrawOrder.BehindFrontArm;
			drawInfo.VisualPositionOffset = new Vector2(-14f, 0f) * drawInfo.drawPlayer.Directions;
			drawInfo.Position += drawInfo.VisualPositionOffset;
			bool itemAnimating = drawInfo.drawPlayer.itemAnimation > 0;
			if (drawInfo.heldItem.useStyle == ItemUseStyleID.GolfPlay && itemAnimating)
				drawInfo.weaponDrawOrder = WeaponDrawOrder.OverFrontArm;

			drawInfo.drawPlayer.ApplyItemPositionOffsetFromMount(ref drawInfo.ItemLocation);
		}

		public override void Load() {
			Terraria.DataStructures.On_PlayerDrawSet.CreateCompositeData += On_PlayerDrawSet_CreateCompositeData;
			Terraria.DataStructures.On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem;
		}

		private void On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItem orig, ref PlayerDrawSet drawinfo) {
			if (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				drawinfo.drawPlayer.mount.Draw(drawinfo.DrawDataCache, 3, drawinfo.drawPlayer, drawinfo.Position, drawinfo.colorMount, drawinfo.playerEffect, drawinfo.shadow);
			}
			orig(ref drawinfo);
		}

		private void On_PlayerDrawSet_CreateCompositeData(On_PlayerDrawSet.orig_CreateCompositeData orig, ref PlayerDrawSet self) {
			if (self.drawPlayer.mount.Active && self.drawPlayer.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				self.mountHandlesHeadDraw = true;
				// self.mountDrawsEyelid = true;
			}

			orig(ref self);
		}
	}

	public class ExampleBeastTransformationPlayer : ModPlayer {

		public override void Load() {
			Terraria.On_Player.ApplyItemPositionOffsetFromMount += On_Player_ApplyItemPositionOffsetFromMount;
			Terraria.On_Player.ApplyHeadOffsetFromMount += On_Player_ApplyHeadOffsetFromMount;
			Terraria.On_Player.GetHelmetOffsetAddonFromMount += On_Player_GetHelmetOffsetAddonFromMount;
			Terraria.On_Player.GetBeardOffsetAddonFromMount += On_Player_GetBeardOffsetAddonFromMount;
			// DoEyebrellaRainEffect Skipping for now.
			// Skipping solar shield offset
			// MowTheLawn
			// CheckDrowning breathing reed
		}

		private Vector2 On_Player_GetBeardOffsetAddonFromMount(On_Player.orig_GetBeardOffsetAddonFromMount orig, Player self, Vector2 beardOffset) {
			Vector2 originalReturn = orig(self, beardOffset);

			if (self.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				int b = self.beard; // sbyte->int
				if ((uint)(b - 1) <= 3u) {
					beardOffset += new Vector2(8f, 4f) * self.Directions;
					if (self.mount.Frame == 6)
						beardOffset += new Vector2(0f, 2f) * self.Directions;
					else if (self.mount.Frame == 7)
						beardOffset += new Vector2(-2f, 4f) * self.Directions;
				}
				return beardOffset;
			}

			return originalReturn;
		}

		private Vector2 On_Player_GetHelmetOffsetAddonFromMount(On_Player.orig_GetHelmetOffsetAddonFromMount orig, Player self) {
			Vector2 originalReturn = orig(self);

			Vector2 zero = Vector2.Zero;

			if (self.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				switch (self.head) {
					case 13:
					case 15:
					case 16:
					case 59:
					case 60:
					case 63:
					case 64:
					case 81:
					case 126:
					case 231:
					case 279:
						zero += new Vector2(0f, -2f) * self.Directions;
						break;
					case 2:
					case 5:
					case 10:
					case 12:
					case 25:
					case 84:
					case 85:
					case 116:
					case 117:
					case 138:
					case 141:
					case 143:
					case 160:
					case 178:
					case 183:
					case 191:
					case 194:
					case 197:
					case 217:
					case 218:
					case 233:
					case 245:
					case 265:
					case 274:
					case 277:
						zero += new Vector2(-2f, 0f) * self.Directions;
						break;
					case 163:
					case 228:
					case 229:
					case 235:
						zero += new Vector2(-2f, 2f) * self.Directions;
						break;
					case 222:
					case 242:
					case 243:
					case 244:
						zero += new Vector2(0f, 2f) * self.Directions;
						break;
					case 73:
					case 91:
					case 227:
					case 280:
						zero += new Vector2(-2f, -2f) * self.Directions;
						break;
					case 195:
						zero += new Vector2(-4f, -2f) * self.Directions;
						break;
					case 241:
						zero += new Vector2(-4f, -14f) * self.Directions;
						break;
					case 272:
						zero += new Vector2(-4f, 4f) * self.Directions;
						break;
					case 203:
						zero += new Vector2(0f, -4f) * self.Directions;
						break;
					case 267:
						zero += new Vector2(-2f, -4f) * self.Directions;
						break;
				}

				return zero;
			}

			return originalReturn;
		}

		private void On_Player_ApplyHeadOffsetFromMount(On_Player.orig_ApplyHeadOffsetFromMount orig, Player self, ref Vector2 pos) {
			orig(self, ref pos);

			int frame = self.mount.Frame;
			Vector2 vector = Vector2.Zero;

			if (self.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				vector = new Vector2(23f, -8f) * self.Directions;
				switch (frame) {
					case 5:
						vector += new Vector2(0f, 4f) * self.Directions;
						break;
					case 6:
						vector += new Vector2(2f, 2f) * self.Directions;
						break;
					case 7:
						vector += new Vector2(2f, 0f) * self.Directions;
						break;
					case 8:
					case 9:
					case 11:
					case 12:
					case 13:
					case 15:
						vector += new Vector2(4f, 4f) * self.Directions;
						break;
					case 10:
					case 14:
						vector += new Vector2(4f, 2f) * self.Directions;
						break;
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
						vector += new Vector2(4f, 4f) * self.Directions;
						break;
				}
			}

			pos += vector;
		}

		private void On_Player_ApplyItemPositionOffsetFromMount(On_Player.orig_ApplyItemPositionOffsetFromMount orig, Player self, ref Vector2 pos) {
			orig(self, ref pos);

			int bodyYFrame = self.bodyFrame.Y / self.bodyFrame.Height;
			Vector2 itemOffset = Vector2.Zero;
			bool itemAnimating = self.itemAnimation > 0;
			bool notAnimatingButHoldStyle = !itemAnimating && self.HeldItem.holdStyle > 0;
			if (!itemAnimating && !notAnimatingButHoldStyle)
				return;

			bool useStyleShoot = self.HeldItem.useStyle == ItemUseStyleID.Shoot;
			bool useStyleEatFood = self.HeldItem.useStyle == ItemUseStyleID.EatFood;
			bool useStyleDrinkLiquid = self.HeldItem.useStyle == ItemUseStyleID.DrinkLiquid;
			bool usingFishingPole = self.HeldItem.fishingPole != 0;
			bool useStyleRaiseLamp = self.HeldItem.useStyle == ItemUseStyleID.RaiseLamp;
			bool useStyleGolfPlay = self.HeldItem.useStyle == ItemUseStyleID.GolfPlay;
			bool heldYoyo = self.HeldItem.type > ItemID.None && ItemID.Sets.Yoyo[self.HeldItem.type];
			bool useStyleMowTheLawn = self.HeldItem.useStyle == ItemUseStyleID.MowTheLawn;
			bool heldNebulaBlaze = self.HeldItem.type == ItemID.NebulaBlaze;
			bool heldKite = self.HeldItem.type > ItemID.None && ItemID.Sets.IsAKite[self.HeldItem.type];
			bool holdStyleHoldFront = self.HeldItem.holdStyle == ItemHoldStyleID.HoldFront;
			bool holdStyleHoldUp = self.HeldItem.holdStyle == ItemHoldStyleID.HoldUp;
			bool holdStyleHoldGuitar = self.HeldItem.holdStyle == ItemHoldStyleID.HoldGuitar;

			if (self.mount.Type == ModContent.MountType<ExampleBeastTransformation>()) {
				if (useStyleEatFood) {
					if (itemAnimating)
						itemOffset += new Vector2(7f, -4f) * self.Directions;
					else
						itemOffset += new Vector2(3f, 2f) * self.Directions;
				}
				else if (usingFishingPole) {
					itemOffset += new Vector2(-2f, 0f) * self.Directions;
				}
				else if (useStyleDrinkLiquid) {
					if (itemAnimating)
						itemOffset += new Vector2(14f, -10f) * self.Directions;
					else
						itemOffset += new Vector2(3f, 2f) * self.Directions;
				}
				else if (useStyleMowTheLawn) {
					if (itemAnimating)
						itemOffset += new Vector2(4f, 0f) * self.Directions;
					else
						itemOffset += new Vector2(1f, 0f) * self.Directions;
				}
				else if (useStyleGolfPlay) {
					itemOffset += new Vector2(6f, 0f) * self.Directions;
				}
				else if (useStyleRaiseLamp) {
					if (itemAnimating)
						itemOffset += new Vector2(-6f, 6f) * self.Directions;
					else
						itemOffset += new Vector2(-10f, 10f) * self.Directions;
				}
				else if (heldKite) {
					itemOffset += new Vector2(4f, -4f) * self.Directions;
				}
				else if (holdStyleHoldGuitar) {
					itemOffset += new Vector2(6f, 0f) * self.Directions;
				}
				else if (holdStyleHoldFront && !itemAnimating) {
					itemOffset += new Vector2(1f, 4f) * self.Directions;
				}
				else if (holdStyleHoldUp) {
					if (itemAnimating && self.HeldItem.type == ItemID.BreathingReed)
						itemOffset += new Vector2(-4f, 0f).RotatedBy((pos - self.Center).ToRotation(), Vector2.Zero);
					else
						itemOffset += new Vector2(6f, 0f) * self.Directions;
				}
				else if (heldYoyo) {
					switch (bodyYFrame) {
						case 2:
							itemOffset += new Vector2(10f, -10f) * self.Directions;
							break;
						case 3:
							itemOffset += new Vector2(8f, 0f) * self.Directions;
							break;
						case 4:
							itemOffset += new Vector2(2f, 2f) * self.Directions;
							break;
					}
				}
				else if (heldNebulaBlaze) {
					switch (bodyYFrame) {
						case 2:
							itemOffset += new Vector2(-10f, 0f) * self.Directions;
							break;
						case 3:
							itemOffset += new Vector2(10f, 0f) * self.Directions;
							break;
						case 4:
							itemOffset += new Vector2(10f, 0f) * self.Directions;
							break;
					}
				}
				else if (useStyleShoot) {
					itemOffset += new Vector2(4f, 0f) * self.Directions;
				}
				else {
					switch (bodyYFrame) {
						case 1:
							itemOffset += new Vector2(1f, -1f) * self.Directions;
							break;
						case 2:
							itemOffset += new Vector2(4f, 2f) * self.Directions;
							break;
						case 3:
							itemOffset += new Vector2(2f, 0f) * self.Directions;
							break;
					}
				}
			}

			pos += itemOffset;
		}
	}
}
