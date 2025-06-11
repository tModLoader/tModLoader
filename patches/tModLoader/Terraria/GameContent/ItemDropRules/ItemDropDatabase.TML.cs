using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.GameContent.ItemDropRules;

partial class ItemDropDatabase
{
	private Dictionary<int, List<IItemDropRule>> _entriesByItemId = new Dictionary<int, List<IItemDropRule>>();
	private Dictionary<int, List<int>> _itemIdsByType = new Dictionary<int, List<int>>();

	/// <summary>
	/// Retrieves all the registered <see cref="IItemDropRule"/> for this specific item type.
	/// </summary>
	public List<IItemDropRule> GetRulesForItemID(int itemID)
	{
		List<IItemDropRule> list = new List<IItemDropRule>();

		if (_entriesByItemId.TryGetValue(itemID, out List<IItemDropRule> value))
			list.AddRange(value);

		return list;
	}

	public IItemDropRule RegisterToItem(int type, IItemDropRule entry)
	{
		RegisterToItemId(type, entry);
		if (type > 0 && _itemIdsByType.TryGetValue(type, out List<int> value)) {
			for (int i = 0; i < value.Count; i++) {
				RegisterToItemId(value[i], entry);
			}
		}

		return entry;
	}

	public IItemDropRule RegisterToMultipleItems(IItemDropRule entry, params int[] itemIds)
	{
		for (int i = 0; i < itemIds.Length; i++) {
			RegisterToItem(itemIds[i], entry);
		}

		return entry;
	}

	public void RegisterToItemId(int itemId, IItemDropRule entry)
	{
		if (!_entriesByItemId.ContainsKey(itemId))
			_entriesByItemId[itemId] = new List<IItemDropRule>();

		_entriesByItemId[itemId].Add(entry);
	}

	private void RemoveFromItemId(int itemId, IItemDropRule entry)
	{
		if (_entriesByItemId.ContainsKey(itemId))
			_entriesByItemId[itemId].Remove(entry);
	}

	public IItemDropRule RemoveFromItem(int type, IItemDropRule entry)
	{
		RemoveFromItemId(type, entry);
		if (type > 0 && _itemIdsByType.TryGetValue(type, out List<int> value)) {
			for (int i = 0; i < value.Count; i++) {
				RemoveFromItemId(value[i], entry);
			}
		}

		return entry;
	}

