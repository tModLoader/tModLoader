namespace Terraria.DataStructures;

#nullable enable

/// <summary>
/// Used when attempting to add an item to the player's inventory, but it cannot fit so it spawns in the world instead. <br/>
/// Used in vanilla when a fished item can't fit in the player's inventory.
/// </summary>
public class EntitySource_OverfullInventory : IEntitySource
{
	public Player Player { get; }

	public string? Context { get; }

	public EntitySource_OverfullInventory(Player player, string? context = null)
	{
		Player = player;
		Context = context;
	}
}
