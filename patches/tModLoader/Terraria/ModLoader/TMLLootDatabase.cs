using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	// can and/or will be used for other tml loots later. (ie pots loot)
	public class TMLLootDatabase
	{
		private Dictionary<int, ChestLoot> npcShopByType = new();
		private List<ChestLoot.Entry> globalNpcShopEntries = new();

		public TMLLootDatabase() {
			Initialize();
		}

		public void RegisterNpcShop(int npcId, ChestLoot chestLoot) => npcShopByType.Add(npcId, chestLoot);

		public void RegisterGlobalNpcShop(ChestLoot.Entry entry) => globalNpcShopEntries.Add(entry);

		public ChestLoot GetNpcShopById(int npcId) => npcShopByType[npcId];

		public List<ChestLoot.Entry> GetGlobalNpcShopEntries() => globalNpcShopEntries;

		public void Initialize() {
			RegisterMerchant();
			RegisterArmsDealer();
			RegisterDryad();
			RegisterBombGuy();
			RegisterClothier();
			RegisterGoblin();
			RegisterWizard();
			RegisterMechanic();
			RegisterSantaClaus();
			RegisterTruffle();
			RegisterSteampunker();
			RegisterDyeTrader();
			RegisterPartyGirl();
			RegisterCyborg();
			RegisterPainter();
			RegisterWitchDoctor();
			RegisterPirate();
			RegisterStylist();
			RegisterSkeletonMerchant();
			RegisterBartender();
			RegisterGolfer();
			RegisterZoologist();
			RegisterPrincess();

			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				ChestLoot chest = npcShopByType.TryGetValue(i, out ChestLoot chestLoot) ? chestLoot : new();
				NPCLoader.SetupShop(i, chest);

				if (!npcShopByType.ContainsKey(i))
					RegisterNpcShop(i, chest);
				else
					npcShopByType[i] = chest;
			}

			RegisterGlobalNpcShop();

			var clone = npcShopByType.ToDictionary(x => x.Key, x => x.Value);
			foreach (var lbt in npcShopByType) {
				NPCLoader.PostSetupShop(lbt.Key, lbt.Value);
				clone[lbt.Key] = lbt.Value;
			}
			npcShopByType = clone;
		}

		private void RegisterGlobalNpcShop() {
			ChestLoot.Entry forestPylon = new(new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.ZonePurity)); // should be kept as "!Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && !Main.player[Main.myPlayer].ZoneGlowshroom"?
			forestPylon.OnSuccess(new ChestLoot.Entry(ChestLoot.Condition.RemixWorld)
				.OnSuccess(new ChestLoot.Entry(new ChestLoot.Condition(NetworkText.Empty, () => (double)(Main.player[Main.myPlayer].Center.Y / 16f) > Main.rockLayer && Main.player[Main.myPlayer].Center.Y / 16f < (Main.maxTilesY - 350)))
				.OnSuccess(4876)))
				.OnFail(new ChestLoot.Entry(new ChestLoot.Condition(NetworkText.Empty, () => (double)(Main.player[Main.myPlayer].Center.Y / 16f) < Main.worldSurface))
				.OnSuccess(4876));

			ChestLoot.Entry cavePylon = new ChestLoot.Entry(ChestLoot.Condition.RemixWorld)
				.OnSuccess(new ChestLoot.Entry(new ChestLoot.Condition(NetworkText.Empty, () => !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && (double)(Main.player[Main.myPlayer].Center.Y / 16f) >= Main.worldSurface))
				.OnSuccess(4917))
				.OnFail(new ChestLoot.Entry(new ChestLoot.Condition(NetworkText.Empty, () => !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && !Main.player[Main.myPlayer].ZoneGlowshroom && (double)(Main.player[Main.myPlayer].Center.Y / 16f) >= Main.worldSurface))
				.OnSuccess(4917));

			ChestLoot.Entry entry = new(new ChestLoot.Condition(NetworkText.Empty, () => (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment > 0.8999999761581421 || Main.remixWorld) && TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16()) && !Main.LocalPlayer.ZoneCrimson && !Main.LocalPlayer.ZoneCorrupt));
			entry.OnSuccess(forestPylon);
			entry.OnSuccess(4920, ChestLoot.Condition.InSnowBiome);
			entry.OnSuccess(4919, ChestLoot.Condition.InDesertBiome);
			entry.OnSuccess(4916, ChestLoot.Condition.InBeachBiome, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.position.Y < Main.worldSurface * 16f));
			entry.OnSuccess(4875, ChestLoot.Condition.InJungleBiome);
			entry.OnSuccess(4916, ChestLoot.Condition.InHallowBiome);
			entry.OnSuccess(4921, ChestLoot.Condition.InGlowshroomBiome, new ChestLoot.Condition(NetworkText.Empty, () => !Main.remixWorld || Main.LocalPlayer.Center.Y / 16f < (Main.maxTilesY - 200)));

			/*foreach (ModPylon pylon in PylonLoader.modPylons) {
				int? pylonReturn = pylon.IsPylonForSale(Main.LocalPlayer, Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421);
				if (pylonReturn.HasValue) {
					entry.OnSuccess(pylonReturn.Value, new ChestLoot.Condition(NetworkText.Empty, () => pylon.IsPylonForSale(Main.LocalPlayer, Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421).Value > 0));
				}
			}*/

			RegisterGlobalNpcShop(entry);
		}

		private void RegisterMerchant() {
			ChestLoot shop = new();
			shop.Add(88);
			shop.Add(87);
			shop.Add(35);
			shop.Add(1991);
			shop.Add(3509);
			shop.Add(3506);
			shop.Add(8);
			shop.Add(28);
			shop.Add(110);
			shop.Add(40);
			shop.Add(42);
			shop.Add(965);
			shop.Add(967, ChestLoot.Condition.InSnowBiome);
			shop.Add(33, ChestLoot.Condition.InJungleBiome);
			shop.Add(4074, ChestLoot.Condition.TimeDay, ChestLoot.Condition.HappyWindyDay);
			shop.Add(279, ChestLoot.Condition.BloodMoon);
			shop.Add(282, ChestLoot.Condition.TimeNight);
			shop.Add(346, ChestLoot.Condition.DownedSkeletron);
			shop.Add(488, ChestLoot.Condition.Hardmode);
			shop.Add(931, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(930)));
			shop.Add(1614, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(930)));
			shop.Add(1786);
			shop.Add(1348, ChestLoot.Condition.Hardmode);
			shop.Add(3198, ChestLoot.Condition.Hardmode);
			shop.Add(4063, new ChestLoot.Condition(NetworkText.Empty, () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode));
			shop.Add(4673, new ChestLoot.Condition(NetworkText.Empty, () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode));
			shop.Add(3108, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(3107)));
			RegisterNpcShop(1, shop);
		}

		private void RegisterArmsDealer() {
			ChestLoot shop = new();
			shop.Add(97);
			shop.Add(4915, new ChestLoot.Condition(NetworkText.Empty, () => Main.bloodMoon || Main.hardMode), new ChestLoot.Condition(NetworkText.Empty, () => WorldGen.SavedOreTiers.Silver == 168));
			shop.Add(278, new ChestLoot.Condition(NetworkText.Empty, () => Main.bloodMoon || Main.hardMode), new ChestLoot.Condition(NetworkText.Empty, () => WorldGen.SavedOreTiers.Silver != 168));
			shop.Add(47, new ChestLoot.Condition(NetworkText.Empty, () => (NPC.downedBoss2 || !Main.dayTime) || Main.hardMode)); // TODO: Or(ICondition) and And(ICondition) method extensions?
			shop.Add(95);
			shop.Add(98);
			shop.Add(4701, ChestLoot.Condition.InGraveyard, ChestLoot.Condition.DownedSkeletron);
			shop.Add(324, ChestLoot.Condition.TimeNight);
			shop.Add(524, ChestLoot.Condition.Hardmode);
			shop.Add(1432, ChestLoot.Condition.Hardmode);
			shop.Add(1261, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1258)));
			shop.Add(1836, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1835)));
			shop.Add(3108, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(3107)));
			shop.Add(1783, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1782)));
			shop.Add(1785, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1784)));
			shop.Add(1736, ChestLoot.Condition.Halloween);
			shop.Add(1737, ChestLoot.Condition.Halloween);
			shop.Add(1738, ChestLoot.Condition.Halloween);
			RegisterNpcShop(2, shop);
		}

		private void RegisterDryad() {
			ChestLoot shop = new();
			shop.Add(2886, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CrimsonWorld);
			shop.Add(2171, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CrimsonWorld);
			shop.Add(4508, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CrimsonWorld);
			shop.Add(67, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CorruptionWorld);
			shop.Add(59, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CorruptionWorld);
			shop.Add(4504, ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CorruptionWorld);
			shop.Add(66, ChestLoot.Condition.NotBloodMoon);
			shop.Add(62, ChestLoot.Condition.NotBloodMoon);
			shop.Add(63, ChestLoot.Condition.NotBloodMoon);
			shop.Add(745, ChestLoot.Condition.NotBloodMoon);
			shop.Add(59, ChestLoot.Condition.Hardmode, ChestLoot.Condition.InGraveyard, ChestLoot.Condition.CrimsonWorld);
			shop.Add(2171, ChestLoot.Condition.Hardmode, ChestLoot.Condition.InGraveyard, ChestLoot.Condition.CorruptionWorld);
			shop.Add(27);
			shop.Add(114);
			shop.Add(1828);
			shop.Add(747);
			shop.Add(746, ChestLoot.Condition.Hardmode);
			shop.Add(369, ChestLoot.Condition.Hardmode);
			shop.Add(4505, ChestLoot.Condition.Hardmode);
			shop.Add(194, ChestLoot.Condition.InGlowshroomBiome);
			shop.Add(1853, ChestLoot.Condition.Halloween);
			shop.Add(1854, ChestLoot.Condition.Halloween);
			shop.Add(3215, ChestLoot.Condition.DownedKingSlime);
			shop.Add(3216, ChestLoot.Condition.DownedQueenBee);
			shop.Add(3219, ChestLoot.Condition.DownedEyeOfCthulhu);
			shop.Add(3218, ChestLoot.Condition.DownedBrainOfCthulhu);
			shop.Add(3217, ChestLoot.Condition.DownedEaterOfWorlds);
			shop.Add(3220, ChestLoot.Condition.DownedSkeletron);
			shop.Add(3221, ChestLoot.Condition.DownedSkeletron);
			shop.Add(4047);
			shop.Add(4045);
			shop.Add(4044);
			shop.Add(4043);
			shop.Add(4042);
			shop.Add(4046);
			shop.Add(4041);
			shop.Add(4241);
			shop.Add(4048);
			for (int i = 0; i < 3; i++) {
				shop.Add(4430 + i, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 0));
			}
			for (int i = 0; i < 3; i++) {
				shop.Add(4433 + i, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 1));
			}
			for (int i = 0; i < 3; i++) {
				shop.Add(4436 + i, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 2));
			}
			for (int i = 0; i < 3; i++) {
				shop.Add(4439 + i, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 3));
			}
			RegisterNpcShop(3, shop);
		}

		private void RegisterBombGuy() {
			ChestLoot shop = new();
			shop.Add(168);
			shop.Add(166);
			shop.Add(167);
			shop.Add(265, ChestLoot.Condition.Hardmode);
			shop.Add(937, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedPlantera, ChestLoot.Condition.DownedPirates);
			shop.Add(1347, ChestLoot.Condition.Hardmode);
			shop.Add(4827, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(4827)));
			shop.Add(4824, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(4824)));
			shop.Add(4825, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(4825)));
			shop.Add(4826, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(4826)));
			RegisterNpcShop(4, shop);
		}

		private void RegisterClothier() {
			ChestLoot shop = new();
			shop.Add(254);
			shop.Add(981);
			shop.Add(242, ChestLoot.Condition.TimeDay);
			shop.Add(245, ChestLoot.Condition.IsMoonFull);
			shop.Add(246, ChestLoot.Condition.IsMoonFull);
			shop.Add(1288, ChestLoot.Condition.IsMoonFull, ChestLoot.Condition.TimeNight);
			shop.Add(1289, ChestLoot.Condition.IsMoonFull, ChestLoot.Condition.TimeNight);
			shop.Add(325, ChestLoot.Condition.IsMoonWaningGibbous);
			shop.Add(326, ChestLoot.Condition.IsMoonWaningGibbous);
			shop.Add(269);
			shop.Add(270);
			shop.Add(271);
			for (int i = 0; i < 3; i++) {
				shop.Add(503 + i, ChestLoot.Condition.DownedClown);
			}
			shop.Add(322, ChestLoot.Condition.BloodMoon);
			shop.Add(3362, ChestLoot.Condition.BloodMoon);
			shop.Add(3363, ChestLoot.Condition.BloodMoon);
			for (int i = 0; i < 2; i++) {
				shop.Add(2856 + i * 2, ChestLoot.Condition.DownedCultist, ChestLoot.Condition.TimeDay);
			}
			for (int i = 0; i < 2; i++) {
				shop.Add(2857 + i * 2, ChestLoot.Condition.DownedCultist, ChestLoot.Condition.TimeNight);
			}
			for (int i = 0; i < 3; i++) {
				shop.Add(3242 + i, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(411)));
			}
			for (int i = 0; i < 2; i++) {
				shop.Add(4685 + i, ChestLoot.Condition.InGraveyard);
			}
			for (int i = 0; i < 6; i++) {
				shop.Add(4704 + i, ChestLoot.Condition.InGraveyard);
			}
			shop.Add(1429, ChestLoot.Condition.InSnowBiome);
			shop.Add(1740, ChestLoot.Condition.Halloween);
			shop.Add(869, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonThirdQuarter);
			shop.Add(4994, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaningCrescent);
			shop.Add(4997, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaningCrescent);
			shop.Add(864, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonNew);
			shop.Add(865, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonNew);
			shop.Add(4995, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingCrescent);
			shop.Add(4998, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingCrescent);
			shop.Add(873, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter);
			shop.Add(874, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter);
			shop.Add(875, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter);
			shop.Add(4996, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingGibbous);
			shop.Add(4999, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingGibbous);
			shop.Add(1275, ChestLoot.Condition.DownedFrost);
			shop.Add(1276, ChestLoot.Condition.DownedFrost);
			shop.Add(3246, ChestLoot.Condition.Halloween);
			shop.Add(3247, ChestLoot.Condition.Halloween);
			shop.Add(3730, ChestLoot.Condition.BirthdayPartyIsUp);
			shop.Add(3731, ChestLoot.Condition.BirthdayPartyIsUp);
			shop.Add(3733, ChestLoot.Condition.BirthdayPartyIsUp);
			shop.Add(3734, ChestLoot.Condition.BirthdayPartyIsUp);
			shop.Add(3735, ChestLoot.Condition.BirthdayPartyIsUp);
			shop.Add(4744, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000));
			RegisterNpcShop(5, shop);
		}

		private void RegisterGoblin() {
			ChestLoot shop = new();
			shop.Add(128);
			shop.Add(286);
			shop.Add(398);
			shop.Add(84);
			shop.Add(407);
			shop.Add(161);
			RegisterNpcShop(6, shop);
		}

		private void RegisterWizard() {
			ChestLoot shop = new();
			shop.Add(487);
			shop.Add(496);
			shop.Add(500);
			shop.Add(507);
			shop.Add(508);
			shop.Add(531);
			shop.Add(149);
			shop.Add(576);
			shop.Add(3186);
			shop.Add(1739, ChestLoot.Condition.Halloween);
			RegisterNpcShop(7, shop);
		}

		private void RegisterMechanic() {
			ChestLoot shop = new();
			shop.Add(509);
			shop.Add(850);
			shop.Add(851);
			shop.Add(3612);
			shop.Add(510);
			shop.Add(530);
			shop.Add(513);
			shop.Add(538);
			shop.Add(529);
			shop.Add(541);
			shop.Add(542);
			shop.Add(543);
			shop.Add(852);
			shop.Add(853);
			shop.Add(4261);
			shop.Add(3707);
			shop.Add(2739);
			shop.Add(849);
			shop.Add(3616);
			shop.Add(2799);
			shop.Add(3619);
			shop.Add(3627);
			shop.Add(3629);
			shop.Add(585);
			shop.Add(584);
			shop.Add(583);
			shop.Add(4484);
			shop.Add(4485);
			shop.Add(2295, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(369)), new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase % 2 == 0));
			RegisterNpcShop(8, shop);
		}

		private void RegisterSantaClaus() {
			ChestLoot shop = new();
			shop.Add(588);
			shop.Add(589);
			shop.Add(590);
			shop.Add(597);
			shop.Add(598);
			shop.Add(596);
			for (int i = 1873; i < 1906; i++) {
				shop.Add(i);
			}
			RegisterNpcShop(9, shop);
		}

		private void RegisterTruffle() {
			ChestLoot shop = new();
			shop.Add(756, ChestLoot.Condition.DownedMechBossAny);
			shop.Add(787, ChestLoot.Condition.DownedMechBossAny);
			shop.Add(868);
			shop.Add(1551, ChestLoot.Condition.DownedPlantera);
			shop.Add(1181);
			shop.Add(783);
			RegisterNpcShop(10, shop);
		}

		private void RegisterSteampunker() {
			ChestLoot shop = new();
			shop.Add(779);
			shop.Add(748, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase >= 4));
			for (int i = 0; i < 3; i++) {
				shop.Add(839 + i, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase < 4));
			}
			shop.Add(948, ChestLoot.Condition.DownedGolem);
			shop.Add(3623);
			shop.Add(3603);
			shop.Add(3604);
			shop.Add(3607);
			shop.Add(3605);
			shop.Add(3606);
			shop.Add(3608);
			shop.Add(3618);
			shop.Add(3602);
			shop.Add(3663);
			shop.Add(3609);
			shop.Add(3610);
			shop.Add(995);
			shop.Add(2203, ChestLoot.Condition.DownedEyeOfCthulhu, ChestLoot.Condition.DownedEowOrBoc, ChestLoot.Condition.DownedSkeletron);
			shop.Add(2193, ChestLoot.Condition.CrimsonWorld);
			shop.Add(4142, ChestLoot.Condition.CorruptionWorld);
			shop.Add(2192, ChestLoot.Condition.InGraveyard);
			shop.Add(2204, ChestLoot.Condition.InJungleBiome);
			shop.Add(2198, ChestLoot.Condition.InSnowBiome);
			shop.Add(2197, new ChestLoot.Condition(NetworkText.Empty, () => (Main.LocalPlayer.position.Y / 16.0) < Main.worldSurface * 0.3499999940395355));
			shop.Add(2196, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(832)));
			shop.Add(1263);

			ChestLoot.Entry entry = new(new ChestLoot.Condition(NetworkText.Empty, () => Main.eclipse || Main.bloodMoon));
			entry.OnSuccess(784, ChestLoot.Condition.CrimsonWorld);
			entry.OnSuccess(782, ChestLoot.Condition.CorruptionWorld);
			entry.OnFail(781, ChestLoot.Condition.InHallowBiome);
			entry.OnFail(780, ChestLoot.Condition.InHallowBiome);
			shop.Add(entry);

			shop.Add(1344, ChestLoot.Condition.Hardmode);
			shop.Add(4472, ChestLoot.Condition.Hardmode);
			shop.Add(1742, ChestLoot.Condition.Halloween);
			RegisterNpcShop(11, shop);
		}

		private void RegisterDyeTrader() {
			ChestLoot shop = new();
			shop.Add(1037);
			shop.Add(2874);
			shop.Add(1120);
			shop.Add(1969, new ChestLoot.Condition(NetworkText.Empty, () => Main.netMode == NetmodeID.MultiplayerClient));
			shop.Add(3248, ChestLoot.Condition.Halloween);
			shop.Add(1741, ChestLoot.Condition.Halloween);
			shop.Add(2871, ChestLoot.Condition.IsMoonFull);
			shop.Add(2872, ChestLoot.Condition.IsMoonFull);
			shop.Add(4663, ChestLoot.Condition.BloodMoon);
			shop.Add(4662, ChestLoot.Condition.InGraveyard);
			RegisterNpcShop(12, shop);
		}

		private void RegisterPartyGirl() {
			ChestLoot shop = new();
			shop.Add(859);
			shop.Add(4743, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			shop.Add(1000);
			shop.Add(1168);
			shop.Add(1449, ChestLoot.Condition.TimeDay);
			shop.Add(4552, ChestLoot.Condition.TimeNight);
			shop.Add(1345);
			shop.Add(1450);
			shop.Add(3253);
			shop.Add(4553);
			shop.Add(2700);
			shop.Add(2738);
			shop.Add(4470);
			shop.Add(4681);
			shop.Add(4682, ChestLoot.Condition.InGraveyard);
			shop.Add(4702, ChestLoot.Condition.NightLanternsUp);
			shop.Add(3548, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(3548)));
			shop.Add(3369, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(229)));
			shop.Add(3369, ChestLoot.Condition.DownedGolem);
			shop.Add(3214, ChestLoot.Condition.Hardmode);
			shop.Add(2868, ChestLoot.Condition.Hardmode);
			for (int i = 0; i < 4; i++)
			{
				shop.Add(970 + i, ChestLoot.Condition.Hardmode);
			}
			shop.Add(4791);
			shop.Add(3747);
			shop.Add(3732);
			shop.Add(3742);

			ChestLoot.Entry entry = new(ChestLoot.Condition.BirthdayPartyIsUp);
			entry.OnSuccess(3749);
			entry.OnSuccess(3746);
			entry.OnSuccess(3739);
			entry.OnSuccess(3740);
			entry.OnSuccess(3741);
			entry.OnSuccess(3737);
			entry.OnSuccess(3738);
			entry.OnSuccess(3736);
			entry.OnSuccess(3745);
			entry.OnSuccess(3744);
			entry.OnSuccess(3743);
			shop.Add(entry);
			RegisterNpcShop(13, shop);
		}

		private void RegisterCyborg() {
			ChestLoot shop = new();
			shop.Add(771);
			shop.Add(772, ChestLoot.Condition.BloodMoon);
			shop.Add(773, new ChestLoot.Condition(NetworkText.Empty, () => !Main.dayTime || Main.eclipse));
			shop.Add(774, ChestLoot.Condition.Eclipse);
			shop.Add(4445, ChestLoot.Condition.DownedMartians);
			shop.Add(4446, ChestLoot.Condition.DownedMartians, new ChestLoot.Condition(NetworkText.Empty, () => Main.bloodMoon || Main.eclipse));
			shop.Add(4459, ChestLoot.Condition.Hardmode);
			shop.Add(760, ChestLoot.Condition.Hardmode);
			shop.Add(1346, ChestLoot.Condition.Hardmode);
			shop.Add(4409, ChestLoot.Condition.InGraveyard);
			shop.Add(4392, ChestLoot.Condition.InGraveyard);
			for (int i = 0; i < 3; i++) {
				shop.Add(1743 + i, ChestLoot.Condition.Halloween);
			}
			shop.Add(2862, ChestLoot.Condition.DownedMartians);
			shop.Add(3664, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(3384) || Main.LocalPlayer.HasItem(3664)));
			RegisterNpcShop(14, shop);
		}

		private void RegisterPainter() {
			ChestLoot shop = new();
			shop.Add(1071);
			shop.Add(1072);
			shop.Add(1100);
			for (int i = 1073; i <= 1083; i++) {
				shop.Add(i);
			}
			shop.Add(1097);
			shop.Add(1098);
			shop.Add(1966);
			shop.Add(4668, ChestLoot.Condition.InGraveyard);
			shop.Add(1967, ChestLoot.Condition.Hardmode);
			shop.Add(1968, ChestLoot.Condition.Hardmode);
			ChestLoot.Entry entry = new(ChestLoot.Condition.NotInGraveyard);
			entry.OnSuccess(1490);
			entry.OnSuccess(1481, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 0));
			entry.OnSuccess(1482, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 1));
			entry.OnSuccess(1483, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 2));
			entry.OnSuccess(1484, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 3));
			shop.Add(entry);
			shop.Add(1492, ChestLoot.Condition.InCrimsonBiome);
			shop.Add(1488, ChestLoot.Condition.InCorruptBiome);
			shop.Add(1489, ChestLoot.Condition.InHallowBiome);
			shop.Add(1486, ChestLoot.Condition.InJungleBiome);
			shop.Add(1487, ChestLoot.Condition.InSnowBiome);
			shop.Add(1491, ChestLoot.Condition.InDesertBiome);
			shop.Add(1493, ChestLoot.Condition.BloodMoon);
			entry = new(ChestLoot.Condition.NotInGraveyard, new ChestLoot.Condition(NetworkText.Empty, () => (Main.player[Main.myPlayer].position.Y / 16.0) < Main.worldSurface * 0.3499999940395355));
			entry.OnSuccess(1485);
			entry.OnSuccess(1494, ChestLoot.Condition.Hardmode);
			shop.Add(entry);
			for (int i = 0; i < 7; i++) {
				shop.Add(4723 + i, ChestLoot.Condition.InGraveyard);
			}
			for (int i = 1948; i <= 1957; i++) {
				shop.Add(i, ChestLoot.Condition.Christmas);
			}
			for (int i = 2158; i <= 2160; i++) {
				shop.Add(i);
			}
			for (int i = 2008; i <= 2014; i++) {
				shop.Add(i);
			}
			RegisterNpcShop(15, shop);
		}

		private void RegisterWitchDoctor() {
			ChestLoot shop = new();
			shop.Add(1430);
			shop.Add(986);
			shop.Add(2999, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(108)));
			shop.Add(1158, ChestLoot.Condition.TimeNight);
			ChestLoot.Entry entry = new(ChestLoot.Condition.Hardmode);
			entry.OnSuccess(1159, ChestLoot.Condition.DownedPlantera);
			entry.OnSuccess(1160, ChestLoot.Condition.DownedPlantera);
			entry.OnSuccess(1161, ChestLoot.Condition.DownedPlantera);
			entry.OnSuccess(1167, ChestLoot.Condition.DownedPlantera, ChestLoot.Condition.InJungleBiome);
			entry.OnSuccess(1339, ChestLoot.Condition.DownedPlantera);
			entry.OnSuccess(1171, ChestLoot.Condition.InJungleBiome);
			entry.OnSuccess(1162, ChestLoot.Condition.InJungleBiome, ChestLoot.Condition.TimeNight);
			shop.Add(entry);
			shop.Add(909);
			shop.Add(910);
			for (int i = 0; i < 6; i++) {
				shop.Add(940 + i);
			}
			shop.Add(4922);
			shop.Add(4417);
			shop.Add(1836, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1835)));
			shop.Add(1261, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.HasItem(1258)));
			shop.Add(1791, ChestLoot.Condition.Halloween);
			RegisterNpcShop(16, shop);
		}

		private void RegisterPirate() {
			ChestLoot shop = new();
			shop.Add(928);
			shop.Add(929);
			shop.Add(876);
			shop.Add(877);
			shop.Add(878);
			shop.Add(2434);
			shop.Add(1180, new ChestLoot.Condition(NetworkText.Empty, () => {
				int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
				return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
			}));
			shop.Add(1337, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMechBossAny, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(208)));
			RegisterNpcShop(17, shop);
		}

		private void RegisterStylist() {
			ChestLoot shop = new();
			shop.Add(1990);
			shop.Add(1979);
			shop.Add(1977, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax));
			shop.Add(1978, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax));
			shop.Add(1980, new ChestLoot.Condition(NetworkText.Empty, () => {
				long num9 = 0L;
				for (int num10 = 0; num10 < Main.InventoryAmmoSlotsStart; num10++)
				{
					if (Main.player[Main.myPlayer].inventory[num10].type == ItemID.CopperCoin)
						num9 += Main.player[Main.myPlayer].inventory[num10].stack;
					else if (Main.player[Main.myPlayer].inventory[num10].type == ItemID.SilverCoin)
						num9 += Main.player[Main.myPlayer].inventory[num10].stack * 100;
					else if (Main.player[Main.myPlayer].inventory[num10].type == ItemID.GoldCoin)
						num9 += Main.player[Main.myPlayer].inventory[num10].stack * 10000;
					else if (Main.player[Main.myPlayer].inventory[num10].type == ItemID.PlatinumCoin)
						num9 += Main.player[Main.myPlayer].inventory[num10].stack * 1000000;
				}
				return num9 >= 1000000;
			}));
			shop.Add(1981, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase % 2 == (!Main.dayTime).ToInt())); // (Main.moonPhase % 2 == 0 && Main.dayTime) || (Main.moonPhase % 2 == 1 && !Main.dayTime)
			shop.Add(1982, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.team != 0));
			shop.Add(1983, ChestLoot.Condition.Hardmode);
			shop.Add(1984, new ChestLoot.Condition(NetworkText.Empty, () => NPC.AnyNPCs(208)));
			shop.Add(1985, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedDestroyer, ChestLoot.Condition.DownedTwins, ChestLoot.Condition.DownedSkeletronPrime);
			shop.Add(1986, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMechBossAny);
			shop.Add(2863, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMartians);
			shop.Add(3259, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMartians);
			shop.Add(5104);
			RegisterNpcShop(18, shop);
		}

		private void RegisterSkeletonMerchant() {
			ChestLoot shop = new();
			shop.Add(3001, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 2) == 0));
			shop.Add(28, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 2) == 0));
			shop.Add(3002, new ChestLoot.Condition(NetworkText.Empty, () => !Main.dayTime || (Main.moonPhase % 2) == 0));
			shop.Add(282, new ChestLoot.Condition(NetworkText.Empty, () => Main.dayTime && Main.moonPhase != 0));
			shop.Add(3004, new ChestLoot.Condition(NetworkText.Empty, () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0));
			shop.Add(8, new ChestLoot.Condition(NetworkText.Empty, () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0));
			shop.Add(3003, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase <= 1 || Main.moonPhase >= 4));
			shop.Add(40, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase > 1 && Main.moonPhase < 4));
			shop.Add(3310, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 4) == 0));
			shop.Add(3313, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 4) == 1));
			shop.Add(3312, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 4) == 2));
			shop.Add(3311, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase % 4) == 3));
			shop.Add(166);
			shop.Add(965);
			shop.Add(3316, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase < 4));
			shop.Add(3315, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => Main.moonPhase >= 4));
			shop.Add(3314, ChestLoot.Condition.Hardmode);
			shop.Add(3258, ChestLoot.Condition.Hardmode, ChestLoot.Condition.BloodMoon);
			shop.Add(3034, ChestLoot.Condition.IsMoonFull, ChestLoot.Condition.TimeNight);
			RegisterNpcShop(20, shop);
		}

		private void RegisterBartender() {
			const int mechBoss = 2;
			const int golem = 4;

			int[][] items =
			{
				new int[] {				3800, 3801, 3802, 3871, 3872, 3873 },
				new int[] { 3818, 3824, 3832, 3829, 3797, 3798, 3799, 3874, 3875, 3876 },
				new int[] { 3819, 3825, 3833, 3830, 3803, 3804, 3805, 3877, 3878, 3879 },
				new int[] { 3820, 3826, 3834, 3831, 3806, 3807, 3808, 3880, 3881, 3882 },
			};
			int[][] conditions =
			{
				new int[] {				1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | golem, 1 | golem, 1 | golem },
				new int[] { 1, 1, 1, 1, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | golem, 1 | golem, 1 | golem },
				new int[] { 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss, 1 | golem, 1 | golem, 1 | golem },
				new int[] { 1 | golem, 1 | golem, 1 | golem, 1 | golem, 1, 1 | mechBoss, 1 | mechBoss, 1 | mechBoss | golem, 1 | golem, 1 | golem },
			};
			int[][] prices =
			{
				new int[] {				25, 25, 25, 25, 25, 25 },
				new int[] { 5, 5, 5, 5, 25, 25, 25, 25, 25, 25 },
				new int[] { 25, 25, 25, 25, 25, 25, 25, 25, 25, 25 },
				new int[] { 100, 100, 100, 100, 25, 25, 25, 25, 25, 25 },
			};

			ChestLoot shop = new();
			ChestLoot.Condition[] mechCond = { ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMechBossAny };
			ChestLoot.Condition[] golemCond = { ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedGolem };

			shop.Add(353);

			ChestLoot.Entry entry = new(ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => NPC.downedGolemBoss || NPC.downedMechBossAny));
			entry.OnSuccess(3828, ChestLoot.Condition.DownedGolem);
			entry.OnSuccess(3828, ChestLoot.Condition.DownedMechBossAny);
			entry.OnFail(3828);

			entry.ChainedEntries[true][0].item.shopCustomPrice = Item.buyPrice(gold: 4);
			entry.ChainedEntries[true][1].item.shopCustomPrice = Item.buyPrice(gold: 1);
			entry.ChainedEntries[false][0].item.shopCustomPrice = Item.buyPrice(silver: 25);

			shop.Add(entry);
			shop.Add(3816);
			shop.Add(3813);
			shop[^1].item.shopCustomPrice = 75;
			shop[^1].item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;

			for (int i = 0; i < items.GetLength(0); i++) {
				for (int j = 0; j < items.GetLength(1); i++) {
					int condType = conditions[i][j];
					if ((condType & golem) > 0) {
						shop.Add(items[i][j], golemCond);
						shop[^1].item.shopCustomPrice = prices[i][j];
						shop[^1].item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
					}
					else if ((condType & mechBoss) > 0) {
						shop.Add(items[i][j], mechCond);
						shop[^1].item.shopCustomPrice = prices[i][j];
						shop[^1].item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
					}
					else {
						shop.Add(0);
					}
				}
			}
			RegisterNpcShop(21, shop);
		}

		private void RegisterGolfer() {
			ChestLoot chestLoot = new();
			chestLoot.Add(4587);
			chestLoot.Add(4590);
			chestLoot.Add(4589);
			chestLoot.Add(4588);
			chestLoot.Add(4083);
			chestLoot.Add(4084);
			chestLoot.Add(4085);
			chestLoot.Add(4086);
			chestLoot.Add(4087);
			chestLoot.Add(4088);
			chestLoot.Add(4039, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4094, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4093, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4092, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4089);
			chestLoot.Add(3989);
			chestLoot.Add(4095);
			chestLoot.Add(4040);
			chestLoot.Add(4319);
			chestLoot.Add(4320);
			chestLoot.Add(4591, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 1000));
			chestLoot.Add(4594, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 1000));
			chestLoot.Add(4593, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 1000));
			chestLoot.Add(4592, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 1000));
			chestLoot.Add(4135);
			chestLoot.Add(4138);
			chestLoot.Add(4136);
			chestLoot.Add(4137);
			chestLoot.Add(4049);
			chestLoot.Add(4265, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4598, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 2000));
			chestLoot.Add(4597, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 2000));
			chestLoot.Add(4596, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 2000));
			chestLoot.Add(4264, ChestLoot.Condition.DownedSkeletron, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 2000));
			chestLoot.Add(4599, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated > 500));
			chestLoot.Add(4600, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 1000));
			chestLoot.Add(4601, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000));
			chestLoot.Add(4658, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 0));
			chestLoot.Add(4659, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 1));
			chestLoot.Add(4660, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 2));
			chestLoot.Add(4661, new ChestLoot.Condition(NetworkText.Empty, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 3));
			RegisterNpcShop(22, chestLoot);
		}

		private void RegisterZoologist() {
			ChestLoot chestLoot = new();
			chestLoot.Add(4776, new ChestLoot.Condition(NetworkText.Empty, Chest.BestiaryGirl_IsFairyTorchAvailable));
			chestLoot.Add(4767);
			chestLoot.Add(4759);
			chestLoot.Add(4672, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f));
			chestLoot.Add(4829, new ChestLoot.Condition(NetworkText.Empty, () => !NPC.boughtCat));
			chestLoot.Add(4830, new ChestLoot.Condition(NetworkText.Empty, () => !NPC.boughtDog && Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f));
			chestLoot.Add(4910, new ChestLoot.Condition(NetworkText.Empty, () => !NPC.boughtBunny && Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f));
			chestLoot.Add(4871, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4907, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f));
			chestLoot.Add(4677, new ChestLoot.Condition(NetworkText.Empty, () => NPC.downedTowerSolar));
			chestLoot.Add(4676, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f));
			chestLoot.Add(4762, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4716, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f));
			chestLoot.Add(4785, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4786, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4787, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4788, ChestLoot.Condition.Hardmode, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f));
			chestLoot.Add(4955, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f));
			chestLoot.Add(4736, ChestLoot.Condition.Hardmode, ChestLoot.Condition.BloodMoon);
			chestLoot.Add(4701, ChestLoot.Condition.DownedPlantera);
			chestLoot.Add(4765, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f));
			chestLoot.Add(4766, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f));
			chestLoot.Add(4777, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f));
			chestLoot.Add(4763, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.6f));
			chestLoot.Add(4735, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f));
			chestLoot.Add(4951, new ChestLoot.Condition(NetworkText.Empty, () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f));
			chestLoot.Add(4768, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 0));
			chestLoot.Add(4769, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 0));
			chestLoot.Add(4770, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 1));
			chestLoot.Add(4771, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 1));
			chestLoot.Add(4772, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 2));
			chestLoot.Add(4773, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 2));
			chestLoot.Add(4560, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 3));
			chestLoot.Add(4775, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 4));
			RegisterNpcShop(23, chestLoot);
		}

		private void RegisterPrincess() {
			ChestLoot shop = new();
			shop.Add(5071);
			shop.Add(5073);
			shop.Add(5073);
			for (int i = 5076; i < 5087; i++)
			{
				shop.Add(i);
			}
			shop.Add(5044, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMoonLord);
			shop.Add(1309, ChestLoot.Condition.TenthAnniversary);
			shop.Add(1859, ChestLoot.Condition.TenthAnniversary);
			shop.Add(1358, ChestLoot.Condition.TenthAnniversary);
			shop.Add(857, ChestLoot.Condition.TenthAnniversary, ChestLoot.Condition.InDesertBiome);
			shop.Add(4144, ChestLoot.Condition.TenthAnniversary, ChestLoot.Condition.BloodMoon);
			ChestLoot.Entry entry = new(ChestLoot.Condition.TenthAnniversary, ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedPirates);
			entry.OnSuccess(2584, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 0));
			entry.OnSuccess(854, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 1));
			entry.OnSuccess(855, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 2));
			entry.OnSuccess(905, new ChestLoot.Condition(NetworkText.Empty, () => (Main.moonPhase / 2) == 3));
			shop.Add(entry);
			shop.Add(5088);
			RegisterNpcShop(24, shop);
		}
	}
}