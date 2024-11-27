using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.ItemDropRules.VanillaChests;
using Terraria.ID;
using Terraria.Localization;
using Terraria.WorldBuilding;
using static Terraria.GameContent.ItemDropRules.ItemDropRule;
using static Terraria.ModLoader.ChestLootLoader;
using Conditions = Terraria.GameContent.ItemDropRules.Conditions;

namespace Terraria.ModLoader;

public static class ChestLootLoader
{
	private static readonly Dictionary<string, List<IItemDropRule>> lootPools = [];
	private static readonly Dictionary<string, List<ItemPoolEntry>> itemPools = [];
	static ChestLootLoader()
	{
		RegisterDefaultLootPools();
	}

	public static void AddLootPool(string name, List<IItemDropRule> lootPool)
	{
		lootPools.Add(name, lootPool);
	}
	public static void AddItemPool(string name, List<ItemPoolEntry> itemPool)
	{
		itemPools.Add(name, itemPool);
	}

	internal static void Unload()
	{
		RegisterDefaultLootPools();
	}

	internal static void RegisterDefaultLootPools()
	{
		//TODO: ordered with random replacement: jungle chests, normal water chests
		lootPools.Clear();
		itemPools.Clear();
		SetupItemPools();

		lootPools[LootPoolNames.SurfaceCommon] = [
			Common(ItemID.Glowstick, chanceDenominator: 6, minimumDropped: 40, maximumDropped: 75),
			Common(ItemID.ThrowingKnife, chanceDenominator: 6, minimumDropped: 150, maximumDropped: 300),
			Common(ItemID.HerbBag, chanceDenominator: 6, minimumDropped: 1, maximumDropped: 4),
			Common(ItemID.CanOfWorms, chanceDenominator: 6, minimumDropped: 1, maximumDropped: 4),
			Common(ItemID.Grenade, chanceDenominator: 3, minimumDropped: 3, maximumDropped: 5),
			new OneFromRulesRule(2,
				new DropFromItemPoolRule(ItemPoolNames.CopperBar, 1, amountDroppedMinimum: 3, amountDroppedMaximum: 10),
				new DropFromItemPoolRule(ItemPoolNames.IronBar, 1, amountDroppedMinimum: 3, amountDroppedMaximum: 10)
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
		lootPools[LootPoolNames.UndergroundCommon] = [
			ByCondition(new Conditions.IsNotChestType(TileID.Containers2, 10), ItemID.Bomb, 3, 10, 20),
			Common(ItemID.AngelStatue, 5),
			Common(ItemID.Rope, 3, 50, 100),
			new OneFromRulesRule(2,
				new DropFromItemPoolRule(ItemPoolNames.IronBar, 1, amountDroppedMinimum: 5, amountDroppedMaximum: 15),
				new DropFromItemPoolRule(ItemPoolNames.SilverBar, 1, amountDroppedMinimum: 5, amountDroppedMaximum: 15)
			),
			new OneFromRulesRule(2,
				Common(ItemID.WoodenArrow, minimumDropped: 25, maximumDropped: 49),
				Common(ItemID.Shuriken, minimumDropped: 25, maximumDropped: 49)
			),
			Common(ItemID.LesserHealingPotion, minimumDropped: 3, maximumDropped: 5),
			new OneFromRulesRule(3, 2,
				Common(ItemID.RegenerationPotion, maximumDropped: 2),
				Common(ItemID.ShinePotion, maximumDropped: 2),
				Common(ItemID.NightOwlPotion, maximumDropped: 2),
				Common(ItemID.SwiftnessPotion, maximumDropped: 2),
				Common(ItemID.ArcheryPotion, maximumDropped: 2),
				Common(ItemID.GillsPotion, maximumDropped: 2),
				Common(ItemID.HunterPotion, maximumDropped: 2),
				Common(ItemID.MiningPotion, maximumDropped: 2),
				Common(ItemID.TrapsightPotion, maximumDropped: 2)
			),
			new CommonDrop(ItemID.RecallPotion, 2, 2, 4, 2),
			SequentialRules(2,
				ByCondition(new Conditions.IsChestType(TileID.Containers, 11), ItemID.IceTorch, minimumDropped: 10, maximumDropped: 20),
				Common(ItemID.Torch, minimumDropped: 10, maximumDropped: 20)
			),
			Common(ItemID.SilverCoin, 2, 50, 89)
		];
		lootPools[LootPoolNames.CavernsCommon] = [
			Common(ItemID.SuspiciousLookingEye, 5),
			Common(ItemID.Dynamite, 3),
			Common(ItemID.JestersArrow, minimumDropped: 25, maximumDropped: 50),
			new OneFromRulesRule(2,
				new DropFromItemPoolRule(ItemPoolNames.GoldBar, 1, amountDroppedMinimum: 3, amountDroppedMaximum: 10),
				new DropFromItemPoolRule(ItemPoolNames.SilverBar, 1, amountDroppedMinimum: 3, amountDroppedMaximum: 10)
			),
			new OneFromRulesRule(2,
				Common(ItemID.FlamingArrow, minimumDropped: 25, maximumDropped: 50),
				Common(ItemID.ThrowingKnife, minimumDropped: 25, maximumDropped: 50)
			),
			Common(ItemID.HealingPotion, minimumDropped: 3, maximumDropped: 5),
			new OneFromRulesRule(3, 2,
				Common(ItemID.SpelunkerPotion, maximumDropped: 2),
				Common(ItemID.FeatherfallPotion, maximumDropped: 2),
				Common(ItemID.NightOwlPotion, maximumDropped: 2),
				Common(ItemID.WaterWalkingPotion, maximumDropped: 2),
				Common(ItemID.ArcheryPotion, maximumDropped: 2),
				Common(ItemID.GravitationPotion, maximumDropped: 2)
			),
			new OneFromRulesRule(3, 1,
				Common(ItemID.ThornsPotion, maximumDropped: 2),
				Common(ItemID.InvisibilityPotion, maximumDropped: 2),
				Common(ItemID.HunterPotion, maximumDropped: 2),
				Common(ItemID.TrapsightPotion, maximumDropped: 2),
				Common(ItemID.TeleportationPotion, maximumDropped: 2),
				Common(ItemID.TitanPotion, maximumDropped: 2)
			),
			new CommonDrop(ItemID.RecallPotion, 2, 2, 4),
			new OneFromRulesRule(2,
				SequentialRules(2,
					ByCondition(new Conditions.IsChestType(TileID.Containers, 11), ItemID.IceTorch, minimumDropped: 15, maximumDropped: 29),
					Common(ItemID.Torch, minimumDropped: 15, maximumDropped: 29)
				),
				Common(ItemID.Glowstick, minimumDropped: 15, maximumDropped: 29)
			),
			Common(ItemID.GoldCoin, 2, 1, 2)
		];
		lootPools[LootPoolNames.HellCommon] = [
			Common(ItemID.Bomb, 3, 10, 20),
			new OneFromRulesRule(2,
				Common(ItemID.MeteoriteBar, minimumDropped: 15, maximumDropped: 29),
				new DropFromItemPoolRule(ItemPoolNames.GoldBar, amountDroppedMinimum: 15, amountDroppedMaximum: 29)
			),
			new DropFromItemPoolRule(ItemPoolNames.HellChestAmmo, 2, 50, 74),
			Common(ItemID.RestorationPotion, minimumDropped: 15, maximumDropped: 20),
			new OneFromRulesRule(4, 3,
				Common(ItemID.SpelunkerPotion, maximumDropped: 2),
				Common(ItemID.FeatherfallPotion, maximumDropped: 2),
				Common(ItemID.ManaRegenerationPotion, maximumDropped: 2),
				Common(ItemID.ObsidianSkinPotion, maximumDropped: 2),
				Common(ItemID.MagicPowerPotion, maximumDropped: 2),
				Common(ItemID.InvisibilityPotion, maximumDropped: 2),
				Common(ItemID.HunterPotion, maximumDropped: 2),
				Common(ItemID.HeartreachPotion, maximumDropped: 2)
			),
			new OneFromRulesRule(3, 2,
				Common(ItemID.GravitationPotion, maximumDropped: 2),
				Common(ItemID.ThornsPotion, maximumDropped: 2),
				Common(ItemID.WaterWalkingPotion, maximumDropped: 2),
				Common(ItemID.ObsidianSkinPotion, maximumDropped: 2),
				Common(ItemID.BattlePotion, maximumDropped: 2),
				Common(ItemID.TeleportationPotion, maximumDropped: 2),
				Common(ItemID.InfernoPotion, maximumDropped: 2),
				Common(ItemID.LifeforcePotion, maximumDropped: 2)
			),
			new OneFromRulesRule(3,
				Common(ItemID.RecallPotion, minimumDropped: 1, maximumDropped: 2),
				Common(ItemID.PotionOfReturn, minimumDropped: 1, maximumDropped: 2)
			),
			new OneFromRulesRule(2,
				Common(ItemID.Torch, minimumDropped: 15, maximumDropped: 29),
				Common(ItemID.Glowstick, minimumDropped: 15, maximumDropped: 29)
			),
			Common(ItemID.GoldCoin, 2, 2, 4)
		];

		lootPools[LootPoolNames.HeightBasedCommon] = [
			SequentialRules(1,
				new LeadingConditionRule(new Conditions.UndergroundChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.UndergroundCommon)),
				new LeadingConditionRule(new Conditions.CavernsChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.CavernsCommon)),
				new DropLootPoolRule(LootPoolNames.Hell)
			)
		];

		lootPools[LootPoolNames.HeightBasedGeneric] = [
			SequentialRules(1,
				new LeadingConditionRule(new Conditions.UndergroundChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.Underground)),
				new LeadingConditionRule(new Conditions.CavernsChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.Caverns)),
				new DropLootPoolRule(LootPoolNames.Hell)
			)
		];

		lootPools[LootPoolNames.SurfaceWooden] = [
			new DropFromItemPoolRule(ItemPoolNames.SurfaceWoodenRare),
			new DropLootPoolRule(LootPoolNames.SurfaceCommon),
		];

		lootPools[LootPoolNames.Wooden] = [
			SequentialRules(1,
				new LeadingConditionRule(new Conditions.SurfaceChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.SurfaceWooden)),
				new DropLootPoolRule(LootPoolNames.HeightBasedGeneric)
			)
		];
		lootPools[LootPoolNames.Underground] = [
			new DropFromItemPoolRule(ItemPoolNames.UndergroundRare),
				new DropLootPoolRule(LootPoolNames.UndergroundCommon)
		];
		lootPools[LootPoolNames.CavernsRare] = [
			new LeadingConditionRule(new Conditions.LavaLayerChest()).WithOnSuccess(SequentialRules(1,
				ByCondition(new Conditions.TenthAnniversaryIsUp(), ItemID.LavaCharm, 15),
				ByCondition(new Conditions.TenthAnniversaryIsNotUp(), ItemID.LavaCharm, 20),
				new DropFromItemPoolRule(ItemPoolNames.CavernsRareNoLavaCharm)
			)).WithOnFailedConditions(new DropFromItemPoolRule(ItemPoolNames.CavernsRareNoLavaCharm))
		];
		lootPools[LootPoolNames.JungleRare] = [
			SequentialRules(1,
				Common(ItemID.Seaweed, 50),
				Common(ItemID.FiberglassFishingPole, 15),
				Common(ItemID.FlowerBoots, 20),
				new ChestSequentialPrimaryRule(ItemPoolNames.JungleRareSequential, () => GenVars.JungleItemCount)
			)
		];
		lootPools[LootPoolNames.WaterAnywhereRare] = [
			SequentialRules(1,
				ByCondition(new Conditions.TenthAnniversaryIsUp(), ItemID.WaterWalkingBoots, 7),
				ByCondition(new Conditions.TenthAnniversaryIsNotUp(), ItemID.WaterWalkingBoots, 10),
				new ChestSequentialPrimaryRule(ItemPoolNames.WaterAnywhereRareSequential, () => GenVars.WaterItemCount)
			)
		];
		lootPools[LootPoolNames.Caverns] = [
			new DropLootPoolRule(LootPoolNames.CavernsRare),
			new DropLootPoolRule(LootPoolNames.CavernsCommon),
		];
		lootPools[LootPoolNames.Hell] = [
			new DropFromItemPoolRule(ItemPoolNames.HellRare),
			new DropLootPoolRule(LootPoolNames.HellCommon),
		];

		lootPools[LootPoolNames.PyramidGold] = [
			new DropFromItemPoolRule(ItemPoolNames.PyramidGoldRare),
			ByCondition(new Conditions.TenthAnniversaryIsUp(), ItemID.PharaohsMask).WithOnSuccess(Common(ItemID.PharaohsRobe)),
			new DropLootPoolRule(LootPoolNames.SurfaceCommon),
		];
		lootPools[LootPoolNames.Frozen] = [
			new DropFromItemPoolRule(ItemPoolNames.FrozenRare, 1),
			Common(ItemID.IceMirror, 5),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.SandstoneHigh] = [
			new DropFromItemPoolRule(ItemPoolNames.SandstoneHighRare),
			ByCondition(new Conditions.UndergroundChest(), ItemID.ScarabBomb, 3, 10, 20),
			ByCondition(new Conditions.CavernsChest(), ItemID.EncumberingStone, 7),
			ByCondition(new Conditions.CavernsChest(), ItemID.DesertMinecart, 15),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.SandstoneLow] = [
			new DropFromItemPoolRule(ItemPoolNames.SandstoneLowRare),
			ByCondition(new Conditions.UndergroundChest(), ItemID.ScarabBomb, 3, 10, 20),
			ByCondition(new Conditions.CavernsChest(), ItemID.EncumberingStone, 7),
			ByCondition(new Conditions.CavernsChest(), ItemID.DesertMinecart, 15),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.Jungle] = [
			new DropLootPoolRule(LootPoolNames.JungleRare),
			Common(ItemID.LivingMahoganyWand, 6).WithOnSuccess(Common(ItemID.LivingMahoganyLeafWand)),
			Common(ItemID.BeeMinecart, 10),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.WaterOceanCave] = [
			new DropFromItemPoolRule(ItemPoolNames.WaterOceanCaveRare),
			new DropLootPoolRule(LootPoolNames.WaterSimple),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.WaterAnywhere] = [
			new DropLootPoolRule(LootPoolNames.WaterAnywhereRare),
			new DropLootPoolRule(LootPoolNames.WaterSimple),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.WaterSimple] = [
			Common(ItemID.SharkBait, 2),
			Common(ItemID.SandcastleBucket, 2)
		];
		lootPools[LootPoolNames.LivingWood] = [
			new DropFromItemPoolRule(ItemPoolNames.LivingWoodRare),
			new OneFromRulesRule(10,
				Common(ItemID.SunflowerMinecart),
				Common(ItemID.LadybugMinecart)
			),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.MushroomHigh] = [
			Common(ItemID.ShroomMinecart, 2),
			Common(ItemID.MushroomHat, 3).WithOnSuccess(Common(ItemID.MushroomVest)).WithOnSuccess(Common(ItemID.MushroomPants)),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.MushroomLow] = [
			new DropFromItemPoolRule(ItemPoolNames.MushroomLowUncommon),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon)
		];
		lootPools[LootPoolNames.Shadow] = [
			new ChestSequentialPrimaryRule(ItemPoolNames.ShadowRare, () => GenVars.hellChest),
			Common(ItemID.TreasureMagnet, 5),
			Common(ItemID.HellMinecart, 10),
			Common(ItemID.OrnateShadowKey, 10),
			Common(ItemID.HellCake, 10),
			new DropLootPoolRule(LootPoolNames.HellCommon)
		];
		lootPools[LootPoolNames.FloatingIsland] = [
			new FloatingIslandPrimaryRule(),
			Common(ItemID.CreativeWings, 40),
			new DropLootPoolRule(LootPoolNames.HeightBasedCommon),
			Common(ItemID.SkyMill, 3),
			new DropFromItemPoolRule(ItemPoolNames.FloatingIslandPainting),
			Common(ItemID.Cloud, minimumDropped: 50, maximumDropped: 100)
		];
		lootPools[LootPoolNames.BiomeChestExtras] = [
			Common(ItemID.RemnantsofDevotion, 2)
		];
	}
	private static void SetupItemPools()
	{
		AddItemPool(ItemPoolNames.SurfaceWoodenRare, [
			new(ItemID.Spear),
			new(ItemID.Blowpipe),
			new(ItemID.WoodenBoomerang),
			new(ItemID.Aglet),
			new(ItemID.ClimbingClaws),
			new(ItemID.Umbrella),
			new(ItemID.CordageGuide),
			new(ItemID.WandofSparking, [new Conditions.NotRemixSeed()]),
			new(ItemID.MagicDagger, [new Conditions.RemixSeed()]),
			new(ItemID.Radar),
			new(ItemID.PortableStool)
		]);
		AddItemPool(ItemPoolNames.UndergroundRare, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots),
			new(ItemID.Mace),
			new(ItemID.ShoeSpikes)
		]);
		AddItemPool(ItemPoolNames.CavernsRareNoLavaCharm, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots),
			new(ItemID.Mace),
			new(ItemID.ShoeSpikes),
			new(ItemID.FlareGun, ChainedRules: [Common(ItemID.Flare, minimumDropped: 25, maximumDropped: 50)]),
			..GetReplacementValues(7, (ItemID.Extractinator, 1f / 15f))
		]);
		AddItemPool(ItemPoolNames.HellRare, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots)
		]);
		AddItemPool(ItemPoolNames.PyramidGoldRare, [
			new(ItemID.PharaohsMask, Conditions: [new Conditions.TenthAnniversaryIsNotUp()], ChainedRules: [Common(ItemID.PharaohsRobe)], Weight: 1),
			new(ItemID.FlyingCarpet, Conditions: [new Conditions.TenthAnniversaryIsUp()], Weight: 1),
			new(ItemID.FlyingCarpet, Weight: 4),
			new(ItemID.SandstorminaBottle, Weight: 4)
		]);
		AddItemPool(ItemPoolNames.DungeonGoldRare, [ // ordered generation
			new(ItemID.Muramasa),
			new(ItemID.CobaltShield),
			new(ItemID.AquaScepter, [new Conditions.NotRemixSeed()]),
			new(ItemID.BubbleGun, [new Conditions.RemixSeed()]),
			new(ItemID.BlueMoon),
			new(ItemID.MagicMissile),
			new(ItemID.Valor),
			new(ItemID.GoldenKey, Weight: 0),
			new(ItemID.Handgun)
		]);
		AddItemPool(ItemPoolNames.FrozenRare, [
			new(ItemID.IceBoomerang),
			new(ItemID.IceBlade),
			new(ItemID.IceSkates),
			new(ItemID.SnowballCannon, [new Conditions.NotRemixSeed()]),
			new(ItemID.IceBow, [new Conditions.RemixSeed()]),
			new(ItemID.BlizzardinaBottle),
			new(ItemID.FlurryBoots),
			..GetReplacementValues(6, (ItemID.Extractinator, 0.049), (ItemID.Fish, 0.02))
		]);
		AddItemPool(ItemPoolNames.LivingWoodRare, [
			new(ItemID.LivingWoodWand, ChainedRules: [Common(ItemID.LeafWand)], Weight: 2),
			new(ItemID.BabyBirdStaff)
		]);
		AddItemPool(ItemPoolNames.MushroomLowUncommon, [
			new(ItemID.ShroomMinecart),
			new(ItemID.MushroomHat, ChainedRules: [Common(ItemID.MushroomVest), Common(ItemID.MushroomPants)])
		]);
		AddItemPool(ItemPoolNames.SandstoneHighRare, [
			new(ItemID.AncientChisel),
			new(ItemID.SandBoots),
			new(ItemID.MysticCoilSnake),
			new(ItemID.MagicConch)
		]);
		AddItemPool(ItemPoolNames.SandstoneLowRare, [
			new(ItemID.ThunderSpear),
			new(ItemID.ThunderStaff),
			new(ItemID.CatBast)
		]);
		AddItemPool(ItemPoolNames.FloatingIslandRare, [ // ordered generation
			new(ItemID.ShinyRedBalloon),
			new(ItemID.Starfury),
			new(ItemID.LuckyHorseshoe),
			new(ItemID.CelestialMagnet),
		]);
		AddItemPool(ItemPoolNames.FloatingIslandPainting, [
			new(ItemID.SeeTheWorldForWhatItIs),
			new(ItemID.HighPitch),
			new(ItemID.BlessingfromTheHeavens),
			new(ItemID.Constellation),
			new(ItemID.LoveisintheTrashSlot),
			new(ItemID.SunOrnament),
		]);
		AddItemPool(ItemPoolNames.WaterOceanCaveRare, [
			new(ItemID.WaterWalkingBoots),
			new(ItemID.BreathingReed),
			new(ItemID.Trident),
			new(ItemID.Flipper),
			new(ItemID.FloatingTube),
		]);
		AddItemPool(ItemPoolNames.WebRare, [
			new(ItemID.WebSlinger)
		]);
		AddItemPool(ItemPoolNames.ShadowRare, [ // ordered generation
			new(ItemID.DarkLance),
			new(ItemID.Sunfury),
			new(ItemID.FlowerofFire, [new Conditions.NotRemixSeed()]),
			new(ItemID.UnholyTrident, [new Conditions.RemixSeed()]),
			new(ItemID.Flamelash),
			new(ItemID.HellwingBow),
		]);

		AddItemPool(ItemPoolNames.JungleRareSequential, [
			new(ItemID.FeralClaws),
			new(ItemID.AnkletoftheWind),
			new(ItemID.StaffofRegrowth),
			new(ItemID.Boomstick),
		]);

		AddItemPool(ItemPoolNames.WaterAnywhereRareSequential, [
			new(ItemID.BreathingReed),
			new(ItemID.FloatingTube),
			new(ItemID.Trident),
			new(ItemID.Flipper),
		]);

		AddItemPool(ItemPoolNames.CopperBar, [
			new(ItemID.CopperBar, [new Conditions.SavedOreTierCopper(TileID.Copper)]),
			new(ItemID.TinBar, [new Conditions.SavedOreTierCopper(TileID.Tin)])
		]);
		AddItemPool(ItemPoolNames.IronBar, [
			new(ItemID.IronBar, [new Conditions.SavedOreTierIron(TileID.Iron)]),
			new(ItemID.LeadBar, [new Conditions.SavedOreTierIron(TileID.Lead)])
		]);
		AddItemPool(ItemPoolNames.SilverBar, [
			new(ItemID.SilverBar, [new Conditions.SavedOreTierSilver(TileID.Silver)]),
			new(ItemID.TungstenBar, [new Conditions.SavedOreTierSilver(TileID.Tungsten)])
		]);
		AddItemPool(ItemPoolNames.GoldBar, [
			new(ItemID.GoldBar, [new Conditions.SavedOreTierGold(TileID.Gold)]),
			new(ItemID.PlatinumBar, [new Conditions.SavedOreTierGold(TileID.Platinum)])
		]);
		AddItemPool(ItemPoolNames.HellChestAmmo, [
			new(ItemID.HellfireArrow),
			new(ItemID.TungstenBullet, [new Conditions.SavedOreTierSilver(TileID.Tungsten)]),
			new(ItemID.SilverBullet, [new Conditions.SavedOreTierSilver(TileID.Silver)])
		]);
	}
	internal static IEnumerable<ItemPoolEntry> GetReplacementValues(double baseWeight, params (short type, double chance)[] values)
	{
		double totalChance = 0;
		foreach ((_, double chance) in values) {
			totalChance += chance;
		}
		foreach ((int type, double chance) in values) {
			yield return new(type, Weight: (float)((chance * baseWeight) / (1 - totalChance)));
		}
	}
	public static List<IItemDropRule> GetLootPool(string name) => lootPools.TryGetValue(name, out var pool) ? pool : null;
	public static List<ItemPoolEntry> GetItemPool(string name) => itemPools.TryGetValue(name, out var pool) ? pool : null;
	public static Dictionary<string, List<IItemDropRule>>.KeyCollection GetLootPools() => lootPools.Keys;
	public static Dictionary<string, List<ItemPoolEntry>>.KeyCollection GetItemPools() => itemPools.Keys;
	public static class ItemPoolNames
	{
		public const string CopperBar = nameof(CopperBar);
		public const string IronBar = nameof(IronBar);
		public const string SilverBar = nameof(SilverBar);
		public const string GoldBar = nameof(GoldBar);
		public const string HellChestAmmo = nameof(HellChestAmmo);

		public const string SurfaceWoodenRare = nameof(SurfaceWoodenRare);
		public const string UndergroundRare = nameof(UndergroundRare);
		public const string CavernsRareNoLavaCharm = nameof(CavernsRareNoLavaCharm);
		public const string HellRare = nameof(HellRare);
		public const string PyramidGoldRare = nameof(PyramidGoldRare);
		public const string SandstoneHighRare = nameof(SandstoneHighRare);
		public const string SandstoneLowRare = nameof(SandstoneLowRare);
		public const string DungeonGoldRare = nameof(DungeonGoldRare);
		public const string FrozenRare = nameof(FrozenRare);
		public const string MushroomLowUncommon = nameof(MushroomLowUncommon);
		public const string LivingWoodRare = nameof(LivingWoodRare);
		public const string FloatingIslandRare = nameof(FloatingIslandRare);
		public const string FloatingIslandPainting = nameof(FloatingIslandPainting);
		public const string WaterOceanCaveRare = nameof(WaterOceanCaveRare);
		public const string WebRare = nameof(WebRare);
		public const string ShadowRare = nameof(ShadowRare);

		public const string JungleRareSequential = nameof(JungleRareSequential);
		public const string WaterAnywhereRareSequential = nameof(WaterAnywhereRareSequential);
	}
	public static class LootPoolNames
	{
		public const string Wooden = nameof(Wooden);

		public const string HeightBasedCommon = nameof(HeightBasedCommon);
		public const string HeightBasedGeneric = nameof(HeightBasedGeneric);

		public const string SurfaceCommon = nameof(SurfaceCommon);
		public const string UndergroundCommon = nameof(UndergroundCommon);
		public const string CavernsCommon = nameof(CavernsCommon);
		public const string HellCommon = nameof(HellCommon);

		public const string SurfaceWooden = nameof(SurfaceWooden);
		public const string Underground = nameof(Underground);
		public const string Caverns = nameof(Caverns);
		public const string Hell = nameof(Hell);
		public const string PyramidGold = nameof(PyramidGold);
		public const string SandstoneHigh = nameof(SandstoneHigh);
		public const string SandstoneLow = nameof(SandstoneLow);
		public const string DungeonGold = nameof(DungeonGold);
		public const string Frozen = nameof(Frozen);
		public const string Jungle = nameof(Jungle);
		public const string MushroomHigh = nameof(MushroomHigh);
		public const string MushroomLow = nameof(MushroomLow);
		public const string LivingWood = nameof(LivingWood);
		public const string FloatingIsland = nameof(FloatingIsland);
		public const string WaterOceanCave = nameof(WaterOceanCave);
		public const string WaterAnywhere = nameof(WaterAnywhere);
		public const string WaterSimple = nameof(WaterSimple);
		public const string Web = nameof(Web);
		public const string Shadow = nameof(Shadow);

		public const string CavernsRare = nameof(CavernsRare);
		public const string JungleRare = nameof(JungleRare);
		public const string WaterAnywhereRare = nameof(WaterAnywhereRare);
		public const string BiomeChestExtras = nameof(BiomeChestExtras);
	}
}
public record class ItemPoolEntry(int Type, List<IItemDropRuleCondition> Conditions = null, List<IItemDropRule> ChainedRules = null, float Weight = 1f)
{
	public List<IItemDropRuleCondition> Conditions { get; } = Conditions ?? [];
	public List<IItemDropRule> ChainedRules { get; } = ChainedRules ?? [];
}
