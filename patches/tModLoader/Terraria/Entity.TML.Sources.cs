using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace Terraria
{
	partial class Entity
	{
		// Common

		public IEntitySource GetSource_FromThis(string? context = null)
			=> new EntitySource_Parent(this, context);

		public IEntitySource GetSource_FromAI(string? context = null)
			=> new EntitySource_Parent(this, context);

		public IEntitySource GetSource_DropAsItem(string? context = null)
			=> new EntitySource_DropAsItem(this, context);
		
		public IEntitySource GetSource_Loot(string? context = null)
			=> new EntitySource_Loot(this, context);

		public IEntitySource GetSource_GiftOrReward(string? context = null)
			=> new EntitySource_Gift(this, context);

		// Item Use / Equipment Effects

		public IEntitySource GetSource_Accessory(Item item, string? context = null)
			=> new EntitySource_ItemUse(this, item, context);

		public IEntitySource GetSource_OpenItem(int itemType, string? context = null)
			=> new EntitySource_ItemOpen(this, itemType, context);
		
		public IEntitySource GetSource_ItemUse(Item item, string? context = null)
			=> new EntitySource_ItemUse(this, item, context);

		public IEntitySource GetSource_ItemUse_WithPotentialAmmo(Item item, int ammoItemId, string? context = null)
			=> new EntitySource_ItemUse_WithAmmo(this, item, ammoItemId, context);

		// Damage / Death
		
		public IEntitySource GetSource_OnHit(Entity victim, string? context = null)
			=> new EntitySource_OnHit(this, victim, context);
		
		public IEntitySource GetSource_OnHurt(Entity attacker, string? context = null)
			=> new EntitySource_OnHit(attacker, this, context);

		public IEntitySource GetSource_Death(string? context = null)
			=> new EntitySource_Death(this, context);
		
		// Etc

		public IEntitySource GetSource_Misc(string context)
			=> new EntitySource_Misc(/*this,*/ context);

		public IEntitySource GetSource_TileInteraction(int tileCoordsX, int tileCoordsY, string? context = null)
			=> new EntitySource_TileInteraction(this, tileCoordsX, tileCoordsY, context);

		public IEntitySource GetSource_ReleaseEntity(string? context = null)
			=> new EntitySource_Parent(this, context);

		public IEntitySource GetSource_CatchEntity(string? context = null)
			=> new EntitySource_Parent(this, context);

		// Common - Static

		public static IEntitySource? GetSource_None()
			=> null;

		public static IEntitySource? InheritSource(Entity entity)
			=> entity != null ? entity.GetSource_FromThis() : GetSource_None();

		// Spawning - Static

		public static IEntitySource GetSource_NaturalSpawn()
			=> new EntitySource_SpawnNPC();
		
		public static IEntitySource GetSource_TownSpawn()
			=> new EntitySource_SpawnNPC();
	}
}
