using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Mounts
{
	public class ExampleCarMount : ModMount
	{
		public override void SetDefaults() {
			MountData.spawnDust = ModContent.DustType<Dusts.Sparkle>();
			MountData.buff = ModContent.BuffType<Buffs.ExampleCarMountBuff>();
			MountData.heightBoost = 20;
			MountData.fallDamage = 0.5f;
			MountData.runSpeed = 11f;
			MountData.dashSpeed = 8f;
			MountData.flightTimeMax = 0;
			MountData.fatigueMax = 0;
			MountData.jumpHeight = 5;
			MountData.acceleration = 0.19f;
			MountData.jumpSpeed = 4f;
			MountData.blockExtraJumps = false;
			MountData.totalFrames = 4;
			MountData.constantJump = true;
			int[] array = new int[MountData.totalFrames];
			for (int l = 0; l < array.Length; l++) {
				array[l] = 20;
			}
			MountData.playerYOffsets = array;
			MountData.xOffset = 13;
			MountData.bodyFrame = 3;
			MountData.yOffset = -12;
			MountData.playerHeadOffset = 22;
			MountData.standingFrameCount = 4;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;
			MountData.runningFrameCount = 4;
			MountData.runningFrameDelay = 12;
			MountData.runningFrameStart = 0;
			MountData.flyingFrameCount = 0;
			MountData.flyingFrameDelay = 0;
			MountData.flyingFrameStart = 0;
			MountData.inAirFrameCount = 1;
			MountData.inAirFrameDelay = 12;
			MountData.inAirFrameStart = 0;
			MountData.idleFrameCount = 4;
			MountData.idleFrameDelay = 12;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = true;
			MountData.swimFrameCount = MountData.inAirFrameCount;
			MountData.swimFrameDelay = MountData.inAirFrameDelay;
			MountData.swimFrameStart = MountData.inAirFrameStart;
			if (Main.netMode == NetmodeID.Server) {
				return;
			}

			MountData.textureWidth = MountData.backTexture.Width() + 20;
			MountData.textureHeight = MountData.backTexture.Height();
		}

		public override void UpdateEffects(Player player) {
			// This code simulates some wind resistance for the balloons. 
			var balloons = (CarSpecificData)player.mount._mountSpecificData;
			float ballonMovementScale = 0.05f;
			for (int i = 0; i < balloons.count; i++) {
				if (Math.Abs(balloons.rotations[i]) > Math.PI / 2)
					ballonMovementScale *= -1;
				balloons.rotations[i] += -player.velocity.X * ballonMovementScale * Main.rand.NextFloat();
				balloons.rotations[i] = balloons.rotations[i].AngleLerp(0, 0.05f);
			}

			// This code spawns some dust if we are moving fast enough.
			if (!(Math.Abs(player.velocity.X) > 4f)) {
				return;
			}
			Rectangle rect = player.getRect();
			Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, ModContent.DustType<Dusts.Sparkle>());
		}

		// Since only a single instance of ModMountData ever exists, we can use player.mount._mountSpecificData to store additional data related to a specific mount.
		// Using something like this for gameplay effects would require ModPlayer syncing, but this example is purely visual.
		internal class CarSpecificData
		{
			// count tracks how many balloons are still left. See ExamplePerson.Hurt to see how count decreases whenever damage is taken.
			internal int count;
			internal float[] rotations;
			internal static float[] offsets = new float[] { 0, 14, -14 };
			public CarSpecificData() {
				count = 3;
				rotations = new float[count];
			}
		}

		public override void SetMount(Player player, ref bool skipDust) {
			// When this mount is mounted, we initialize _mountSpecificData with a new CarSpecificData object which will track some extra visuals for the mount.
			player.mount._mountSpecificData = new CarSpecificData();

			// This code bypasses the normal mount spawning dust and replaces it with our own visual.
			for (int i = 0; i < 16; i++) {
				Dust.NewDustPerfect(player.Center + new Vector2(80, 0).RotatedBy(i * Math.PI * 2 / 16f), MountData.spawnDust);
			}
			skipDust = true;
		}

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			// Draw is called for each mount texture we provide, so we check drawType to avoid duplicate draws.
			if (drawType == 0) {
				// We draw some extra balloons before _Back texture
				var balloons = (CarSpecificData)drawPlayer.mount._mountSpecificData;
				int timer = DateTime.Now.Millisecond % 800 / 200;
				Texture2D balloonTexture = Mod.GetTexture("Content/Items/Armor/SimpleAccessory_Balloon").Value;
				for (int i = 0; i < balloons.count; i++) {
					playerDrawData.Add(new DrawData(balloonTexture, drawPosition + new Vector2((-36 + CarSpecificData.offsets[i]) * drawPlayer.direction, 14), new Rectangle(28, balloonTexture.Height / 4 * ((timer + i) % 4), 28, 42), drawColor, rotation + balloons.rotations[i], new Vector2(14 + drawPlayer.direction * 7, 42), drawScale, spriteEffects ^ SpriteEffects.FlipHorizontally, 0));
				}
			}
			// by returning true, the regular drawing will still happen.
			return true;
		}
	}
}