using System;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalProjectile
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

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults(Projectile projectile)
		{
		}
	}
}
