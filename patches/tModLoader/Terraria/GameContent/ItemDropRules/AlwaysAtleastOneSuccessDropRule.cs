using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules;

/// <summary>
/// Re-runs all drop rules if none succeeded.
/// </summary>
public class AlwaysAtleastOneSuccessDropRule : IItemDropRule, INestedItemDropRule
{
	private class PersonalDropRateReportingRule : IItemDropRuleChainAttempt
	{
		private readonly Action<float> report;

		public PersonalDropRateReportingRule(Action<float> report)
		{
			this.report = report;
		}

		public IItemDropRule RuleToChain => throw new NotImplementedException();
		public bool CanChainIntoRule(ItemDropAttemptResult parentResult) => throw new NotImplementedException();
		public void ReportDroprates(float personalDropRate, List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) => report(personalDropRate);
	}

	public IItemDropRule[] rules;

	public List<IItemDropRuleChainAttempt> ChainedRules {
		get;
		private set;
	}

	public AlwaysAtleastOneSuccessDropRule(params IItemDropRule[] rules)
	{
		this.rules = rules;
		ChainedRules = new List<IItemDropRuleChainAttempt>();
	}

	public bool CanDrop(DropAttemptInfo info) => true;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		return new() { State = ItemDropAttemptResultState.DidNotRunCode };
	}

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction)
	{
		while (true) {
			bool anyDropped = false;
			foreach (var rule in rules) {
				if (resolveAction(rule, info).State == ItemDropAttemptResultState.Success)
					anyDropped = true;
			}

			if (anyDropped)
				return new() { State = ItemDropAttemptResultState.Success };
		}
	}

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float reroll = 1f;
		foreach (var rule in rules) {
			reroll *= 1f - GetPersonalDropRate(rule);
		}

		float scale = reroll == 1f ? 1f : 1f / (1f - reroll);
		foreach (var rule in rules) {
			rule.ReportDroprates(drops, ratesInfo.With(scale));
		}

		/*for (int i = rules.Length - 1; i >= 1; i--) {
			rules[i].ReportDroprates(drops, ratesInfo);
		}

		float reroll = rules.Select(r => 1 - GetPersonalDropRate(r)).Aggregate((a, b) => a * b);

		float scale = 1 / (1 - reroll);
		float[] overall = new float[drops.Count];
		foreach (DropRateInfo drop in drops) {
			rules[j].ReportDroprates(drops, new(drop.dropRate * scale));
		}*/

		Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
	}

	public static float GetPersonalDropRate(IItemDropRule rule)
	{
		var chained = rule.ChainedRules.ToArray();
		rule.ChainedRules.Clear();

		float dropRate = 0;
		rule.ChainedRules.Add(new PersonalDropRateReportingRule(f => dropRate = f));
		rule.ReportDroprates(new(), new(1f));
		rule.ChainedRules.Clear();
		rule.ChainedRules.AddRange(chained);
		return dropRate;
	}
}
