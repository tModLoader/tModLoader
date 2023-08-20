using System.Collections.Concurrent;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

public static class ItemDropRuleExtensions
{
	private static readonly ConcurrentDictionary<IItemDropRule, Ref<bool>> disableableDropRules = new();

	public static bool IsDisabled(this IItemDropRule dropRule)
	{
		return disableableDropRules.GetValueOrDefault(dropRule)?.Value ?? false;
	}

	public static void Disable(this IItemDropRule dropRule)
	{
		disableableDropRules.TryAdd(dropRule, new Ref<bool>(true));
	}

	internal static void Clear()
	{
		disableableDropRules.Clear();
	}
}
