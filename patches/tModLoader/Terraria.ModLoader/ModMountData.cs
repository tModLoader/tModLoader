using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

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

		public virtual bool Autoload(ref string name, ref string texture, IDictionary<MountTextureType, string> extraTextures)
		{
			return mod.Properties.Autoload;
		}

		internal void SetupMount(Mount.MountData mountData)
		{
			ModMountData newMountData = (ModMountData)MemberwiseClone();
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

		public virtual bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
		{
			return true;
		}

		public virtual bool CustomBodyFrame()
		{
			return false;
		}
        public virtual void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
        }
        public virtual bool CanUseAbility(Player player, Vector2 mousePosition, bool toggleOn)
        {
            return true;
        }
    }
}
