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
	public NPC NPCBoughtFrom;

	/// <summary>
	/// The player that bought this item.
	/// </summary>
	public Player PlayerCustomer;

	public BuyItemCreationContext(Item destinationStack, NPC npcBoughtFrom, Player playerCustomer)
	{
		DestinationStack = destinationStack;
		NPCBoughtFrom = npcBoughtFrom;
		PlayerCustomer = playerCustomer;
	}
}