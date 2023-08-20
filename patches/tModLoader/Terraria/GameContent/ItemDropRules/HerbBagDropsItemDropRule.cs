using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria.GameContent.ItemDropRules;

// added by TML.
// TODO: different ItemDropRule class that would be less hardcoded?
/// <summary>
/// Used just by Herb Bag. Horribly hardcoded. Do not use if you can.
/// </summary>
public class HerbBagDropsItemDropRule : BaseItemDropRule
{
	public int[] dropIds;

	public HerbBagDropsItemDropRule(params int[] options)
	{
		dropIds = options;
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result;

		int amount = Main.rand.Next(2, 5);
		if (Main.rand.Next(3) == 0)
			amount++;

		for (int i = 0; i < amount; i++) {
			int stack = Main.rand.Next(2, 5);
			if (Main.rand.Next(3) == 0)
				stack += Main.rand.Next(1, 5);

			CommonCode.DropItem(info, dropIds[info.rng.Next(dropIds.Length)], stack);
		}

		result = default(ItemDropAttemptResult);
		result.State = ItemDropAttemptResultState.Success;
		return result;
	}

	public override void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float num = (float)1f / (float)1f;
		float num2 = num * ratesInfo.parentDroprateChance;
		float dropRate = 1f / (float)(dropIds.Length + 3.83f) * num2;
		for (int i = 0; i < dropIds.Length; i++) {
			drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
		}

		Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
	}
}
