using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal static class VanillaUnderwaterCaveMainItemRule
{
	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public static void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		foreach (int denominator in new[] { 10, 7 }) {
			string seedCondition = denominator == 7 ? "Tenth-anniversary world" : "Non-anniversary world";
			float fallbackRate = parentDropRate * (denominator - 1f) / denominator;
			drops.Add(new ChestDropRateInfo(863, 1, 1, parentDropRate / denominator, $"Underwater cave chest main item; {seedCondition}, 1/{denominator} replacement roll."));
			drops.Add(new ChestDropRateInfo(186, 1, 1, fallbackRate, $"Underwater cave chest main item; {seedCondition}, cycle counter is one and the 863 roll fails."));
			drops.Add(new ChestDropRateInfo(4404, 1, 1, fallbackRate, $"Underwater cave chest main item; {seedCondition}, cycle counter is two and the 863 roll fails."));
			drops.Add(new ChestDropRateInfo(277, 1, 1, fallbackRate, $"Underwater cave chest main item; {seedCondition}, cycle counter is three and the 863 roll fails."));
			drops.Add(new ChestDropRateInfo(187, 1, 1, fallbackRate, $"Underwater cave chest main item; {seedCondition}, default cycle branch and the 863 roll fails."));
			drops.Add(new ChestDropRateInfo(268, 1, 1, fallbackRate, $"Underwater cave chest main item in Extra Liquid worlds; cycle counter is four and the 863 roll fails ({seedCondition})."));
		}
	}

	internal static int RollItem(UnifiedRandom random, ref int cycleCount, bool extraLiquid, bool tenthAnniversaryWorld)
	{
		cycleCount++;
		int denominator = tenthAnniversaryWorld ? 7 : 10;
		if (random.Next(denominator) == 0)
			return 863;

		switch (cycleCount) {
			case 1:
				return 186;
			case 2:
				return 4404;
			case 3:
				return 277;
			default:
				if (extraLiquid && cycleCount == 4)
					return 268;

				cycleCount = 0;
				return 187;
		}
	}
}