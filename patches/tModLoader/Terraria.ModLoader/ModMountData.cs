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

		public int Type
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

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
			
			ModMountData newMountData = (ModMountData)Activator.CreateInstance(GetType());
			newMountData.mountData = mountData;
			mountData.modMountData = newMountData;
			newMountData.mod = mod;
			newMountData.SetDefaults();
			
		}

		public virtual void SetDefaults()
		{
		}

		public virtual void UpdateEffects(Player player)
		{
            
		}
        
	}
}
