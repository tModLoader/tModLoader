using System.Collections.Generic;

namespace Terraria.GameContent.ItemDropRules
{
	/// <summary>
	/// Re-runs all drop rules if none succeded.
	/// </summary>
	public class AlwaysAtleastOneSuccessDropRule : IItemDropRule
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

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) => new() { State = ItemDropAttemptResultState.DidNotRunCode };

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction) {
			ItemDropAttemptResult result = default;

			bool fail = true;
			bool wasAtleastOneSoCanStop = false;
			while (fail) {
				for (int i = 0; i < rules.Length; i++) {
					IItemDropRule rule = rules[i];
					result = resolveAction(rule, info);
					if (result.State == ItemDropAttemptResultState.Success) {
						wasAtleastOneSoCanStop = true;
					}
				}
				if (wasAtleastOneSoCanStop)
					fail = false;
			}

			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			float baseChance = ratesInfo.parentDroprateChance;
			rules[0].ReportDroprates(drops, ratesInfo.With(baseChance));

			Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
		}
	}
}
