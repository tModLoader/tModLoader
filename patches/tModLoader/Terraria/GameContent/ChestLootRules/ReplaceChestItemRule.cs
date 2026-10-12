using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class ReplaceChestItemRule : IChestLootRule
{
	private readonly int _oldItemType;

	private readonly int _newItemType;

	internal ReplaceChestItemRule(int oldItemType, int newItemType)
	{
		ChestLootRuleUtilities.ValidateItemType(oldItemType, nameof(oldItemType));
		ChestLootRuleUtilities.ValidateItemType(newItemType, nameof(newItemType));
		_oldItemType = oldItemType;
		_newItemType = newItemType;
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		chest.ReplaceItem(_oldItemType, _newItemType);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
		=> drops.Add(new ChestDropRateInfo(_newItemType, 1, 1, parentDropRate, $"Replaces item {_oldItemType} when that item is present and the replacement fits."));

}