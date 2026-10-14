using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class VanillaChestHealingGlowstickDropRule : IChestLootRule
{
	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		if (TryRoll(random, flag2: false, vampireSeed: false, glowstickType: 282, out int itemType, out int stack))
			chest.AddItem(itemType, stack);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		ReportDropRates(drops, parentDropRate, "AddBuriedChest healing-item or glowstick bonus.");
	}

	internal void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate, string condition)
	{
		foreach (bool vampireSeed in new[] { false, true }) {
			float selectionRate = vampireSeed ? 3f / 8f : 1f / 4f;
			int stackMin = vampireSeed ? 30 : 15;
			int stackMax = vampireSeed ? 60 : 30;
			string seedCondition = vampireSeed ? "Vampire seed" : "non-Vampire world";
			drops.Add(new ChestDropRateInfo(8, stackMin, stackMax, parentDropRate * selectionRate, $"{condition} Healing item selected; flag2 is false; {seedCondition}."));
			drops.Add(new ChestDropRateInfo(974, stackMin, stackMax, parentDropRate * selectionRate, $"{condition} Healing item selected; flag2 is true; {seedCondition}."));
			drops.Add(new ChestDropRateInfo(282, stackMin, stackMax, parentDropRate * selectionRate, $"{condition} Glowsticks selected; Rainbow Glowsticks is disabled; {seedCondition}."));
			drops.Add(new ChestDropRateInfo(5643, stackMin, stackMax, parentDropRate * selectionRate, $"{condition} Glowsticks selected; Rainbow Glowsticks is enabled; {seedCondition}."));
		}
	}

	internal bool TryDropAtSlot(Chest chest, int slot, UnifiedRandom random, bool flag2, bool vampireSeed, int glowstickType)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (chest.item == null || slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));

		if (!TryRoll(random, flag2, vampireSeed, glowstickType, out int itemType, out int stack))
			return false;

		chest.item[slot].SetDefaults(itemType);
		chest.item[slot].stack = stack;
		return true;
	}

	private static bool TryRoll(UnifiedRandom random, bool flag2, bool vampireSeed, int glowstickType, out int itemType, out int stack)
	{
		itemType = ItemID.None;
		stack = 0;
		if (random.Next(2) != 0 && (!vampireSeed || random.Next(2) != 0))
			return false;

		itemType = random.Next(2) == 0 ? (flag2 ? 974 : 8) : glowstickType;
		stack = random.Next(15, 31);
		if (vampireSeed)
			stack *= 2;

		return true;
	}
}