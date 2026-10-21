using System.Collections.Generic;
using Terraria.ID;
using static Terraria.WorldGen;

namespace Terraria.GameContent.ItemDropRules;

/// <summary>Drops pot coins using depth, style, difficulty and world progression multipliers.</summary>
public sealed class PotCoinsRule : IItemDropRule
{
	private readonly int j;
	private readonly bool aboveRock;
	private readonly float valueMultiplier;

	public PotCoinsRule(int tileY, bool aboveRock, float valueMultiplier)
	{
		j = tileY;
		this.aboveRock = aboveRock;
		this.valueMultiplier = valueMultiplier;
	}

	public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();
	public bool CanDrop(DropAttemptInfo info) => info.pot != null && !info.IsInSimulation;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		float value = 200 + genRand.Next(-100, 101);
		if (j < Main.worldSurface)
			value *= 0.5f;
		else if (aboveRock)
			value *= 0.75f;
		else if (j > Main.maxTilesY - 250)
			value *= 1.25f;

		value *= 1f + Main.rand.Next(-20, 21) * 0.01f;
		if (Main.rand.Next(4) == 0)
			value *= 1f + Main.rand.Next(5, 11) * 0.01f;

		if (Main.rand.Next(8) == 0)
			value *= 1f + Main.rand.Next(10, 21) * 0.01f;

		if (Main.rand.Next(12) == 0)
			value *= 1f + Main.rand.Next(20, 41) * 0.01f;

		if (Main.rand.Next(16) == 0)
			value *= 1f + Main.rand.Next(40, 81) * 0.01f;

		if (Main.rand.Next(20) == 0)
			value *= 1f + Main.rand.Next(50, 101) * 0.01f;

		if (Main.expertMode)
			value *= 2.5f;

		if (Main.expertMode && Main.rand.Next(2) == 0)
			value *= 1.25f;

		if (Main.expertMode && Main.rand.Next(3) == 0)
			value *= 1.5f;

		if (Main.expertMode && Main.rand.Next(4) == 0)
			value *= 1.75f;

		value *= valueMultiplier;
		if (NPC.downedBoss1)
			value *= 1.1f;

		if (NPC.downedBoss2)
			value *= 1.1f;

		if (NPC.downedBoss3)
			value *= 1.1f;

		if (NPC.downedMechBoss1)
			value *= 1.1f;

		if (NPC.downedMechBoss2)
			value *= 1.1f;

		if (NPC.downedMechBoss3)
			value *= 1.1f;

		if (NPC.downedPlantBoss)
			value *= 1.1f;

		if (NPC.downedQueenBee)
			value *= 1.1f;

		if (NPC.downedGolemBoss)
			value *= 1.1f;

		if (NPC.downedPirates)
			value *= 1.1f;

		if (NPC.downedGoblins)
			value *= 1.1f;

		if (NPC.downedFrost)
			value *= 1.1f;

		while ((int)value > 0) {
			if (value > 1000000f) {
				int platinum = (int)(value / 1000000f);
				if (platinum > 50 && Main.rand.Next(2) == 0)
					platinum /= Main.rand.Next(3) + 1;

				if (Main.rand.Next(2) == 0)
					platinum /= Main.rand.Next(3) + 1;

				value -= 1000000 * platinum;
				CommonCode.DropItem(info, 74, platinum);
				continue;
			}

			if (value > 10000f) {
				int gold = (int)(value / 10000f);
				if (gold > 50 && Main.rand.Next(2) == 0)
					gold /= Main.rand.Next(3) + 1;

				if (Main.rand.Next(2) == 0)
					gold /= Main.rand.Next(3) + 1;

				value -= 10000 * gold;
				CommonCode.DropItem(info, 73, gold);
				continue;
			}

			if (value > 100f) {
				int silver = (int)(value / 100f);
				if (silver > 50 && Main.rand.Next(2) == 0)
					silver /= Main.rand.Next(3) + 1;

				if (Main.rand.Next(2) == 0)
					silver /= Main.rand.Next(3) + 1;

				value -= 100 * silver;
				CommonCode.DropItem(info, 72, silver);
				continue;
			}

			int copper = (int)value;
			if (copper > 50 && Main.rand.Next(2) == 0)
				copper /= Main.rand.Next(3) + 1;

			if (Main.rand.Next(2) == 0)
				copper /= Main.rand.Next(4) + 1;

			if (copper < 1)
				copper = 1;

			value -= copper;
			CommonCode.DropItem(info, 71, copper);
		}
		return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
	}

	// Coin stacks depend on runtime context and cannot be represented by a fixed rate.
	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) => Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
}
