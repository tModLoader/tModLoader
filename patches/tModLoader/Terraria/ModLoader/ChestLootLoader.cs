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

	private static readonly Dictionary<(string name, int type), SimpleItemDropRuleCondition> genVarConditions = [];
	public static SimpleItemDropRuleCondition GenVarCondition(string name, int type)
	{
		if (genVarConditions.TryGetValue((name, type), out SimpleItemDropRuleCondition condition))
			return condition;
		FieldInfo field = typeof(GenVars).GetField(name) ?? throw new ArgumentException($"No such static field {nameof(GenVars)}.{name} exists", nameof(name));
		if (field.FieldType != typeof(int))
			throw new ArgumentException($"Field type must be {typeof(int)}", nameof(name));
		DynamicMethod getterMethod = new($"GenVars.{name}_Equals_{type}", typeof(bool), [], true);
		ILGenerator gen = getterMethod.GetILGenerator();

		gen.Emit(OpCodes.Ldsfld, field);
		gen.Emit(OpCodes.Ldc_I4, type);
		gen.Emit(OpCodes.Ceq);
		gen.Emit(OpCodes.Ret);

		genVarConditions[(name, type)] = condition = new Condition(LocalizedText.Empty, getterMethod.CreateDelegate<Func<bool>>()).ToDropCondition(ShowItemDropInUI.Always);
		return condition;
	}
	public static ItemPoolEntry DropItemFromGenVar(string name, int type) => new(type, [GenVarCondition(name, type)]);
	private static readonly Dictionary<(string name, int type), SimpleItemDropRuleCondition> savedOreTierConditions = [];
	public static SimpleItemDropRuleCondition SavedOreTierCondition(string name, int type)
	{
		if (savedOreTierConditions.TryGetValue((name, type), out SimpleItemDropRuleCondition condition))
			return condition;
		FieldInfo field = typeof(WorldGen.SavedOreTiers).GetField(name) ?? throw new ArgumentException($"No such static field {nameof(WorldGen.SavedOreTiers)}.{name} exists", nameof(name));
		if (field.FieldType != typeof(int))
			throw new ArgumentException($"Field type must be {typeof(int)}", nameof(name));
		DynamicMethod getterMethod = new($"WorldGen.SavedOreTiers.{name}_Equals_{type}", typeof(bool), [], true);
		ILGenerator gen = getterMethod.GetILGenerator();

		gen.Emit(OpCodes.Ldsfld, field);
		gen.Emit(OpCodes.Ldc_I4, type);
		gen.Emit(OpCodes.Ceq);
		gen.Emit(OpCodes.Ret);

		savedOreTierConditions[(name, type)] = condition = new Condition(LocalizedText.Empty, getterMethod.CreateDelegate<Func<bool>>()).ToDropCondition(ShowItemDropInUI.Always);
		return condition;
	}
	public static ItemPoolEntry DropItemFromSavedOreTier(string name, int oreType, int itemType) => new(itemType, [SavedOreTierCondition(name, oreType)]);
	internal static void RegisterDefaultLootPools()
	{
		//TODO: ordered with random replacement: jungle chests, normal water chests
		genVarConditions.Clear();
		savedOreTierConditions.Clear();
		lootPools.Clear();
		itemPools.Clear();
		SetupItemPools();

		lootPools[LootPoolNames.SurfaceGeneric] = [
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
		lootPools[LootPoolNames.UndergroundGeneric] = [
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
		lootPools[LootPoolNames.CavernsGeneric] = [
			Common(ItemID.SuspiciousLookingEye, 5),
			Common(ItemID.Dynamite, 3),
			Common(ItemID.JestersArrow, 25, 50),
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
		lootPools[LootPoolNames.HellGeneric] = [
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

		lootPools[LootPoolNames.HeightBasedGeneric] = [
			SequentialRules(1,
				new LeadingConditionRule(new Conditions.SurfaceChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.SurfaceGeneric)),
				new LeadingConditionRule(new Conditions.UndergroundChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.UndergroundGeneric)),
				new LeadingConditionRule(new Conditions.CavernsChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.CavernsGeneric)),
				new DropLootPoolRule(LootPoolNames.HellGeneric)
			)
		];

		lootPools[LootPoolNames.Wooden] = [
			SequentialRules(1,
				new LeadingConditionRule(new Conditions.SurfaceChest())
					.WithOnSuccess(new DropLootPoolRule(LootPoolNames.SurfaceWooden)),
				new DropLootPoolRule(LootPoolNames.HeightBasedGeneric)
			)
		];
		lootPools[LootPoolNames.Underground] = [
			new DropFromItemPoolRule(ItemPoolNames.UndergroundPrimary),
				new DropLootPoolRule(LootPoolNames.Underground)
		];
		lootPools[LootPoolNames.CavernsPrimary] = [
			new LeadingConditionRule(new Conditions.LavaLayerChest()).WithOnSuccess(SequentialRules(1,
				ByCondition(new Conditions.TenthAnniversaryIsUp(), ItemID.LavaCharm, 15),
				ByCondition(new Conditions.TenthAnniversaryIsNotUp(), ItemID.LavaCharm, 20),
				new DropFromItemPoolRule(ItemPoolNames.CavernsPrimaryNoLavaCharm)
			)).WithOnFailedConditions(new DropFromItemPoolRule(ItemPoolNames.CavernsPrimaryNoLavaCharm))
		];

		lootPools[LootPoolNames.Caverns] = [
			new DropFromItemPoolRule(ItemPoolNames.SurfaceWoodenPrimary),
			new DropLootPoolRule(LootPoolNames.SurfaceGeneric),
		];

		lootPools[LootPoolNames.PyramidGold] = [
			new DropFromItemPoolRule(ItemPoolNames.PyramidGoldPrimary),
			ByCondition(new Conditions.TenthAnniversaryIsUp(), ItemID.PharaohsMask).WithOnSuccess(Common(ItemID.PharaohsRobe)),
			new DropLootPoolRule(LootPoolNames.SurfaceGeneric),
		];
		lootPools[LootPoolNames.Frozen] = [
			new DropFromItemPoolRule(ItemPoolNames.FrozenPrimary, 1),
			Common(ItemID.IceMirror, 5)
		];
		lootPools[LootPoolNames.SandstoneHigh] = [
			new DropFromItemPoolRule(ItemPoolNames.SandstoneHighPrimary),
			Common(ItemID.ScarabBomb, 3, 10, 20),
			Common(ItemID.EncumberingStone, 7),
			Common(ItemID.DesertMinecart, 15),
		];
		lootPools[LootPoolNames.SandstoneLow] = [
			new DropFromItemPoolRule(ItemPoolNames.SandstoneLowPrimary),
			Common(ItemID.ScarabBomb, 3, 10, 20),
			Common(ItemID.EncumberingStone, 7),
			Common(ItemID.DesertMinecart, 15),
		];
		lootPools[LootPoolNames.Jungle] = [
			Common(ItemID.LivingMahoganyWand, 6).WithOnSuccess(Common(ItemID.LivingMahoganyLeafWand)),
			Common(ItemID.BeeMinecart, 10)
		];
		lootPools[LootPoolNames.WaterOceanCave] = [
			new DropFromItemPoolRule(ItemPoolNames.WaterOceanCavePrimary),
			new DropLootPoolRule(LootPoolNames.WaterSimple)
		];
		lootPools[LootPoolNames.WaterSimple] = [
			Common(ItemID.SharkBait, 2),
			Common(ItemID.SandcastleBucket, 2)
		];
		lootPools[LootPoolNames.LivingWood] = [
			new DropFromItemPoolRule(ItemPoolNames.LivingWoodPrimary),
			new OneFromRulesRule(10,
				Common(ItemID.SunflowerMinecart),
				Common(ItemID.LadybugMinecart)
			)
		];
		lootPools[LootPoolNames.MushroomHigh] = [
			Common(ItemID.ShroomMinecart, 2),
			Common(ItemID.MushroomHat, 3).WithOnSuccess(Common(ItemID.MushroomVest)).WithOnSuccess(Common(ItemID.MushroomPants))
		];
		lootPools[LootPoolNames.MushroomLow] = [
			new DropFromItemPoolRule(ItemPoolNames.MushroomLowSecondary)
		];
		lootPools[LootPoolNames.Shadow] = [
			new ShadowChestPrimaryRule(),
			Common(ItemID.TreasureMagnet, 5),
			Common(ItemID.HellMinecart, 10),
			Common(ItemID.OrnateShadowKey, 10),
			Common(ItemID.HellCake, 10),
		];
		lootPools[LootPoolNames.FloatingIsland] = [
			new FloatingIslandPrimaryRule(),
			Common(ItemID.CreativeWings, 40),
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
		AddItemPool(ItemPoolNames.SurfaceWoodenPrimary, [
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
		AddItemPool(ItemPoolNames.UndergroundPrimary, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots),
			new(ItemID.Mace),
			new(ItemID.ShoeSpikes)
		]);
		AddItemPool(ItemPoolNames.CavernsPrimaryNoLavaCharm, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots),
			new(ItemID.Mace),
			new(ItemID.ShoeSpikes),
			new(ItemID.FlareGun, ChainedRules: [Common(ItemID.Flare, minimumDropped: 25, maximumDropped: 50)]),
			..GetReplacementValues(7, (ItemID.Extractinator, 1f / 15f))
		]);
		AddItemPool(ItemPoolNames.HellPrimary, [
			new(ItemID.BandofRegeneration),
			new(ItemID.MagicMirror),
			new(ItemID.CloudinaBottle),
			new(ItemID.HermesBoots)
		]);
		AddItemPool(ItemPoolNames.PyramidGoldPrimary, [
			new(ItemID.PharaohsMask, Conditions: [new Conditions.TenthAnniversaryIsNotUp()], ChainedRules: [Common(ItemID.PharaohsRobe)], Weight: 1),
			new(ItemID.FlyingCarpet, Conditions: [new Conditions.TenthAnniversaryIsUp()], Weight: 1),
			new(ItemID.FlyingCarpet, Weight: 4),
			new(ItemID.SandstorminaBottle, Weight: 4)
		]);
		AddItemPool(ItemPoolNames.DungeonGoldPrimary, [ // ordered generation
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
		AddItemPool(ItemPoolNames.FrozenPrimary, [
			new(ItemID.IceBoomerang),
			new(ItemID.IceBlade),
			new(ItemID.IceSkates),
			new(ItemID.SnowballCannon, [new Conditions.NotRemixSeed()]),
			new(ItemID.IceBow, [new Conditions.RemixSeed()]),
			new(ItemID.BlizzardinaBottle),
			new(ItemID.FlurryBoots),
			..GetReplacementValues(6, (ItemID.Extractinator, 0.049), (ItemID.Fish, 0.02))
		]);
		AddItemPool(ItemPoolNames.LivingWoodPrimary, [
			new(ItemID.LivingWoodWand, ChainedRules: [Common(ItemID.LeafWand)], Weight: 2),
			new(ItemID.BabyBirdStaff)
		]);
		AddItemPool(ItemPoolNames.MushroomLowSecondary, [
			new(ItemID.ShroomMinecart),
			new(ItemID.MushroomHat, ChainedRules: [Common(ItemID.MushroomVest, ItemID.MushroomPants)])
		]);
		AddItemPool(ItemPoolNames.SandstoneHighPrimary, [
			new(ItemID.AncientChisel),
			new(ItemID.SandBoots),
			new(ItemID.MysticCoilSnake),
			new(ItemID.MagicConch)
		]);
		AddItemPool(ItemPoolNames.SandstoneLowPrimary, [
			new(ItemID.ThunderSpear),
			new(ItemID.ThunderStaff),
			new(ItemID.CatBast)
		]);
		AddItemPool(ItemPoolNames.FloatingIslandPrimary, [ // ordered generation
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
		AddItemPool(ItemPoolNames.WaterOceanCavePrimary, [
			new(ItemID.WaterWalkingBoots),
			new(ItemID.BreathingReed),
			new(ItemID.Trident),
			new(ItemID.Flipper),
			new(ItemID.FloatingTube),
		]);
		AddItemPool(ItemPoolNames.WebPrimary, [
			new(ItemID.WebSlinger)
		]);
		AddItemPool(ItemPoolNames.ShadowPrimary, [ // ordered generation
			new(ItemID.DarkLance),
			new(ItemID.Sunfury),
			new(ItemID.FlowerofFire, [new Conditions.NotRemixSeed()]),
			new(ItemID.UnholyTrident, [new Conditions.RemixSeed()]),
			new(ItemID.Flamelash),
			new(ItemID.HellwingBow),
		]);

		AddItemPool(ItemPoolNames.CopperBar, [
			DropItemFromGenVar(nameof(GenVars.copperBar), ItemID.CopperBar),
			DropItemFromGenVar(nameof(GenVars.copperBar), ItemID.TinBar)
		]);
		AddItemPool(ItemPoolNames.IronBar, [
			DropItemFromGenVar(nameof(GenVars.ironBar), ItemID.IronBar),
			DropItemFromGenVar(nameof(GenVars.ironBar), ItemID.LeadBar)
		]);
		AddItemPool(ItemPoolNames.SilverBar, [
			DropItemFromGenVar(nameof(GenVars.silverBar), ItemID.SilverBar),
			DropItemFromGenVar(nameof(GenVars.silverBar), ItemID.TungstenBar)
		]);
		AddItemPool(ItemPoolNames.GoldBar, [
			DropItemFromGenVar(nameof(GenVars.goldBar), ItemID.GoldBar),
			DropItemFromGenVar(nameof(GenVars.goldBar), ItemID.PlatinumBar)
		]);
		AddItemPool(ItemPoolNames.HellChestAmmo, [
			new(ItemID.HellfireArrow),
			DropItemFromSavedOreTier(nameof(WorldGen.SavedOreTiers.Silver), TileID.Tungsten, ItemID.TungstenBullet),
			DropItemFromSavedOreTier(nameof(WorldGen.SavedOreTiers.Silver), TileID.Silver, ItemID.SilverBullet)
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
	public static class ItemPoolNames
	{
		public const string CopperBar = nameof(CopperBar);
		public const string IronBar = nameof(IronBar);
		public const string SilverBar = nameof(SilverBar);
		public const string GoldBar = nameof(GoldBar);
		public const string HellChestAmmo = nameof(HellChestAmmo);

		public const string SurfaceWoodenPrimary = nameof(SurfaceWoodenPrimary);
		public const string UndergroundPrimary = nameof(UndergroundPrimary);
		public const string CavernsPrimaryNoLavaCharm = nameof(CavernsPrimaryNoLavaCharm);
		public const string HellPrimary = nameof(HellPrimary);
		public const string PyramidGoldPrimary = nameof(PyramidGoldPrimary);
		public const string SandstoneHighPrimary = nameof(SandstoneHighPrimary);
		public const string SandstoneLowPrimary = nameof(SandstoneLowPrimary);
		public const string DungeonGoldPrimary = nameof(DungeonGoldPrimary);
		public const string FrozenPrimary = nameof(FrozenPrimary);
		public const string MushroomLowSecondary = nameof(MushroomLowSecondary);
		public const string LivingWoodPrimary = nameof(LivingWoodPrimary);
		public const string FloatingIslandPrimary = nameof(FloatingIslandPrimary);
		public const string FloatingIslandPainting = nameof(FloatingIslandPainting);
		public const string WaterOceanCavePrimary = nameof(WaterOceanCavePrimary);
		public const string WebPrimary = nameof(WebPrimary);
		public const string ShadowPrimary = nameof(ShadowPrimary);
	}
	public static class LootPoolNames
	{
		public const string Wooden = nameof(Wooden);

		public const string HeightBasedGeneric = nameof(HeightBasedGeneric);

		public const string SurfaceGeneric = nameof(SurfaceGeneric);
		public const string UndergroundGeneric = nameof(UndergroundGeneric);//TODO
		public const string CavernsGeneric = nameof(CavernsGeneric);//TODO
		public const string HellGeneric = nameof(HellGeneric);//TODO

		public const string SurfaceWooden = nameof(SurfaceWooden);
		public const string Underground = nameof(Underground);
		public const string Caverns = nameof(Caverns);
		public const string PyramidGold = nameof(PyramidGold);
		public const string SandstoneHigh = nameof(SandstoneHigh);//TODO
		public const string SandstoneLow = nameof(SandstoneLow);//TODO
		public const string DungeonGold = nameof(DungeonGold);//TODO
		public const string Frozen = nameof(Frozen);//TODO
		public const string Jungle = nameof(Jungle);//TODO
		public const string MushroomHigh = nameof(MushroomHigh);//TODO
		public const string MushroomLow = nameof(MushroomLow);//TODO
		public const string LivingWood = nameof(LivingWood);//TODO
		public const string FloatingIsland = nameof(FloatingIsland);//TODO
		public const string WaterOceanCave = nameof(WaterOceanCave);//TODO
		public const string WaterSimple = nameof(WaterSimple);//TODO
		public const string Web = nameof(Web);//TODO
		public const string Shadow = nameof(Shadow);//TODO

		public const string CavernsPrimary = nameof(CavernsPrimary);
		public const string BiomeChestExtras = nameof(BiomeChestExtras);//TODO
	}
}
public record class ItemPoolEntry(int Type, List<IItemDropRuleCondition> Conditions = null, List<IItemDropRule> ChainedRules = null, float Weight = 1f)
{
	public List<IItemDropRuleCondition> Conditions { get; } = Conditions ?? [];
	public List<IItemDropRule> ChainedRules { get; } = ChainedRules ?? [];
}
