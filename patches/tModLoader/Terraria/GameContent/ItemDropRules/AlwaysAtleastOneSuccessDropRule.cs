using System.Collections.Generic;

namespace Terraria.GameContent.ItemDropRules
{
	/// <summary>
	/// Re-runs all drop rules if none succeded.
	/// </summary>
	public class AlwaysAtleastOneSuccessDropRule : IItemDropRule, INestedItemDropRule
	{
		public IItemDropRule[] rules;

		public List<IItemDropRuleChainAttempt> ChainedRules {
			get;
			private set;
		}

		public AlwaysAtleastOneSuccessDropRule(params IItemDropRule[] rules) {
			this.rules = rules;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public bool CanDrop(DropAttemptInfo info) => true;

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			return new() { State = ItemDropAttemptResultState.DidNotRunCode };
		}

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction) {
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

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			foreach (var rule in rules) {
				rule.ReportDroprates(drops, ratesInfo);
			}
			Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
		}
	}
}
