#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Used when an an 'on-hurt' effect is triggered. The <see cref="Victim"/> is also the <see cref="EntitySource_Parent.Entity"/> (owner of the effect)<br/><br/>
///
/// Recommend setting <see cref="IEntitySource.Context"/> to indicate the effect. Many vanilla set bonuses or accessories use this source.
/// </summary>
public class EntitySource_OnHurt : EntitySource_Parent
{
	/// <summary>
	/// The attacking entity. Note that this may be a <see cref="Projectile"/> (possibly owned by a player), a <see cref="Player"/> or even a <see cref="NPC"/>
	/// </summary>
	public Entity Attacker { get; }

	/// <summary>
	/// The entity being attacked. Nnormally a Player, but could be an NPC if a mod decides to use this source in such a way
	/// </summary>
	public Entity Victim => Entity;

	public EntitySource_OnHurt(Entity victim, Entity attacker, string? context = null) : base(victim, context)
	{
		Attacker = attacker;
	}
}
