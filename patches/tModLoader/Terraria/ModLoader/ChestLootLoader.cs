using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.WorldBuilding;
using static Terraria.GameContent.ItemDropRules.ItemDropRule;

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
	public static class ItemPoolNames
	{
		public const string CopperBar = nameof(CopperBar);
		public const string IronBar = nameof(IronBar);
		public const string SilverBar = nameof(SilverBar);
		public const string GoldBar = nameof(GoldBar);
		public const string SurfaceWoodenPrimary = nameof(SurfaceWoodenPrimary);
		public const string DesertHighPrimary = nameof(DesertHighPrimary);
		public const string DesertLowPrimary = nameof(DesertLowPrimary);
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
		genVarConditions.Clear();
		lootPools.Clear();
		itemPools.Clear();
		AddItemPool(ItemPoolNames.SurfaceWoodenPrimary, [
			new(ItemID.Spear),
			new(ItemID.Blowpipe),
			new(ItemID.WoodenBoomerang),
			new(ItemID.Aglet),
			new(ItemID.ClimbingClaws),
			new(ItemID.Umbrella),
			new(ItemID.CordageGuide),
			new(ItemID.WandofSparking),
			new(ItemID.Radar),
			new(ItemID.PortableStool)
		]);
		AddItemPool(ItemPoolNames.DesertHighPrimary, [
			new(ItemID.AncientChisel),
			new(ItemID.SandBoots),
			new(ItemID.MysticCoilSnake),
			new(ItemID.MagicConch)
		]);
		AddItemPool(ItemPoolNames.DesertLowPrimary, [
			new(ItemID.ThunderSpear),
			new(ItemID.ThunderStaff),
			new(ItemID.CatBast)
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
		lootPools["SurfaceWooden"] = [
			new DropFromItemPoolRule(ItemPoolNames.SurfaceWoodenPrimary, 1),
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
	public static List<ItemPoolEntry> GetItemPool(string name) => itemPools.TryGetValue(name, out var pool) ? pool : null;
}
public record class ItemPoolEntry(int Type, List<IItemDropRuleCondition> Conditions = null, List<IItemDropRule> ChainedRules = null, float Weight = 1f)
{
	public List<IItemDropRuleCondition> Conditions { get; } = Conditions ?? [];
	public List<IItemDropRule> ChainedRules { get; } = ChainedRules ?? [];
}
