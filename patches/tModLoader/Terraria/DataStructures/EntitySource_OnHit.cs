#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Used along with <see cref="EntitySource_Parent"/>. The <see cref="Attacker"/> is also the <see cref="EntitySource_Parent.Entity"/> (owner of the effect)
/// </summary>
public interface IEntitySource_OnHit
{
	/// <summary>
	/// The attacking entity. Note that this may be a <see cref="Projectile"/> (possibly owned by a player), a <see cref="Player"/> or even a <see cref="NPC"/>
	/// </summary>
	public Entity Attacker { get; }

	/// <summary>
	/// The entity being attacked. Normally an NPC, but could be an Player if a mod decides to use this source in such a way
	/// </summary>
	public Entity Victim { get; }
}

/// <summary>
/// Use the interface, <see cref="IEntitySource_OnHit"/> instead when checking entity sources in <c>OnSpawn</c> <br/><br/>
///
/// Recommend setting <see cref="IEntitySource.Context"/> to indicate the effect. Many vanilla set bonuses or accessories use this source.
/// </summary>
public class EntitySource_OnHit : EntitySource_Parent, IEntitySource_OnHit
{
	public Entity Attacker => Entity;

	public Entity Victim { get; }

	public EntitySource_OnHit(Entity attacker, Entity victim, string? context = null) : base(attacker, context)
	{
		Victim = victim;
	}
}
