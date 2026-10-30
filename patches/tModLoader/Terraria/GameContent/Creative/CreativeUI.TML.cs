using Terraria.UI;

namespace Terraria.GameContent.Creative;

public partial class CreativeUI
{
	/// <summary>
	/// The main category selected in the power menu, or 0 if none is. Vanilla builds a new menu, with nothing selected, after each world is entered.
	/// </summary>
	internal int CurrentMainCategory => _initialized ? _uiState?.CurrentMainCategory ?? 0 : 0;

	/// <summary>
	/// Discards the power menu so it's built again on the next draw. Used when mods unload, so the menu doesn't keep references to their content.
	/// </summary>
	internal void ResetMenu()
	{
		// UserInterface keeps previous states in its history, so replace it rather than only clearing its state.
		_powersUI.SetState(null);
		_powersUI = new UserInterface();
		_uiState = null;
		_initialized = false;
	}

	public static ItemSacrificeResult ResearchItem(int type)
	{
		if (!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(type, out int amountNeeded))
			return ItemSacrificeResult.CannotSacrifice;

		var item = new Item(type, amountNeeded);
		return Main.CreativeMenu.SacrificeItem(ref item, out _);
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
