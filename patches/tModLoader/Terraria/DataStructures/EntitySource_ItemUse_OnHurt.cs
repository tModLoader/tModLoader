#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Combination of <see cref="IEntitySource_OnHurt"/> and <see cref="IEntitySource_WithStatsFromItem"/> <br/><br/>
///
/// Used for on-hurt accessories in vanilla (Star Cloak, Brain of Confusion etc). <br/>
/// Modders should be aware that it is <b>not</b> necessary to use this instead of <see cref="EntitySource_OnHurt"/> <br/>
/// The combination with <see cref="IEntitySource_WithStatsFromItem"/> will only have an effect if the item has damage/crit/armor pen stats which vanilla accessories do not <br/>
/// Some mods may wish to transfer other accessory bonuses from the item to spawned projectiles.
/// </summary>
public class EntitySource_ItemUse_OnHurt : EntitySource_ItemUse, IEntitySource_OnHurt
{
	public Entity? Attacker { get; }

	public Entity Victim => Entity;

	public EntitySource_ItemUse_OnHurt(Player player, Item item, Entity? attacker, string? context = null) : base(player, item, context)
	{
		Attacker = attacker;
	}
}
