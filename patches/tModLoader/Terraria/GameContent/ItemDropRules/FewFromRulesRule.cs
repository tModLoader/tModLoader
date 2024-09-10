using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules;

// added by TML
/// <summary>
/// Runs multiple drop rules if successes.
/// </summary>
public class FewFromRulesRule : IItemDropRule, INestedItemDropRule
{
	public int amount;
	public IItemDropRule[] options;
	public int chanceDenominator;

	public List<IItemDropRuleChainAttempt> ChainedRules {
		get;
		private set;
	}

	public FewFromRulesRule(int amount, int chanceDenominator, params IItemDropRule[] options)
	{
		if (amount > options.Length) {
			throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
		}

		this.amount = amount;
		this.chanceDenominator = chanceDenominator;
		this.options = options;
		ChainedRules = new List<IItemDropRuleChainAttempt>();
	}

	public bool CanDrop(DropAttemptInfo info) => true;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result = default(ItemDropAttemptResult);
		result.State = ItemDropAttemptResultState.DidNotRunCode;
		return result;
	}

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction)
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

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float personalDroprate = 1f / chanceDenominator;
		float multiplier = amount / (float)options.Length * personalDroprate;
		for (int i = 0; i < options.Length; i++) {
			options[i].ReportDroprates(drops, ratesInfo.With(multiplier));
		}

		Chains.ReportDroprates(ChainedRules, personalDroprate, drops, ratesInfo);
	}
}
