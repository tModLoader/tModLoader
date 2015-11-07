using System;
using System.IO;
using Microsoft.Xna.Framework;
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

		public virtual void SaveCustomData(BinaryWriter writer)
		{
		}

		public virtual void LoadCustomData(BinaryReader reader)
		{
		}

		public virtual void OnHitNPC(float x, float y, Entity victim)
		{
		}

		public virtual bool CanHitNPC(NPC npc)
		{
			return true;
		}

		public virtual void UpdateBiomes()
		{
		}

		public virtual void UpdateBiomeVisuals(string biomeName, bool inZone, Vector2 activationSource)
		{
		}

		public virtual void UpdateDead()
		{
		}
	}
}
