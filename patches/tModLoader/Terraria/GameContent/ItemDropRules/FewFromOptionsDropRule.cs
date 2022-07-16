using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.GameContent.ItemDropRules
{
	public class FewFromOptionsDropRule : IItemDropRule
	{
		public int amount;
		public int chanceDenominator;
		public int chanceNumerator;
		public int[] dropIds;

		public List<IItemDropRuleChainAttempt> ChainedRules {
			get;
			private set;
		}

		public FewFromOptionsDropRule(int amount, int chanceDenominator, int chanceNumerator, params int[] options) {
			if (amount >= options.Length) {
				throw new ArgumentOutOfRangeException(nameof(amount), $"{nameof(amount)} must be less than the number of {nameof(options)}");
			}

			this.amount = amount;
			this.chanceDenominator = chanceDenominator;
			this.chanceNumerator = chanceNumerator;
			dropIds = options;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public bool CanDrop(DropAttemptInfo info) => true;

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			if (info.player.RollLuck(chanceDenominator) < chanceNumerator) {
				List<int> savedDropIds = dropIds.ToList();
				int count = 0;

				while (count++ < amount) {
					int index = info.rng.Next(savedDropIds.Count);
					CommonCode.DropItemFromNPC(info.npc, savedDropIds[index], 1, false, info.player, info.item);
					savedDropIds.RemoveAt(index);
				}

				return new() { State = ItemDropAttemptResultState.Success };
			}

			return new() { State = ItemDropAttemptResultState.FailedRandomRoll };
		}

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			float personalDropRate = (float)chanceNumerator / (float)chanceDenominator;
			float num2 = personalDropRate * ratesInfo.parentDroprateChance;
			float dropRate = 1f / (float)(dropIds.Length - amount) * num2;
			for (int i = 0; i < dropIds.Length; i++) {
				drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
			}

			Chains.ReportDroprates(ChainedRules, personalDropRate, drops, ratesInfo);
		}
	}
}
