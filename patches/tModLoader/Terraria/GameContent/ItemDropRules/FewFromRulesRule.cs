using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules
{
	public class FewFromRulesRule : IItemDropRule, INestedItemDropRule
	{
		public int amount;
		public IItemDropRule[] options;
		public int chanceDenominator;

		public List<IItemDropRuleChainAttempt> ChainedRules {
			get;
			private set;
		}

		public FewFromRulesRule(int amount, int chanceNumerator, params IItemDropRule[] options) {
			this.amount = amount;
			chanceDenominator = chanceNumerator;
			this.options = options;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public bool CanDrop(DropAttemptInfo info) => true;

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result = default(ItemDropAttemptResult);
			result.State = ItemDropAttemptResultState.DidNotRunCode;
			return result;
		}

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction) {
			int num = -1;
			ItemDropAttemptResult result;
			int count = 0;
			if (info.rng.Next(chanceDenominator) == 0) {
				List<IItemDropRule> savedDropIds = options.ToList();
				num = info.rng.Next(savedDropIds.Count);
				resolveAction(savedDropIds[num], info);
				savedDropIds.RemoveAt(num);

				while (++count < amount) {
					num = info.rng.Next(savedDropIds.Count);
					resolveAction(savedDropIds[num], info);
					savedDropIds.RemoveAt(num);
				}

				result = default(ItemDropAttemptResult);
				result.State = ItemDropAttemptResultState.Success;
				return result;
			}

			result = default(ItemDropAttemptResult);
			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			float num = 1f / (float)chanceDenominator;
			float num2 = num * ratesInfo.parentDroprateChance;
			float multiplier = 1f / (float)(options.Length - amount + 1) * num2;
			for (int i = 0; i < options.Length; i++) {
				options[i].ReportDroprates(drops, ratesInfo.With(multiplier));
			}

			Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
		}
	}
}
