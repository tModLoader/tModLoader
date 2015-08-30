using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Terraria.ModLoader
{
	public class ModMountData
	{
		//public Texture2D backTexture;
		//public Texture2D backTextureGlow;
		//public Texture2D backTextureExtra;
		//public Texture2D backTextureExtraGlow;
		//public Texture2D frontTexture;
		//public Texture2D frontTextureGlow;
		//public Texture2D frontTextureExtra;
		//public Texture2D frontTextureExtraGlow;
		//public int textureWidth;
		//public int textureHeight;
		//public int xOffset;
		//public int yOffset;
		//public int bodyFrame;
		//public int playerHeadOffset;
		//public int heightBoost;
		//public int buff;
		//public int extraBuff;
		//public int flightTimeMax;
		//public float fallDamage;
		//public bool usesHover;
		//public float runSpeed;
		//public float dashSpeed;
		//public float swimSpeed;
		//public float acceleration;
		//public float jumpSpeed;
		//public int jumpHeight;
		//public int index;
		//public int spawnDust;
		//public int[] array;
		//public int totalFrames;
		//public int standingFrameStart;
		//public int standingFrameCount;
		//public int standingFrameDelay;
		//public int runningFrameStart;
		//public int runningFrameCount;
		//public int runningFrameDelay;
		//public int flyingFrameStart;
		//public int flyingFrameCount;
		//public int flyingFrameDelay;
		//public int inAirFrameStart;
		//public int inAirFrameCount;
		//public int inAirFrameDelay;
		//public int idleFrameStart;
		//public int idleFrameCount;
		//public int idleFrameDelay;
		//public bool idleFrameLoop;
		//public int swimFrameStart;
		//public int swimFrameCount;
		//public int swimFrameDelay;
		//public int dashingFrameStart;
		//public int dashingFrameCount;
		//public int dashingFrameDelay;

		internal string texture;

		public Mount.MountData mountData
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}
		//public string Name
		//{
		//    get;
		//    set;
		//}
		public ModMountData()
		{
			mountData = new Mount.MountData();
		}

		public virtual bool Autoload(ref string name, ref string textures)
		{
			return mod.Properties.Autoload;
		}

		internal void SetupMount(Mount.MountData mountData)
		{
			ErrorLogger.Log("SetupMount Start");
			ModMountData newMountData = (ModMountData)Activator.CreateInstance(GetType());
			newMountData.mountData = mountData;
			mountData.modMountData = newMountData;
			newMountData.mod = mod;
			newMountData.SetDefaults();
			ErrorLogger.Log("SetupMount End");
		}

		public virtual void SetDefaults()
		{
		}
        
	}
}
