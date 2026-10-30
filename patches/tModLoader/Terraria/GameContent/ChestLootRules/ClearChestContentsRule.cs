using System;
using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class ClearChestContentsRule : IChestLootRule
{
	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		chest.ClearItems();
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
	}
}