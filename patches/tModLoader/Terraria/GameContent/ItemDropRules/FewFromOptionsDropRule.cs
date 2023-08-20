using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.ItemDropRules;

// added by TML
/// <summary>
/// Runs multiple rules if successes.
/// </summary>
public class FewFromOptionsDropRule : BaseItemDropRule
{
	public int[] dropIds;
	public int chanceDenominator;
	public int chanceNumerator;
	public int amount;

	public FewFromOptionsDropRule(int amount, int chanceDenominator, int chanceNumerator, params int[] options)
	{
		if (amount > options.Length) {
			throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
		}

		this.amount = amount;
		this.chanceDenominator = chanceDenominator;
		this.chanceNumerator = chanceNumerator;
		dropIds = options;
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		if (info.player.RollLuck(chanceDenominator) < chanceNumerator) {
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

	public override void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float num = (float)chanceNumerator / (float)chanceDenominator;
		float num2 = num * ratesInfo.parentDroprateChance;
		float dropRate = 1f / (float)(dropIds.Length - amount + 1) * num2;
		for (int i = 0; i < dropIds.Length; i++) {
			drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
		}

		Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
	}
}
