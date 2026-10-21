using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

// Shared validation for chest loot rules.
internal static class ChestLootRuleUtilities
{
	internal static void ValidateArguments(Chest chest, UnifiedRandom random)
	{
		if (chest == null)
			throw new ArgumentNullException(nameof(chest));
		if (random == null)
			throw new ArgumentNullException(nameof(random));
	}

	internal static void ValidateItemType(int itemType, string paramName)
	{
		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(paramName);
	}
}