	private void RegisterBossBags()
	{
		short item = ItemID.QueenSlimeBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.VolatileGelatin));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.GelBalloon, 1, 25, 74));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.QueenSlimeMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.QueenSlimeMountSaddle, 2));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.QueenSlimeHook, 2));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Smolstar, 3));
		RegisterToItem(item, ItemDropRule.FewFromOptionsNotScalingWithLuckWithX(2, 1, 1, ItemID.CrystalNinjaHelmet, ItemID.CrystalNinjaChestplate, ItemID.CrystalNinjaLeggings));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.QueenSlimeBoss));

		item = ItemID.FairyQueenBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.EmpressFlightBooster));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FairyQueenMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.RainbowWings, 10));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.PiercingStarlight, ItemID.FairyQueenMagicItem, ItemID.FairyQueenRangedItem, ItemID.RainbowWhip));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.HallowBossDye, 4));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SparkleGuitar, 20));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.RainbowCursor, 20));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.HallowBoss));

		item = ItemID.KingSlimeBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.RoyalGel));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.KingSlimeMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SlimySaddle, 2));
		RegisterToItem(item, ItemDropRule.FewFromOptionsNotScalingWithLuckWithX(2, 1, 1, ItemID.NinjaHood, ItemID.NinjaShirt, ItemID.NinjaPants));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.SlimeGun, ItemID.SlimeHook));
		RegisterToItem(item, ItemDropRule.Common(ItemID.Solidifier));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.KingSlime));

		item = ItemID.PlanteraBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.SporeSac));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.PlanteraMask, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.TempleKey));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Seedling, 15));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.TheAxe, 20));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.PygmyStaff, 2));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.ThornHook, 10));
		IItemDropRule itemDropRuleGrenadeLauncher = ItemDropRule.Common(ItemID.GrenadeLauncher);
		itemDropRuleGrenadeLauncher.OnSuccess(ItemDropRule.Common(ItemID.RocketI, 1, 50, 149), hideLootReport: true);
		RegisterToItem(item, new OneFromRulesRule(1, itemDropRuleGrenadeLauncher, ItemDropRule.Common(ItemID.VenusMagnum), ItemDropRule.Common(ItemID.NettleBurst), ItemDropRule.Common(ItemID.LeafBlower), ItemDropRule.Common(ItemID.FlowerPow), ItemDropRule.Common(ItemID.WaspGun), ItemDropRule.Common(ItemID.Seedler)));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.Plantera));

		item = ItemID.SkeletronPrimeBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalBatteryPiece));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SkeletronPrimeMask, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofFright, 1, 25, 40));
		RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.SkeletronPrime));

		item = ItemID.DestroyerBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalWagonPiece));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DestroyerMask, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofMight, 1, 25, 40));
		RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.TheDestroyer));

		item = ItemID.TwinsBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalWheelPiece));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.TwinMask, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofSight, 1, 25, 40));
		RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.Retinazer));

		item = ItemID.EyeOfCthulhuBossBag;
		Conditions.IsCrimson conditionIsCrimson = new Conditions.IsCrimson();
		Conditions.IsCorruption conditionIsCorruption = new Conditions.IsCorruption();
		RegisterToItem(item, ItemDropRule.Common(ItemID.EoCShield));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EyeMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Binoculars, 30));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsCrimson, ItemID.CrimtaneOre, 1, 30, 87));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsCrimson, ItemID.CrimsonSeeds, 1, 1, 3));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsCorruption, ItemID.UnholyArrow, 1, 20, 49));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsCorruption, ItemID.DemoniteOre, 1, 30, 87));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsCorruption, ItemID.CorruptSeeds, 1, 1, 3));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.EyeofCthulhu));

		item = ItemID.BrainOfCthulhuBossBag;
		Conditions.NotMasterMode conditionIsNotMaster = new Conditions.NotMasterMode();
		Conditions.IsMasterMode conditionIsMaster = new Conditions.IsMasterMode();
		RegisterToItem(item, ItemDropRule.Common(ItemID.BrainOfConfusion));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BrainMask, 7));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsNotMaster, ItemID.CrimtaneOre, 1, 80, 110));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsNotMaster, ItemID.TissueSample, 1, 20, 40));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsMaster, ItemID.CrimtaneOre, 1, 110, 135)); // Correctly gives the Master drop in For the Worthy worlds set to Expert Mode.
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsMaster, ItemID.TissueSample, 1, 30, 50));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BoneRattle, 20));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.BrainofCthulhu));

		item = ItemID.EaterOfWorldsBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.WormScarf));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EaterMask, 7));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsNotMaster, ItemID.DemoniteOre, 1, 80, 110));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsNotMaster, ItemID.ShadowScale, 1, 20, 40));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsMaster, ItemID.DemoniteOre, 1, 110, 135));
		RegisterToItem(item, ItemDropRule.ByCondition(conditionIsMaster, ItemID.ShadowScale, 1, 30, 50));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EatersBone, 20));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.EaterofWorldsHead));

		item = ItemID.DeerclopsBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.BoneHelm));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DeerclopsMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DizzyHat, 14));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.ChesterPetItem, 3));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Eyebrella, 3));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DontStarveShaderItem, 3));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.PewMaticHorn, ItemID.WeatherPain, ItemID.HoundiusShootius, ItemID.LucyTheAxe));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.Deerclops));

		item = ItemID.QueenBeeBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.HiveBackpack));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BeeMask, 7));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BeeGun, ItemID.BeeKeeper, ItemID.BeesKnees));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.HoneyComb, 3));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Nectar, 9));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.HoneyedGoggles, 9));
		RegisterToItem(item, ItemDropRule.Common(ItemID.HiveWand));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BeeHat, ItemID.BeeShirt, ItemID.BeePants));
		RegisterToItem(item, ItemDropRule.Common(ItemID.Beenade, 1, 10, 29));
		RegisterToItem(item, ItemDropRule.Common(ItemID.BeeWax, 1, 17, 29));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.QueenBee));

		item = ItemID.SkeletronBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.BoneGlove));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.SkeletronMask, ItemID.SkeletronHand, ItemID.BookofSkulls));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.SkeletronHead));

		item = ItemID.WallOfFleshBossBag;
		RegisterToItem(item, ItemDropRule.ByCondition(new Conditions.NotUsedDemonHeart(), ItemID.DemonHeart));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FleshMask, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.Pwnhammer));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.SummonerEmblem, ItemID.SorcererEmblem, ItemID.WarriorEmblem, ItemID.RangerEmblem));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.LaserRifle, ItemID.BreakerBlade, ItemID.ClockworkAssaultRifle, ItemID.FireWhip));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.WallofFlesh));

		item = ItemID.CultistBossBag;
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskCultist, 7));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.CultistBoss));

		item = ItemID.MoonLordBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.GravityGlobe));
		RegisterToItem(item, ItemDropRule.Common(ItemID.SuspiciousLookingTentacle));
		RegisterToItem(item, ItemDropRule.Common(ItemID.LongRainbowTrailWings));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskMoonlord, 7));
		RegisterToItem(item, ItemDropRule.Common(ItemID.LunarOre, 1, 90, 110));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.MeowmereMinecart, 10));
		RegisterToItem(item, ItemDropRule.ByCondition(new Conditions.NoPortalGun(), ItemID.PortalGun));
		RegisterToItem(item, new FromOptionsWithoutRepeatsDropRule(2, ItemID.Meowmere, ItemID.Terrarian, ItemID.StarWrath, ItemID.SDMG, ItemID.Celeb2, ItemID.LastPrism, ItemID.LunarFlareBook, ItemID.RainbowCrystalStaff, ItemID.MoonlordTurretStaff));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.MoonLordCore));

		item = ItemID.BossBagBetsy;
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskBetsy, 7));
		RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.DD2SquireBetsySword, ItemID.DD2BetsyBow, ItemID.ApprenticeStaffT3, ItemID.MonkStaffT3));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BetsyWings, 4));
		RegisterToItem(item, ItemDropRule.Common(ItemID.DefenderMedal, 1, 30, 49));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.DD2Betsy));

		item = ItemID.GolemBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.ShinyStone));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.GolemMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Picksaw, 3));
		IItemDropRule itemDropRuleStynger = ItemDropRule.Common(ItemID.Stynger);
		itemDropRuleStynger.OnSuccess(ItemDropRule.Common(ItemID.StyngerBolt, 1, 60, 99), hideLootReport: true);
		RegisterToItem(item, new OneFromRulesRule(1, itemDropRuleStynger, ItemDropRule.Common(ItemID.PossessedHatchet), ItemDropRule.Common(ItemID.SunStone), ItemDropRule.Common(ItemID.EyeoftheGolem), ItemDropRule.Common(ItemID.HeatRay), ItemDropRule.Common(ItemID.StaffofEarth), ItemDropRule.Common(ItemID.GolemFist)));
		RegisterToItem(item, ItemDropRule.Common(ItemID.BeetleHusk, 1, 18, 23));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.Golem));

		item = ItemID.FishronBossBag;
		RegisterToItem(item, ItemDropRule.Common(ItemID.ShrimpyTruffle));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DukeFishronMask, 7));
		RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FishronWings, 10));
		RegisterToItem(item, new LeadingConditionRule(new Conditions.NotRemixSeed())).OnSuccess(ItemDropRule.OneFromOptions(1, ItemID.Flairon, ItemID.Tsunami, ItemID.RazorbladeTyphoon, ItemID.TempestStaff, ItemID.BubbleGun));
		RegisterToItem(item, new LeadingConditionRule(new Conditions.RemixSeed())).OnSuccess(ItemDropRule.OneFromOptions(1, ItemID.Flairon, ItemID.Tsunami, ItemID.RazorbladeTyphoon, ItemID.TempestStaff, ItemID.AquaScepter));
		RegisterToItem(item, ItemDropRule.CoinsBasedOnNPCValue(NPCID.DukeFishron));
	}

	private void RegisterCrateDrops()
	{
		#region Wooden Crate and Pearlwood Crate
		IItemDropRule[] themed = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 40),
			ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 40),
			ItemDropRule.NotScalingWithLuck(ItemID.Extractinator, 50)
		};
		IItemDropRule[] hardmodeThemed = new IItemDropRule[]
		{
			ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 200),
			ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 40),
			ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 40),
			ItemDropRule.NotScalingWithLuck(ItemID.Anchor, 25)
		};
		IItemDropRule[] coin = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 1, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.SilverCoin, 1, 20, 90)
		};
		IItemDropRule[] ores = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 4, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 4, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 4, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 4, 15)
		};
		IItemDropRule[] hardmodeOres = new IItemDropRule[] {
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 4, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 4, 15)
		};
		IItemDropRule[] bars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 2, 5)
		};
		IItemDropRule[] hardmodeBars = new IItemDropRule[] {
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 2, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 2, 3)
		};
		IItemDropRule[] potions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.SwiftnessPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.IronskinPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.NightOwlPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.ShinePotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.GillsPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 1, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.TrapsightPotion, 1, 1, 3) // dangersense
		};
		IItemDropRule[] extraPotions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.LesserHealingPotion, 1, 5, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.LesserManaPotion, 1, 5, 15)
		};
		IItemDropRule[] extraBait = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 3, 1, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.ApprenticeBait, 1, 1, 4)
		};

		IItemDropRule bc_surfaceLoot = ItemDropRule.OneFromOptionsNotScalingWithLuck(20, ItemID.Aglet, ItemID.ClimbingClaws, ItemID.PortableStool, ItemID.CordageGuide, ItemID.Radar);

		IItemDropRule[] woodenCrateDrop = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
			bc_surfaceLoot,
			ItemDropRule.SequentialRulesNotScalingWithLuck(7, coin),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(7, ores), new OneFromRulesRule(8, bars)),
			new OneFromRulesRule(7, potions),
		};
		IItemDropRule[] pearlwoodCrateDrop = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
			bc_surfaceLoot,
			ItemDropRule.SequentialRulesNotScalingWithLuck(7, coin),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1,
				ItemDropRule.SequentialRulesNotScalingWithLuck(7,
					new OneFromRulesRule(2, hardmodeOres),
					new OneFromRulesRule(1, ores)
				),
				ItemDropRule.SequentialRulesNotScalingWithLuck(8,
					new OneFromRulesRule(2, hardmodeBars),
					new OneFromRulesRule(1, bars)
				)
			),
			new OneFromRulesRule(7, potions),
		};

		RegisterToItem(ItemID.WoodenCrate, ItemDropRule.AlwaysAtleastOneSuccess(woodenCrateDrop));
		RegisterToItem(ItemID.WoodenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(pearlwoodCrateDrop));
		RegisterToMultipleItems(new OneFromRulesRule(3, extraPotions), ItemID.WoodenCrate, ItemID.WoodenCrateHard);
		RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(3, extraBait), ItemID.WoodenCrate, ItemID.WoodenCrateHard);
		#endregion

		#region Iron Crate and Mythril Crate
		themed = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.GingerBeard, 25),
			ItemDropRule.NotScalingWithLuck(ItemID.TartarSauce, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.FalconBlade, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 20)
		};
		hardmodeThemed = new IItemDropRule[]
		{
			ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 60),
			ItemDropRule.NotScalingWithLuck(ItemID.GingerBeard, 25),
			ItemDropRule.NotScalingWithLuck(ItemID.TartarSauce, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.FalconBlade, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 20)
		};
		ores = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 12, 21)
		};
		hardmodeOres = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 12, 21),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 12, 21)
		};
		bars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 1, 4, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 1, 4, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 4, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 4, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 4, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 4, 7)
		};
		hardmodeBars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 3, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 3, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 3, 7),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 3, 7)
		};
		potions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.CalmingPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.FlipperPotion, 1, 2, 4)
		};
		extraPotions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 15)
		};
		extraBait = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 3, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 4)
		};

		IItemDropRule[] ironCrate = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 10),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(6, ores), new OneFromRulesRule(4, bars)),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] mythrilCrate = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 10),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1,
				ItemDropRule.SequentialRulesNotScalingWithLuck(6,
					new OneFromRulesRule(2, hardmodeOres),
					new OneFromRulesRule(1, ores)
				),
				ItemDropRule.SequentialRulesNotScalingWithLuck(4,
					new OneFromRulesRule(3, 2, hardmodeBars),
					new OneFromRulesRule(1, bars)
				)
			),
			new OneFromRulesRule(4, potions),
		};

		RegisterToItem(ItemID.IronCrate, ItemDropRule.AlwaysAtleastOneSuccess(ironCrate));
		RegisterToItem(ItemID.IronCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(mythrilCrate));
		RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), ItemID.IronCrate, ItemID.IronCrateHard);
		RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(2, extraBait), ItemID.IronCrate, ItemID.IronCrateHard);
		#endregion

		#region Gold Crate and Titanium Crate
		themed = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.LifeCrystal, 8),
			ItemDropRule.NotScalingWithLuck(ItemID.HardySaddle, 10),
		};
		hardmodeThemed = new IItemDropRule[]
		{
			ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.LifeCrystal, 8),
			ItemDropRule.NotScalingWithLuck(ItemID.HardySaddle, 10),
		};
		ores = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 25, 34)
		};
		hardmodeOres = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 1, 25, 34),
			ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 1, 25, 34)
		};
		bars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 8, 11)
		};
		hardmodeBars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 1, 8, 11),
			ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 1, 8, 11)
		};
		potions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 5),
			ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 5)
		};
		extraPotions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 20),
			ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 20)
		};

		IItemDropRule[] goldCrate = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 8, 20),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(5, ores), new OneFromRulesRule(3, 1, bars)),
			new OneFromRulesRule(3, potions),
			ItemDropRule.NotScalingWithLuck(ItemID.EnchantedSword, 30),
		};
		IItemDropRule[] titaniumCrate = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 8, 20),
			ItemDropRule.SequentialRulesNotScalingWithLuck(1,
				ItemDropRule.SequentialRulesNotScalingWithLuck(5,
					new OneFromRulesRule(2, hardmodeOres),
					new OneFromRulesRule(1, ores)
				),
				ItemDropRule.SequentialRulesNotScalingWithLuckWithNumerator(3, 1,
					new OneFromRulesRule(3, 2, hardmodeBars),
					new OneFromRulesRule(1, bars)
				)
			),
			new OneFromRulesRule(3, potions),
			ItemDropRule.NotScalingWithLuck(ItemID.EnchantedSword, 15),
		};

		RegisterToItem(ItemID.GoldenCrate, ItemDropRule.AlwaysAtleastOneSuccess(goldCrate));
		RegisterToItem(ItemID.GoldenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(titaniumCrate));
		RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
		RegisterToMultipleItems(new CommonDrop(ItemID.MasterBait, 3, 3, 7, 2), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
		#endregion

		#region Biome Crates
		#region Biome related
		IItemDropRule[] bc_jungle = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.FlowerBoots, 20),
			ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.AnkletoftheWind, ItemID.Boomstick, ItemID.FeralClaws, ItemID.StaffofRegrowth, ItemID.FiberglassFishingPole),
		};
		IItemDropRule bc_bamboo = ItemDropRule.NotScalingWithLuck(ItemID.BambooBlock, 3, 20, 50);
		IItemDropRule bc_seaweed = ItemDropRule.NotScalingWithLuck(ItemID.Seaweed, 20);

		IItemDropRule bc_sky = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.LuckyHorseshoe, ItemID.CelestialMagnet, ItemID.Starfury, ItemID.ShinyRedBalloon);
		IItemDropRule bc_cloud = ItemDropRule.NotScalingWithLuck(ItemID.Cloud, 2, 50, 100);
		IItemDropRule bc_fledgeWings = ItemDropRule.NotScalingWithLuck(ItemID.CreativeWings, 40, 1, 1);
		IItemDropRule bc_skyPaintings = ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemID.HighPitch, ItemID.BlessingfromTheHeavens, ItemID.Constellation,
			ItemID.SeeTheWorldForWhatItIs, ItemID.LoveisintheTrashSlot, ItemID.SunOrnament); // Sun Ornament == Eye of The Sun

		IItemDropRule bc_son = ItemDropRule.NotScalingWithLuck(ItemID.SoulofNight, 2, 2, 5);
		IItemDropRule bc_corrupt = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BallOHurt, ItemID.BandofStarpower, ItemID.Musket, ItemID.ShadowOrb, ItemID.Vilethorn);
		IItemDropRule bc_crimson = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.TheUndertaker, ItemID.TheRottedFork, ItemID.CrimsonRod, ItemID.PanicNecklace, ItemID.CrimsonHeart);
		IItemDropRule bc_cursed = ItemDropRule.NotScalingWithLuck(ItemID.CursedFlame, 2, 2, 5);
		IItemDropRule bc_ichor = ItemDropRule.NotScalingWithLuck(ItemID.Ichor, 2, 2, 5);

		IItemDropRule bc_sol = ItemDropRule.NotScalingWithLuck(ItemID.SoulofLight, 2, 2, 5);
		IItemDropRule bc_shard = ItemDropRule.NotScalingWithLuck(ItemID.CrystalShard, 2, 4, 10);

		IItemDropRule bc_lockbox = ItemDropRule.Common(ItemID.LockBox);
		IItemDropRule bc_book = ItemDropRule.NotScalingWithLuck(ItemID.Book, 2, 5, 15);

		IItemDropRule ruleSnowballCannonIceBow = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.SnowballCannon);
		ruleSnowballCannonIceBow.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.IceBow), hideLootReport: true);

		IItemDropRule[] bc_iceList = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.IceBoomerang),
			ItemDropRule.NotScalingWithLuck(ItemID.IceBlade),
			ItemDropRule.NotScalingWithLuck(ItemID.IceSkates),
			ruleSnowballCannonIceBow,
			ItemDropRule.NotScalingWithLuck(ItemID.BlizzardinaBottle),
			ItemDropRule.NotScalingWithLuck(ItemID.FlurryBoots),
		};
		IItemDropRule bc_ice = new OneFromRulesRule(1, bc_iceList);

		IItemDropRule bc_fish = ItemDropRule.NotScalingWithLuck(ItemID.Fish, 20);

		IItemDropRule bc_scarab = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.AncientChisel, ItemID.ScarabFishingRod, ItemID.SandBoots, ItemID.ThunderSpear, ItemID.ThunderStaff, ItemID.CatBast, ItemID.MysticCoilSnake, ItemID.MagicConch);
		IItemDropRule bc_bomb = ItemDropRule.NotScalingWithLuck(ItemID.ScarabBomb, 4, 4, 6);
		IItemDropRule bc_fossil = ItemDropRule.NotScalingWithLuck(ItemID.FossilOre, 4, 10, 16); // sturdy fossil
		IItemDropRule bc_sandstormBottle = ItemDropRule.NotScalingWithLuck(ItemID.SandstorminaBottle, 35);

		IItemDropRule[] bc_lava = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.LavaCharm, 20),
			ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.FlameWakerBoots, ItemID.SuperheatedBlood, ItemID.LavaFishbowl, ItemID.LavaFishingHook, ItemID.VolcanoSmall),
		};
		IItemDropRule bc_pot = ItemDropRule.NotScalingWithLuck(ItemID.PotSuspended, 4, 2, 2);
		IItemDropRule bc_obsi = ItemDropRule.Common(ItemID.ObsidianLockbox);
		IItemDropRule bc_wet = ItemDropRule.NotScalingWithLuck(ItemID.WetBomb, 3, 7, 10);
		IItemDropRule bc_plant = ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemID.PottedLavaPlantPalm, ItemID.PottedLavaPlantBush, ItemID.PottedLavaPlantBramble, ItemID.PottedLavaPlantBulb, ItemID.PottedLavaPlantTendrils);
		IItemDropRule bc_ornate = ItemDropRule.NotScalingWithLuck(ItemID.OrnateShadowKey, 20);
		IItemDropRule bc_hellcart = ItemDropRule.NotScalingWithLuck(ItemID.HellMinecart, 20); // Demonic Hellcart
		IItemDropRule bc_cake = ItemDropRule.NotScalingWithLuck(ItemID.HellCake, 20);

		IItemDropRule[] bc_sea = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.WaterWalkingBoots, 10),
			ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BreathingReed, ItemID.FloatingTube, ItemID.Trident, ItemID.Flipper),
		};
		IItemDropRule bc_pile = ItemDropRule.NotScalingWithLuck(ItemID.ShellPileBlock, 3, 20, 50);
		IItemDropRule bc_sharkbait = ItemDropRule.NotScalingWithLuck(ItemID.SharkBait, 10);
		IItemDropRule bc_sand = ItemDropRule.NotScalingWithLuck(ItemID.SandcastleBucket, 10);
		#endregion

		#region Pseudo-global
		IItemDropRule bc_goldCoin = ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 12);

		ores = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 20, 35)
		};
		hardmodeOres = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 1, 20, 35)
		};
		bars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 6, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 6, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 6, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 6, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 6, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 6, 16)
		};
		hardmodeBars = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 1, 5, 16)
		};
		potions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4)
		};
		extraPotions = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 17),
			ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 17)
		};
		extraBait = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 2, 2, 6),
			ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 6)
		};
		#endregion

		IItemDropRule hardmodeBiomeCrateOres = ItemDropRule.SequentialRulesNotScalingWithLuck(7,
			new OneFromRulesRule(2, hardmodeOres),
			new OneFromRulesRule(1, ores)
		);
		IItemDropRule hardmodeBiomeCrateBars = ItemDropRule.SequentialRulesNotScalingWithLuck(4,
			new OneFromRulesRule(3, 2, hardmodeBars),
			new OneFromRulesRule(1, bars)
		);

		IItemDropRule[] jungle = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_jungle),

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),

			bc_bamboo,
			bc_seaweed,
		};
		IItemDropRule[] bramble = new IItemDropRule[]
		{
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_jungle),

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_bamboo,
			bc_seaweed,
		};
		IItemDropRule[] sky = new IItemDropRule[]
		{
			bc_sky,
			bc_fledgeWings,
			bc_cloud,
			bc_skyPaintings,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] azure = new IItemDropRule[]
		{
			bc_sky,
			bc_fledgeWings,
			bc_cloud,
			bc_skyPaintings,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] corrupt = new IItemDropRule[] {
			bc_corrupt,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] defiled = new IItemDropRule[] {
			bc_corrupt,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_son,
			bc_cursed,
		};
		IItemDropRule[] crimson = new IItemDropRule[] {
			bc_crimson,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] hematic = new IItemDropRule[] {
			bc_crimson,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_son,
			bc_ichor,
		};
		IItemDropRule[] hallowed = new IItemDropRule[] {
			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] divine = new IItemDropRule[] {
			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_sol,
			bc_shard,
		};
		IItemDropRule[] dungeon = new IItemDropRule[] {
			bc_lockbox,
			bc_book,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] stockade = new IItemDropRule[] {
			bc_lockbox,
			bc_book,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] frozen = new IItemDropRule[] {
			bc_ice,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),

			bc_fish,
		};
		IItemDropRule[] boreal = new IItemDropRule[] {
			bc_ice,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_fish,
		};
		IItemDropRule[] oasis = new IItemDropRule[] {
			bc_scarab,
			bc_bomb,
			bc_sandstormBottle,

			bc_goldCoin,
			bc_fossil,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] mirage = new IItemDropRule[] {
			bc_scarab,
			bc_bomb,
			bc_sandstormBottle,

			bc_goldCoin,
			bc_fossil,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),
		};
		IItemDropRule[] obsidian = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_lava),

			bc_pot,
			bc_obsi,
			bc_wet,
			bc_plant,
			bc_hellcart,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),

			bc_ornate,
			bc_cake,
		};
		IItemDropRule[] hellstone = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_lava),

			bc_pot,
			bc_obsi,
			bc_wet,
			bc_plant,
			bc_hellcart,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_ornate,
			bc_cake,
		};
		IItemDropRule[] ocean = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_sea),
			bc_sharkbait,

			bc_goldCoin,
			new OneFromRulesRule(7, ores),
			new OneFromRulesRule(4, bars),
			new OneFromRulesRule(4, potions),

			bc_pile,
			bc_sand,
		};
		IItemDropRule[] seaside = new IItemDropRule[] {
			ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_sea),
			bc_sharkbait,

			bc_goldCoin,
			hardmodeBiomeCrateOres,
			hardmodeBiomeCrateBars,
			new OneFromRulesRule(4, potions),

			bc_pile,
			bc_sand,
		};

		RegisterToItem(ItemID.JungleFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(jungle));
		RegisterToItem(ItemID.JungleFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(bramble));
		RegisterToItem(ItemID.FloatingIslandFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(sky));
		RegisterToItem(ItemID.FloatingIslandFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(azure));
		RegisterToItem(ItemID.CorruptFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(corrupt));
		RegisterToItem(ItemID.CorruptFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(defiled));
		RegisterToItem(ItemID.CrimsonFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(crimson));
		RegisterToItem(ItemID.CrimsonFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(hematic));
		RegisterToItem(ItemID.HallowedFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(hallowed));
		RegisterToItem(ItemID.HallowedFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(divine));
		RegisterToItem(ItemID.DungeonFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(dungeon));
		RegisterToItem(ItemID.DungeonFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(stockade));
		RegisterToItem(ItemID.FrozenCrate, ItemDropRule.AlwaysAtleastOneSuccess(frozen));
		RegisterToItem(ItemID.FrozenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(boreal));
		RegisterToItem(ItemID.OasisCrate, ItemDropRule.AlwaysAtleastOneSuccess(oasis));
		RegisterToItem(ItemID.OasisCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(mirage));
		RegisterToItem(ItemID.LavaCrate, ItemDropRule.AlwaysAtleastOneSuccess(obsidian));
		RegisterToItem(ItemID.LavaCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(hellstone));
		RegisterToItem(ItemID.OceanCrate, ItemDropRule.AlwaysAtleastOneSuccess(ocean));
		RegisterToItem(ItemID.OceanCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(seaside));

		int[] allCrates = new int[]
		{
			ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard,
			ItemID.FloatingIslandFishingCrate, ItemID.FloatingIslandFishingCrateHard,
			ItemID.CorruptFishingCrate, ItemID.CorruptFishingCrateHard,
			ItemID.CrimsonFishingCrate, ItemID.CrimsonFishingCrateHard,
			ItemID.HallowedFishingCrate, ItemID.HallowedFishingCrateHard,
			ItemID.DungeonFishingCrate, ItemID.DungeonFishingCrateHard,
			ItemID.FrozenCrate, ItemID.FrozenCrateHard,
			ItemID.OasisCrate, ItemID.OasisCrateHard,
			ItemID.LavaCrate, ItemID.LavaCrateHard,
			ItemID.OceanCrate, ItemID.OceanCrateHard,
		};
		RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), allCrates);
		RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(2, extraBait), allCrates);
		#endregion
	}

	private void RegisterObsidianLockbox()
	{
		IItemDropRule ruleFlowerOfFireUnholyTrident = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.FlowerofFire);
		ruleFlowerOfFireUnholyTrident.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.UnholyTrident), hideLootReport: true);

		IItemDropRule[] obsidianLockBoxList = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.DarkLance),
			ItemDropRule.NotScalingWithLuck(ItemID.Sunfury),
			ruleFlowerOfFireUnholyTrident,
			ItemDropRule.NotScalingWithLuck(ItemID.Flamelash),
			ItemDropRule.NotScalingWithLuck(ItemID.HellwingBow),
		};

		RegisterToItem(ItemID.ObsidianLockbox, new OneFromRulesRule(1, obsidianLockBoxList));
		RegisterToItem(ItemID.ObsidianLockbox, ItemDropRule.NotScalingWithLuck(ItemID.TreasureMagnet, 5));
	}

	private void RegisterLockbox()
	{
		IItemDropRule ruleAquaScepterBubbleGun = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.AquaScepter);
		ruleAquaScepterBubbleGun.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.BubbleGun), hideLootReport: true);

		IItemDropRule[] goldenLockBoxList = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.Valor),
			ItemDropRule.NotScalingWithLuck(ItemID.Muramasa),
			ItemDropRule.NotScalingWithLuck(ItemID.CobaltShield),
			ruleAquaScepterBubbleGun,
			ItemDropRule.NotScalingWithLuck(ItemID.BlueMoon),
			ItemDropRule.NotScalingWithLuck(ItemID.MagicMissile),
			ItemDropRule.NotScalingWithLuck(ItemID.Handgun),
		};

		RegisterToItem(ItemID.LockBox, new OneFromRulesRule(1, goldenLockBoxList));
		RegisterToItem(ItemID.LockBox, ItemDropRule.NotScalingWithLuck(ItemID.ShadowKey, 3));
	}

	private void RegisterHerbBag()
	{
		RegisterToItem(ItemID.HerbBag, new HerbBagDropsItemDropRule(ItemID.Daybloom, ItemID.Moonglow, ItemID.Blinkroot, ItemID.Waterleaf, ItemID.Deathweed, ItemID.Fireblossom, ItemID.Shiverthorn, ItemID.DaybloomSeeds, ItemID.MoonglowSeeds, ItemID.BlinkrootSeeds, ItemID.WaterleafSeeds, ItemID.DeathweedSeeds, ItemID.FireblossomSeeds, ItemID.ShiverthornSeeds));
	}

	private void RegisterGoodieBag()
	{
		IItemDropRule[] paintings = new IItemDropRule[] {
			ItemDropRule.NotScalingWithLuck(ItemID.JackingSkeletron),
			ItemDropRule.NotScalingWithLuck(ItemID.BitterHarvest),
			ItemDropRule.NotScalingWithLuck(ItemID.BloodMoonCountess),
			ItemDropRule.NotScalingWithLuck(ItemID.HallowsEve),
			ItemDropRule.NotScalingWithLuck(ItemID.MorbidCuriosity),
		};

		IItemDropRule catSet = ItemDropRule.NotScalingWithLuck(ItemID.CatMask);
		catSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.CatShirt));
		catSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.CatPants));

		IItemDropRule creeperSet = ItemDropRule.NotScalingWithLuck(ItemID.CreeperMask);
		creeperSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.CreeperShirt));
		creeperSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.CreeperPants));

		IItemDropRule ghostSet = ItemDropRule.NotScalingWithLuck(ItemID.GhostMask);
		ghostSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.GhostShirt));

		IItemDropRule leprechaunSet = ItemDropRule.NotScalingWithLuck(ItemID.LeprechaunHat);
		leprechaunSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.LeprechaunShirt));
		leprechaunSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.LeprechaunPants));

		IItemDropRule pixieSet = ItemDropRule.NotScalingWithLuck(ItemID.PixieShirt);
		pixieSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.PixiePants));

		IItemDropRule princessSet = ItemDropRule.NotScalingWithLuck(ItemID.PrincessHat);
		princessSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.PrincessDressNew));

		IItemDropRule pumpkinSet = ItemDropRule.NotScalingWithLuck(ItemID.PumpkinMask);
		pumpkinSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.PumpkinShirt));
		pumpkinSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.PumpkinPants));

		IItemDropRule robotSet = ItemDropRule.NotScalingWithLuck(ItemID.RobotMask);
		robotSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.RobotShirt));
		robotSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.RobotPants));

		IItemDropRule unicornSet = ItemDropRule.NotScalingWithLuck(ItemID.UnicornMask);
		unicornSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.UnicornShirt));
		unicornSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.UnicornPants));

		IItemDropRule vampireSet = ItemDropRule.NotScalingWithLuck(ItemID.VampireMask);
		vampireSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.VampireShirt));
		vampireSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.VampirePants));

		IItemDropRule witchSet = ItemDropRule.NotScalingWithLuck(ItemID.WitchHat);
		witchSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.WitchDress));
		witchSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.WitchBoots));

		IItemDropRule aintTypingThatSet = ItemDropRule.NotScalingWithLuck(ItemID.BrideofFrankensteinMask);
		aintTypingThatSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.BrideofFrankensteinDress));

		IItemDropRule karateTortoiseSet = ItemDropRule.NotScalingWithLuck(ItemID.KarateTortoiseMask);
		karateTortoiseSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.KarateTortoiseShirt));
		karateTortoiseSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.KarateTortoisePants));

		IItemDropRule reaperSet = ItemDropRule.NotScalingWithLuck(ItemID.ReaperHood);
		reaperSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.ReaperRobe));

		IItemDropRule foxSet = ItemDropRule.NotScalingWithLuck(ItemID.FoxMask);
		foxSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.FoxShirt));
		foxSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.FoxPants));

		IItemDropRule spaceCreatureSet = ItemDropRule.NotScalingWithLuck(ItemID.SpaceCreatureMask);
		spaceCreatureSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.SpaceCreatureShirt));
		spaceCreatureSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.SpaceCreaturePants));

		IItemDropRule wolfSet = ItemDropRule.NotScalingWithLuck(ItemID.WolfMask);
		wolfSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.WolfShirt));
		wolfSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.WolfPants));

		IItemDropRule treasureHunterSet = ItemDropRule.NotScalingWithLuck(ItemID.TreasureHunterShirt);
		treasureHunterSet.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.TreasureHunterPants));

		IItemDropRule[] vanitySets = new IItemDropRule[] {
			catSet,
			creeperSet,
			ghostSet,
			leprechaunSet,
			pixieSet,
			princessSet,
			pumpkinSet,
			robotSet,
			unicornSet,
			vampireSet,
			witchSet,
			aintTypingThatSet,
			karateTortoiseSet,
			reaperSet,
			foxSet,
			ItemDropRule.NotScalingWithLuck(ItemID.CatEars),
			spaceCreatureSet,
			wolfSet,
			treasureHunterSet
		};

		IItemDropRule[] rules = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.UnluckyYarn, 150),
			ItemDropRule.NotScalingWithLuck(ItemID.BatHook, 150),
			ItemDropRule.NotScalingWithLuck(ItemID.RottenEgg, 4, 10, 40),
			new OneFromRulesRule(10, paintings),
			new OneFromRulesRule(1, vanitySets),
		};

		RegisterToItem(ItemID.GoodieBag, new SequentialRulesNotScalingWithLuckRule(1, rules));
	}

	// code by Snek
	private void RegisterPresent()
	{
		IItemDropRule snowGlobeRule = ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.SnowGlobe, chanceDenominator: 15);

		IItemDropRule redRyderRule = ItemDropRule.NotScalingWithLuck(ItemID.RedRyder, chanceDenominator: 150);
		redRyderRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.MusketBall, minimumDropped: 30, maximumDropped: 60));

		IItemDropRule mrsClauseRule = ItemDropRule.NotScalingWithLuck(ItemID.MrsClauseHat);
		mrsClauseRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.MrsClauseShirt));
		mrsClauseRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.MrsClauseHeels));

		IItemDropRule parkaRule = ItemDropRule.NotScalingWithLuck(ItemID.ParkaHood);
		parkaRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.ParkaCoat));
		parkaRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.ParkaPants));

		IItemDropRule treeRule = ItemDropRule.NotScalingWithLuck(ItemID.TreeMask);
		treeRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.TreeShirt));
		treeRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.TreeTrunks));

		IItemDropRule[] vanityRules = new IItemDropRule[]
		{
			mrsClauseRule,
			parkaRule,
			treeRule,
			ItemDropRule.NotScalingWithLuck(ItemID.SnowHat),
			ItemDropRule.NotScalingWithLuck(ItemID.UglySweater)
		};

		IItemDropRule vanityRule = new OneFromRulesRule(chanceDenominator: 15, vanityRules);

		IItemDropRule foodRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(chanceDenominator: 7, ItemID.ChristmasPudding, ItemID.SugarCookie, ItemID.GingerbreadCookie);

		IItemDropRule blockRule = new OneFromRulesRule(1, ItemDropRule.Common(ItemID.PineTreeBlock, 1, 20, 49), ItemDropRule.Common(ItemID.CandyCaneBlock, 1, 20, 49), ItemDropRule.Common(ItemID.GreenCandyCaneBlock, 1, 20, 49));

		IItemDropRule[] rules = new IItemDropRule[]
		{
			snowGlobeRule,
			ItemDropRule.NotScalingWithLuck(ItemID.Coal, chanceDenominator: 30),
			ItemDropRule.NotScalingWithLuck(ItemID.DogWhistle, chanceDenominator: 400),
			redRyderRule,
			ItemDropRule.NotScalingWithLuck(ItemID.CandyCaneSword, chanceDenominator: 150),
			ItemDropRule.NotScalingWithLuck(ItemID.CnadyCanePickaxe, chanceDenominator: 150),
			ItemDropRule.NotScalingWithLuck(ItemID.CandyCaneHook, chanceDenominator: 150),
			ItemDropRule.NotScalingWithLuck(ItemID.FruitcakeChakram, chanceDenominator: 150),
			ItemDropRule.NotScalingWithLuck(ItemID.HandWarmer, chanceDenominator: 150),
			ItemDropRule.NotScalingWithLuck(ItemID.Toolbox, chanceDenominator: 300),
			ItemDropRule.NotScalingWithLuck(ItemID.ReindeerAntlers, chanceDenominator: 40),
			ItemDropRule.NotScalingWithLuck(ItemID.Holly, chanceDenominator: 10),
			vanityRule,
			foodRule,
			ItemDropRule.NotScalingWithLuck(ItemID.Eggnog, chanceDenominator: 8, maximumDropped: 3),
			ItemDropRule.NotScalingWithLuck(ItemID.StarAnise, chanceDenominator: 9, minimumDropped: 20, maximumDropped: 40),
			blockRule
		};

		RegisterToItem(ItemID.Present, new SequentialRulesNotScalingWithLuckRule(chanceDenominator: 1, rules));
	}

	private void RegisterCanOfWorms()
	{
		RegisterToItem(ItemID.CanOfWorms, ItemDropRule.Common(ItemID.Worm, 1, 5, 8));
		RegisterToItem(ItemID.CanOfWorms, new CommonDrop(ItemID.EnchantedNightcrawler, 10, 1, 3, 3));
		RegisterToItem(ItemID.CanOfWorms, ItemDropRule.Common(ItemID.GoldWorm, 20));
	}

	private void RegisterOyster()
	{
		IItemDropRule[] rules = new IItemDropRule[]
		{
			ItemDropRule.NotScalingWithLuck(ItemID.PinkPearl, 15),
			ItemDropRule.NotScalingWithLuck(ItemID.BlackPearl, 3),
			ItemDropRule.NotScalingWithLuck(ItemID.WhitePearl),
		};

		RegisterToItem(ItemID.Oyster, ItemDropRule.SequentialRulesNotScalingWithLuck(5, rules));
		RegisterToItem(ItemID.Oyster, ItemDropRule.Common(ItemID.ShuckedOyster));
	}

	private void RegisterCapricorns()
	{
		RegisterToItem(ItemID.CapricornLegs, ItemDropRule.Common(ItemID.CapricornTail));
		RegisterToItem(ItemID.CapricornTail, ItemDropRule.Common(ItemID.CapricornLegs));
	}
}
