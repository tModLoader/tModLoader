namespace Terraria.GameContent.Creative;

public partial class CreativeUI
{
	public static ItemSacrificeResult ResearchItem(int type)
	{
		if (!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(type, out int amountNeeded))
			return ItemSacrificeResult.CannotSacrifice;

		return SacrificeItem(new Item(type, amountNeeded), out _);
	}

	/// <summary>
	/// Method that allows you to easily get how many items of a type you have researched so far
	/// </summary>
	/// <param name="type">The item type to check.</param>
	/// <param name="fullyResearched">True if it is fully researched.</param>
	/// <returns></returns>
	public static int GetSacrificeCount(int type, out bool fullyResearched)
	{
		fullyResearched = false;

		if (!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(type, out int amountNeeded))
			return 0;

		Main.LocalPlayerCreativeTracker.ItemSacrifices._sacrificesCountByItemIdCache.TryGetValue(type, out int amountSacrificed);

		fullyResearched = amountSacrificed >= amountNeeded;

		return amountSacrificed;
	}

	/// <summary>
	/// Method that allows you to easily get how many items of a type you need to fully research that item
	/// </summary>
	/// <param name="type">The item type to check.</param>
	/// <returns>The number of sacrifices remaining , or null if the item can never be unlocked.</returns>
	public static int? GetSacrificesRemaining(int type)
	{
		if (!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(type, out int amountNeeded))
			return null;

		Main.LocalPlayerCreativeTracker.ItemSacrifices._sacrificesCountByItemIdCache.TryGetValue(type, out int amountSacrificed);

		return Utils.Clamp(amountNeeded - amountSacrificed, 0, amountNeeded);
	}
}
