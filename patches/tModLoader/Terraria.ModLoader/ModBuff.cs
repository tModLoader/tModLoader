using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModBuff
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

		public int Type
		{
			get;
			internal set;
		}

		internal string texture;

		public ModBuff()
		{
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		//internal void SetupBuff(Projectile projectile)
		//{
		//    ModProjectile newProjectile = (ModProjectile)Activator.CreateInstance(GetType());
		//    newProjectile.projectile = projectile;
		//    projectile.modProjectile = newProjectile;
		//    newProjectile.mod = mod;
		//    newProjectile.SetDefaults();
		//}
		public virtual void UpdateBuffs(int k, int i)
		{
		}

		public virtual void SetDefaults()
		{
		}
	}
}
