using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ExampleMod.Buffs;

namespace ExampleMod.Mounts
{
	/*
	 This guide will focus on making a custom minecart.
	 A minecart works exactly like a mount with the exception of hooking onto rails and only working on rails
	 If you want to add more stuff to your minecart do it the same way Car.cs does
	 */

	public class ExampleMinecart : ModMountData
	{
		public override void SetDefaults()
		{
			// What separates mounts and minecarts are these 3 lines
			mountData.Minecart = true;
			// This makes the minecarts item autoequip in the minecart slot
			MountID.Sets.Cart[ModContent.MountType<ExampleMinecart>()] = true; 
			// The specified method takes care of spawning dust when stopping or jumping. Use DelegateMethods.Minecart.Sparks for normal sparks.
			mountData.MinecartDust = GreenSparks; 

			mountData.spawnDust = 16;
			mountData.buff = ModContent.BuffType<ExampleMinecartBuff>(); // serves the same purpose as for Car.cs

			// Movement fields:
			mountData.flightTimeMax = 0; // always set flight time to 0 for minecarts
			mountData.fallDamage = 1f; // how much fall damage will the player take in the minecart
			mountData.runSpeed = 10f; // how fast can the minecart go
			mountData.acceleration = 0.04f; // how fast does the minecart accelerate
			mountData.jumpHeight = 15; // how far does the minecart jump
			mountData.jumpSpeed = 5.15f; // how fast does the minecart jump
			mountData.blockExtraJumps = true; // Can the player not use a could in a bottle when in the minecart?
			mountData.heightBoost = 12;

			// Drawing fields:
			mountData.playerYOffsets = new int[] { 9, 9, 9 }; // where is the players Y position on the mount for each frame of animation
			mountData.xOffset = 2; // the X offset of the minecarts sprite
			mountData.yOffset = 9; // the Y offset of the minecarts sprite
			mountData.bodyFrame = 3; // which body frame is being used from the player when the player is boarded on the minecart
			mountData.playerHeadOffset = 14; // Affects where the player head is drawn on the map

			// Animation fields: The following is the mount animation values shared by vanilla minecarts. It can be edited if you know what you are doing.
			mountData.totalFrames = 3;
			mountData.standingFrameCount = 1;
			mountData.standingFrameDelay = 12;
			mountData.standingFrameStart = 0;
			mountData.runningFrameCount = 3;
			mountData.runningFrameDelay = 12;
			mountData.runningFrameStart = 0;
			mountData.flyingFrameCount = 0;
			mountData.flyingFrameDelay = 0;
			mountData.flyingFrameStart = 0;
			mountData.inAirFrameCount = 0;
			mountData.inAirFrameDelay = 0;
			mountData.inAirFrameStart = 0;
			mountData.idleFrameCount = 1;
			mountData.idleFrameDelay = 10;
			mountData.idleFrameStart = 0;
			mountData.idleFrameLoop = false;
			if (Main.netMode != NetmodeID.Server) {
				mountData.textureWidth = mountData.frontTexture.Width;
				mountData.textureHeight = mountData.frontTexture.Height;
			}
		}

		// This code adapted from DelegateMethods.Minecart.Sparks. Custom sparks are just and example and are not required.
		private void GreenSparks(Vector2 dustPosition)
		{
			dustPosition += new Vector2((Main.rand.Next(2) == 0) ? 13 : (-13), 0f).RotatedBy(DelegateMethods.Minecart.rotation);
			int num = Dust.NewDust(dustPosition, 1, 1, ModContent.DustType<Dusts.ExampleMinecartDust>(), Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
			Main.dust[num].noGravity = true;
			Main.dust[num].fadeIn = Main.dust[num].scale + 1f + 0.01f * (float)Main.rand.Next(0, 51);
			Main.dust[num].noGravity = true;
			Main.dust[num].velocity *= (float)Main.rand.Next(15, 51) * 0.01f;
			Main.dust[num].velocity.X *= (float)Main.rand.Next(25, 101) * 0.01f;
			Main.dust[num].velocity.Y -= (float)Main.rand.Next(15, 31) * 0.1f;
			Main.dust[num].position.Y -= 4f;
			if (Main.rand.Next(3) != 0)
				Main.dust[num].noGravity = false;
			else
				Main.dust[num].scale *= 0.6f;
		}
	}
}
