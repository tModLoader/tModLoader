namespace Terraria.DataStructures;

// Various additional ItemCreationContext classes for tML purposes.

/// <summary>
/// Created in the context of a player buying an item from a shop. Very similar to <seealso cref="RecipeItemCreationContext"/> in
/// functionality.
/// </summary>
public class BuyItemCreationContext : ItemCreationContext
{
	/// <summary>
	/// An item stack that the bought item will be combined with (via OnStack).
	/// </summary>
	public Item DestinationStack;

	/// <summary>
	/// The NPC that this item was bought from.
	/// </summary>
	public NPC VendorNPC;

	public BuyItemCreationContext(Item destinationStack, NPC vendorNPC)
	{
		DestinationStack = destinationStack;
		VendorNPC = vendorNPC;
	}
}

/// <summary>
/// Used when an item is given directly to the player's inventory, possibly overflowing into the world if the inventory is full.<br/>
/// Typical uses include opening loot bags, fishing, and gifts/rewards from npcs
/// Called from <see cref="Player.QuickSpawnItem(IEntitySource, Item, GetItemSettings)"/>
/// </summary>
public class GivePlayerCreationContext : ItemCreationContext
{
	/// <summary>
	/// The source for creating the item
	/// </summary>
	public IEntitySource Source;

	public GivePlayerCreationContext(IEntitySource source)
	{
		Source = source;
	}
}