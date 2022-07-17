using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules
{
	public class FewFromRulesRule : IItemDropRule, INestedItemDropRule
	{
		public int amount;
		public int chanceDenominator;
		public IItemDropRule[] options;

		public List<IItemDropRuleChainAttempt> ChainedRules {
			get;
			private set;
		}

		public FewFromRulesRule(int amount, int chanceDenominator, params IItemDropRule[] options) {
			if (amount >= options.Length) {
				throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
			}

			this.amount = amount;
			this.chanceDenominator = chanceDenominator;
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
			if (info.rng.Next(chanceDenominator) == 0) {
				List<IItemDropRule> savedDropIds = options.ToList();
				int count = 0;

				while (count++ < amount) {
					int num = info.rng.Next(savedDropIds.Count);
					resolveAction(savedDropIds[num], info);
					savedDropIds.RemoveAt(num);
				}

				return new() { State = ItemDropAttemptResultState.Success };
			}

			return new() { State = ItemDropAttemptResultState.FailedRandomRoll };
		}

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			float personalDropRate = 1f / (float)chanceDenominator;
			float num2 = personalDropRate * ratesInfo.parentDroprateChance;
			float multiplier = 1f / (float)(options.Length - amount) * num2;
			for (int i = 0; i < options.Length; i++) {
				options[i].ReportDroprates(drops, ratesInfo.With(multiplier));
			}

			Chains.ReportDroprates(ChainedRules, personalDropRate, drops, ratesInfo);
		}
	}
}
