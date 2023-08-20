using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.ItemDropRules;

// added by TML
/// <summary>
/// Runs multiple drop rules if successes.
/// </summary>
public class FewFromRulesRule : BaseItemDropRule, INestedItemDropRule
{
	public int amount;
	public IItemDropRule[] options;
	public int chanceDenominator;

	public FewFromRulesRule(int amount, int chanceNumerator, params IItemDropRule[] options)
	{
		if (amount > options.Length) {
			throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
		}

		this.amount = amount;
		chanceDenominator = chanceNumerator;
		this.options = options;
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result = default(ItemDropAttemptResult);
		result.State = ItemDropAttemptResultState.DidNotRunCode;
		return result;
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction)
	{
		if (info.rng.Next(chanceDenominator) == 0) {
			List<IItemDropRule> savedDropIds = options.ToList();
			int count = 0;

			int num = info.rng.Next(savedDropIds.Count);
			resolveAction(savedDropIds[num], info);
			savedDropIds.RemoveAt(num);

			while (++count < amount) {
				num = info.rng.Next(savedDropIds.Count);
				resolveAction(savedDropIds[num], info);
				savedDropIds.RemoveAt(num);
			}

			return new() { State = ItemDropAttemptResultState.Success };
		}

		return new() { State = ItemDropAttemptResultState.FailedRandomRoll };
	}

	public override void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float personalDroprate = 1f / (float)chanceDenominator;
		float num2 = personalDroprate * ratesInfo.parentDroprateChance;
		float multiplier = 1f / (float)(options.Length - amount) * num2;
		for (int i = 0; i < options.Length; i++) {
			options[i].ReportDroprates(drops, ratesInfo.With(multiplier));
		}

		Chains.ReportDroprates(ChainedRules, personalDroprate, drops, ratesInfo);
	}
}
