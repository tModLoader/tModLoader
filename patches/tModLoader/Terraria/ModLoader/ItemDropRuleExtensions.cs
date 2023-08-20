using System.Collections.Concurrent;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

public static class ItemDropRuleExtensions
{
	private sealed class ItemDropRuleSettings
	{
		public bool Disabled { get; set; }
		// TODO: Add conditions list.
	}

	private static readonly ConcurrentDictionary<IItemDropRule, ItemDropRuleSettings> disableableDropRules = new();

	public static bool IsDisabled(this IItemDropRule dropRule)
	{
		return disableableDropRules.GetValueOrDefault(dropRule)?.Disabled ?? false;
	}

	public static void Disable(this IItemDropRule dropRule)
	{
		disableableDropRules.TryAdd(dropRule, new ItemDropRuleSettings { Disabled = true });
	}

	internal static void Clear()
	{
		disableableDropRules.Clear();
	}
}
