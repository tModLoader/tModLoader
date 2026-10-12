using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> Adds one item type with a configurable chance and stack range. </summary>
public sealed class CommonChestDropRule : IChestLootRule
{
	/// <summary> The item type produced by the rule. </summary>
	public int ItemType { get; }
	/// <summary> The chance numerator. </summary>
	public int ChanceNumerator { get; }
	/// <summary> The chance denominator. </summary>
	public int ChanceDenominator { get; }
	/// <summary> The minimum stack size. </summary>
	public int StackMin { get; }
	/// <summary> The maximum stack size. </summary>
	public int StackMax { get; }
	internal bool SuccessOnNonZero { get; }

	/// <summary> Creates a common item rule. The chance is numerator divided by denominator, and stack bounds are inclusive. </summary>
	public CommonChestDropRule(int itemType, int chanceDenominator = 1, int stackMin = 1, int stackMax = 1, int chanceNumerator = 1, bool successOnNonZero = false)
	{
		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(itemType));
		if (chanceDenominator <= 0)
			throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
		if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
			throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
		if (stackMin <= 0 || stackMax < stackMin)
			throw new ArgumentOutOfRangeException(nameof(stackMin));

		ItemType = itemType;
		ChanceNumerator = chanceNumerator;
		ChanceDenominator = chanceDenominator;
		StackMin = stackMin;
		StackMax = stackMax;
		SuccessOnNonZero = successOnNonZero;
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (!RollSucceeded(random))
			return;

		chest.AddItem(ItemType, NextStack(random));
	}

	internal bool TryDropAtFirstEmptySlot(Chest chest, UnifiedRandom random, int requiredItemSlot = 1)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		// Vanilla rolls before checking the source slot; preserve its random-number consumption order.
		if ((ChanceNumerator != ChanceDenominator || SuccessOnNonZero) && !RollSucceeded(random))
			return false;
		if (chest.item == null || requiredItemSlot < 0 || requiredItemSlot >= chest.item.Length || chest.item[requiredItemSlot] == null || chest.item[requiredItemSlot].stack == 0)
			return false;

		int itemCount = Math.Min(chest.maxItems, chest.item.Length);
		for (int i = 1; i < itemCount; i++) {
			Item chestItem = chest.item[i];
			if (chestItem == null || chestItem.stack != 0)
				continue;

			chestItem.SetDefaults(ItemType);
			if (StackMin != 1 || StackMax != 1)
				chestItem.stack = NextStack(random);

			return true;
		}

		return false;
	}

	internal bool TryDropAtSlot(Chest chest, int slot, UnifiedRandom random, bool stackBeforeSetDefaults = false, bool applyRandomPrefix = false)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if ((ChanceNumerator != ChanceDenominator || SuccessOnNonZero) && !RollSucceeded(random))
			return false;

		SetItemAtSlot(chest, slot, random, stackBeforeSetDefaults, applyRandomPrefix);
		return true;
	}

	internal void SetItemAtSlot(Chest chest, int slot, UnifiedRandom random, bool stackBeforeSetDefaults = false, bool applyRandomPrefix = false)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (chest.item == null)
			throw new InvalidOperationException("The chest has no item slots.");
		if (slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));

		bool hasVariableStack = StackMin != 1 || StackMax != 1;
		int stack = hasVariableStack && stackBeforeSetDefaults ? NextStack(random) : 1;
		chest.item[slot].SetDefaults(ItemType);
		if (hasVariableStack) {
			if (!stackBeforeSetDefaults)
				stack = NextStack(random);

			chest.item[slot].stack = stack;
		}

		if (applyRandomPrefix)
			chest.item[slot].Prefix(-1);
	}

	private bool RollSucceeded(UnifiedRandom random)
	{
		int roll = random.Next(ChanceDenominator);
		return SuccessOnNonZero ? roll != 0 : roll < ChanceNumerator;
	}

	internal bool RollChance(UnifiedRandom random) => RollSucceeded(random);

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
		=> ReportDropRates(drops, parentDropRate, null);

	internal void ReportDropRates(List<ChestDropRateInfo> drops, string condition)
		=> ReportDropRates(drops, 1f, condition);

	private void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate, string condition)
	{
		ArgumentNullException.ThrowIfNull(drops);
		drops.Add(new ChestDropRateInfo(ItemType, StackMin, StackMax, parentDropRate * SuccessfulRollCount / ChanceDenominator, condition));
	}

	private int SuccessfulRollCount => SuccessOnNonZero ? ChanceDenominator - 1 : ChanceNumerator;

	private int NextStack(UnifiedRandom random)
	{
		if (StackMax < int.MaxValue)
			return random.Next(StackMin, StackMax + 1);

		long range = (long)StackMax - StackMin + 1;
		long offset = Math.Min((long)(random.NextDouble() * range), range - 1);
		return (int)(StackMin + offset);
	}
}