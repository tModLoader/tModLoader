using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// Keeps track of the modded categories and powers on the Journey Mode power menu.
/// </summary>
public static class CreativeMenuLoader
{
	public static int CategoryCount => CreativeCategoryID.Count + categories.Count;

	public static int PowerCount => powers.Count;

	internal static readonly List<ModCreativeCategory> categories = new List<ModCreativeCategory>();
	internal static readonly List<ModCreativePower> powers = new List<ModCreativePower>();

	private static readonly Dictionary<int, bool> shownCategories = new Dictionary<int, bool>();
	private static readonly HashSet<int> decidingCategories = new HashSet<int>();
	private static readonly HashSet<int> warnedLoopCategories = new HashSet<int>();
	private static bool cacheShownCategories;

	internal static int Add(ModCreativeCategory category)
	{
		categories.Add(category);
		return CategoryCount - 1;
	}

	internal static int Add(ModCreativePower power)
	{
		powers.Add(power);
		return powers.Count - 1;
	}

	public static ModCreativeCategory GetCategory(int type)
	{
		return type >= CreativeCategoryID.Count && type < CategoryCount ? categories[type - CreativeCategoryID.Count] : null;
	}

	public static ModCreativePower GetPower(int type)
	{
		return type >= 0 && type < powers.Count ? powers[type] : null;
	}

	/// <summary>
	/// Whether <paramref name="category"/> is shown on the main power strip. It's hidden if any mod returns false from <see cref="ModSystem.CanShowCreativeCategory"/>.<br/>
	/// Safe to call from <see cref="ModSystem.CanShowCreativeCategory"/>, for example to show a category only while another one is hidden.
	/// While the power menu is being built, each category is decided once and every mod gets the same answer.<br/>
	/// If categories depend on each other in a loop, a category asked about while it's still being decided counts as shown, and a warning is logged, rather than the game crashing.
	/// </summary>
	public static bool IsCategoryShown(int category)
	{
		if (cacheShownCategories && shownCategories.TryGetValue(category, out bool shown))
			return shown;

		if (!decidingCategories.Add(category)) {
			if (warnedLoopCategories.Add(category))
				Logging.tML.Warn($"Journey Mode category {category} depends on itself through {nameof(ModSystem)}.{nameof(ModSystem.CanShowCreativeCategory)}. Treating it as shown to break the loop.");

			return true;
		}

		try {
			shown = SystemLoader.CanShowCreativeCategory(category);
		}
		finally {
			decidingCategories.Remove(category);
		}

		if (cacheShownCategories)
			shownCategories[category] = shown;

		return shown;
	}

	/// <summary>
	/// Decides which categories are shown, once each, for a menu being built. Returns the hidden ones.
	/// </summary>
	internal static List<int> FindHiddenCategories(IEnumerable<int> candidates)
	{
		cacheShownCategories = true;
		try {
			List<int> hidden = new List<int>();
			foreach (int category in candidates) {
				if (!IsCategoryShown(category))
					hidden.Add(category);
			}

			return hidden;
		}
		finally {
			cacheShownCategories = false;
			shownCategories.Clear();
		}
	}

	/// <summary>
	/// Whether modded powers can be placed in <paramref name="category"/>: the main strip (<see cref="CreativeCategoryID.None"/>), vanilla's Time, Weather and Personal strips, or a modded category.
	/// </summary>
	internal static bool CanHoldPowers(int category)
	{
		return category is CreativeCategoryID.None or CreativeCategoryID.Time or CreativeCategoryID.Weather or CreativeCategoryID.Personal || GetCategory(category) != null;
	}

	internal static void Unload()
	{
		// Drop the menu before mods unload, so it no longer references their categories, powers or any changes they made to it.
		// Vanilla builds it again each time a world is entered.
		Main.CreativeMenu.ResetMenu();
		categories.Clear();
		powers.Clear();
		warnedLoopCategories.Clear();
	}
}
