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
			//what separates mounts and minecarts are these 3 lines
			mountData.Minecart = true;
			mountData.MinecartDust = new Action<Vector2>(DelegateMethods.Minecart.Sparks); // this will make the minecart spawn dust when slowing down
			MountID.Sets.Cart[ModContent.MountType<ExampleMinecart>()] = true; //this makes the minecarts item autoequip in the minecart slot

			int[] array = new int[3];
			for (int i = 0; i < array.Length; i++) 
			{
				array[i] = 9; //edit this to edit the playerYoffset
			}
			mountData.spawnDust = 16;
			mountData.buff = ModContent.BuffType<ExampleMinecartBuff>(); //serves the same purpose as for Car.cs
			mountData.flightTimeMax = 0; //always set flight time to 0 for minecarts
			mountData.fallDamage = 1f; //hwo much fall dmg will the player take in the minecart
			mountData.runSpeed = 10f; //how fast can the minecart go
			mountData.acceleration = 0.04f; //how fast does the minecart accelerate
			mountData.jumpHeight = 15; //how far does the minecart jump
			mountData.jumpSpeed = 5.15f; //how fast does the minecart jump
			mountData.blockExtraJumps = true; //Can the player use a could in a bottle when in the minecart?
			mountData.heightBoost = 12;
			mountData.playerYOffsets = array; //where is the players Y position on the mount
			mountData.xOffset = 2; //the X offset of the minecarts sprite
			mountData.yOffset = 9; //the Y offset of the minecarts sprite
			mountData.bodyFrame = 3; //which body frame is being used from the player when the player is boarded on the minecart

			#region Sheeting
			//sheeting for the minecart is already done, edit this only if your minecart doesnt match vanilla minecart sheets
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

			#endregion
		}
	}
}
