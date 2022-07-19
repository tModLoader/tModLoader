using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.GameContent.ItemDropRules
{
	partial class ItemDropDatabase
	{
		private Dictionary<int, List<IItemDropRule>> _entriesByItemId = new Dictionary<int, List<IItemDropRule>>();
		private Dictionary<int, List<int>> _itemIdsByType = new Dictionary<int, List<int>>();

		public List<IItemDropRule> GetRulesForItemID(int itemID, bool includeGlobalDrops = true) {
			List<IItemDropRule> list = new List<IItemDropRule>();

			if (_entriesByItemId.TryGetValue(itemID, out List<IItemDropRule> value))
				list.AddRange(value);

			return list;
		}

		public IItemDropRule RegisterToItem(int type, IItemDropRule entry) {
			RegisterToItemId(type, entry);
			if (type > 0 && _itemIdsByType.TryGetValue(type, out List<int> value)) {
				for (int i = 0; i < value.Count; i++) {
					RegisterToItemId(value[i], entry);
				}
			}

			return entry;
		}

		public IItemDropRule RegisterToMultipleItems(IItemDropRule entry, params int[] itemIds) {
			for (int i = 0; i < itemIds.Length; i++) {
				RegisterToItem(itemIds[i], entry);
			}

			return entry;
		}

		public void RegisterToItemId(int itemId, IItemDropRule entry) {
			if (!_entriesByItemId.ContainsKey(itemId))
				_entriesByItemId[itemId] = new List<IItemDropRule>();

			_entriesByItemId[itemId].Add(entry);
		}

		private void RemoveFromItemId(int itemId, IItemDropRule entry) {
			if (_entriesByItemId.ContainsKey(itemId))
				_entriesByItemId[itemId].Remove(entry);
		}

		public IItemDropRule RemoveFromItem(int type, IItemDropRule entry) {
			RemoveFromItemId(type, entry);
			if (type > 0 && _itemIdsByType.TryGetValue(type, out List<int> value)) {
				for (int i = 0; i < value.Count; i++) {
					RemoveFromItemId(value[i], entry);
				}
			}

			return entry;
		}

		private void RegisterBossBags() {
			short item = ItemID.QueenSlimeBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.VolatileGelatin));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4986, 1, 25, 75));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.QueenSlimeMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4981, 2));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4980, 2));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4758, 3));
			RegisterToItem(item, ItemDropRule.FewFromOptionsNotScalingWithLuckWithX(2, 1, 2, ItemID.CrystalNinjaHelmet, ItemID.CrystalNinjaChestplate, ItemID.CrystalNinjaLeggings));

			item = ItemID.FairyQueenBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.EmpressFlightBooster));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FairyQueenMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.RainbowWings, 10));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 4923, 4952, 4953, 4914));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4778, 4));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(4715, 20));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.RainbowCursor, 20));

			item = ItemID.KingSlimeBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.RoyalGel));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.KingSlimeMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SlimySaddle, 2));
			RegisterToItem(item, ItemDropRule.FewFromOptionsNotScalingWithLuckWithX(2, 1, 2, ItemID.NinjaHood, ItemID.NinjaShirt, ItemID.NinjaPants));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SlimeGun, 2));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SlimeHook, 2));
			RegisterToItem(item, ItemDropRule.Common(ItemID.Solidifier));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin));

			item = ItemID.PlanteraBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.SporeSac));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.PlanteraMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.TempleKey));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Seedling, 15));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.TheAxe, 20));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.PygmyStaff, 2));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.ThornHook, 10));
			IItemDropRule itemDropRule2 = ItemDropRule.Common(758);
			itemDropRule2.OnSuccess(ItemDropRule.Common(771, 1, 50, 150), hideLootReport: true);
			RegisterToItem(item, new OneFromRulesRule(1, itemDropRule2, ItemDropRule.Common(1255), ItemDropRule.Common(788), ItemDropRule.Common(1178), ItemDropRule.Common(1259), ItemDropRule.Common(1155), ItemDropRule.Common(3018)));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 15, 15));

			item = ItemID.SkeletronPrimeBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalBatteryPiece));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SkeletronPrimeMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofFright, 1, 25, 40));
			RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 12, 12));

			item = ItemID.DestroyerBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalWagonPiece));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.SkeletronPrimeMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofMight, 1, 25, 40));
			RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 12, 12));

			item = ItemID.TwinsBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.MechanicalWheelPiece));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.TwinMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SoulofSight, 1, 25, 40));
			RegisterToItem(item, ItemDropRule.Common(ItemID.HallowedBar, 1, 20, 35));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 12, 12));

			item = ItemID.EyeOfCthulhuBossBag;
			Conditions.IsCrimson condition4 = new Conditions.IsCrimson();
			Conditions.IsCorruption condition5 = new Conditions.IsCorruption();
			RegisterToItem(item, ItemDropRule.Common(ItemID.EoCShield));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EyeMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Binoculars, 30));
			RegisterToItem(item, ItemDropRule.ByCondition(condition4, ItemID.CrimtaneOre, 1, 30, 87));
			RegisterToItem(item, ItemDropRule.ByCondition(condition4, ItemID.CrimsonSeeds, 1, 1, 3));
			RegisterToItem(item, ItemDropRule.ByCondition(condition5, 47, 1, 20, 49));
			RegisterToItem(item, ItemDropRule.ByCondition(condition5, 56, 1, 30, 87));
			RegisterToItem(item, ItemDropRule.ByCondition(condition5, 59, 1, 1, 3));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 3, 3));

			item = ItemID.BrainOfCthulhuBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.BrainOfConfusion));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BrainMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.CrimtaneOre, 1, 40, 90));
			RegisterToItem(item, ItemDropRule.Common(ItemID.TissueSample, 1, 10, 19));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BoneRattle, 20));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin));

			item = ItemID.EaterOfWorldsBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.WormScarf));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EaterMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.DemoniteOre, 1, 30, 59));
			RegisterToItem(item, ItemDropRule.Common(ItemID.ShadowScale, 1, 10, 19));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.EatersBone, 20));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 3, 3));

			item = ItemID.DeerclopsBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.BoneHelm));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DeerclopsMask, 7));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 5098, ItemID.Eyebrella, ItemID.DontStarveShaderItem));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 5117, 5118, 5119, 5095));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 10, 10));

			item = ItemID.QueenBeeBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.HiveBackpack));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BeeMask, 7));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BeeGun, ItemID.BeeKeeper, ItemID.BeesKnees));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.HoneyComb, 3));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Nectar, 11));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.HoneyedGoggles, 9));
			RegisterToItem(item, ItemDropRule.Common(ItemID.HiveWand));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BeeHat, ItemID.BeeShirt, ItemID.BeePants));
			RegisterToItem(item, ItemDropRule.Common(ItemID.Beenade, 1, 10, 30));
			RegisterToItem(item, ItemDropRule.Common(ItemID.BeeWax, 1, 17, 30));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 10, 10));

			item = ItemID.SkeletronBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.BoneGlove));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.SkeletronMask, ItemID.SkeletronHand, ItemID.BookofSkulls));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 5, 5));

			item = ItemID.WallOfFleshBossBag;
			RegisterToItem(item, ItemDropRule.ByCondition(new Conditions.NotUsedDemonHeart(), ItemID.DemonHeart));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FleshMask, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.Pwnhammer));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.SummonerEmblem, ItemID.SorcererEmblem, ItemID.WarriorEmblem, ItemID.RangerEmblem));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.LaserRifle, ItemID.BreakerBlade, ItemID.ClockworkAssaultRifle, ItemID.FireWhip));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 8, 8));

			item = ItemID.CultistBossBag;
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskCultist, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SilverCoin, 1, 20, 20));

			item = ItemID.MoonLordBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.GravityGlobe));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SuspiciousLookingTentacle));
			RegisterToItem(item, ItemDropRule.Common(4954));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskMoonlord, 7));
			RegisterToItem(item, ItemDropRule.Common(ItemID.LunarOre, 1, 90, 110));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.MeowmereMinecart, 10));
			RegisterToItem(item, ItemDropRule.ByCondition(new Conditions.NoPortalGun(), ItemID.PortalGun));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.Meowmere, ItemID.Terrarian, ItemID.StarWrath, ItemID.SDMG, ItemID.Celeb2, ItemID.LastPrism, ItemID.LunarFlareBook, ItemID.RainbowCrystalStaff, 3569));

			item = ItemID.BossBagBetsy;
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BossMaskBetsy, 7));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 3827, 3859, 3870, 3858));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.BetsyWings, 4));
			RegisterToItem(item, ItemDropRule.Common(ItemID.DefenderMedal, 1, 30, 49));

			item = ItemID.GolemBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.ShinyStone));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.GolemMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.Picksaw, 3));
			IItemDropRule itemDropRule3 = ItemDropRule.Common(1258);
			itemDropRule3.OnSuccess(ItemDropRule.Common(1261, 1, 60, 180), hideLootReport: true);
			RegisterToItem(item, new OneFromRulesRule(1, itemDropRule3, ItemDropRule.Common(1122), ItemDropRule.Common(899), ItemDropRule.Common(1248), ItemDropRule.Common(1295), ItemDropRule.Common(1296), ItemDropRule.Common(1297)));
			RegisterToItem(item, ItemDropRule.Common(ItemID.BeetleHusk, 1, 18, 23));
			RegisterToItem(item, ItemDropRule.Common(ItemID.GoldCoin, 1, 12, 12));

			item = ItemID.FishronBossBag;
			RegisterToItem(item, ItemDropRule.Common(ItemID.ShrimpyTruffle));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.DukeFishronMask, 7));
			RegisterToItem(item, ItemDropRule.NotScalingWithLuck(ItemID.FishronWings, 10));
			RegisterToItem(item, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 2611, 2624, 2622, 2621, 2623));
			RegisterToItem(item, ItemDropRule.Common(ItemID.SilverCoin, 1, 20, 20));
		}

		private void RegisterCrateDrops() {
			IItemDropRule[] sailfishBootsTsunamiExtractinator = new IItemDropRule[3];
			sailfishBootsTsunamiExtractinator[0] = ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 40);
			sailfishBootsTsunamiExtractinator[1] = new CommonDrop(ItemID.TsunamiInABottle, 1600, chanceNumerator: 39);
			sailfishBootsTsunamiExtractinator[2] = ItemDropRule.NotScalingWithLuck(ItemID.Extractinator, 53);
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(1, sailfishBootsTsunamiExtractinator));
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(45, ItemID.Aglet, ItemID.Umbrella, ItemID.ClimbingClaws, 3068, ItemID.Radar), ItemID.WoodenCrate, ItemID.WoodenCrateHard);
			IItemDropRule silverCoinGoldCoin = new CommonDrop(ItemID.SilverCoin, 21, 20, 90, 2);
			silverCoinGoldCoin.OnFailedRoll(ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 21, 1, 5));
			RegisterToItem(ItemID.WoodenCrate, silverCoinGoldCoin);
			IItemDropRule[] preHmOres1 = new IItemDropRule[4];
			preHmOres1[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 7, 6, 23);
			preHmOres1[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 7, 6, 23);
			preHmOres1[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 7, 6, 23);
			preHmOres1[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 7, 6, 23);
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(1, preHmOres1));
			IItemDropRule[] preHmBars1 = new IItemDropRule[4];
			preHmBars1[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 28, 2, 7);
			preHmBars1[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 28, 2, 7);
			preHmBars1[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 28, 2, 7);
			preHmBars1[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 28, 2, 7);
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(3, preHmBars1));
			IItemDropRule[] classicPotions1 = new IItemDropRule[10];
			classicPotions1[0] = ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 7, 1, 3);
			classicPotions1[1] = ItemDropRule.NotScalingWithLuck(ItemID.SwiftnessPotion, 7, 1, 3);
			classicPotions1[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronskinPotion, 7, 1, 3);
			classicPotions1[3] = ItemDropRule.NotScalingWithLuck(ItemID.NightOwlPotion, 7, 1, 3);
			classicPotions1[4] = ItemDropRule.NotScalingWithLuck(ItemID.ShinePotion, 7, 1, 3);
			classicPotions1[5] = ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 7, 1, 3);
			classicPotions1[6] = ItemDropRule.NotScalingWithLuck(ItemID.GillsPotion, 7, 1, 3);
			classicPotions1[7] = ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 7, 1, 3);
			classicPotions1[8] = ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 7, 1, 3);
			classicPotions1[9] = ItemDropRule.NotScalingWithLuck(2329, 7, 1, 3);
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(1, classicPotions1));
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.LesserHealingPotion, 3, 5, 15), ItemDropRule.NotScalingWithLuck(ItemID.LesserManaPotion, 3, 5, 15)));
			RegisterToItem(ItemID.WoodenCrate, new OneFromRulesRule(1, new CommonDrop(ItemID.ApprenticeBait, 9, 1, 4, 2), new CommonDropNotScalingWithLuck(ItemID.JourneymanBait, 9, 1, 4)));

			IItemDropRule[] anchorSailfishTsunamiSundial = new IItemDropRule[4];
			anchorSailfishTsunamiSundial[0] = new CommonDrop(ItemID.Anchor, 8000, chanceNumerator: 302);
			anchorSailfishTsunamiSundial[1] = new CommonDrop(ItemID.SailfishBoots, 8000, chanceNumerator: 199);
			anchorSailfishTsunamiSundial[2] = new CommonDrop(ItemID.TsunamiInABottle, 8000, chanceNumerator: 179);
			anchorSailfishTsunamiSundial[3] = ItemDropRule.NotScalingWithLuck(ItemID.Sundial, 200);
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, anchorSailfishTsunamiSundial));
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, silverCoinGoldCoin));
			IItemDropRule[] preHmOres2 = new IItemDropRule[4];
			preHmOres2[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 14, 6, 23);
			preHmOres2[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 14, 6, 23);
			preHmOres2[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 14, 6, 23);
			preHmOres2[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 14, 6, 23);
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, preHmOres2));
			IItemDropRule[] hmOres1 = new IItemDropRule[2];
			hmOres1[0] = ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 14, 6, 23);
			hmOres1[1] = ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 14, 6, 23);
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, hmOres1));
			IItemDropRule[] preHmBars2 = new IItemDropRule[4];
			preHmBars2[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 56, 2, 7);
			preHmBars2[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 56, 2, 7);
			preHmBars2[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 56, 2, 7);
			preHmBars2[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 56, 2, 7);
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(3, preHmBars2));
			IItemDropRule[] hmBars1 = new IItemDropRule[2];
			hmBars1[0] = ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 56, 2, 5);
			hmBars1[1] = ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 56, 2, 5);
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(3, hmBars1));
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, classicPotions1));
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.LesserHealingPotion, 3, 5, 15), ItemDropRule.NotScalingWithLuck(ItemID.LesserManaPotion, 3, 5, 15)));
			RegisterToItem(ItemID.WoodenCrateHard, new OneFromRulesRule(1, new CommonDrop(ItemID.ApprenticeBait, 9, 1, 4, 2), new CommonDropNotScalingWithLuck(ItemID.JourneymanBait, 9, 1, 4)));

			IItemDropRule[] gingerTartarFalconSailfishTsunami = new IItemDropRule[5];
			gingerTartarFalconSailfishTsunami[0] = ItemDropRule.NotScalingWithLuck(ItemID.GingerBeard, 25);
			gingerTartarFalconSailfishTsunami[1] = new CommonDrop(ItemID.TartarSauce, 125, chanceNumerator: 6);
			gingerTartarFalconSailfishTsunami[2] = new CommonDrop(ItemID.FalconBlade, 625, chanceNumerator: 38);
			gingerTartarFalconSailfishTsunami[3] = new CommonDrop(ItemID.SailfishBoots, 3125, chanceNumerator: 133);
			gingerTartarFalconSailfishTsunami[4] = new CommonDrop(ItemID.TsunamiInABottle, 8000, chanceNumerator: 323);
			RegisterToItem(ItemID.IronCrate, new OneFromRulesRule(1, gingerTartarFalconSailfishTsunami));
			RegisterToItem(ItemID.IronCrate, ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 10));
			IItemDropRule[] preHmOres3 = new IItemDropRule[6];
			preHmOres3[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 6, 18, 29);
			preHmOres3[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 6, 18, 29);
			preHmOres3[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 6, 18, 29);
			preHmOres3[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 6, 18, 29);
			preHmOres3[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 6, 18, 29);
			preHmOres3[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 6, 18, 29);
			RegisterToItem(ItemID.IronCrate, new OneFromRulesRule(1, preHmOres3));
			IItemDropRule[] preHmBars3 = new IItemDropRule[6];
			preHmBars3[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 24, 6, 9);
			preHmBars3[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 24, 6, 9);
			preHmBars3[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 24, 6, 9);
			preHmBars3[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 24, 6, 9);
			preHmBars3[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 24, 6, 9);
			preHmBars3[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 24, 6, 9);
			RegisterToItem(ItemID.IronCrate, new OneFromRulesRule(5, preHmBars3));
			IItemDropRule[] classicPotions2 = new IItemDropRule[8];
			classicPotions2[0] = ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 4, 2, 4);
			classicPotions2[1] = ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 4, 2, 4);
			classicPotions2[2] = ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 4, 2, 4);
			classicPotions2[3] = ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 4, 2, 4);
			classicPotions2[4] = ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 4, 2, 4);
			classicPotions2[5] = ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 4, 2, 4);
			classicPotions2[6] = ItemDropRule.NotScalingWithLuck(ItemID.CalmingPotion, 4, 2, 4);
			classicPotions2[7] = ItemDropRule.NotScalingWithLuck(ItemID.FlipperPotion, 4, 2, 4);
			RegisterToMultipleItems(new OneFromRulesRule(1, classicPotions2), ItemID.IronCrate, ItemID.IronCrateHard);
			RegisterToMultipleItems(new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 2, 5, 15), ItemDropRule.NotScalingWithLuck(ItemID.LesserManaPotion, 2, 5, 15)), ItemID.IronCrate, ItemID.IronCrateHard);
			RegisterToMultipleItems(new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 3, 2, 4), ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 6, 2, 4)), ItemID.IronCrate, ItemID.IronCrateHard);

			IItemDropRule[] sundialGingerTartarFalconSailfishTsunami = new IItemDropRule[6];
			sundialGingerTartarFalconSailfishTsunami[0] = ItemDropRule.NotScalingWithLuck(ItemID.Sundial, 60);
			sundialGingerTartarFalconSailfishTsunami[1] = new CommonDrop(ItemID.GingerBeard, 1500, chanceNumerator: 59);
			sundialGingerTartarFalconSailfishTsunami[2] = new CommonDrop(ItemID.TartarSauce, 1250, chanceNumerator: 59);
			sundialGingerTartarFalconSailfishTsunami[3] = new CommonDrop(ItemID.FalconBlade, 10000, chanceNumerator: 598);
			sundialGingerTartarFalconSailfishTsunami[4] = new CommonDrop(ItemID.SailfishBoots, 3125, chanceNumerator: 131);
			sundialGingerTartarFalconSailfishTsunami[5] = new CommonDrop(ItemID.TsunamiInABottle, 8000, chanceNumerator: 318);
			RegisterToItem(ItemID.IronCrateHard, new OneFromRulesRule(1, sundialGingerTartarFalconSailfishTsunami));
			IItemDropRule[] preHmOres4 = new IItemDropRule[6];
			preHmOres4[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 12, 18, 29);
			preHmOres4[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 12, 18, 29);
			preHmOres4[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 12, 18, 29);
			preHmOres4[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 12, 18, 29);
			preHmOres4[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 12, 18, 29);
			preHmOres4[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 12, 18, 29);
			RegisterToItem(ItemID.IronCrateHard, new OneFromRulesRule(1, preHmOres4));
			IItemDropRule[] hmOres2 = new IItemDropRule[4];
			hmOres2[0] = ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 12, 18, 29);
			hmOres2[1] = ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 12, 18, 29);
			hmOres2[2] = ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 12, 18, 29);
			hmOres2[3] = ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 12, 18, 29);
			RegisterToItem(ItemID.IronCrateHard, new OneFromRulesRule(1, hmOres2));
			IItemDropRule[] preHmBars4 = new IItemDropRule[6];
			preHmBars4[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 72, 6, 9);
			preHmBars4[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 72, 6, 9);
			preHmBars4[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 72, 6, 9);
			preHmBars4[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 72, 6, 9);
			preHmBars4[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 72, 6, 9);
			preHmBars4[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 72, 6, 9);
			RegisterToItem(ItemID.IronCrateHard, new OneFromRulesRule(5, preHmBars3));
			IItemDropRule[] hmBars2 = new IItemDropRule[4];
			hmBars2[0] = ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 36, 5, 9);
			hmBars2[1] = ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 36, 5, 9);
			hmBars2[2] = ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 36, 5, 9);
			hmBars2[3] = ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 36, 5, 9);
			RegisterToItem(ItemID.IronCrateHard, new OneFromRulesRule(5, hmBars2));

			RegisterToItem(ItemID.GoldenCrate, new OneFromRulesRule(1, new CommonDropNotScalingWithLuck(ItemID.LifeCrystal, 15, 1, 1), new CommonDrop(ItemID.HardySaddle, 75, 1, 1, 7)));
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 8, 20), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
			IItemDropRule[] preHmOres5 = new IItemDropRule[4];
			preHmOres5[0] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 5, 30, 44);
			preHmOres5[1] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 5, 30, 44);
			preHmOres5[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 5, 30, 44);
			preHmOres5[3] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 5, 30, 44);
			RegisterToItem(ItemID.GoldenCrate, new OneFromRulesRule(1, preHmOres5));
			IItemDropRule[] preHmBars5 = new IItemDropRule[4];
			preHmBars5[0] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 15, 10, 14);
			preHmBars5[1] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 15, 10, 14);
			preHmBars5[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 15, 10, 14);
			preHmBars5[3] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 15, 10, 14);
			RegisterToItem(ItemID.GoldenCrate, new OneFromRulesRule(4, preHmBars5));
			IItemDropRule[] classicPotions3 = new IItemDropRule[5];
			classicPotions3[0] = ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 3, 2, 5);
			classicPotions3[1] = ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 3, 2, 5);
			classicPotions3[2] = ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 3, 2, 5);
			classicPotions3[3] = ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 3, 2, 5);
			classicPotions3[4] = ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 3, 2, 5);
			RegisterToMultipleItems(new OneFromRulesRule(1, classicPotions3), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
			RegisterToMultipleItems(new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 2, 5, 20), ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 2, 5, 20)), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
			RegisterToMultipleItems(new CommonDrop(ItemID.MasterBait, 3, 3, 7, 2), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.EnchantedSword, 50), ItemID.GoldenCrate, ItemID.GoldenCrateHard);

			RegisterToItem(ItemID.GoldenCrateHard, new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.Sundial, 20), new CommonDrop(ItemID.LifeCrystal, 300, 1, 1, 19), new CommonDrop(ItemID.HardySaddle, 750, 1, 1, 49)));
			IItemDropRule[] preHmOres6 = new IItemDropRule[4];
			preHmOres6[0] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 10, 30, 44);
			preHmOres6[1] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 10, 30, 44);
			preHmOres6[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 10, 30, 44);
			preHmOres6[3] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 10, 30, 44);
			RegisterToItem(ItemID.GoldenCrateHard, new OneFromRulesRule(1, preHmOres6));
			IItemDropRule[] hmOres3 = new IItemDropRule[4];
			hmOres3[0] = ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 10, 30, 44);
			hmOres3[1] = ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 10, 30, 44);
			hmOres3[2] = ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 10, 30, 44);
			hmOres3[3] = ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 10, 30, 44);
			RegisterToItem(ItemID.GoldenCrateHard, new OneFromRulesRule(1, hmOres3));
			IItemDropRule[] preHmBars6 = new IItemDropRule[4];
			preHmBars6[0] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 45, 10, 14);
			preHmBars6[1] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 45, 10, 14);
			preHmBars6[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 45, 10, 14);
			preHmBars6[3] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 45, 10, 14);
			RegisterToItem(ItemID.GoldenCrateHard, new OneFromRulesRule(4, preHmBars6));
			IItemDropRule[] hmBars3 = new IItemDropRule[4];
			hmBars3[0] = ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 45, 10, 14);
			hmBars3[1] = ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 45, 10, 14);
			hmBars3[2] = ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 45, 10, 14);
			hmBars3[3] = ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 45, 10, 14);
			RegisterToItem(ItemID.GoldenCrateHard, new OneFromRulesRule(8, hmBars3));

			RegisterToMultipleItems(ItemDropRule.SequentialRules(1, ItemDropRule.NotScalingWithLuck(ItemID.FlowerBoots, 20), ItemDropRule.OneFromOptions(1, ItemID.AnkletoftheWind, ItemID.Boomstick, ItemID.FeralClaws, ItemID.StaffofRegrowth, ItemID.FiberglassFishingPole)), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, 4978, ItemID.Starfury, ItemID.ShinyRedBalloon), ItemID.FloatingIslandFishingCrate, ItemID.FloatingIslandFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BallOHurt, ItemID.BandofStarpower, ItemID.Musket, ItemID.ShadowOrb, ItemID.Vilethorn), ItemID.CorruptFishingCrate, ItemID.CorruptFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.LockBox), ItemID.DungeonFishingCrate, ItemID.DungeonFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.Book, 2, 5, 15), ItemID.DungeonFishingCrate, ItemID.DungeonFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.IceBoomerang, ItemID.IceBlade, ItemID.IceSkates, ItemID.SnowballCannon, ItemID.BlizzardinaBottle, ItemID.FlurryBoots), ItemID.FrozenCrate, ItemID.FrozenCrateHard);
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.AncientChisel, ItemID.ScarabFishingRod, ItemID.SandBoots, 4061, 4062, ItemID.CatBast, ItemID.MysticCoilSnake, ItemID.MagicConch), ItemID.OasisCrate, ItemID.OasisCrateHard);
			IItemDropRule[] lavaDropRules = new IItemDropRule[6];
			lavaDropRules[0] = ItemDropRule.Common(ItemID.LavaCharm, 20);
			lavaDropRules[1] = new CommonDrop(ItemID.FlameWakerBoots, 100, chanceNumerator: 19);
			lavaDropRules[2] = new CommonDrop(ItemID.SuperheatedBlood, 100, chanceNumerator: 19);
			lavaDropRules[3] = new CommonDrop(ItemID.LavaFishbowl, 100, chanceNumerator: 19);
			lavaDropRules[4] = new CommonDrop(ItemID.LavaFishingHook, 100, chanceNumerator: 19);
			lavaDropRules[5] = new CommonDrop(ItemID.VolcanoSmall, 100, chanceNumerator: 19);
			RegisterToMultipleItems(new OneFromRulesRule(1, lavaDropRules), ItemID.LavaCrate, ItemID.LavaCrateHard);
			IItemDropRule[] oceanDropRules = new IItemDropRule[6];
			oceanDropRules[0] = ItemDropRule.Common(ItemID.SharkBait, 10);
			oceanDropRules[1] = new CommonDrop(ItemID.WaterWalkingBoots, 100, chanceNumerator: 9);
			oceanDropRules[2] = new CommonDrop(ItemID.BreathingReed, 400, chanceNumerator: 81);
			oceanDropRules[3] = new CommonDrop(ItemID.FloatingTube, 400, chanceNumerator: 81);
			oceanDropRules[4] = new CommonDrop(ItemID.Trident, 400, chanceNumerator: 81);
			oceanDropRules[5] = new CommonDrop(ItemID.Flipper, 400, chanceNumerator: 81);
			RegisterToMultipleItems(new OneFromRulesRule(1, oceanDropRules), ItemID.OceanCrate, ItemID.OceanCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.ScarabBomb, 4, 4, 6), ItemID.OasisCrate, ItemID.OasisCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(4858, 4, 2, 2), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.Common(ItemID.ObsidianLockbox), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.WetBomb, 3, 7, 10), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemID.PottedLavaPlantPalm, ItemID.PottedLavaPlantBush, ItemID.PottedLavaPlantBramble, ItemID.PottedLavaPlantBulb, ItemID.PottedLavaPlantTendrils), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 12), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			IItemDropRule[] preHmOres7 = new IItemDropRule[8];
			preHmOres7[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 7, 30, 49);
			preHmOres7[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 7, 30, 49);
			preHmOres7[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 7, 30, 49);
			preHmOres7[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 7, 30, 49);
			preHmOres7[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 7, 30, 49);
			preHmOres7[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 7, 30, 49);
			preHmOres7[6] = ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 7, 30, 49);
			preHmOres7[7] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 7, 30, 49);
			RegisterToMultipleItems(new OneFromRulesRule(1, preHmOres7), ItemID.JungleFishingCrate, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate);
			IItemDropRule[] preHmOres8 = new IItemDropRule[8];
			preHmOres8[0] = ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 14, 30, 49);
			preHmOres8[1] = ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 14, 30, 49);
			preHmOres8[2] = ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 14, 30, 49);
			preHmOres8[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 14, 30, 49);
			preHmOres8[4] = ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 14, 30, 49);
			preHmOres8[5] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 14, 30, 49);
			preHmOres8[6] = ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 14, 30, 49);
			preHmOres8[7] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 14, 30, 49);
			RegisterToMultipleItems(new OneFromRulesRule(1, preHmOres8), ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			IItemDropRule[] preHmBars7 = new IItemDropRule[6];
			preHmBars7[0] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 4, 10, 20);
			preHmBars7[1] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 4, 10, 20);
			preHmBars7[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 4, 10, 20);
			preHmBars7[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 4, 10, 20);
			preHmBars7[4] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 4, 10, 20);
			preHmBars7[5] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 4, 10, 20);
			RegisterToMultipleItems(new OneFromRulesRule(1, preHmBars7), ItemID.JungleFishingCrate, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate);
			IItemDropRule[] preHmBars8 = new IItemDropRule[6];
			preHmBars8[0] = ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 12, 10, 20);
			preHmBars8[1] = ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 12, 10, 20);
			preHmBars8[2] = ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 12, 10, 20);
			preHmBars8[3] = ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 12, 10, 20);
			preHmBars8[4] = ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 12, 10, 20);
			preHmBars8[5] = ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 12, 10, 20);
			RegisterToMultipleItems(new OneFromRulesRule(1, preHmBars8), ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			IItemDropRule[] hmBars4 = new IItemDropRule[6];
			hmBars4[0] = ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 6, 8, 20);
			hmBars4[1] = ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 6, 8, 20);
			hmBars4[2] = ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 6, 8, 20);
			hmBars4[3] = ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 6, 8, 20);
			hmBars4[4] = ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 6, 8, 20);
			hmBars4[5] = ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 6, 8, 20);
			RegisterToMultipleItems(new OneFromRulesRule(1, hmBars4), ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			IItemDropRule[] classicPotions4 = new IItemDropRule[6];
			classicPotions4[0] = ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 4, 2, 4);
			classicPotions4[1] = ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 4, 2, 4);
			classicPotions4[2] = ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 4, 2, 4);
			classicPotions4[3] = ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 4, 2, 4);
			classicPotions4[4] = ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 4, 2, 4);
			classicPotions4[5] = ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 4, 2, 4);
			RegisterToMultipleItems(new OneFromRulesRule(1, classicPotions4), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			RegisterToMultipleItems(new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 2, 5, 17), ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 2, 5, 17)), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			RegisterToMultipleItems(new OneFromRulesRule(1, ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 2, 2, 6), ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 2, 2, 6)), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard, ItemID.FloatingIslandFishingCrate, ItemID.CorruptFishingCrate, ItemID.CrimsonFishingCrate, ItemID.HallowedFishingCrate, ItemID.DungeonFishingCrate, ItemID.FrozenCrate, ItemID.OasisCrate, ItemID.LavaCrate, ItemID.OceanCrate, ItemID.FloatingIslandFishingCrateHard, ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard, ItemID.HallowedFishingCrateHard, ItemID.DungeonFishingCrateHard, ItemID.FrozenCrateHard, ItemID.OasisCrateHard, ItemID.LavaCrateHard, ItemID.OceanCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.BambooBlock, 3, 20, 50), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.Seaweed, 20), ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.SoulofNight, 2, 2, 5), ItemID.CorruptFishingCrateHard, ItemID.CrimsonFishingCrateHard);
			RegisterToItem(ItemID.HallowedFishingCrateHard, ItemDropRule.NotScalingWithLuck(ItemID.SoulofLight, 2, 2, 5));
			RegisterToItem(ItemID.CorruptFishingCrateHard, ItemDropRule.NotScalingWithLuck(ItemID.CursedFlame, 2, 2, 5));
			RegisterToItem(ItemID.CrimsonFishingCrateHard, ItemDropRule.NotScalingWithLuck(ItemID.Ichor, 2, 2, 5));
			RegisterToItem(ItemID.HallowedFishingCrateHard, ItemDropRule.NotScalingWithLuck(ItemID.CrystalShard, 2, 4, 10));
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.Fish, 20), ItemID.FrozenCrate, ItemID.FrozenCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.OrnateShadowKey, 20), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.HellCake, 20), ItemID.LavaCrate, ItemID.LavaCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.ShellPileBlock, 3, 20, 50), ItemID.OceanCrate, ItemID.OceanCrateHard);
			RegisterToMultipleItems(ItemDropRule.NotScalingWithLuck(ItemID.SandcastleBucket, 10), ItemID.OceanCrate, ItemID.OceanCrateHard);
		}

		private void RegisterObsidianLockbox() {
			RegisterToItem(ItemID.ObsidianLockbox, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.DarkLance, ItemID.Sunfury, ItemID.FlowerofFire, ItemID.Flamelash, ItemID.HellwingBow, ItemID.TreasureMagnet));
		}

		private void RegisterLockbox() {
			RegisterToItem(ItemID.LockBox, ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.Valor, ItemID.Muramasa, ItemID.CobaltShield, ItemID.AquaScepter, ItemID.BlueMoon, ItemID.MagicMissile, ItemID.Handgun));
			RegisterToItem(ItemID.LockBox, ItemDropRule.NotScalingWithLuck(ItemID.ShadowKey, 3));
		}

		private void RegisterHerbBag() {
			RegisterToItem(ItemID.HerbBag, new HerbBagDropsItemDropRule(ItemID.Daybloom, ItemID.Moonglow, ItemID.Blinkroot, ItemID.Waterleaf, ItemID.Deathweed, ItemID.Fireblossom, ItemID.Shiverthorn, ItemID.DaybloomSeeds, ItemID.MoonglowSeeds, ItemID.BlinkrootSeeds, ItemID.WaterleafSeeds, ItemID.DeathweedSeeds, ItemID.FireblossomSeeds, ItemID.ShiverthornSeeds));
		}

		private void RegisterGoodieBag() {
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
		private void RegisterPresent() {
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

			IItemDropRule vanityRule = new OneFromRulesRule(chanceNumerator: 15, vanityRules);

			IItemDropRule foodRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(chanceDenominator: 7, ItemID.ChristmasPudding, ItemID.SugarCookie, ItemID.GingerbreadCookie);

			IItemDropRule blockRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(chanceDenominator: 1, ItemID.PineTreeBlock, ItemID.CandyCaneBlock, ItemID.GreenCandyCaneBlock);

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
				ItemDropRule.NotScalingWithLuck(ItemID.ReindeerBells, chanceDenominator: 40),
				ItemDropRule.NotScalingWithLuck(ItemID.Holly, chanceDenominator: 10),
				vanityRule,
				foodRule,
				ItemDropRule.NotScalingWithLuck(ItemID.Eggnog, chanceDenominator: 8, maximumDropped: 3),
				ItemDropRule.NotScalingWithLuck(ItemID.StarAnise, chanceDenominator: 9, minimumDropped: 20, maximumDropped: 40),
				blockRule
			};

			RegisterToItem(ItemID.Present, new SequentialRulesNotScalingWithLuckRule(chanceDenominator: 1, rules));
		}

		private void RegisterCanOfWorms() {
			RegisterToItem(ItemID.CanOfWorms, ItemDropRule.Common(ItemID.Worm, 1, 5, 8));
			RegisterToItem(ItemID.CanOfWorms, new CommonDrop(3191, 10, 1, 3, 3));
			RegisterToItem(ItemID.CanOfWorms, ItemDropRule.Common(2895, 20));
		}

		private void RegisterOyster() {
			IItemDropRule[] rules = new IItemDropRule[]
			{
				ItemDropRule.NotScalingWithLuck(4414, 15),
				ItemDropRule.NotScalingWithLuck(4413, 3),
				ItemDropRule.NotScalingWithLuck(4412),
			};

			RegisterToItem(ItemID.Oyster, ItemDropRule.SequentialRulesNotScalingWithLuck(5, rules));
			RegisterToItem(ItemID.Oyster, ItemDropRule.Common(4411));
		}
	}
}
