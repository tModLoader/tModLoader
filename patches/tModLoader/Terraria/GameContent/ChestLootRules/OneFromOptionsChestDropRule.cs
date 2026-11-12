using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> Selects one item uniformly from the options after a successful chance roll. Duplicate options increase an item's weight. </summary>
public sealed class OneFromOptionsChestDropRule : IChestLootRule
{
	private readonly int[] _itemTypes;
	internal int OptionCount => _itemTypes.Length;
	internal int FirstOptionItemType => _itemTypes[0];
	internal int SecondOptionItemType => _itemTypes.Length > 1 ? _itemTypes[1] : ItemID.None;

	private readonly bool _stackDyes;
	private readonly bool _applyRandomPrefix;
	internal bool SuccessOnNonZero { get; }
	internal bool SuccessOnMaximum { get; }
	internal int SuccessRollValue { get; }
	internal bool StackBeforeOption { get; }

	/// <summary> The minimum stack size. </summary>
	public int StackMin { get; }

	/// <summary> The maximum stack size. </summary>
	public int StackMax { get; }

	/// <summary> The chance numerator. </summary>
	public int ChanceNumerator { get; }

	/// <summary> The chance denominator. </summary>
	public int ChanceDenominator { get; }

	/// <summary> Creates an option rule. The option array is copied and duplicate options retain their weights. </summary>
	public OneFromOptionsChestDropRule(int chanceDenominator, params int[] itemTypes)
		: this(chanceDenominator, 1, 1, 1, false, false, false, false, false, false, -1, itemTypes)
	{
	}

