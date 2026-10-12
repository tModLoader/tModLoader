using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> Stores chest rules and provides <see cref="For"/>, drop-rate reporting, and rule resolution. </summary>
public static class ChestLootDatabase
{
	private static readonly Dictionary<int, List<IChestLootRule>> _rulesByChestID = new();

	private static readonly (IChestLootRule Rule, string Condition)[] _vanillaSupplementalRules = {
		(new CommonChestDropRule(4409, 25), "Coat Everything Echo variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(1869, 2), "World Is Frozen variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(3384, 7), "Portal Gun In Chests variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(678, 10), "Get Good World variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(5346, 15), "No Traps variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(5001, 15), "Drunk World variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(678, 30), "Drunk World variation; chest has loot and an empty slot."),
		(new OneFromBundlesChestDropRule(7,
			new[] { 1763, 1765, 1764 },
			new[] { 1752, 1753 },
			new[] { 1766, 1775, 1776 },
			new[] { 1777, 1778 },
			new[] { 1819, 1820 },
			new[] { 1857 },
			new[] { 4740, 4741, 4742 },
			new[] { 4738, 4739 },
			new[] { 4685, 4686 }), "Vampire seed; chest has loot and enough consecutive empty slots."),
		(new OneFromBundlesChestDropRule(1, new[] { 3360, 3361 }), "First qualifying living mahogany chest in the world."),
		(new OneFromBundlesChestDropRule(5, new[] { 3360, 3361 }), "Subsequent qualifying living mahogany chests; the unique generation flag is already set."),
		(new OneFromOptionsChestDropRule(3, 1, true, new[] { 3556, 2870, 1067, 1066, 5075, 4469 }), "Rainbow Stuff variation; chest has loot and an empty slot."),
		(new CommonChestDropRule(52, 5), "AddBuriedChest underground non-fixed-loot branch."),
		(new CommonChestDropRule(965, 3, 50, 100), "AddBuriedChest underground non-fixed-loot branch."),
		(new CommonChestDropRule(965, 2, 50, 100), "AddBuriedChest surface non-fixed-loot branch."),
		(new CommonChestDropRule(282, 6, 40, 75), "AddBuriedChest non-fixed-loot branch; standard glowsticks."),
		(new CommonChestDropRule(5643, 6, 40, 75), "AddBuriedChest Rainbow Glowsticks variation."),
		(new CommonChestDropRule(168, 3, 3, 5), "AddBuriedChest non-fixed-loot branch."),
		(new CommonChestDropRule(188, 2, 3, 5), "AddBuriedChest non-fixed-loot branch."),
		(new VanillaChestStackBonusDropRule(3093, 6, 1, (5, 2), (10, 3)), "AddBuriedChest item 3093 bonus-stack rolls."),
		(new VanillaChestStackBonusDropRule(4345, 6, 1, (5, 2), (10, 3)), "AddBuriedChest item 4345 bonus-stack rolls."),
		(new CommonChestDropRule(28, 2, 3, 5), "AddBuriedChest non-fixed-loot branch."),
		(new CommonChestDropRule(279, 6, 150, 300), "AddBuriedChest above-surface chest branch."),
		(new CommonChestDropRule(167, 3), "AddBuriedChest non-fixed-loot branch above the underworld."),
		(new CommonChestDropRule(167, 3), "AddBuriedChest non-fixed-loot underworld branch."),
		(new CommonChestDropRule(73, 2, 1, 2), "AddBuriedChest non-fixed-loot surface branch."),
		(new CommonChestDropRule(73, 2, 2, 4), "AddBuriedChest non-fixed-loot lower-depth branch."),
		(new CommonChestDropRule(2350, 3, 3, 5, 2, successOnNonZero: true), "AddBuriedChest above-surface non-fixed-loot branch."),
		(new CommonChestDropRule(2350, 2, 2, 4), "AddBuriedChest underground non-fixed-loot branch."),
		(new CommonChestDropRule(2350, 3, 2, 4, 2, successOnNonZero: true), "AddBuriedChest lower-depth non-fixed-loot branch."),
		(new CommonChestDropRule(72, 2, 10, 29), "AddBuriedChest above-surface non-fixed-loot branch."),
		(new CommonChestDropRule(72, 2, 50, 89), "AddBuriedChest underground non-fixed-loot branch."),
		(new CommonChestDropRule(9, 2, 50, 99), "AddBuriedChest above-surface non-fixed-loot branch."),
		(new CommonChestDropRule(51, 4, 25, 50), "AddBuriedChest underground non-fixed-loot branch."),
		(new CommonChestDropRule(4423, 3, 10, 19), "AddBuriedChest desert chest bonus branch."),
		(new CommonChestDropRule(166, 3, 10, 19), "AddBuriedChest non-desert underground bonus branch."),
		(new CommonChestDropRule(2204, 4), "AddBuriedChest special chest flag4 bonus."),
		(new CommonChestDropRule(753, 50), "AddBuriedChest special chest flag4 bonus."),
		(new CommonChestDropRule(2198, 7), "AddBuriedChest special chest flag2 bonus."),
		(new CommonChestDropRule(669, 50), "AddBuriedChest special chest flag2 bonus."),
		(new CommonChestDropRule(2197, 3), "AddBuriedChest special chest flag12 bonus."),
		(new CommonChestDropRule(2195), "AddBuriedChest flag13 guaranteed bonus."),
		(new CommonChestDropRule(2192, 8), "AddBuriedChest special chest flag9 bonus."),
		(new CommonChestDropRule(5515, 5), "AddBuriedChest flag9 bonus in Get Good World."),
		(new CommonChestDropRule(5234, 2), "AddBuriedChest selected dungeon chest styles 23-27."),
		(new CommonChestDropRule(5234, 2), "AddBuriedChest style 13 bonus in Containers2."),
		(new CommonChestDropRule(2767, 5), "AddBuriedChest flag13 dungeon chest bonus."),
		(new CommonChestDropRule(2766, 5, 3, 7, 4, successOnNonZero: true), "AddBuriedChest flag13 dungeon chest bonus."),
		(new CommonChestDropRule(4425, 2), "AddBuriedChest flag5 special-chest bonus."),
		(new CommonChestDropRule(4460, 2), "AddBuriedChest flag5 special-chest bonus."),
		(new CommonChestDropRule(5629, 3), "AddBuriedChest flag6 special-chest bonus."),
		(new OneFromOptionsChestDropRule(6, 1, new[] { 4429, 4427 }), "AddBuriedChest flag6 special-chest selection."),
		(new CommonChestDropRule(5528, 3, 1, 1, 2, successOnNonZero: true), "AddBuriedChest flag6 special-chest bonus."),
		(new CommonChestDropRule(3199, 5), "AddBuriedChest flag2 special-chest bonus."),
		(new CommonChestDropRule(4346, 7), "AddBuriedChest flag3 special-chest bonus."),
		(new CommonChestDropRule(4066, 15), "AddBuriedChest flag3 special-chest bonus."),
		(new CommonChestDropRule(4426, 10), "AddBuriedChest flag4 special-chest bonus."),
		(new CommonChestDropRule(5525, 10), "AddBuriedChest flag4 special-chest bonus."),
		(new CommonChestDropRule(866), "AddBuriedChest main item 848 companion."),
		(new CommonChestDropRule(933), "AddBuriedChest main item 832 companion."),
		(new OneFromBundlesChestDropRule(1, new[] { 848, 866 }), "AddBuriedChest tenth-anniversary fixed main-item bundle."),
		(new CommonChestDropRule(4978, 40), "AddBuriedChest flag12 prefix bonus."),
		(new CommonChestDropRule(5120, 5), "AddBuriedChest flag2 bonus selection."),
		(new CommonChestDropRule(43, 5), "AddBuriedChest non-flag2 bonus selection."),
		(new CommonChestDropRule(974, 2, 10, 20), "AddBuriedChest flag2-specific non-fixed-loot stack."),
		(new CommonChestDropRule(8, 2, 10, 20), "AddBuriedChest non-flag2 non-fixed-loot stack."),
		(new CommonChestDropRule(227, 2, 15, 20), "AddBuriedChest non-fixed-loot branch."),
		(new OneFromBundlesChestDropRule(2, true, new[] { 4450 }, new[] { 4779, 4780, 4781 }), "AddBuriedChest flag7 selects either the single-item bonus or three-item bundle."),
		(new CommonChestDropRule(5010, 5), "AddBuriedChest flag8 special-chest bonus."),
		(new CommonChestDropRule(4443, 10), "AddBuriedChest flag8 special-chest bonus."),
		(new CommonChestDropRule(4737, 10), "AddBuriedChest flag8 special-chest bonus."),
		(new CommonChestDropRule(4551, 10), "AddBuriedChest flag8 special-chest bonus."),
		(new CommonChestDropRule(329), "AddBuriedChest; first qualifying shadow-key chest in the world."),
		(new CommonChestDropRule(329, 3), "AddBuriedChest; later qualifying shadow-key chest while the unique key has already generated."),
		(new CommonChestDropRule(5465), "AddBuriedChest; first qualifying Ram Rune chest in the world."),
		(new CommonChestDropRule(5465, 8), "AddBuriedChest; later qualifying Ram Rune chest while the unique rune has already generated."),
		(new CommonChestDropRule(6156, 4), "AddBuriedChest flag9 chest bonus."),
		(new CommonChestDropRule(997, 20), "AddBuriedChest bonus roll; the preceding extra-item roll failed."),
		(new CommonChestDropRule(997, 15), "AddBuriedChest deeper branch fallback bonus roll."),
		(new CommonChestDropRule(906, 20), "AddBuriedChest lava-layer special item outside tenth-anniversary worlds."),
		(new CommonChestDropRule(906, 15), "AddBuriedChest lava-layer special item in tenth-anniversary worlds."),
		(new CommonChestDropRule(930, 20), "AddBuriedChest alternate bonus roll after the item 997 roll fails."),
		(new CommonChestDropRule(5379, 1, 50, 99), "AddBuriedChest Rainbow Stuff bonus paired with item 930."),
		(new CommonChestDropRule(931, 1, 25, 50), "AddBuriedChest standard bonus paired with item 930."),
		(new CommonChestDropRule(1260), "Rainbow Stuff seed and a qualifying fixed main item."),
		(new CommonChestDropRule(5597, 4), "Vampire seed when no biome/dungeon/hell-specific main-item flags apply."),
		(new OneFromOptionsChestDropRule(12, 1, new[] { 5499, 5500, 5501, 5502, 5503, 5504, 5505, 5506, 5507, 5508, 5509, 5484, 5485, 5534 }), "AddBuriedChest voice-change roll."),
		(new OneFromOptionsChestDropRule(12, 1, new[] { 5509, 5505, 5506, 5508 }), "AddBuriedChest vampire voice-change roll after the first voice-change roll fails."),
		(new OneFromOptionsChestDropRule(10, 1, new[] { 1800, 900 }), "Vampire seed overwrite when the 1-in-10 roll is zero."),
		(new OneFromOptionsChestDropRule(10, 1, 1, 1, false, false, false, false, 1, new[] { 1800, 900 }), "Vampire seed overwrite when the 1-in-10 roll is one."),
		(new OneFromOptionsChestDropRule(1, new[] { 5258, 5226, 5254, 5238, 5255, 5388 }), "AddBuriedChest special chest flag12 selection."),
		(new CommonChestDropRule(751, 1, 50, 100), "AddBuriedChest special chest flag12 stack bonus."),
		(new OneFromOptionsChestDropRule(1, new[] { 49, 50, 53, 54 }), "AddBuriedChest hell-chest fallback when no fixed main item was selected."),
		(new OneFromOptionsChestDropRule(1, new[] { 49, 50, 53, 54, 5011, 975 }), "AddBuriedChest rock-layer fallback; vampire and error-world rolls may replace the selected item."),
		(new OneFromOptionsChestDropRule(1, new[] { 49, 50, 53, 54, 5011, 975, 930 }), "AddBuriedChest deeper fixed-item fallback; 930 also grants a seed-specific secondary stack."),
		(new OneFromOptionsChestDropRule(1, new[] { 4056, 4055, 4262, 4263 }), "AddBuriedChest underground desert chest; one item from the upper desert-loot pool."),
		(new OneFromOptionsChestDropRule(1, new[] { 4061, 4062, 4276 }), "AddBuriedChest underground desert chest; one item from the lower desert-loot pool."),
		(new OneFromOptionsChestDropRule(1, new[] { 670, 724, 950, 1319, 987, 1579, 6153 }), "AddBuriedChest bee-themed biome chest; one item from its randomized main-loot pool; item 1319 becomes 725 in Remix worlds."),
		(new OneFromOptionsChestDropRule(1, new[] { 863, 186, 277, 187, 4404 }), "Underwater chest main item in standard worlds."),
		(new OneFromOptionsChestDropRule(1, new[] { 863, 186, 277, 187, 4404, 268 }), "Underwater chest main item in Extra Liquid worlds."),
		(new OneFromOptionsChestDropRule(1, new[] { 159, 65, 158, 2219 }), "Sky-island house main item after the first four houses."),
		(new OneFromOptionsChestDropRule(1, new[] { 832, 832, 4281 }), "Evil-biome house main item; item 832 has two of three outcomes and item 4281 has one of three."),
		(new VanillaDungeonStyleMainItemRule(), "Dungeon-style house main item; the initial zero option triggers a second three-way roll."),
		(new OneFromOptionsChestDropRule(1, new[] { 4008, 238, 2275, 3352, 3262, 3334, 4818, 1325, 4144, 3350, 4347, 1309, 1863, 485, 748, 1825, 1321, 5451, 3385, 3386, 3387, 3388, 4951, 4420, 3043, 2341, 2342, 2800, 3623, 4980, 4273, 4711 }), "Error World replacement options, conditional on the outer adjustment roll and balanced-chest mode being disabled; each of 32 options has equal conditional probability, while the adjustment denominator depends on active secret seeds."),
		(new OneFromOptionsChestDropRule(2, 1, 25, 50, true, false, false, new[] { 41, 279 }), "AddBuriedChest non-fixed-loot branch; one randomly selected ore stack."),
		(new OneFromOptionsChestDropRule(2, 1, 25, 50, true, false, false, new[] { 40, 42 }), "AddBuriedChest non-fixed-loot branch; one randomly selected gem stack."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.CopperBar, ItemID.IronBar }), "AddBuriedChest stack from copper and iron bars in standard-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.TinBar, ItemID.IronBar }), "AddBuriedChest stack from tin and iron bars in tin worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.CopperBar, ItemID.LeadBar }), "AddBuriedChest stack from copper and lead bars in lead worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.TinBar, ItemID.LeadBar }), "AddBuriedChest stack from tin and lead bars in alternate-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 5, 14, false, true, false, false, new int[] { ItemID.IronBar, ItemID.SilverBar }), "AddBuriedChest stack from iron and silver bars in standard-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 5, 14, false, true, false, false, new int[] { ItemID.LeadBar, ItemID.SilverBar }), "AddBuriedChest stack from lead and silver bars in lead worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 5, 14, false, true, false, false, new int[] { ItemID.IronBar, ItemID.TungstenBar }), "AddBuriedChest stack from iron and tungsten bars in tungsten worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 5, 14, false, true, false, false, new int[] { ItemID.LeadBar, ItemID.TungstenBar }), "AddBuriedChest stack from lead and tungsten bars in alternate-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.GoldBar, ItemID.SilverBar }), "AddBuriedChest stack from gold and silver bars in standard-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.PlatinumBar, ItemID.SilverBar }), "AddBuriedChest stack from platinum and silver bars in platinum worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.GoldBar, ItemID.TungstenBar }), "AddBuriedChest stack from gold and tungsten bars in tungsten worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 3, 10, false, true, false, false, new int[] { ItemID.PlatinumBar, ItemID.TungstenBar }), "AddBuriedChest stack from platinum and tungsten bars in alternate-ore worlds."),
		(new OneFromOptionsChestDropRule(2, 1, 15, 29, false, true, false, false, new int[] { 117, ItemID.GoldBar }), "AddBuriedChest stack from item 117 and gold bars."),
		(new OneFromOptionsChestDropRule(2, 1, 15, 29, false, true, false, false, new int[] { 117, ItemID.PlatinumBar }), "AddBuriedChest stack from item 117 and platinum bars."),
		(new OneFromOptionsChestDropRule(2, 1, 50, 75, false, true, false, false, new[] { 265, 4915 }), "AddBuriedChest stack from item 265 and the alternate silver-tier ore bar."),
		(new OneFromOptionsChestDropRule(2, 1, 50, 75, false, true, false, false, new[] { 265, 278 }), "AddBuriedChest stack from item 265 and the standard silver-tier ore bar."),
		(new OneFromOptionsChestDropRule(3, 2, 25, 50, true, true, false, new[] { 40, 42 }), "AddBuriedChest non-fixed-loot branch; gem stack on a nonzero Next(3) roll."),
		(new OneFromOptionsChestDropRule(2, 1, 10, 20, true, false, false, new[] { 8, 31 }), "AddBuriedChest non-fixed-loot branch; one randomly selected material stack."),
		(new OneFromOptionsChestDropRule(3, 2, 1, 2, true, true, false, new[] { 296, 295, 299, 302, 303, 305 }), "AddBuriedChest surface branch: randomized potion stack when Next(3) is nonzero."),
		(new OneFromOptionsChestDropRule(3, 1, 1, 2, true, false, true, new[] { 301, 297, 304, 2329, 2351, 2326 }), "AddBuriedChest surface branch: second randomized potion stack when Next(3) is the maximum value."),
		(new OneFromOptionsChestDropRule(2, 1, 15, 30, false, true, false, false, new[] { 8, 282 }), "AddBuriedChest lower-depth branch: healing item or standard glowsticks."),
		(new OneFromOptionsChestDropRule(2, 1, 15, 30, false, true, false, false, new[] { 8, 5643 }), "AddBuriedChest lower-depth branch in Rainbow Glowsticks worlds."),
		(new OneFromOptionsChestDropRule(3, 2, 1, 2, true, true, false, new[] { 292, 298, 299, 290, 2322, 2325 }), "AddBuriedChest underground branch: randomized potion stack when Next(3) is nonzero."),
		(new OneFromOptionsChestDropRule(4, 3, 1, 2, true, true, false, new[] { 296, 295, 293, 288, 294, 297, 304, 2323 }), "AddBuriedChest lower-depth branch: randomized potion stack when Next(4) is nonzero."),
		(new OneFromOptionsChestDropRule(3, 2, 1, 2, true, true, false, new[] { 305, 301, 302, 288, 300, 2351, 2348, 2345 }), "AddBuriedChest lower-depth branch: second randomized potion stack when Next(3) is nonzero."),
		(new OneFromOptionsChestDropRule(3, 2, 1, 2, true, true, false, new[] { 289, 298, 299, 290, 303, 291, 304, 2322, 2329 }), "AddBuriedChest underground branch: randomized potion stack when Next(3) is nonzero."),
		(new VanillaChestHealingGlowstickDropRule(), "AddBuriedChest depth branch; requires a successful primary or Vampire-seed fallback roll."),
		(new VanillaJungleChestMainItemRule(), "Jungle shrine chest main-item cycle and rare override selection."),
		(new OneFromOptionsChestDropRule(3, 1, 1, 2, true, true, false, false, new[] { 2350, 4870 }), "AddBuriedChest lower-depth branch: 1/3 chance of a randomized two-item stack."),
		(new OneFromOptionsChestDropRule(1, new[] { 280, 281, 284, 285, 953, 946, 3068, 3069, 3084, 4341, 6165 }), "AddBuriedChest underground non-fixed-loot pool in non-remix worlds."),
		(new OneFromOptionsChestDropRule(1, new[] { 280, 281, 284, 285, 953, 946, 3068, 517, 3084, 4341, 6165 }), "AddBuriedChest underground non-fixed-loot pool in remix worlds.")
	};

	/// <summary> Gets the <see cref="ChestLoot"/> registration interface for a chest type. </summary>
	public static ChestLoot For(int chestID) => new(chestID);

	/// <summary> Gets drop rates for registered rules and conditional vanilla branches. Vanilla branches may not be scoped to the chest type; consult <see cref="ChestDropRateInfo.Condition"/> to interpret them. These entries are not aggregate probabilities for the chest type. </summary>
	public static List<ChestDropRateInfo> GetDropRates(int chestID)
	{
		List<ChestDropRateInfo> drops = new();
		foreach (IChestLootRule rule in GetRules(chestID))
			rule.ReportDropRates(drops);

		foreach ((IChestLootRule rule, string condition) in _vanillaSupplementalRules) {
			if (IsMainItemPool(rule))
				continue;

			if (rule is CommonChestDropRule commonRule)
				commonRule.ReportDropRates(drops, condition);
			else if (rule is OneFromBundlesChestDropRule bundleRule)
				bundleRule.ReportDropRates(drops, 1f, condition);
			else if (rule is OneFromOptionsChestDropRule optionsRule)
				optionsRule.ReportDropRates(drops, condition);
			else if (rule is VanillaChestStackBonusDropRule stackBonusRule)
				stackBonusRule.ReportDropRates(drops, 1f, condition);
			else if (rule is VanillaChestHealingGlowstickDropRule healingGlowstickRule)
				healingGlowstickRule.ReportDropRates(drops, 1f, condition);
			else
				rule.ReportDropRates(drops);
		}

		VanillaChestMainItemDropRates.ReportDropRates(chestID, drops);
		return drops;
	}

	private static bool IsMainItemPool(IChestLootRule rule)
	{
		if (rule is VanillaJungleChestMainItemRule or VanillaDungeonStyleMainItemRule)
			return true;

		if (rule is not OneFromOptionsChestDropRule optionsRule)
			return false;

		return (optionsRule.FirstOptionItemType, optionsRule.OptionCount) switch {
			(4056, 4) or (4061, 3) or (670, 7) or (863, 5) or (863, 6) or (159, 4) or (832, 3) => true,
			_ => false
		};
	}

	/// <summary> Gets drop rates for registered rules and conditional vanilla branches. Vanilla branches may not be scoped to the chest type; consult <see cref="ChestDropRateInfo.Condition"/> to interpret them. These entries are not aggregate probabilities for the chest type. </summary>
	public static List<ChestDropRateInfo> GetDropRates(int chestID, int mainItemInChest)
	{
		if (mainItemInChest < ItemID.None || mainItemInChest >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(mainItemInChest));

		List<ChestDropRateInfo> drops = GetDropRates(chestID);
		if (mainItemInChest != ItemID.None)
			drops.Add(new ChestDropRateInfo(mainItemInChest, 1, 1, 1f, "Caller-supplied AddBuriedChest main item; conditional on successful placement and no world-generation replacement."));

		return drops;
	}

	internal static bool TryDropVanillaSupplement(Chest chest, UnifiedRandom random, int itemType, int chanceDenominator)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is CommonChestDropRule commonRule && commonRule.ItemType == itemType && commonRule.ChanceDenominator == chanceDenominator)
				return commonRule.TryDropAtFirstEmptySlot(chest, random);
		}

		throw new ArgumentException($"No declared vanilla chest supplement for item {itemType} at 1/{chanceDenominator} chance.");
	}

	internal static bool TryDropVanillaAtSlot(Chest chest, int slot, UnifiedRandom random, int itemType, int chanceDenominator, bool stackBeforeSetDefaults = false, int stackMin = 1, int stackMax = 1, int chanceNumerator = 1, bool applyRandomPrefix = false)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is CommonChestDropRule commonRule && commonRule.ItemType == itemType && commonRule.ChanceDenominator == chanceDenominator && commonRule.ChanceNumerator == chanceNumerator && commonRule.StackMin == stackMin && commonRule.StackMax == stackMax)
				return commonRule.TryDropAtSlot(chest, slot, random, stackBeforeSetDefaults, applyRandomPrefix);
		}

		throw new ArgumentException($"No declared vanilla chest rule for item {itemType} with chance {chanceNumerator}/{chanceDenominator} and stack range {stackMin}-{stackMax}.");
	}

	internal static bool RollVanillaCommonChance(UnifiedRandom random, int itemType, int chanceDenominator)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is CommonChestDropRule commonRule && commonRule.ItemType == itemType && commonRule.ChanceDenominator == chanceDenominator)
				return commonRule.RollChance(random);
		}

		throw new InvalidOperationException($"The declared vanilla chest chance for item {itemType} at 1/{chanceDenominator} was not registered.");
	}

	internal static bool TryDropVanillaStackBonusAtSlot(Chest chest, int slot, UnifiedRandom random, int itemType, int chanceDenominator)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is VanillaChestStackBonusDropRule stackBonusRule && stackBonusRule.ItemType == itemType && stackBonusRule.ChanceDenominator == chanceDenominator)
				return stackBonusRule.TryDropAtSlot(chest, slot, random);
		}

		throw new InvalidOperationException($"The declared vanilla chest stack-bonus rule for item {itemType} at 1/{chanceDenominator} chance was not registered.");
	}

	internal static bool TryDropVanillaHealingGlowsticksAtSlot(Chest chest, int slot, UnifiedRandom random, bool flag2, bool vampireSeed, int glowstickType)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is VanillaChestHealingGlowstickDropRule healingGlowstickRule)
				return healingGlowstickRule.TryDropAtSlot(chest, slot, random, flag2, vampireSeed, glowstickType);
		}

		throw new InvalidOperationException("The vanilla healing-item and glowstick chest rule was not registered.");
	}

	internal static bool TryApplyVanillaErrorWorldReplacementAtSlot(Chest chest, int slot, UnifiedRandom random, int chanceDenominator, bool balancedChests)
	{
		if (random.Next(chanceDenominator) != 0)
			return false;

		if (!balancedChests) {
			int itemType = RollVanillaOptionItemType(random, 32, 4008);
			if (itemType > 0)
				SetVanillaMainItemAtSlot(chest, slot, itemType);
		}

		chest.item[slot].Prefix(-1);
		return true;
	}

	internal static int RollVanillaJungleChestMainItem(UnifiedRandom random, int jungleItemCount)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is VanillaJungleChestMainItemRule jungleChestRule)
				return jungleChestRule.RollItem(random, jungleItemCount);
		}

		throw new InvalidOperationException("The vanilla jungle chest main-item rule was not registered.");
	}

	internal static int RollVanillaDungeonStyleMainItem(UnifiedRandom random, bool tenthAnniversaryWorld)
		=> VanillaDungeonStyleMainItemRule.RollItem(random, tenthAnniversaryWorld);

	internal static int RollVanillaUnderwaterCaveMainItem(UnifiedRandom random, ref int cycleCount, bool extraLiquid, bool tenthAnniversaryWorld)
		=> VanillaUnderwaterCaveMainItemRule.RollItem(random, ref cycleCount, extraLiquid, tenthAnniversaryWorld);

	internal static void GetVanillaDungeonBiomeChestInfo(int sequenceIndex, int chestCount, bool crimson, out ushort chestTileType, out int chestStyle, out int mainItemType)
		=> VanillaDungeonBiomeMainItemRule.GetDungeonBiomeChestInfo(sequenceIndex, chestCount, crimson, out chestTileType, out chestStyle, out mainItemType);

	internal static void DropVanillaFlag13Bonus(Chest chest, int slot, UnifiedRandom random)
	{
		int roll = random.Next(5);
		int itemType = roll == 0 ? 2767 : 2766;
		int stackMin = roll == 0 ? 1 : 3;
		int stackMax = roll == 0 ? 1 : 7;

		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is CommonChestDropRule commonRule && commonRule.ItemType == itemType && commonRule.StackMin == stackMin && commonRule.StackMax == stackMax) {
				commonRule.SetItemAtSlot(chest, slot, random);
				return;
			}
		}

		throw new InvalidOperationException($"The declared flag13 chest rule for item {itemType} was not registered.");
	}

	internal static void SetVanillaCommonAtSlot(Chest chest, int slot, UnifiedRandom random, int itemType, int stackMin = 1, int stackMax = 1, bool stackBeforeSetDefaults = false, bool applyRandomPrefix = false)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is CommonChestDropRule commonRule && commonRule.ItemType == itemType && commonRule.StackMin == stackMin && commonRule.StackMax == stackMax) {
				commonRule.SetItemAtSlot(chest, slot, random, stackBeforeSetDefaults, applyRandomPrefix);
				return;
			}
		}

		throw new InvalidOperationException($"The declared vanilla chest rule for item {itemType} with stack range {stackMin}-{stackMax} was not registered.");
	}

	internal static void SetVanillaMainItemAtSlot(Chest chest, int slot, int itemType, bool applyRandomPrefix = false)
	{
		if (chest == null)
			throw new ArgumentNullException(nameof(chest));
		if (chest.item == null || slot < 0 || slot >= chest.maxItems || slot >= chest.item.Length)
			throw new ArgumentOutOfRangeException(nameof(slot));

		chest.item[slot].SetDefaults(itemType);
		if (applyRandomPrefix)
			chest.item[slot].Prefix(-1);
	}

	internal static bool TryDropVanillaMonsterVanity(Chest chest, UnifiedRandom random)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromBundlesChestDropRule bundleRule && bundleRule.FirstBundleItemType == 1763)
				return bundleRule.TryDropAtFirstEmptySlot(chest, random);
		}

		throw new InvalidOperationException("The vanilla monster vanity chest rule was not registered.");
	}

	internal static void SetVanillaLivingMahoganyBundle(Chest chest, int slot, UnifiedRandom random)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromBundlesChestDropRule bundleRule && bundleRule.ChanceDenominator == 1 && bundleRule.FirstBundleItemType == 3360) {
				bundleRule.SetBundleAtSlot(chest, slot, random);
				return;
			}
		}

		throw new InvalidOperationException("The guaranteed living mahogany chest bundle was not registered.");
	}

	internal static bool TryDropVanillaLivingMahoganyBundle(Chest chest, int slot, UnifiedRandom random)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromBundlesChestDropRule bundleRule && bundleRule.ChanceDenominator == 5 && bundleRule.FirstBundleItemType == 3360)
				return bundleRule.TryDropAtSlot(chest, slot, random);
		}

		throw new InvalidOperationException("The conditional living mahogany chest bundle was not registered.");
	}

	internal static int DropVanillaBundleAtSlot(Chest chest, int slot, UnifiedRandom random, int firstItemType, int chanceDenominator)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromBundlesChestDropRule bundleRule && bundleRule.ChanceDenominator == chanceDenominator && bundleRule.FirstBundleItemType == firstItemType)
				return bundleRule.DropAtSlot(chest, slot, random);
		}

		throw new InvalidOperationException($"The vanilla chest bundle rule beginning with item {firstItemType} at 1/{chanceDenominator} chance was not registered.");
	}

	internal static void SetVanillaBundleAtSlot(Chest chest, int slot, UnifiedRandom random, int firstItemType)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromBundlesChestDropRule bundleRule && bundleRule.ChanceDenominator == 1 && bundleRule.FirstBundleItemType == firstItemType) {
				bundleRule.SetBundleAtSlot(chest, slot, random);
				return;
			}
		}

		throw new InvalidOperationException($"The guaranteed vanilla chest bundle beginning with item {firstItemType} was not registered.");
	}

	internal static bool TryDropVanillaOptions(Chest chest, UnifiedRandom random, int chanceDenominator, bool stackDyes, int chanceNumerator, int firstOptionItemType)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromOptionsChestDropRule optionsRule && optionsRule.ChanceDenominator == chanceDenominator && optionsRule.ChanceNumerator == chanceNumerator && optionsRule.FirstOptionItemType == firstOptionItemType)
				return optionsRule.TryDropAtFirstEmptySlot(chest, random, stackDyes);
		}

		throw new ArgumentException($"No declared vanilla chest option rule beginning with item {firstOptionItemType} at {chanceNumerator}/{chanceDenominator} chance.");
	}

	internal static bool TryDropVanillaOptionsAtSlot(Chest chest, int slot, UnifiedRandom random, int chanceDenominator, int optionCount, bool applyRandomPrefix = false, int replacementOptionIndex = -1, int replacementItemType = ItemID.None, int firstOptionItemType = ItemID.None, int chanceNumerator = 0, int stackMin = 1, int stackMax = 1, bool stackBeforeSetDefaults = false, bool successOnNonZero = false, bool successOnMaximum = false, bool stackBeforeOption = false, int secondOptionItemType = ItemID.None, int successRollValue = -1)
	{
		if (chanceNumerator == 0)
			chanceNumerator = chanceDenominator;

		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromOptionsChestDropRule optionsRule && optionsRule.ChanceDenominator == chanceDenominator && optionsRule.ChanceNumerator == chanceNumerator && optionsRule.OptionCount == optionCount && optionsRule.StackMin == stackMin && optionsRule.StackMax == stackMax && optionsRule.StackBeforeSetDefaults == stackBeforeSetDefaults && optionsRule.SuccessOnNonZero == successOnNonZero && optionsRule.SuccessOnMaximum == successOnMaximum && optionsRule.StackBeforeOption == stackBeforeOption && optionsRule.SuccessRollValue == successRollValue && (firstOptionItemType == ItemID.None || optionsRule.FirstOptionItemType == firstOptionItemType) && (secondOptionItemType == ItemID.None || optionsRule.SecondOptionItemType == secondOptionItemType))
				return optionsRule.TryDropAtSlot(chest, slot, random, applyRandomPrefix, replacementOptionIndex, replacementItemType, stackBeforeSetDefaults, stackBeforeOption);
		}

		throw new ArgumentException($"No declared vanilla chest option rule at 1/{chanceDenominator} chance.");
	}

	internal static int RollVanillaOptionIndex(UnifiedRandom random, int optionCount, int firstOptionItemType)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromOptionsChestDropRule optionsRule && optionsRule.ChanceDenominator == 1 && optionsRule.OptionCount == optionCount && optionsRule.FirstOptionItemType == firstOptionItemType)
				return optionsRule.RollOptionIndex(random);
		}

		throw new ArgumentException($"No declared vanilla chest option rule with {optionCount} options beginning with item {firstOptionItemType}.");
	}

	internal static int RollVanillaOptionItemType(UnifiedRandom random, int optionCount, int firstOptionItemType, int replacementOptionIndex = -1, int replacementItemType = ItemID.None, int chanceDenominator = 1)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromOptionsChestDropRule optionsRule && optionsRule.ChanceDenominator == chanceDenominator && optionsRule.OptionCount == optionCount && optionsRule.FirstOptionItemType == firstOptionItemType) {
				int optionIndex = optionsRule.RollOptionIndex(random);
				return optionsRule.GetOptionItemType(optionIndex, replacementOptionIndex, replacementItemType);
			}
		}

		throw new ArgumentException($"No declared vanilla chest option rule with {optionCount} options beginning with item {firstOptionItemType}.");
	}

	internal static void SetVanillaOptionAtSlot(Chest chest, int slot, int optionIndex, int optionCount, int firstOptionItemType, bool applyRandomPrefix)
	{
		foreach ((IChestLootRule rule, _) in _vanillaSupplementalRules) {
			if (rule is OneFromOptionsChestDropRule optionsRule && optionsRule.ChanceDenominator == 1 && optionsRule.OptionCount == optionCount && optionsRule.FirstOptionItemType == firstOptionItemType) {
				optionsRule.SetOptionAtSlot(chest, slot, optionIndex, applyRandomPrefix);
				return;
			}
		}

		throw new ArgumentException($"No declared vanilla chest option rule with {optionCount} options beginning with item {firstOptionItemType}.");
	}

	internal static List<IChestLootRule> GetRules(int chestID)
		=> _rulesByChestID.TryGetValue(chestID, out List<IChestLootRule> rules) ? new List<IChestLootRule>(rules) : new List<IChestLootRule>();

	internal static IChestLootRule Register(int chestID, IChestLootRule rule)
	{
		if (rule == null)
			throw new ArgumentNullException(nameof(rule));

		if (!_rulesByChestID.TryGetValue(chestID, out List<IChestLootRule> rules))
			_rulesByChestID[chestID] = rules = new();

		rules.Add(rule);
		return rule;
	}

	internal static IChestLootRule Remove(int chestID, IChestLootRule rule)
	{
		if (rule == null)
			throw new ArgumentNullException(nameof(rule));

		if (_rulesByChestID.TryGetValue(chestID, out List<IChestLootRule> rules))
			rules.Remove(rule);

		return rule;
	}

	/// <summary> Applies this rule to the chest contents. </summary>
	public static void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		foreach (IChestLootRule rule in GetRules(chest.type))
			rule.TryDrop(chest, random);
	}

	internal static void Unload() => _rulesByChestID.Clear();
}
