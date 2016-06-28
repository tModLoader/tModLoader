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
		public virtual ItemInfo Clone()
		{
			return (ItemInfo)MemberwiseClone();
		}
	}

	public class ProjectileInfo : EntityInfo
	{
		public virtual ProjectileInfo Clone()
		{
			return (ProjectileInfo)MemberwiseClone();
		}
	}

	public class NPCInfo : EntityInfo
	{
		public virtual NPCInfo Clone()
		{
			return (NPCInfo)MemberwiseClone();
		}
	}
}
