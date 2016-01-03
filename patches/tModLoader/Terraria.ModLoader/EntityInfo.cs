using System;

namespace Terraria.ModLoader
{
	public class EntityInfo
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
	}

	public class ItemInfo : EntityInfo
	{
		internal ItemInfo Clone()
		{
			return (ItemInfo)MemberwiseClone();
		}
	}

	public class ProjectileInfo : EntityInfo
	{
		internal ProjectileInfo Clone()
		{
			return (ProjectileInfo)MemberwiseClone();
		}
	}

	public class NPCInfo : EntityInfo
	{
		internal NPCInfo Clone()
		{
			return (NPCInfo)MemberwiseClone();
		}
	}
}
