using Terraria.DataStructures;

#nullable enable

namespace Terraria;

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

	// Damage / Death
	
	public IEntitySource GetSource_OnHit(Entity victim, string? context = null)
		=> new EntitySource_OnHit(attacker: this, victim, context);
	
	public IEntitySource GetSource_OnHurt(Entity? attacker, string? context = null)
		=> new EntitySource_OnHurt(victim: this, attacker, context);

	public IEntitySource GetSource_Death(string? context = null)
		=> new EntitySource_Death(this, context);
	
	// Etc

	public IEntitySource GetSource_Misc(string context)
		=> new EntitySource_Misc(/*this,*/ context);

	public IEntitySource GetSource_TileInteraction(int tileCoordsX, int tileCoordsY, string? context = null)
		=> new EntitySource_TileInteraction(this, tileCoordsX, tileCoordsY, context);

	public IEntitySource GetSource_ReleaseEntity(string? context = null)
		=> new EntitySource_Parent(this, context);

	public IEntitySource GetSource_CatchEntity(Entity caughtEntity, string? context = null)
		=> new EntitySource_Caught(this, caughtEntity, context);

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
