using System;
using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class RemoveChestItemRule : IChestLootRule
{
	private readonly int _itemType;

	private readonly int _stack;

	internal RemoveChestItemRule(int itemType, int stack)
	{
		ChestLootRuleUtilities.ValidateItemType(itemType, nameof(itemType));
		if (stack <= 0)
			throw new ArgumentOutOfRangeException(nameof(stack));

		_itemType = itemType;
		_stack = stack;
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		chest.RemoveItem(_itemType, _stack);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
	}
}