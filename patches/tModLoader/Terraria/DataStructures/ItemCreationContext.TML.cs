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