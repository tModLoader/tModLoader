using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Terraria.GameContent.ItemDropRules;
public class DropFromItemPoolRule : IItemDropRule
{
	public string poolName;
	public int chanceDenominator;
	public int amountDroppedMinimum;
	public int amountDroppedMaximum;
	public int chanceNumerator;
	public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }
	public DropFromItemPoolRule(string poolName, int chanceDenominator = 1, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1)
	{
		if (amountDroppedMinimum > amountDroppedMaximum) {
			throw new ArgumentOutOfRangeException(nameof(amountDroppedMinimum), $"{nameof(amountDroppedMinimum)} must be lesser or equal to {nameof(amountDroppedMaximum)}.");
		}

		this.poolName = poolName;
		this.chanceDenominator = chanceDenominator;
		this.amountDroppedMinimum = amountDroppedMinimum;
		this.amountDroppedMaximum = amountDroppedMaximum;
		this.chanceNumerator = chanceNumerator;
		ChainedRules = [];
	}

	public bool CanDrop(DropAttemptInfo info) => true;

	public virtual ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result;
		if (info.RollLuck(chanceDenominator) < chanceNumerator) {
			Tuple<(int type, List<IItemDropRule> chainedRules), double>[] options = GetDropableEntries(info).ToArray();
			if (options.Length == 0) {
				result = default;
				result.State = ItemDropAttemptResultState.DoesntFillConditions;
				return result;
			}
			(int itemId, List<IItemDropRule> chainedRules) = new WeightedRandom<(int, List<IItemDropRule>)>(info.rng, options).Get();
			CommonCode.DropItem(info, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
			for (int i = 0; i < chainedRules.Count; i++) {
				ItemDropResolver.ResolveRule(chainedRules[i], info);
			}
			result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}

		result = default;
		result.State = ItemDropAttemptResultState.FailedRandomRoll;
		return result;
	}

	public virtual void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float num = (float)chanceNumerator / (float)chanceDenominator;
		float dropRate = num * ratesInfo.parentDroprateChance;
		DropRateInfoChainFeed thisRatesInfo = ratesInfo;
		thisRatesInfo.parentDroprateChance = dropRate;

		List<ItemPoolEntry> entries = ChestLootLoader.GetItemPool(poolName);
		float totalWeight = 0;
		foreach (ItemPoolEntry item in entries) {
			bool canShow = true;
			for (int i = 0; i < item.Conditions.Count && canShow; i++) {
				canShow &= item.Conditions[i].CanShowItemDropInUI();
			}
			if (canShow)
				totalWeight += item.Weight;
		}
		foreach (ItemPoolEntry item in entries) {
			bool canShow = true;
			for (int i = 0; i < item.Conditions.Count && canShow; i++) {
				canShow &= item.Conditions[i].CanShowItemDropInUI();
			}
			if (!canShow)
				continue;
			DropRateInfoChainFeed itemRatesInfo = thisRatesInfo;
			itemRatesInfo.parentDroprateChance *= item.Weight / totalWeight;
			itemRatesInfo.conditions = (itemRatesInfo.conditions ?? []).Concat(item.Conditions ?? []).ToList();
			drops.Add(new DropRateInfo(item.Type, amountDroppedMinimum, amountDroppedMaximum, itemRatesInfo.parentDroprateChance, itemRatesInfo.conditions));
			foreach (IItemDropRule ChainedRule in item.ChainedRules) {
				ChainedRule.ReportDroprates(drops, itemRatesInfo);
			}
		}
		Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
	}
	protected IEnumerable<Tuple<(int type, List<IItemDropRule> chainedRules), double>> GetDropableEntries(DropAttemptInfo info)
	{
		List<ItemPoolEntry> entries = ChestLootLoader.GetItemPool(poolName);
		for (int i = 0; i < entries.Count; i++) {
			ItemPoolEntry entry = entries[i];
			bool shouldSkip = false;
			for (int j = 0; j < entry.Conditions.Count && !shouldSkip; j++) {
				if (!entry.Conditions[j].CanDrop(info))
					shouldSkip = true;
			}
			if (shouldSkip)
				continue;
			yield return new((entry.Type, entry.ChainedRules), entry.Weight);
		}
	}
}
