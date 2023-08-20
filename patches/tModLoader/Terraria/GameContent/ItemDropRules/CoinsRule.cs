using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.GameContent.ItemDropRules;

public class CoinsRule : IItemDropRule
{
	public long value; // in copper coins
	public bool withRandomBonus = false;

	public CoinsRule(long value, bool withRandomBonus = false)
	{
		this.value = value;
		this.withRandomBonus = withRandomBonus;
	}

	public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; } = new();
	public bool CanDrop(DropAttemptInfo info) => true;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		double scale = 1f;
		if (withRandomBonus) {
			scale += info.rng.Next(-20, 21) * .01f;
			if (info.rng.Next(5) == 0) scale += info.rng.Next(5, 11) * .01f;
			if (info.rng.Next(10) == 0) scale += info.rng.Next(10, 21) * .01f;
			if (info.rng.Next(15) == 0) scale += info.rng.Next(15, 31) * .01f;
			if (info.rng.Next(20) == 0) scale += info.rng.Next(20, 41) * .01f;
		}

		long money = (long)(value * scale);
		foreach ((int itemId, int count) in ToCoins(money)) {
			CommonCode.DropItem(info, itemId, count);
		}

		return new() { State = ItemDropAttemptResultState.Success };
	}

	public static IEnumerable<(int itemId, int count)> ToCoins(long money)
	{
		int copper = (int)(money % 100);
		money /= 100;
		int silver = (int)(money % 100);
		money /= 100;
		int gold = (int)(money % 100);
		money /= 100;
		int plat = (int)money;

		if (copper > 0) yield return (ItemID.CopperCoin, copper);
		if (silver > 0) yield return (ItemID.SilverCoin, silver);
		if (gold > 0) yield return (ItemID.GoldCoin, gold);
		if (plat > 0) yield return (ItemID.PlatinumCoin, plat);
	}

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		// TODO: is there a sensible way to report variance here? probably not

		foreach ((int itemId, int count) in ToCoins(value)) {
			drops.Add(new DropRateInfo(itemId, count, count, ratesInfo.parentDroprateChance, ratesInfo.conditions));
		}

		Chains.ReportDroprates(ChainedRules, 1, drops, ratesInfo);
	}
}
