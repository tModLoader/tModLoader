using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria
{
	partial class Entity
	{
		// Common

		public IEntitySource GetSource_FromThis()
			=> new EntitySource_Parent(this);

		public IEntitySource GetSource_FromAI()
			=> new EntitySource_Parent(this);

		public IEntitySource GetSource_DropAsItem()
			=> new EntitySource_DropAsItem(this);
		
		public IEntitySource GetSource_Loot()
			=> new EntitySource_Loot(this);

		// Item Use / Equipment Effects

		public IEntitySource GetSource_Accessory(Item item)
			=> new EntitySource_ItemUse(this, item);

		public IEntitySource GetSource_OpenItem(int itemType)
			=> new EntitySource_ItemOpen(this, itemType);
		
		public IEntitySource GetSource_SetBonus(int entitySourceId)
			=> new EntitySource_ByEntitySourceID(this, entitySourceId);

		public IEntitySource GetSource_Item(Item item)
			=> new EntitySource_ItemUse(this, item);

		public IEntitySource GetSource_Item_WithPotentialAmmo(Item item, int ammoItemId)
			=> new EntitySource_ItemUse_WithAmmo(this, item, ammoItemId);

		// Damage / Death
		
		public IEntitySource GetSource_OnHit(Entity victim)
			=> new EntitySource_OnHit(this, victim);
		
		public IEntitySource GetSource_OnHurt(Entity attacker)
			=> new EntitySource_OnHit(attacker, this);

		public IEntitySource GetSource_OnHit(Entity victim, int entitySourceId)
			=> new EntitySource_OnHit_ByEntitySourceID(this, victim, entitySourceId);

		public IEntitySource GetSource_OnHurt(Entity attacker, int entitySourceId)
			=> new EntitySource_OnHit_ByEntitySourceID(attacker, this, entitySourceId);

		public IEntitySource GetSource_Death()
			=> new EntitySource_ByEntitySourceID(this, EntitySourceID.PlayerDeath);
		
		// Etc

		public IEntitySource GetSource_Misc(int entitySourceId)
			=> new EntitySource_ByEntitySourceID(this, entitySourceId);

		public IEntitySource GetSource_TileInteraction(int tileCoordsX, int tileCoordsY)
			=> new EntitySource_TileInteraction(this, tileCoordsX, tileCoordsY);

		public IEntitySource GetSource_ReleaseEntity()
			=> new EntitySource_Parent(this);

		public IEntitySource GetSource_CatchEntity()
			=> new EntitySource_Parent(this);

		// Common - Static

		public static IEntitySource GetSource_None()
			=> null;

		public static IEntitySource InheritSource(Entity entity)
			=> entity != null ? entity.GetSource_FromThis() : GetSource_None();

		// Spawning - Static

		public static IEntitySource GetSource_NaturalSpawn()
			=> new EntitySource_SpawnNPC();
		
		public static IEntitySource GetSource_TownSpawn()
			=> new EntitySource_SpawnNPC();
	}
}