	/// <summary> Creates an option rule. The option array is copied and duplicate options retain their weights. </summary>
	public OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, 1, 1, false, false, false, false, false, false, -1, itemTypes)
	{
	}

	internal OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, bool stackDyes, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, 1, 1, stackDyes, false, false, false, false, false, -1, itemTypes)
	{
	}

	internal OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, int stackMin, int stackMax, bool applyRandomPrefix, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, stackMin, stackMax, false, applyRandomPrefix, false, false, false, false, -1, itemTypes)
	{
	}

	internal OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, int stackMin, int stackMax, bool stackBeforeSetDefaults, bool successOnNonZero, bool successOnMaximum, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, stackMin, stackMax, false, false, false, stackBeforeSetDefaults, successOnNonZero, successOnMaximum, -1, itemTypes)
	{
	}

	internal OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, int stackMin, int stackMax, bool stackBeforeOption, bool stackBeforeSetDefaults, bool successOnNonZero, bool successOnMaximum, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, stackMin, stackMax, false, false, stackBeforeOption, stackBeforeSetDefaults, successOnNonZero, successOnMaximum, -1, itemTypes)
	{
	}

	internal OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, int stackMin, int stackMax, bool stackBeforeOption, bool stackBeforeSetDefaults, bool successOnNonZero, bool successOnMaximum, int successRollValue, params int[] itemTypes)
		: this(chanceDenominator, chanceNumerator, stackMin, stackMax, false, false, stackBeforeOption, stackBeforeSetDefaults, successOnNonZero, successOnMaximum, successRollValue, itemTypes)
	{
	}

	private OneFromOptionsChestDropRule(int chanceDenominator, int chanceNumerator, int stackMin, int stackMax, bool stackDyes, bool applyRandomPrefix, bool stackBeforeOption, bool stackBeforeSetDefaults, bool successOnNonZero, bool successOnMaximum, int successRollValue, params int[] itemTypes)
	{
		if (chanceDenominator <= 0)
			throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
		if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
			throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
		if (successOnNonZero && successOnMaximum)
			throw new ArgumentException("A rule cannot use both nonzero and maximum-value chance semantics.");
		if (successRollValue < -1 || successRollValue >= chanceDenominator)
			throw new ArgumentOutOfRangeException(nameof(successRollValue));
		if (stackMin <= 0 || stackMax < stackMin)
			throw new ArgumentOutOfRangeException(nameof(stackMin));
		if (itemTypes == null || itemTypes.Length == 0)
			throw new ArgumentException("At least one item option is required.", nameof(itemTypes));
		foreach (int itemType in itemTypes) {
			if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
				throw new ArgumentOutOfRangeException(nameof(itemTypes), $"Invalid item type: {itemType}.");
		}

		ChanceNumerator = chanceNumerator;
		ChanceDenominator = chanceDenominator;
		StackMin = stackMin;
		StackMax = stackMax;
		_itemTypes = (int[])itemTypes.Clone();
		_stackDyes = stackDyes;
		_applyRandomPrefix = applyRandomPrefix;
		StackBeforeOption = stackBeforeOption;
		StackBeforeSetDefaults = stackBeforeSetDefaults;
		SuccessOnNonZero = successOnNonZero;
		SuccessOnMaximum = successOnMaximum;
		SuccessRollValue = successRollValue;
	}

	internal bool StackBeforeSetDefaults { get; }

	internal int RollOptionIndex(UnifiedRandom random) => random.Next(_itemTypes.Length);

	internal int GetOptionItemType(int optionIndex, int replacementOptionIndex = -1, int replacementItemType = ItemID.None)
	{
		if (optionIndex < 0 || optionIndex >= _itemTypes.Length)
			throw new ArgumentOutOfRangeException(nameof(optionIndex));

		return optionIndex == replacementOptionIndex ? replacementItemType : _itemTypes[optionIndex];
	}

	internal void SetOptionAtSlot(Chest chest, int slot, int optionIndex, bool applyRandomPrefix)
	{
		if (chest == null)
			throw new ArgumentNullException(nameof(chest));
		if (chest.item == null || slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));
		if (optionIndex < 0 || optionIndex >= _itemTypes.Length)
			throw new ArgumentOutOfRangeException(nameof(optionIndex));

		chest.item[slot].SetDefaults(_itemTypes[optionIndex]);
		if (applyRandomPrefix)
			chest.item[slot].Prefix(-1);
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (!RollSucceeded(random))
			return;

		bool hasVariableStack = StackMin != 1 || StackMax != 1;
		int stack = hasVariableStack && StackBeforeOption ? NextStack(random) : 1;
		int itemType = _itemTypes[random.Next(_itemTypes.Length)];
		if (hasVariableStack && !StackBeforeOption)
			stack = NextStack(random);
		if (_stackDyes && ContentSamples.ItemsByType[itemType].dye > 0)
			stack = 3;
		chest.AddItem(itemType, stack);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
		=> ReportDropRates(drops, parentDropRate, null);

	internal void ReportDropRates(List<ChestDropRateInfo> drops, string condition)
		=> ReportDropRates(drops, 1f, condition);

	private void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate, string condition)
	{
		ArgumentNullException.ThrowIfNull(drops);
		float chancePerOption = parentDropRate * SuccessfulRollCount / ChanceDenominator / _itemTypes.Length;
		foreach (int itemType in _itemTypes) {
			int stackMin = _stackDyes && ContentSamples.ItemsByType[itemType].dye > 0 ? 3 : StackMin;
			int stackMax = _stackDyes && ContentSamples.ItemsByType[itemType].dye > 0 ? 3 : StackMax;
			drops.Add(new ChestDropRateInfo(itemType, stackMin, stackMax, chancePerOption, condition));
		}
	}

	internal bool TryDropAtFirstEmptySlot(Chest chest, UnifiedRandom random, bool stackDyes = false, int requiredItemSlot = 1)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		if (!RollSucceeded(random))
			return false;
		if (chest.item == null || requiredItemSlot < 0 || requiredItemSlot >= chest.item.Length || chest.item[requiredItemSlot] == null || chest.item[requiredItemSlot].stack == 0)
			return false;

		int itemCount = Math.Min(chest.maxItems, chest.item.Length);
		for (int slot = 1; slot < itemCount; slot++) {
			Item chestItem = chest.item[slot];
			if (chestItem == null || chestItem.stack != 0)
				continue;

			bool hasVariableStack = StackMin != 1 || StackMax != 1;
			int stack = hasVariableStack && StackBeforeOption ? NextStack(random) : 1;
			int itemType = _itemTypes[random.Next(_itemTypes.Length)];
			if (hasVariableStack && !StackBeforeOption)
				stack = NextStack(random);
			chestItem.SetDefaults(itemType);
			if (hasVariableStack)
				chestItem.stack = stack;
			if ((stackDyes || _stackDyes) && chestItem.dye > 0)
				chestItem.stack = 3;

			return true;
		}

		return false;
	}

	internal bool TryDropAtSlot(Chest chest, int slot, UnifiedRandom random, bool applyRandomPrefix = false, int replacementOptionIndex = -1, int replacementItemType = ItemID.None, bool stackBeforeSetDefaults = false, bool stackBeforeOption = false)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if ((ChanceNumerator != ChanceDenominator || SuccessOnNonZero || SuccessOnMaximum || SuccessRollValue >= 0) && !RollSucceeded(random))
			return false;
		if (chest.item == null)
			throw new InvalidOperationException("The chest has no item slots.");
		if (slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));
		if (replacementOptionIndex < -1 || replacementOptionIndex >= _itemTypes.Length)
			throw new ArgumentOutOfRangeException(nameof(replacementOptionIndex));
		if (replacementOptionIndex >= 0 && (replacementItemType <= ItemID.None || replacementItemType >= ItemLoader.ItemCount))
			throw new ArgumentOutOfRangeException(nameof(replacementItemType));

		stackBeforeOption |= StackBeforeOption;
		stackBeforeSetDefaults |= StackBeforeSetDefaults;
		bool hasVariableStack = StackMin != 1 || StackMax != 1;
		int stack = hasVariableStack && stackBeforeOption ? NextStack(random) : 1;
		int optionIndex = random.Next(_itemTypes.Length);
		if (hasVariableStack && !stackBeforeOption && stackBeforeSetDefaults)
			stack = NextStack(random);

		int itemType = optionIndex == replacementOptionIndex ? replacementItemType : _itemTypes[optionIndex];
		chest.item[slot].SetDefaults(itemType);
		if (hasVariableStack) {
			if (!stackBeforeOption && !stackBeforeSetDefaults)
				stack = NextStack(random);

			chest.item[slot].stack = stack;
		}
		if (applyRandomPrefix || _applyRandomPrefix)
			chest.item[slot].Prefix(-1);

		return true;
	}

	private int NextStack(UnifiedRandom random) => StackMax == StackMin ? StackMin : random.Next(StackMin, StackMax + 1);

	private bool RollSucceeded(UnifiedRandom random)
	{
		int roll = random.Next(ChanceDenominator);
		if (SuccessOnNonZero)
			return roll != 0;
		if (SuccessOnMaximum)
			return roll == ChanceDenominator - 1;
		if (SuccessRollValue >= 0)
			return roll == SuccessRollValue;
		return roll < ChanceNumerator;
	}

	private int SuccessfulRollCount => SuccessOnNonZero || SuccessOnMaximum || SuccessRollValue >= 0 ? (SuccessOnNonZero ? ChanceDenominator - 1 : 1) : ChanceNumerator;
}