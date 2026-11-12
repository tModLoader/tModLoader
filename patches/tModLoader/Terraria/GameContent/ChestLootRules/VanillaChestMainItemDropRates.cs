using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal static class VanillaChestMainItemDropRates
{
	internal static void ReportDropRates(int chestID, List<ChestDropRateInfo> drops)
	{
		if (chestID == ChestID.Ivy)
			new VanillaJungleChestMainItemRule().ReportDropRates(drops);

		if (chestID == ChestID.Gold)
			new VanillaDungeonStyleMainItemRule().ReportDropRates(drops);

		if (chestID == ChestID.Water) {
			ReportOptions(drops, new[] { 863, 186, 277, 187, 4404 }, "Underwater surface chest main item in standard worlds.", 1f);
			ReportOptions(drops, new[] { 863, 186, 277, 187, 4404 }, "Underwater surface chest main item; Extra Liquid world before its second selection.", 1f);
			ReportOptions(drops, new[] { 863, 186, 277, 187, 4404, 268 }, "Underwater surface chest main item; Extra Liquid world final selection.", 1f);
			VanillaUnderwaterCaveMainItemRule.ReportDropRates(drops);
		}

		if (chestID == ChestID.Desert || chestID == ChestID.FromTile(TileID.Containers, 42)) {
			ReportOptions(drops, new[] { 4056, 4055, 4262, 4263 }, "Underground desert chest upper item pool when the upper layer is selected.", 1f);
			ReportOptions(drops, new[] { 4061, 4062, 4276 }, "Underground desert chest lower item pool when the lower layer is selected.", 1f);
		}

		if (chestID == ChestID.Frozen || chestID == ChestID.Snow) {
			ReportOptions(drops, new[] { 670, 724, 950, 1319, 987, 1579, 6153 }, "Bee-themed chest main-item pool outside Remix worlds.", 1f);
			ReportOptions(drops, new[] { 670, 724, 950, 725, 987, 1579, 6153 }, "Bee-themed chest main-item pool in Remix worlds; item 1319 is replaced with 725.", 1f);
		}

		if (chestID == ChestID.Skyware || chestID == ChestID.LockedGold)
			ReportOptions(drops, new[] { 159, 65, 158, 2219 }, "Sky-island house main item; random selection after the first four houses, or selected by house order.", 1f);

		if (chestID == ChestID.LivingWood)
			ReportOptions(drops, new[] { 832, 832, 4281 }, "Evil-biome house main item; 832 has two of three outcomes and 4281 has one of three.", 1f);

		if (chestID == ChestID.Lihzahrd)
			drops.Add(new ChestDropRateInfo(1293, 1, 1, 1f, "Main item passed to a temple chest by dungeon generation."));

		if (chestID == ChestID.WebCovered)
			drops.Add(new ChestDropRateInfo(939, 1, 1, 1f, "Main item passed to a web-covered chest by world generation."));

		VanillaDungeonBiomeMainItemRule.ReportDropRates(chestID, drops);
	}

	private static void ReportOptions(List<ChestDropRateInfo> drops, int[] itemTypes, string condition, float parentDropRate)
	{
		Dictionary<int, int> optionCounts = new();
		foreach (int itemType in itemTypes) {
			optionCounts.TryGetValue(itemType, out int count);
			optionCounts[itemType] = count + 1;
		}

		foreach ((int itemType, int optionCount) in optionCounts)
			drops.Add(new ChestDropRateInfo(itemType, 1, 1, parentDropRate * optionCount / itemTypes.Length, condition));
	}
}