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
	internal static void RegisterDefaultLootPools()
	{
		//TODO: ordered with random replacement: jungle chests, normal water chests
		genVarConditions.Clear();
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

		lootPools[LootPoolNames.Wooden] = [
			new LeadingConditionRule(new Conditions.SurfaceChest())
				.WithOnSuccess(new DropLootPoolRule(LootPoolNames.SurfaceWooden))
		];

		lootPools[LootPoolNames.SurfaceWooden] = [
			new DropFromItemPoolRule(ItemPoolNames.SurfaceWoodenPrimary),
			new DropLootPoolRule(LootPoolNames.SurfaceGeneric),
		];

		lootPools[LootPoolNames.PyramidGold] = [
			new DropFromItemPoolRule(ItemPoolNames.PyramidGoldPrimary),
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
		AddItemPool(ItemPoolNames.PyramidGoldPrimary, [
			new(ItemID.PharaohsMask, Conditions: [new Conditions.TenthAnniversaryIsNotUp()], ChainedRules: [Common(ItemID.PharaohsRobe)], Weight: 1),
			new(ItemID.FlyingCarpet, Conditions: [new Conditions.TenthAnniversaryIsUp()], Weight: 1),
			new(ItemID.FlyingCarpet, Weight: 4),
			new(ItemID.SandstorminaBottle, Weight: 4)
		]);
		AddItemPool(ItemPoolNames.PyramidGoldPrimaryAnniversary, [
			new(ItemID.PharaohsMask, ChainedRules:[Common(ItemID.PharaohsRobe)], Weight: 1),
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
		public const string SurfaceWoodenPrimary = nameof(SurfaceWoodenPrimary);
		public const string PyramidGoldPrimary = nameof(PyramidGoldPrimary);
		public const string PyramidGoldPrimaryAnniversary = nameof(PyramidGoldPrimaryAnniversary);
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

		public const string SurfaceWooden = nameof(SurfaceWooden);
		public const string SurfaceGeneric = nameof(SurfaceGeneric);
		public const string PyramidGold = nameof(PyramidGold);//TODO
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

		public const string BiomeChestExtras = nameof(BiomeChestExtras);//TODO
	}
}
public record class ItemPoolEntry(int Type, List<IItemDropRuleCondition> Conditions = null, List<IItemDropRule> ChainedRules = null, float Weight = 1f)
{
	public List<IItemDropRuleCondition> Conditions { get; } = Conditions ?? [];
	public List<IItemDropRule> ChainedRules { get; } = ChainedRules ?? [];
}
