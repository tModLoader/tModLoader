using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules
{
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

		public FewFromOptionsNotScaledWithLuckDropRule(int amount, int chanceDenominator, int chanceNumerator, params int[] options) {
			this.amount = amount;
			this.chanceDenominator = chanceDenominator;
			dropIds = options;
			this.chanceNumerator = chanceNumerator;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public bool CanDrop(DropAttemptInfo info) => true;

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result;
			int count = 0;
			if (info.rng.Next(chanceDenominator) < chanceNumerator) {
				List<int> savedDropIds = dropIds.ToList();
				int index = info.rng.Next(savedDropIds.Count);
				CommonCode.DropItemFromNPC(info.npc, savedDropIds[index], 1, false);
				savedDropIds.RemoveAt(index);
				while (++count <= amount) {
					int index2 = info.rng.Next(savedDropIds.Count);
					CommonCode.DropItemFromNPC(info.npc, savedDropIds[index2], 1, false);
					savedDropIds.RemoveAt(index2);
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
			float num = (float)chanceNumerator / (float)chanceDenominator;
			float num2 = num * ratesInfo.parentDroprateChance;
			float dropRate = 1f / (float)(dropIds.Length - amount + 1) * num2;
			for (int i = 0; i < dropIds.Length; i++) {
				drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
			}

			Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
		}
	}
}
