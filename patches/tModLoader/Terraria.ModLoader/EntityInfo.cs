using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// There are three classes called ItemInfo, ProjectileInfo, and NPCInfo. You can override any of these three classes to add extra information to items, projectiles, or NPCs respectively, similar to how ModPlayer can be used to add extra information to players.
	/// </summary>
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

	/// <summary>
	/// This class serves as a way of adding custom info to Items. Every item will be instantiated with a list of instances of each ProjectileInfo loaded and can be accessed through Item.GetModInfo method calls
	/// </summary>
	/// <seealso cref="Terraria.ModLoader.EntityInfo" />
	public class ItemInfo : EntityInfo
	{
		/// <summary>
		/// Returns a clone of this ItemInfo. By default this will return a memberwise clone; you will want to override this if your ItemInfo contains object references.
		/// </summary>
		/// <returns></returns>
		public virtual ItemInfo Clone()
		{
			return (ItemInfo)MemberwiseClone();
		}
	}

	/// <summary>
	/// This class serves as a way of adding custom info to Projectiles. Every projectile will be instantiated with a list of instances of each ProjectileInfo loaded and can be accessed through Projectile.GetModInfo method calls
	/// </summary>
	/// <seealso cref="Terraria.ModLoader.EntityInfo" />
	public class ProjectileInfo : EntityInfo
	{
		/// <summary>
		/// Returns a clone of this ProjectileInfo. By default this will return a memberwise clone; you will want to override this if your ProjectileInfo contains object references.
		/// </summary>
		/// <returns></returns>
		public virtual ProjectileInfo Clone()
		{
			return (ProjectileInfo)MemberwiseClone();
		}
	}

	/// <summary>
	/// This class serves as a way of adding custom info to NPC. Every npc will be instantiated with a list of instances of each ProjectileInfo loaded and can be accessed through NPC.GetModInfo method calls
	/// </summary>
	/// <seealso cref="Terraria.ModLoader.EntityInfo" />
	public class NPCInfo : EntityInfo
	{
		/// <summary>
		/// Returns a clone of this NPCInfo. By default this will return a memberwise clone; you will want to override this if your NPCInfo contains object references.
		/// </summary>
		/// <returns></returns>
		public virtual NPCInfo Clone()
		{
			return (NPCInfo)MemberwiseClone();
		}
	}
}
