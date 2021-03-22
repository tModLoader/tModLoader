using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ExampleMod.Buffs;

namespace ExampleMod.Mounts
{
	public class ExampleMinecart : ModMountData
	{
		public override void SetDefaults()
		{
			//what separates mounts and minecarts are these 3 lines
			mountData.Minecart = true;
			mountData.MinecartDirectional = true;
			mountData.MinecartDust = new Action<Vector2>(DelegateMethods.Minecart.Sparks); // this will make the minecart spawn dust when slowing down

			int[] array = new int[3];
			for (int i = 0; i < array.Length; i++) 
			{
				array[i] = 9;
			}
			mountData.spawnDust = 16;
			mountData.buff = ModContent.BuffType<ExampleMinecartBuff>();
			mountData.flightTimeMax = 0;
			mountData.fallDamage = 1f;
			mountData.runSpeed = 10f;
			mountData.dashSpeed = 10f;
			mountData.acceleration = 0.04f;
			mountData.jumpHeight = 15;
			mountData.jumpSpeed = 5.15f;
			mountData.blockExtraJumps = true;
			mountData.totalFrames = 3;
			mountData.heightBoost = 12;
			mountData.playerYOffsets = array;
			mountData.xOffset = 2;
			mountData.yOffset = 9;
			mountData.bodyFrame = 3;
			mountData.playerHeadOffset = 14;
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
			if (Main.netMode != NetmodeID.Server)
			{
				mountData.textureWidth = mountData.frontTexture.Width;
				mountData.textureHeight = mountData.frontTexture.Height;
			}
		}
	}
}
