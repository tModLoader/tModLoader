#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Used along with <see cref="EntitySource_Parent"/>. The <see cref="Victim"/> is also the <see cref="EntitySource_Parent.Entity"/> (owner of the effect)
/// </summary>
public interface IEntitySource_OnHurt
{
	/// <summary>
	/// The attacking entity. Note that this may be a <see cref="Projectile"/> (possibly owned by a player), a <see cref="Player"/> or even a <see cref="NPC"/>
	/// </summary>
	public Entity? Attacker { get; }

	/// <summary>
	/// The entity being attacked. Normally a Player, but could be an NPC if a mod decides to use this source in such a way
	/// </summary>
	public Entity Victim { get; }
}

/// <summary>
/// Use the interface, <see cref="IEntitySource_OnHurt"/> instead when checking entity sources in <c>OnSpawn</c> <br/><br/>
/// 
/// Recommend setting <see cref="IEntitySource.Context"/> to indicate the effect.
/// </summary>
public class EntitySource_OnHurt : EntitySource_Parent, IEntitySource_OnHurt
{
	public Entity? Attacker { get; }

	public Entity Victim => Entity;

	public EntitySource_OnHurt(Entity victim, Entity? attacker, string? context = null) : base(victim, context)
	{
		Attacker = attacker;
	}
}
