using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using static Terraria.GameContent.ItemDropRules.ItemDropRule;

namespace Terraria.ModLoader;

public static class ChestLootLoader
{
	static Dictionary<string, List<IItemDropRule>> lootPools = [];
	static ChestLootLoader()
	{
		RegisterDefaultLootPools();
	}

	public static void Add(string name, List<IItemDropRule> lootPool)
	{
		lootPools.Add(name, lootPool);
	}

	internal static void Unload()
	{
		RegisterDefaultLootPools();
	}

	internal static void RegisterDefaultLootPools()
	{
		lootPools.Clear();
		lootPools["SurfaceWooden"] = [
			new OneFromRulesRule(1,
				Common(ItemID.Spear),
				Common(ItemID.Blowpipe),
				Common(ItemID.WoodenBoomerang),
				Common(ItemID.Aglet),
				Common(ItemID.ClimbingClaws),
				Common(ItemID.Umbrella),
				Common(ItemID.CordageGuide),
				Common(ItemID.WandofSparking),
				Common(ItemID.Radar),
				Common(ItemID.PortableStool)
			),
			Common(ItemID.Glowstick, chanceDenominator: 6, minimumDropped: 40, maximumDropped: 75),
			Common(ItemID.ThrowingKnife, chanceDenominator: 6, minimumDropped: 150, maximumDropped: 300),
			Common(ItemID.HerbBag, chanceDenominator: 6, minimumDropped: 1, maximumDropped: 4),
			Common(ItemID.CanOfWorms, chanceDenominator: 6, minimumDropped: 1, maximumDropped: 4),
			Common(ItemID.Grenade, chanceDenominator: 3, minimumDropped: 3, maximumDropped: 5),
			new OneFromRulesRule(2,
				new OneFromRulesRule(2,
					Common(ItemID.CopperBar, minimumDropped: 3, maximumDropped: 10),
					Common(ItemID.TinBar, minimumDropped: 3, maximumDropped: 10)
				),
				new OneFromRulesRule(2,
					Common(ItemID.IronBar, minimumDropped: 3, maximumDropped: 10),
					Common(ItemID.LeadBar, minimumDropped: 3, maximumDropped: 10)
				)
			),
			Common(ItemID.Rope, chanceDenominator: 2, minimumDropped: 50, maximumDropped: 100),
			new OneFromRulesRule(3, 2,
				Common(ItemID.WoodenArrow, minimumDropped: 25, maximumDropped: 50),
				Common(ItemID.Shuriken, minimumDropped: 25, maximumDropped: 50)
			),
			Common(ItemID.LesserHealingPotion, chanceDenominator: 2, minimumDropped: 3, maximumDropped: 5),
			new CommonDrop(ItemID.RecallPotion, chanceDenominator: 2, chanceNumerator: 3, amountDroppedMinimum: 3, amountDroppedMaximum: 5),
			new OneFromRulesRule(3, 2,
				Common(ItemID.IronskinPotion, maximumDropped: 2),
				Common(ItemID.ShinePotion, maximumDropped: 2),
				Common(ItemID.NightOwlPotion, maximumDropped: 2),
				Common(ItemID.SwiftnessPotion, maximumDropped: 2),
				Common(ItemID.MiningPotion, maximumDropped: 2),
				Common(ItemID.BuilderPotion, maximumDropped: 2)
			),
			new OneFromRulesRule(2,
				Common(ItemID.Torch, minimumDropped: 10, maximumDropped: 20),
				Common(ItemID.Bottle, minimumDropped: 10, maximumDropped: 20)
			),
			Common(ItemID.SilverCoin, chanceDenominator: 2, minimumDropped: 10, maximumDropped: 29),
			Common(ItemID.Wood, chanceDenominator: 2, minimumDropped: 50, maximumDropped: 99),
		];
	}

	public static List<IItemDropRule> GetLootPool(string name) => lootPools.TryGetValue(name, out var pool) ? pool : null;
}
