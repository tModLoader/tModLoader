using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class OneFromBundlesChestDropRule : IChestLootRule
{
	private readonly int[][] _itemBundles;

	private readonly bool _singleRoll;

	/// <summary> The chance denominator. </summary>
	public int ChanceDenominator { get; }
	internal int FirstBundleItemType => _itemBundles[0][0];

	public OneFromBundlesChestDropRule(int chanceDenominator, params int[][] itemBundles)
		: this(chanceDenominator, false, itemBundles)
	{
	}

	internal OneFromBundlesChestDropRule(int chanceDenominator, bool singleRoll, params int[][] itemBundles)
	{
		if (chanceDenominator <= 0)
			throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
		if (itemBundles == null || itemBundles.Length == 0)
			throw new ArgumentException("At least one item bundle is required.", nameof(itemBundles));

		ChanceDenominator = chanceDenominator;
		_singleRoll = singleRoll;
		_itemBundles = new int[itemBundles.Length][];
		for (int i = 0; i < itemBundles.Length; i++) {
			if (itemBundles[i] == null || itemBundles[i].Length == 0)
				throw new ArgumentException("Item bundles cannot be empty.", nameof(itemBundles));
			foreach (int itemType in itemBundles[i]) {
				if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
					throw new ArgumentOutOfRangeException(nameof(itemBundles), $"Invalid item type: {itemType}.");
			}

			_itemBundles[i] = (int[])itemBundles[i].Clone();
		}
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (!_singleRoll && random.Next(ChanceDenominator) != 0)
			return;

		int[] bundle = _itemBundles[random.Next(_itemBundles.Length)];
		foreach (int itemType in bundle)
			chest.AddItem(itemType);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		ReportDropRates(drops, parentDropRate, null);
	}

	internal void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate, string condition)
	{
		float chancePerBundle = parentDropRate / (_singleRoll ? _itemBundles.Length : ChanceDenominator * _itemBundles.Length);
		foreach (int[] bundle in _itemBundles) {
			foreach (int itemType in bundle)
				drops.Add(new ChestDropRateInfo(itemType, 1, 1, chancePerBundle, condition));
		}
	}

	internal bool TryDropAtFirstEmptySlot(Chest chest, UnifiedRandom random, int requiredItemSlot = 1)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (!_singleRoll && random.Next(ChanceDenominator) != 0)
			return false;
		if (chest.item == null || requiredItemSlot < 0 || requiredItemSlot >= chest.item.Length || chest.item[requiredItemSlot] == null || chest.item[requiredItemSlot].stack == 0)
			return false;

		int itemCount = Math.Min(chest.maxItems, chest.item.Length);
		for (int slot = 1; slot < itemCount; slot++) {
			if (chest.item[slot] == null || chest.item[slot].stack != 0)
				continue;

			SetBundleAtSlot(chest, slot, random);
			return true;
		}

		return false;
	}

	internal bool TryDropAtSlot(Chest chest, int slot, UnifiedRandom random)
		=> DropAtSlot(chest, slot, random) > 0;

	internal int DropAtSlot(Chest chest, int slot, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (!_singleRoll && random.Next(ChanceDenominator) != 0)
			return 0;

		int[] bundle = _itemBundles[_itemBundles.Length == 1 ? 0 : random.Next(_itemBundles.Length)];
		SetBundleAtSlot(chest, slot, bundle);
		return bundle.Length;
	}

	internal void SetBundleAtSlot(Chest chest, int slot, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		if (chest.item == null || slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));

		int[] bundle = _itemBundles.Length == 1 ? _itemBundles[0] : _itemBundles[random.Next(_itemBundles.Length)];
		SetBundleAtSlot(chest, slot, bundle);
	}

	private static void SetBundleAtSlot(Chest chest, int slot, int[] bundle)
	{
		for (int offset = 0; offset < bundle.Length; offset++)
			chest.item[slot + offset].SetDefaults(bundle[offset]);
	}
}