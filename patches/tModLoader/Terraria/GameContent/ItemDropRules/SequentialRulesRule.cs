using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria.GameContent.ItemDropRules;

// added by TML
// code by Snek
/// <summary>
/// Runs the provided rules in order, stopping after a rule succeeds.<br/>
/// </summary>
public class SequentialRulesRule : BaseItemDropRule, INestedItemDropRule
{
	public IItemDropRule[] rules;
	public int chanceDenominator;

	public SequentialRulesRule(int chanceDenominator, params IItemDropRule[] rules)
	{
		this.chanceDenominator = chanceDenominator;
		this.rules = rules;
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result = default(ItemDropAttemptResult);
		result.State = ItemDropAttemptResultState.DidNotRunCode;
		return result;
	}

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction)
	{
		ItemDropAttemptResult result = default;
		if (info.player.RollLuck(chanceDenominator) == 0) {
			for (int i = 0; i < rules.Length; i++) {
				IItemDropRule rule = rules[i];
				result = resolveAction(rule, info);
				if (result.State == ItemDropAttemptResultState.Success) {
					return result;
				}
			}
		}

		result.State = ItemDropAttemptResultState.FailedRandomRoll;
		return result;
	}

	public override void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		for (int i = rules.Length - 1; i >= 1; i--) {
			rules[i - 1].OnFailedRoll(rules[i]);
		}

		float selfChance = 1f / chanceDenominator;
		float baseChance = ratesInfo.parentDroprateChance * selfChance;
		rules[0].ReportDroprates(drops, ratesInfo.With(baseChance));

		Chains.ReportDroprates(ChainedRules, selfChance, drops, ratesInfo);

		for (int i = 0; i < rules.Length - 1; i++) {
			rules[i].ChainedRules.RemoveAt(rules[i].ChainedRules.Count - 1);
		}
	}
}
