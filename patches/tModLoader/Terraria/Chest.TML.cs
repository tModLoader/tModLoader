using System;
using System.Reflection;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Chest
	{
		private Item[] ReSetupShop(out int slots, int type) {
			ChestLoot chestLoot = new();
			switch (type) {
				case 1:
					chestLoot.Put(88);
					chestLoot.Put(87);
					chestLoot.Put(35);
					chestLoot.Put(1991);
					chestLoot.Put(3509);
					chestLoot.Put(3506);
					chestLoot.Put(8);
					chestLoot.Put(28);
					chestLoot.Put(110);
					chestLoot.Put(40);
					chestLoot.Put(42);
					chestLoot.Put(965);
					chestLoot.Put(967, () => Main.LocalPlayer.ZoneSnow);
					chestLoot.Put(33, () => Main.LocalPlayer.ZoneJungle);
					chestLoot.Put(4074, () => Main.dayTime && Main.IsItAHappyWindyDay);
					chestLoot.Put(279, () => Main.bloodMoon);
					chestLoot.Put(282, () => !Main.dayTime);
					chestLoot.Put(346, () => NPC.downedBoss3);
					chestLoot.Put(488, () => Main.hardMode);
					chestLoot.Put(931, () => Main.LocalPlayer.HasItem(930));
					chestLoot.Put(1614, () => Main.LocalPlayer.HasItem(930));
					chestLoot.Put(1786);
					chestLoot.Put(1348, () => Main.hardMode);
					chestLoot.Put(3198, () => Main.hardMode);
					chestLoot.Put(4063, () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
					chestLoot.Put(4673, () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
					chestLoot.Put(3108, () => Main.LocalPlayer.HasItem(3107));
					break;
				case 2:
					chestLoot.Put(97);
					chestLoot.Put(4915, () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == 168);
					chestLoot.Put(278, () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver != 168);
					chestLoot.Put(47, () => (NPC.downedBoss2 || !Main.dayTime) || Main.hardMode);
					chestLoot.Put(95);
					chestLoot.Put(98);
					chestLoot.Put(4703, () => Main.LocalPlayer.ZoneGraveyard && NPC.downedBoss3);
					chestLoot.Put(324, () => !Main.dayTime);
					chestLoot.Put(534, () => Main.hardMode);
					chestLoot.Put(1432, () => Main.hardMode);
					chestLoot.Put(1261, () => Main.LocalPlayer.HasItem(1258));
					chestLoot.Put(1836, () => Main.LocalPlayer.HasItem(1835));
					chestLoot.Put(3108, () => Main.LocalPlayer.HasItem(3107));
					chestLoot.Put(1783, () => Main.LocalPlayer.HasItem(1782));
					chestLoot.Put(1785, () => Main.LocalPlayer.HasItem(1784));
					chestLoot.Put(1736, () => Main.halloween);
					chestLoot.Put(1737, () => Main.halloween);
					chestLoot.Put(1738, () => Main.halloween);
					break;
				case 3:
					chestLoot.Put(2886, () => Main.bloodMoon && WorldGen.crimson);
					chestLoot.Put(2171, () => Main.bloodMoon && WorldGen.crimson);
					chestLoot.Put(4508, () => Main.bloodMoon && WorldGen.crimson);
					chestLoot.Put(67, () => Main.bloodMoon && !WorldGen.crimson);
					chestLoot.Put(59, () => Main.bloodMoon && !WorldGen.crimson);
					chestLoot.Put(4504, () => Main.bloodMoon && !WorldGen.crimson);
					chestLoot.Put(66, () => !Main.bloodMoon);
					chestLoot.Put(62, () => !Main.bloodMoon);
					chestLoot.Put(63, () => !Main.bloodMoon);
					chestLoot.Put(745, () => !Main.bloodMoon);
					chestLoot.Put(59, () => Main.hardMode && Main.LocalPlayer.ZoneGraveyard && WorldGen.crimson);
					chestLoot.Put(2171, () => Main.hardMode && Main.LocalPlayer.ZoneGraveyard && !WorldGen.crimson);
					chestLoot.Put(27);
					chestLoot.Put(114);
					chestLoot.Put(1828);
					chestLoot.Put(747);
					chestLoot.Put(746, () => Main.hardMode);
					chestLoot.Put(369, () => Main.hardMode);
					chestLoot.Put(4505, () => Main.hardMode);
					chestLoot.Put(194, () => Main.LocalPlayer.ZoneGlowshroom);
					chestLoot.Put(1853, () => Main.halloween);
					chestLoot.Put(1854, () => Main.halloween);
					chestLoot.Put(3215, () => NPC.downedSlimeKing);
					chestLoot.Put(3216, () => NPC.downedQueenBee);
					chestLoot.Put(3219, () => NPC.downedBoss1);
					chestLoot.Put(3218, () => NPC.downedBoss2 && WorldGen.crimson);
					chestLoot.Put(3217, () => NPC.downedBoss2 && !WorldGen.crimson);
					chestLoot.Put(3220, () => NPC.downedBoss3);
					chestLoot.Put(3221, () => NPC.downedBoss3);
					chestLoot.Put(3222, () => Main.hardMode);
					chestLoot.Put(4047);
					chestLoot.Put(4045);
					chestLoot.Put(4044);
					chestLoot.Put(4043);
					chestLoot.Put(4042);
					chestLoot.Put(4046);
					chestLoot.Put(4041);
					chestLoot.Put(4241);
					chestLoot.Put(4048);
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(4430 + i, () => Main.hardMode && (Main.moonPhase / 2) == 0);
					}
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(4433 + i, () => Main.hardMode && (Main.moonPhase / 2) == 1);
					}
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(4436 + i, () => Main.hardMode && (Main.moonPhase / 2) == 2);
					}
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(4439 + i, () => Main.hardMode && (Main.moonPhase / 2) == 3);
					}
					break;
				case 4:
					chestLoot.Put(168);
					chestLoot.Put(166);
					chestLoot.Put(167);
					chestLoot.Put(265, () => Main.hardMode);
					chestLoot.Put(937, () => Main.hardMode && NPC.downedPlantBoss && NPC.downedPirates);
					chestLoot.Put(1347, () => Main.hardMode);
					chestLoot.Put(4827, () => Main.LocalPlayer.HasItem(4827));
					chestLoot.Put(4824, () => Main.LocalPlayer.HasItem(4824));
					chestLoot.Put(4825, () => Main.LocalPlayer.HasItem(4825));
					chestLoot.Put(4826, () => Main.LocalPlayer.HasItem(4826));
					break;
				case 5:
					chestLoot.Put(254);
					chestLoot.Put(981);
					chestLoot.Put(242, () => Main.dayTime);
					chestLoot.Put(245, () => Main.moonPhase == 0);
					chestLoot.Put(246, () => Main.moonPhase == 0);
					chestLoot.Put(1288, () => Main.moonPhase == 0 && !Main.dayTime);
					chestLoot.Put(1289, () => Main.moonPhase == 0 && !Main.dayTime);
					chestLoot.Put(325, () => Main.moonPhase == 1);
					chestLoot.Put(326, () => Main.moonPhase == 1);
					chestLoot.Put(269);
					chestLoot.Put(270);
					chestLoot.Put(271);
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(503 + i, () => NPC.downedClown);
					}
					chestLoot.Put(322, () => Main.bloodMoon);
					chestLoot.Put(3362, () => Main.bloodMoon && !Main.dayTime); // confused screaming??
					chestLoot.Put(3363, () => Main.bloodMoon && !Main.dayTime);
					for (int i = 0; i < 2; i++) {
						chestLoot.Put(2856 + i * 2, () => NPC.downedAncientCultist && Main.dayTime);
					}
					for (int i = 0; i < 2; i++) {
						chestLoot.Put(2857 + i * 2, () => NPC.downedAncientCultist && !Main.dayTime);
					}
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(3242 + i, () => NPC.AnyNPCs(441));
					}
					for (int i = 0; i < 2; i++) {
						chestLoot.Put(4685 + i, () => Main.LocalPlayer.ZoneGraveyard);
					}
					for (int i = 0; i < 6; i++) {
						chestLoot.Put(4704 + i, () => Main.LocalPlayer.ZoneGraveyard);
					}
					chestLoot.Put(1429, () => Main.LocalPlayer.ZoneSnow);
					chestLoot.Put(1740, () => Main.halloween);
					chestLoot.Put(869, () => Main.hardMode && Main.moonPhase == 2);
					chestLoot.Put(4994, () => Main.hardMode && Main.moonPhase == 3);
					chestLoot.Put(4997, () => Main.hardMode && Main.moonPhase == 3);
					chestLoot.Put(864, () => Main.hardMode && Main.moonPhase == 4);
					chestLoot.Put(865, () => Main.hardMode && Main.moonPhase == 4);
					chestLoot.Put(4995, () => Main.hardMode && Main.moonPhase == 5);
					chestLoot.Put(4998, () => Main.hardMode && Main.moonPhase == 5);
					chestLoot.Put(873, () => Main.hardMode && Main.moonPhase == 6);
					chestLoot.Put(874, () => Main.hardMode && Main.moonPhase == 6);
					chestLoot.Put(875, () => Main.hardMode && Main.moonPhase == 6);
					chestLoot.Put(4996, () => Main.hardMode && Main.moonPhase == 7);
					chestLoot.Put(4999, () => Main.hardMode && Main.moonPhase == 7);
					chestLoot.Put(1275, () => NPC.downedFrost);
					chestLoot.Put(1276, () => NPC.downedFrost);
					chestLoot.Put(3246, () => Main.halloween);
					chestLoot.Put(3247, () => Main.halloween);
					chestLoot.Put(3730, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3731, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3733, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3734, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3735, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(4744, () => Main.LocalPlayer.golferScoreAccumulated >= 2000);
					break;
				case 6:
					chestLoot.Put(128);
					chestLoot.Put(486);
					chestLoot.Put(398);
					chestLoot.Put(84);
					chestLoot.Put(407);
					chestLoot.Put(161);
					break;
				case 7:
					chestLoot.Put(487);
					chestLoot.Put(496);
					chestLoot.Put(500);
					chestLoot.Put(507);
					chestLoot.Put(508);
					chestLoot.Put(531);
					chestLoot.Put(149);
					chestLoot.Put(576);
					chestLoot.Put(3186);
					chestLoot.Put(1739, () => Main.halloween);
					break;
				case 8:
					chestLoot.Put(509);
					chestLoot.Put(850);
					chestLoot.Put(851);
					chestLoot.Put(3612);
					chestLoot.Put(510);
					chestLoot.Put(530);
					chestLoot.Put(513);
					chestLoot.Put(538);
					chestLoot.Put(529);
					chestLoot.Put(541);
					chestLoot.Put(542);
					chestLoot.Put(543);
					chestLoot.Put(852);
					chestLoot.Put(853);
					chestLoot.Put(4261);
					chestLoot.Put(3707);
					chestLoot.Put(2739);
					chestLoot.Put(849);
					chestLoot.Put(3616);
					chestLoot.Put(2799);
					chestLoot.Put(3619);
					chestLoot.Put(3627);
					chestLoot.Put(3629);
					chestLoot.Put(585);
					chestLoot.Put(584);
					chestLoot.Put(583);
					chestLoot.Put(4484);
					chestLoot.Put(4485);
					chestLoot.Put(2295, () => NPC.AnyNPCs(369) && Main.moonPhase % 2 == 0);
					break;
				case 9:
					chestLoot.Put(588);
					chestLoot.Put(589);
					chestLoot.Put(590);
					chestLoot.Put(597);
					chestLoot.Put(598);
					chestLoot.Put(596);
					for (int i = 1873; i < 1906; i++) {
						chestLoot.Put(i);
					}
					break;
				case 10:
					chestLoot.Put(756, () => NPC.downedMechBossAny);
					chestLoot.Put(787, () => NPC.downedMechBossAny);
					chestLoot.Put(868);
					chestLoot.Put(1551, () => NPC.downedPlantBoss);
					chestLoot.Put(1181);
					chestLoot.Put(783);
					break;
				case 11:
					chestLoot.Put(779);
					chestLoot.Put(748, () => Main.moonPhase >= 4);
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(839 + i, () => Main.moonPhase < 4);
					}
					chestLoot.Put(948, () => NPC.downedGolemBoss);
					chestLoot.Put(3623);
					chestLoot.Put(3603);
					chestLoot.Put(3604);
					chestLoot.Put(3607);
					chestLoot.Put(3605);
					chestLoot.Put(3606);
					chestLoot.Put(3608);
					chestLoot.Put(3618);
					chestLoot.Put(3602);
					chestLoot.Put(3663);
					chestLoot.Put(3609);
					chestLoot.Put(3610);
					chestLoot.Put(995);
					chestLoot.Put(2203, () => NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3);
					chestLoot.Put(2193, () => WorldGen.crimson);
					chestLoot.Put(4142, () => !WorldGen.crimson);
					chestLoot.Put(2192, () => Main.LocalPlayer.ZoneGraveyard);
					chestLoot.Put(2204, () => Main.LocalPlayer.ZoneJungle);
					chestLoot.Put(2198, () => Main.LocalPlayer.ZoneSnow);
					chestLoot.Put(2197, () => (Main.LocalPlayer.position.Y / 16.0) < Main.worldSurface * 0.3499999940395355);
					chestLoot.Put(2196, () => Main.LocalPlayer.HasItem(832));
					chestLoot.Put(1263);
					chestLoot.Put(784, () => (Main.eclipse || Main.bloodMoon) && WorldGen.crimson);
					chestLoot.Put(782, () => (Main.eclipse || Main.bloodMoon) && !WorldGen.crimson);
					chestLoot.Put(781, () => !(Main.eclipse || Main.bloodMoon) && Main.LocalPlayer.ZoneHallow);
					chestLoot.Put(780, () => !((Main.eclipse || Main.bloodMoon) && Main.LocalPlayer.ZoneHallow));
					chestLoot.Put(1344, () => Main.hardMode);
					chestLoot.Put(4472, () => Main.hardMode);
					chestLoot.Put(1742, () => Main.halloween);
					break;
				case 12:
					chestLoot.Put(1037);
					chestLoot.Put(2874);
					chestLoot.Put(1120);
					chestLoot.Put(1969, () => Main.netMode == 1); // i wish item ids weren't just magic numbers here. Sadly, then it would be too high effort.
					chestLoot.Put(3248, () => Main.halloween);
					chestLoot.Put(1741, () => Main.halloween);
					chestLoot.Put(2871, () => Main.moonPhase == 0);
					chestLoot.Put(2872, () => Main.moonPhase == 0);
					chestLoot.Put(4663, () => !Main.dayTime && Main.bloodMoon);
					chestLoot.Put(4662, () => Main.LocalPlayer.ZoneGraveyard);
					break;
				case 13:
					chestLoot.Put(859);
					chestLoot.Put(4743, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(1000);
					chestLoot.Put(1168);
					chestLoot.Put(1449, () => Main.dayTime);
					chestLoot.Put(4552, () => !Main.dayTime);
					chestLoot.Put(1345);
					chestLoot.Put(1450);
					chestLoot.Put(3253);
					chestLoot.Put(4553);
					chestLoot.Put(2700);
					chestLoot.Put(2738);
					chestLoot.Put(4470);
					chestLoot.Put(4681);
					chestLoot.Put(4682, () => Main.LocalPlayer.ZoneGraveyard);
					chestLoot.Put(4702, () => LanternNight.LanternsUp);
					chestLoot.Put(3548, () => Main.LocalPlayer.HasItem(3548));
					chestLoot.Put(3369, () => NPC.AnyNPCs(229));
					chestLoot.Put(3546, () => NPC.downedGolemBoss);
					chestLoot.Put(3214, () => Main.hardMode);
					chestLoot.Put(2868, () => Main.hardMode);
					for (int i = 0; i < 4; i++) {
						chestLoot.Put(970 + i, () => Main.hardMode);
					}
					chestLoot.Put(4791);
					chestLoot.Put(3747);
					chestLoot.Put(3732);
					chestLoot.Put(3742);
					chestLoot.Put(3749, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3746, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3739, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3740, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3741, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3737, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3738, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3736, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3745, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3744, () => BirthdayParty.PartyIsUp);
					chestLoot.Put(3743, () => BirthdayParty.PartyIsUp);
					break;
				case 14:
					chestLoot.Put(771);
					chestLoot.Put(772, () => Main.bloodMoon);
					chestLoot.Put(773, () => !Main.dayTime || Main.eclipse);
					chestLoot.Put(774, () => Main.eclipse);
					chestLoot.Put(4445, () => NPC.downedMartians);
					chestLoot.Put(4446, () => NPC.downedMartians && (Main.bloodMoon || Main.eclipse));
					chestLoot.Put(4459, () => Main.hardMode);
					chestLoot.Put(760, () => Main.hardMode);
					chestLoot.Put(1346, () => Main.hardMode);
					chestLoot.Put(4409, () => Main.LocalPlayer.ZoneGraveyard);
					chestLoot.Put(4392, () => Main.LocalPlayer.ZoneGraveyard);
					for (int i = 0; i < 3; i++) {
						chestLoot.Put(1743 + i, () => Main.halloween);
					}
					chestLoot.Put(2862, () => NPC.downedMartians);
					chestLoot.Put(3664, () => Main.LocalPlayer.HasItem(3384) || Main.LocalPlayer.HasItem(3664));
					break;
				case 15:
					chestLoot.Put(1071);
					chestLoot.Put(1072);
					chestLoot.Put(1100);
					for (int i = 1073; i <= 1083; i++) {
						chestLoot.Put(i);
					}
					chestLoot.Put(1097);
					chestLoot.Put(1099);
					chestLoot.Put(1098);
					chestLoot.Put(1966);
					chestLoot.Put(4668, () => Main.LocalPlayer.ZoneGraveyard);
					chestLoot.Put(1967, () => Main.hardMode);
					chestLoot.Put(1968, () => Main.hardMode);
					chestLoot.Put(1490, () => !Main.LocalPlayer.ZoneGraveyard);
					chestLoot.Put(1481, () => !Main.LocalPlayer.ZoneGraveyard && (Main.moonPhase / 2) == 0);
					chestLoot.Put(1482, () => !Main.LocalPlayer.ZoneGraveyard && (Main.moonPhase / 2) == 1);
					chestLoot.Put(1483, () => !Main.LocalPlayer.ZoneGraveyard && (Main.moonPhase / 2) == 3);
					chestLoot.Put(1484, () => !Main.LocalPlayer.ZoneGraveyard && (Main.moonPhase / 2) == 4);
					chestLoot.Put(1492, () => Main.LocalPlayer.ZoneCrimson);
					chestLoot.Put(1488, () => Main.LocalPlayer.ZoneCorrupt);
					chestLoot.Put(1489, () => Main.LocalPlayer.ZoneHallow);
					chestLoot.Put(1486, () => Main.LocalPlayer.ZoneJungle);
					chestLoot.Put(1487, () => Main.LocalPlayer.ZoneSnow);
					chestLoot.Put(1491, () => Main.LocalPlayer.ZoneDesert);
					chestLoot.Put(1493, () => Main.bloodMoon);
					chestLoot.Put(1485, () => !Main.LocalPlayer.ZoneGraveyard && (Main.player[Main.myPlayer].position.Y / 16.0) < Main.worldSurface * 0.3499999940395355);
					chestLoot.Put(1494, () => !Main.LocalPlayer.ZoneGraveyard && (Main.player[Main.myPlayer].position.Y / 16.0) < Main.worldSurface * 0.3499999940395355 && Main.hardMode);
					for (int i = 0; i < 7; i++) {
						chestLoot.Put(4723 + i, () => Main.LocalPlayer.ZoneGraveyard);
					}
					for (int i = 1948; i <= 1957; i++) {
						chestLoot.Put(i, () => Main.xMas);
					}
					for (int i = 2158; i <= 2160; i++) {
						chestLoot.Put(i);
					}
					for (int i = 2008; i <= 2014; i++) {
						chestLoot.Put(i);
					}
					break;
				case 16:
					chestLoot.Put(1430);
					chestLoot.Put(986);
					chestLoot.Put(2999, () => NPC.AnyNPCs(108));
					chestLoot.Put(1158, () => !Main.dayTime);
					chestLoot.Put(1159, () => Main.hardMode && NPC.downedPlantBoss);
					chestLoot.Put(1160, () => Main.hardMode && NPC.downedPlantBoss);
					chestLoot.Put(1161, () => Main.hardMode && NPC.downedPlantBoss);
					chestLoot.Put(1167, () => Main.hardMode && NPC.downedPlantBoss && Main.LocalPlayer.ZoneJungle);
					chestLoot.Put(1339, () => Main.hardMode && NPC.downedPlantBoss);
					chestLoot.Put(1171, () => Main.hardMode && Main.LocalPlayer.ZoneJungle);
					chestLoot.Put(1162, () => Main.hardMode && Main.LocalPlayer.ZoneJungle && !Main.dayTime);
					chestLoot.Put(909);
					chestLoot.Put(910);
					for (int i = 0; i < 6; i++) {
						chestLoot.Put(940 + i);
					}
					chestLoot.Put(4922);
					chestLoot.Put(4417);
					chestLoot.Put(1836, () => Main.LocalPlayer.HasItem(1835));
					chestLoot.Put(1261, () => Main.LocalPlayer.HasItem(1258));
					chestLoot.Put(1791, () => Main.halloween);
					break;
				case 17:
					chestLoot.Put(928);
					chestLoot.Put(929);
					chestLoot.Put(876);
					chestLoot.Put(877);
					chestLoot.Put(878);
					chestLoot.Put(2434);
					chestLoot.Put(1180, () => {
						int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
						return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
					});
					chestLoot.Put(1337, () => Main.hardMode && NPC.downedMechBossAny && NPC.AnyNPCs(208));
					break;
				case 18:
					chestLoot.Put(1990);
					chestLoot.Put(1979);
					chestLoot.Put(1977, () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
					chestLoot.Put(1978, () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
					chestLoot.Put(1980, () => {
						long num9 = 0L;
						for (int num10 = 0; num10 < Main.InventoryAmmoSlotsStart; num10++) {
							if (Main.player[Main.myPlayer].inventory[num10].type == 71)
								num9 += Main.player[Main.myPlayer].inventory[num10].stack;
							else if (Main.player[Main.myPlayer].inventory[num10].type == 72)
								num9 += Main.player[Main.myPlayer].inventory[num10].stack * 100;
							else if (Main.player[Main.myPlayer].inventory[num10].type == 73)
								num9 += Main.player[Main.myPlayer].inventory[num10].stack * 10000;
							else if (Main.player[Main.myPlayer].inventory[num10].type == 74)
								num9 += Main.player[Main.myPlayer].inventory[num10].stack * 1000000;
						}
						return num9 >= 1000000;
					});
					chestLoot.Put(1981, () => Main.moonPhase % 2 == (!Main.dayTime).ToInt()); // (Main.moonPhase % 2 == 0 && Main.dayTime) || (Main.moonPhase % 2 == 1 && !Main.dayTime)
					chestLoot.Put(1982, () => Main.LocalPlayer.team != 0);
					chestLoot.Put(1983, () => Main.hardMode);
					chestLoot.Put(1984, () => NPC.AnyNPCs(208));
					chestLoot.Put(1985, () => Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3);
					chestLoot.Put(1986, () => Main.hardMode && NPC.downedMechBossAny);
					chestLoot.Put(2863, () => Main.hardMode && NPC.downedMartians);
					chestLoot.Put(3259, () => Main.hardMode && NPC.downedMartians);
					chestLoot.Put(5104);
					break;
				case 19:
					for (int num6 = 0; num6 < 40; num6++) {
						if (Main.travelShop[num6] != 0) {
							chestLoot.Put(Main.travelShop[num6]);
						}
					}
					break;
				case 20:
					chestLoot.Put(3001, () => Main.moonPhase % 2 == 0);
					chestLoot.Put(28, () => Main.moonPhase % 2 == 0);
					chestLoot.Put(3002, () => !Main.dayTime || Main.moonPhase == 0);
					chestLoot.Put(282, () => Main.dayTime && Main.moonPhase != 0);
					chestLoot.Put(3004, () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
					chestLoot.Put(8, () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0);
					chestLoot.Put(3003, () => Main.moonPhase <= 1 || Main.moonPhase >= 4);
					chestLoot.Put(40, () => Main.moonPhase > 1 && Main.moonPhase < 4);
					chestLoot.Put(3310, () => Main.moonPhase % 4 == 0);
					chestLoot.Put(3313, () => Main.moonPhase % 4 == 1);
					chestLoot.Put(3312, () => Main.moonPhase % 4 == 2);
					chestLoot.Put(3311, () => Main.moonPhase % 4 == 3);
					chestLoot.Put(166);
					chestLoot.Put(965);
					chestLoot.Put(3316, () => Main.hardMode && Main.moonPhase < 4);
					chestLoot.Put(3315, () => Main.hardMode && Main.moonPhase >= 4);
					chestLoot.Put(3314, () => Main.hardMode);
					chestLoot.Put(3258, () => Main.hardMode && Main.bloodMoon);
					chestLoot.Put(3034, () => Main.moonPhase == 0 && !Main.dayTime);
					break;
				case 21: // i hate this
					void set(int index, int id, int price, Func<bool> condition = null) {
						if (condition?.Invoke() ?? true) {
							chestLoot[index].item.SetDefaults(id);
							chestLoot[index].item.shopCustomPrice = price;
							chestLoot[index].item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
						}
					}

					chestLoot.Put(353);
					chestLoot.Put(3828);
					if (Main.hardMode && NPC.downedGolemBoss)
						chestLoot[^1].item.shopCustomPrice = Item.buyPrice(gold: 4);
					else if (Main.hardMode && NPC.downedMechBossAny)
						chestLoot[^1].item.shopCustomPrice = Item.buyPrice(gold: 1);
					else
						chestLoot[^1].item.shopCustomPrice = Item.buyPrice(silver: 25);
					chestLoot.Put(3816);
					chestLoot.Put(3813);
					chestLoot[^1].item.shopCustomPrice = 75;
					chestLoot[^1].item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
					for (int i = 0; i < 40; i++)
						chestLoot.Put(0);

					int num = 10;
					set(num++, 3818, 5);
					set(num++, 3824, 5);
					set(num++, 3832, 5);
					set(num++, 3829, 5);
					num = 20;
					set(num++, 3819, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3825, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3833, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3830, 25, () => Main.hardMode && NPC.downedMechBossAny);
					num = 30;
					set(num++, 3820, 100, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3826, 100, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3834, 100, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3831, 100, () => Main.hardMode && NPC.downedGolemBoss);

					num = 4;
					set(num++, 3800, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3801, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3802, 25, () => Main.hardMode && NPC.downedMechBossAny);

					num = 14;
					set(num++, 3797, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3798, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3799, 25, () => Main.hardMode && NPC.downedMechBossAny);

					num = 24;
					set(num++, 3803, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3804, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3805, 25, () => Main.hardMode && NPC.downedMechBossAny);

					num = 34;
					set(num++, 3806, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3807, 25, () => Main.hardMode && NPC.downedMechBossAny);
					set(num++, 3808, 25, () => Main.hardMode && NPC.downedMechBossAny);

					num = 7;
					set(num++, 3871, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3872, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3873, 25, () => Main.hardMode && NPC.downedGolemBoss);

					num = 17;
					set(num++, 3874, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3875, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3876, 25, () => Main.hardMode && NPC.downedGolemBoss);

					num = 27;
					set(num++, 3877, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3878, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3879, 25, () => Main.hardMode && NPC.downedGolemBoss);

					num = 37;
					set(num++, 3880, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3881, 25, () => Main.hardMode && NPC.downedGolemBoss);
					set(num++, 3882, 25, () => Main.hardMode && NPC.downedGolemBoss);
					break;
				case 22:
					chestLoot.Put(4587);
					chestLoot.Put(4590);
					chestLoot.Put(4589);
					chestLoot.Put(4588);
					chestLoot.Put(4083);
					chestLoot.Put(4084);
					chestLoot.Put(4085);
					chestLoot.Put(4086);
					chestLoot.Put(4087);
					chestLoot.Put(4088);
					chestLoot.Put(4039, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4094, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4093, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4092, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4089);
					chestLoot.Put(3989);
					chestLoot.Put(4095);
					chestLoot.Put(4040);
					chestLoot.Put(4319);
					chestLoot.Put(4320);
					chestLoot.Put(4591, () => Main.LocalPlayer.golferScoreAccumulated > 1000);
					chestLoot.Put(4594, () => Main.LocalPlayer.golferScoreAccumulated > 1000);
					chestLoot.Put(4593, () => Main.LocalPlayer.golferScoreAccumulated > 1000);
					chestLoot.Put(4592, () => Main.LocalPlayer.golferScoreAccumulated > 1000);
					chestLoot.Put(4135);
					chestLoot.Put(4138);
					chestLoot.Put(4136);
					chestLoot.Put(4137);
					chestLoot.Put(4049);
					chestLoot.Put(4265, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4598, () => Main.LocalPlayer.golferScoreAccumulated > 2000);
					chestLoot.Put(4597, () => Main.LocalPlayer.golferScoreAccumulated > 2000);
					chestLoot.Put(4596, () => Main.LocalPlayer.golferScoreAccumulated > 2000);
					chestLoot.Put(4264, () => Main.LocalPlayer.golferScoreAccumulated > 2000 && NPC.downedBoss3);
					chestLoot.Put(4599, () => Main.LocalPlayer.golferScoreAccumulated > 500);
					chestLoot.Put(4600, () => Main.LocalPlayer.golferScoreAccumulated >= 1000);
					chestLoot.Put(4601, () => Main.LocalPlayer.golferScoreAccumulated >= 2000);
					chestLoot.Put(4658, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 0);
					chestLoot.Put(4659, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 1);
					chestLoot.Put(4660, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 2);
					chestLoot.Put(4661, () => Main.LocalPlayer.golferScoreAccumulated >= 2000 && (Main.moonPhase / 2) == 3);
					break;
				case 23:
					chestLoot.Put(4776, () => BestiaryGirl_IsFairyTorchAvailable());
					chestLoot.Put(4767);
					chestLoot.Put(4759);
					chestLoot.Put(4672, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
					chestLoot.Put(4829, () => !NPC.boughtCat);
					chestLoot.Put(4830, () => !NPC.boughtDog && Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
					chestLoot.Put(4910, () => !NPC.boughtBunny && Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f);
					chestLoot.Put(4871, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
					chestLoot.Put(4907, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
					chestLoot.Put(4677, () => NPC.downedTowerSolar);
					chestLoot.Put(4676, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
					chestLoot.Put(4762, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
					chestLoot.Put(4716, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
					chestLoot.Put(4785, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
					chestLoot.Put(4786, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
					chestLoot.Put(4787, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
					chestLoot.Put(4788, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f && Main.hardMode);
					chestLoot.Put(4955, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f);
					chestLoot.Put(4736, () => Main.hardMode && Main.bloodMoon);
					chestLoot.Put(4701, () => NPC.downedPlantBoss);
					chestLoot.Put(4765, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
					chestLoot.Put(4766, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
					chestLoot.Put(4777, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
					chestLoot.Put(4763, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.6f);
					chestLoot.Put(4735, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f);
					chestLoot.Put(4951, () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);
					chestLoot.Put(4768, () => (Main.moonPhase / 2) == 0);
					chestLoot.Put(4769, () => (Main.moonPhase / 2) == 0);
					chestLoot.Put(4770, () => (Main.moonPhase / 2) == 1);
					chestLoot.Put(4771, () => (Main.moonPhase / 2) == 1);
					chestLoot.Put(4772, () => (Main.moonPhase / 2) == 2);
					chestLoot.Put(4773, () => (Main.moonPhase / 2) == 2);
					chestLoot.Put(4560, () => (Main.moonPhase / 2) == 3);
					chestLoot.Put(4775, () => (Main.moonPhase / 2) == 4);
					break;
				case 24:
					chestLoot.Put(5071);
					chestLoot.Put(5072);
					chestLoot.Put(5073);
					for (int i = 5076; i < 5087; i++) {
						chestLoot.Put(i);
					}
					chestLoot.Put(5044, () => Main.hardMode && NPC.downedMoonlord);
					chestLoot.Put(1309, () => Main.tenthAnniversaryWorld);
					chestLoot.Put(1859, () => Main.tenthAnniversaryWorld);
					chestLoot.Put(1358, () => Main.tenthAnniversaryWorld);
					chestLoot.Put(857, () => Main.tenthAnniversaryWorld && Main.LocalPlayer.ZoneDesert);
					chestLoot.Put(4144, () => Main.tenthAnniversaryWorld && Main.bloodMoon);
					chestLoot.Put(2584, () => Main.tenthAnniversaryWorld && Main.hardMode && NPC.downedPirates && (Main.moonPhase / 2) == 0);
					chestLoot.Put(854, () => Main.tenthAnniversaryWorld && Main.hardMode && NPC.downedPirates && (Main.moonPhase / 2) == 1);
					chestLoot.Put(855, () => Main.tenthAnniversaryWorld && Main.hardMode && NPC.downedPirates && (Main.moonPhase / 2) == 2);
					chestLoot.Put(905, () => Main.tenthAnniversaryWorld && Main.hardMode && NPC.downedPirates && (Main.moonPhase / 2) == 3);
					chestLoot.Put(5088);
					break;
			}

			NPCLoader.SetupShop(type, chestLoot);

			Item[] items = chestLoot.Build(out slots, true);

			item = items;
			NPCLoader.SetupShop(type, this, ref slots);

			return items;
		}
	}
}
