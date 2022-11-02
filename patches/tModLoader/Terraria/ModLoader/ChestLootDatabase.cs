using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class ChestLootDatabase
	{
		private readonly Dictionary<int, ChestLoot> lootByType = new();

		public void Register(int npcId, ChestLoot chestLoot) => lootByType.Add(npcId, chestLoot);

		public ChestLoot GetById(int npcId) => lootByType[npcId];

		public void Initialize() {
			RegisterMerchant();
			RegisterArmsDealer();
			RegisterDryad();
			RegisterBombGuy();
			RegisterClothier();
			RegisterGoblin();
			for (int i = 25; i < NPCLoader.NPCCount; i++) {
				NPCLoader.SetupShop(i, new());
			}

			foreach (var lbt in lootByType) {
				NPCLoader.PostSetupShop(lbt.Key, lbt.Value);
			}
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
			Register(1, shop);
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
			Register(2, shop);
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
			Register(3, shop);
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
			Register(4, shop);
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
			Register(5, shop);
		}

		private void RegisterGoblin() {
			ChestLoot shop = new();
			shop.Add(128);
			shop.Add(286);
			shop.Add(398);
			shop.Add(84);
			shop.Add(407);
			shop.Add(161);
			Register(6, shop);
		}
	}
}
