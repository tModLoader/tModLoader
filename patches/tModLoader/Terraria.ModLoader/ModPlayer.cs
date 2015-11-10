using System;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModPlayer
	{
		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public Player player
		{
			get;
			internal set;
		}

		internal ModPlayer Clone()
		{
			return (ModPlayer)MemberwiseClone();
		}

		public bool TypeEquals(ModPlayer other)
		{
			return mod == other.mod && Name == other.Name;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void ResetEffects()
		{
		}

		public virtual void UpdateDead()
		{
		}

		public virtual void SaveCustomData(BinaryWriter writer)
		{
		}

		public virtual void LoadCustomData(BinaryReader reader)
		{
		}

		public virtual void SetupStartInventory(IList<Item> items)
		{
		}

		public virtual void UpdateBiomes()
		{
		}

		public virtual void UpdateBiomeVisuals()
		{
		}
		//TODO
		//OnFishSelected, GetFishingLevel, GetAnglerReward, GetDyeTraderReward
		//hooks for grappling hooks
	}
}
