using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class VanillaChestStackBonusDropRule : IChestLootRule
{
	private readonly (int ChanceDenominator, int BonusMax)[] _bonuses;

	internal int ItemType { get; }
	internal int ChanceDenominator { get; }

	private int BaseStack { get; }

	internal VanillaChestStackBonusDropRule(int itemType, int chanceDenominator, int baseStack, params (int ChanceDenominator, int BonusMax)[] bonuses)
	{
		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(itemType));
		if (chanceDenominator <= 0)
			throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
		if (baseStack <= 0)
			throw new ArgumentOutOfRangeException(nameof(baseStack));
		if (bonuses == null)
			throw new ArgumentNullException(nameof(bonuses));
		foreach ((int bonusChance, int bonusMax) in bonuses) {
			if (bonusChance <= 0)
				throw new ArgumentOutOfRangeException(nameof(bonuses));
			if (bonusMax <= 0)
				throw new ArgumentOutOfRangeException(nameof(bonuses));
		}

		ItemType = itemType;
		ChanceDenominator = chanceDenominator;
		BaseStack = baseStack;
		_bonuses = ((int ChanceDenominator, int BonusMax)[])bonuses.Clone();
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (random.Next(ChanceDenominator) != 0)
			return;

		chest.AddItem(ItemType, RollStack(random));
	}

	internal bool TryDropAtSlot(Chest chest, int slot, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (random.Next(ChanceDenominator) != 0)
			return false;
		if (chest.item == null || slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));

		chest.item[slot].SetDefaults(ItemType);
		chest.item[slot].stack = RollStack(random);
		return true;
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
		=> ReportDropRates(drops, parentDropRate, null);

	internal void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate, string condition)
	{
		Dictionary<int, float> stackRates = new() { [BaseStack] = 1f };
		foreach ((int bonusChance, int bonusMax) in _bonuses) {
			Dictionary<int, float> updatedRates = new();
			foreach ((int stack, float rate) in stackRates) {
				AddRate(updatedRates, stack, rate * (bonusChance - 1f) / bonusChance);
				float bonusRate = rate / bonusChance / bonusMax;
				for (int bonus = 0; bonus < bonusMax; bonus++)
					AddRate(updatedRates, stack + bonus, bonusRate);
			}

			stackRates = updatedRates;
		}

		float itemChance = parentDropRate / ChanceDenominator;
		foreach ((int stack, float rate) in stackRates)
			drops.Add(new ChestDropRateInfo(ItemType, stack, stack, itemChance * rate, condition));
	}

	private int RollStack(UnifiedRandom random)
	{
		int stack = BaseStack;
		foreach ((int bonusChance, int bonusMax) in _bonuses) {
			if (random.Next(bonusChance) == 0)
				stack += random.Next(bonusMax);
		}

		return stack;
	}

	private static void AddRate(Dictionary<int, float> rates, int stack, float rate)
	{
		rates.TryGetValue(stack, out float currentRate);
		rates[stack] = currentRate + rate;
	}
}