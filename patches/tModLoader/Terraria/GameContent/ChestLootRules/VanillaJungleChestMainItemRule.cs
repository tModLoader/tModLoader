using System;
using System.Collections.Generic;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class VanillaJungleChestMainItemRule : IChestLootRule
{
	private static readonly int[] _cycleItems = { 211, 212, 213, 964 };

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		chest.AddItem(RollItem(random, GenVars.JungleItemCount));
		GenVars.JungleItemCount++;
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		const float baseItemRate = 133f / 150f;
		for (int i = 0; i < _cycleItems.Length; i++)
			drops.Add(new ChestDropRateInfo(_cycleItems[i], 1, 1, parentDropRate * baseItemRate, $"Jungle shrine chest main item when JungleItemCount % 4 == {i} and neither rare override succeeds."));

		drops.Add(new ChestDropRateInfo(2292, 1, 1, parentDropRate / 15f, "Jungle shrine chest main item when the first rare override succeeds."));
		drops.Add(new ChestDropRateInfo(3017, 1, 1, parentDropRate * 7f / 150f, "Jungle shrine chest main item when the first rare override fails and the second succeeds."));
	}

	internal int RollItem(UnifiedRandom random, int jungleItemCount)
	{
		int itemType = _cycleItems[jungleItemCount % _cycleItems.Length];
		if (random.Next(15) == 0)
			return 2292;
		if (random.Next(20) == 0)
			return 3017;

		return itemType;
	}
}