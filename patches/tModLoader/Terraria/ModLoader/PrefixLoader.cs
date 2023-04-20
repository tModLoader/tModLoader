using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.Utilities;

namespace Terraria.ModLoader;

public static class PrefixLoader
{
	// TODO storing twice? could see a better implementation
	internal static readonly IList<ModPrefix> prefixes = new List<ModPrefix>();
	internal static readonly IDictionary<PrefixCategory, List<ModPrefix>> categoryPrefixes;

	public static int PrefixCount { get; private set; } = PrefixID.Count;

	static PrefixLoader()
	{
		categoryPrefixes = new Dictionary<PrefixCategory, List<ModPrefix>>();

		foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory))) {
			categoryPrefixes[category] = new List<ModPrefix>();
		}
	}

	internal static void RegisterPrefix(ModPrefix prefix)
	{
		prefixes.Add(prefix);
		categoryPrefixes[prefix.Category].Add(prefix);
	}

	internal static int ReservePrefixID()
	{
		if (ModNet.AllowVanillaClients)
			throw new Exception("Adding items breaks vanilla client compatibility");

		return PrefixCount++;
	}

	/// <summary>
	/// Returns the ModPrefix associated with specified type
	/// If not a ModPrefix, returns null.
	/// </summary>
	public static ModPrefix GetPrefix(int type)
		=> type >= PrefixID.Count && type < PrefixCount ? prefixes[type - PrefixID.Count] : null;

	/// <summary>
	/// Returns a list of all modded prefixes of a certain category.
	/// </summary>
	public static IReadOnlyList<ModPrefix> GetPrefixesInCategory(PrefixCategory category)
		=> categoryPrefixes[category];

	internal static void ResizeArrays()
	{
		//Sets
		LoaderUtils.ResetStaticMembers(typeof(PrefixID));

		//Etc
		Array.Resize(ref Lang.prefix, PrefixCount);
	}

	internal static void FinishSetup()
	{
		foreach (ModPrefix prefix in prefixes) {
			Lang.prefix[prefix.Type] = prefix.DisplayName;
		}
	}

	internal static void Unload()
	{
		prefixes.Clear();

		PrefixCount = PrefixID.Count;

		foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory))) {
			categoryPrefixes[category].Clear();
		}
	}

	public static bool CanRoll(Item item, int prefix)
	{
		if (!ItemLoader.AllowPrefix(item, prefix))
			return false;

		if (GetPrefix(prefix) is ModPrefix modPrefix) {
			if (!modPrefix.CanRoll(item))
				return false;

			if (modPrefix.Category is PrefixCategory.Custom)
				return true;

			return item.GetPrefixCategory() is PrefixCategory itemCategory && (modPrefix.Category == itemCategory || itemCategory == PrefixCategory.AnyWeapon && IsWeaponSubCategory(modPrefix.Category));
		}

		if (item.GetPrefixCategory() is PrefixCategory category) {
			if (Item.GetVanillaPrefixes(category).Contains(prefix))
				return true;
		}

		return false;
	}

	public static bool Roll(Item item, UnifiedRandom unifiedRandom, out int prefix, bool justCheck)
	{
		if (ItemLoader.ChoosePrefix(item, unifiedRandom) is int forcedPrefix && forcedPrefix > 0 && CanRoll(item, forcedPrefix)) {
			prefix = forcedPrefix;
			return true;
		}

		prefix = 0;
		var wr = new WeightedRandom<int>(unifiedRandom);

		void AddCategory(PrefixCategory category) {
			foreach (ModPrefix modPrefix in categoryPrefixes[category].Where(x => x.CanRoll(item))) {
				wr.Add(modPrefix.Type, modPrefix.RollChance(item));
			}
		}

		if (item.GetPrefixCategory() is not PrefixCategory category)
			return false;

		if (justCheck)
			return true; // if it has a category, there are probably prefixes in that category...

		foreach (int pre in Item.GetVanillaPrefixes(category))
			wr.Add(pre, 1);

		AddCategory(category);
		if (IsWeaponSubCategory(category))
			AddCategory(PrefixCategory.AnyWeapon);

		// try 50 times
		for (int i = 0; i < 50; i ++) {
			prefix = wr.Get();
			if (ItemLoader.AllowPrefix(item, prefix))
				return true;
		}

		return false;
	}

	public static bool IsWeaponSubCategory(PrefixCategory category) => category is PrefixCategory.Melee || category is PrefixCategory.Ranged || category is PrefixCategory.Magic;

	public static void ApplyAccessoryEffects(Player player, Item item)
	{
		if (GetPrefix(item.prefix) is ModPrefix prefix) {
			prefix.ApplyAccessoryEffects(player);
		}

		// Should there be more here for tooltips? Not entirely sure how that's handled in tML. - Mutant
	}
}
