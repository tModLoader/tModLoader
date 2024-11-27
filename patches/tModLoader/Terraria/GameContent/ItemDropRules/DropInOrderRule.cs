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
public class DropInOrderRule : IItemDropRule
{
	public List<int> itemIds;
	public Func<int> counter;
	public int chanceDenominator;
	public int amountDroppedMinimum;
	public int amountDroppedMaximum;
	public int chanceNumerator;
	public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }
	public DropInOrderRule(List<int> itemIds, Func<int> counter, int chanceDenominator = 1, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1)
	{
		if (amountDroppedMinimum > amountDroppedMaximum) {
			throw new ArgumentOutOfRangeException(nameof(amountDroppedMinimum), $"{nameof(amountDroppedMinimum)} must be lesser or equal to {nameof(amountDroppedMaximum)}.");
		}

		this.itemIds = itemIds;
		this.counter = counter;
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
			CommonCode.DropItem(info, itemIds[counter()], info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
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

		foreach (int item in itemIds) {
			drops.Add(new(item, amountDroppedMinimum, amountDroppedMaximum, dropRate / itemIds.Count, ratesInfo.conditions));
		}
		Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
	}
}
