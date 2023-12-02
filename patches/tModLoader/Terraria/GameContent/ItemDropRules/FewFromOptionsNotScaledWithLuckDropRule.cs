using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules;

// added by TML
/// <summary>
/// Runs multiple rules if successes.
/// Does not use player luck.
/// </summary>
public class FewFromOptionsNotScaledWithLuckDropRule : IItemDropRule
{
	public int amount;
	public int[] dropIds;
	public int chanceDenominator;
	public int chanceNumerator;

	public List<IItemDropRuleChainAttempt> ChainedRules {
		get;
		private set;
	}

	public FewFromOptionsNotScaledWithLuckDropRule(int amount, int chanceDenominator, int chanceNumerator, params int[] options)
	{
		if (amount > options.Length) {
			throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
		}

		this.amount = amount;
		this.chanceDenominator = chanceDenominator;
		dropIds = options;
		this.chanceNumerator = chanceNumerator;
		ChainedRules = new List<IItemDropRuleChainAttempt>();
	}

	public bool CanDrop(DropAttemptInfo info) => true;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		if (info.rng.Next(chanceDenominator) < chanceNumerator) {
			List<int> savedDropIds = dropIds.ToList();
			int count = 0;

			int index = info.rng.Next(savedDropIds.Count);
			CommonCode.DropItem(info, savedDropIds[index], 1);
			savedDropIds.RemoveAt(index);

			while (++count < amount) {
				int index2 = info.rng.Next(savedDropIds.Count);
				CommonCode.DropItem(info, savedDropIds[index2], 1);
				savedDropIds.RemoveAt(index2);
			}

			return new() { State = ItemDropAttemptResultState.Success };
		}

		return new() { State = ItemDropAttemptResultState.FailedRandomRoll };
	}

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float pesonalDroprate = (float)chanceNumerator / (float)chanceDenominator;
		float num2 = pesonalDroprate * ratesInfo.parentDroprateChance;
		float dropRate = amount / (float)dropIds.Length * num2;
		for (int i = 0; i < dropIds.Length; i++) {
			drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
		}

		Chains.ReportDroprates(ChainedRules, pesonalDroprate, drops, ratesInfo);
	}
}
