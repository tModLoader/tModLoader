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
		private Dictionary<string, ChestLoot> npcShopByName = new();
		private List<ChestLoot.Entry> globalNpcShopEntries = new();

		public void RegisterNpcShop(int npcId, ChestLoot chestLoot, string shopName = "Shop") {
			npcShopByName.Add(CalculateShopName(npcId, shopName), chestLoot);
		}

		public void RegisterGlobalNpcShop(ChestLoot.Entry entry) => globalNpcShopEntries.Add(entry);

		public ChestLoot GetNpcShop(string fullName) {
			if (npcShopByName.ContainsKey(fullName))
				return npcShopByName[fullName];

			return null;
		}

		public static string CalculateShopName(int npcId, string shopName = "Shop")
		{
			return $"{(npcId < NPCID.Count ? $"Terraria/{NPCID.Search.GetName(npcId)}" : NPCLoader.GetNPC(npcId).FullName)}/{shopName}";
		}

		public Dictionary<string, ChestLoot> GetNpcShopsOf(int npcId) {
			return npcShopByName.Where(x => {
				var split = x.Key.Split('/');
				return split[0] == (npcId < NPCID.Count ? "Terraria" : NPCLoader.GetNPC(npcId).Mod.Name)
				&& split[1] == (npcId < NPCID.Count ? NPCID.Search.GetName(npcId) : NPCLoader.GetNPC(npcId).Name);
			}).ToDictionary(x => x.Key, x => x.Value);
		}

		public List<ChestLoot.Entry> GetGlobalNpcShopEntries() => globalNpcShopEntries;

		public void Initialize() {
			npcShopByName.Clear();
			globalNpcShopEntries.Clear();

			NPCShops();
		}

		public static readonly int[] VanillaShopIndexToNpcType = {
			NPCID.Merchant, NPCID.ArmsDealer, NPCID.Dryad, // 1, 2, 3
			NPCID.Demolitionist, NPCID.Clothier, NPCID.GoblinTinkerer, // 4, 5, 6
			NPCID.Wizard, NPCID.Mechanic, NPCID.SantaClaus, // 7, 8, 9
			NPCID.Truffle, NPCID.Steampunker, NPCID.DyeTrader, // 10, 11, 12
			NPCID.PartyGirl, NPCID.Cyborg, NPCID.Painter, // 13, 14, 15
			NPCID.WitchDoctor, NPCID.Pirate, NPCID.Stylist, //16, 17, 18
			NPCID.TravellingMerchant,						//19
			NPCID.SkeletonMerchant, NPCID.DD2Bartender, NPCID.Golfer, //20, 21, 22
			NPCID.BestiaryGirl, NPCID.Princess, NPCID.Painter //23, 24, 25
		};

		public void NPCShops() {
			// not updating shops to 1.4.4 yet
			// it took me lot of hours last time ;-;
			RegisterMerchant();
			RegisterArmsDealer();
			RegisterDryad();
			RegisterBombGuy();
			RegisterClothier();
			RegisterGoblin();
			RegisterWizard();
			RegisterMechanic();
			RegisterSantaClaws();
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
				NPCLoader.SetupShop(i);
			}

			RegisterGlobalNpcShop();

			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				NPCLoader.PostSetupShop(i);
			}
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

		private static void RegisterMerchant() {
			var flareGunCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player carries a Flare Gun in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.FlareGun));
			var drumSetCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When either the Eater of Worlds, Brain of Cthulhu, Skeletron, or Wall of Flesh have been defeated"), () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
			var nailGunCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Nail Gun"), () => Main.LocalPlayer.HasItem(ItemID.NailGun));

			new ChestLoot()
				.Add(ItemID.MiningHelmet)
				.Add(ItemID.PiggyBank)
				.Add(ItemID.IronAnvil)
				.Add(ItemID.BugNet)
				.Add(ItemID.CopperPickaxe)
				.Add(ItemID.CopperAxe)
				.Add(ItemID.Torch)
				.Add(ItemID.LesserHealingPotion)
				.Add(ItemID.HealingPotion,		ChestLoot.Condition.Hardmode)
				.Add(ItemID.LesserManaPotion)
				.Add(ItemID.ManaPotion,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.WoodenArrow)
				.Add(ItemID.Shuriken)
				.Add(ItemID.Rope)
				.Add(ItemID.Marshmallow,		ChestLoot.Condition.InSnowBiome)
				.Add(ItemID.Furnace,			ChestLoot.Condition.InJungleBiome)
				.Add(ItemID.PinWheel,			ChestLoot.Condition.TimeDay, ChestLoot.Condition.HappyWindyDay)
				.Add(ItemID.ThrowingKnife,		ChestLoot.Condition.BloodMoon)
				.Add(ItemID.Glowstick,			ChestLoot.Condition.TimeNight)
				.Add(ItemID.SharpeningStation,	ChestLoot.Condition.Hardmode)
				.Add(ItemID.Safe,				ChestLoot.Condition.DownedSkeletron)
				.Add(ItemID.DiscoBall,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.Flare,				flareGunCondition)
				.Add(ItemID.BlueFlare,			flareGunCondition)
				.Add(ItemID.Sickle)
				.Add(ItemID.GoldDust,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.DrumSet,			drumSetCondition)
				.Add(ItemID.DrumStick,			drumSetCondition)
				.Add(ItemID.NailGun,			nailGunCondition)
				.RegisterShop(NPCID.Merchant);
		}

		private static void RegisterArmsDealer() {
			var silverBulletCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Blood Moon, in a world with Silver Ore. (Always available in Hardmode)"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Silver);
			var tungstenBulletCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Blood Moon, in a world with Tungsten Ore. (Always available in Hardmode)"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Tungsten);
			var unholyArrowCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During the night after defeating the Eater of Worlds or Brain of Cthulhu. (Always available in Hardmode)"), () => (NPC.downedBoss2 && !Main.dayTime) || Main.hardMode);
			var styngerCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Stynger"), () => Main.LocalPlayer.HasItem(ItemID.Stynger));
			var stakeCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Stake Launcher"), () => Main.LocalPlayer.HasItem(ItemID.StakeLauncher));
			var nailCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Nail Gun"), () => Main.LocalPlayer.HasItem(ItemID.NailGun));
			var candyCornCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Candy Corn Rifle"), () => Main.LocalPlayer.HasItem(ItemID.CandyCornRifle));
			var jackCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player possesses a Jack 'O Lantern Launcher"), () => Main.LocalPlayer.HasItem(ItemID.JackOLanternLauncher));

			new ChestLoot()
				.Add(ItemID.MusketBall)
				.Add(ItemID.SilverBullet,		silverBulletCondition)
				.Add(ItemID.TungstenBullet,		tungstenBulletCondition)
				.Add(ItemID.UnholyArrow,		unholyArrowCondition)
				.Add(ItemID.FlintlockPistol)
				.Add(ItemID.Minishark)
				.Add(ItemID.IllegalGunParts,	ChestLoot.Condition.TimeNight)
				.Add(ItemID.AmmoBox,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.Shotgun,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.EmptyBullet,		ChestLoot.Condition.Hardmode)
				.Add(ItemID.StyngerBolt,		styngerCondition)
				.Add(ItemID.Stake,				stakeCondition)
				.Add(ItemID.Nail,				nailCondition)
				.Add(ItemID.CandyCorn,			candyCornCondition)
				.Add(ItemID.ExplosiveJackOLantern, jackCondition)
				.Add(ItemID.NurseHat,			ChestLoot.Condition.Halloween)
				.Add(ItemID.NurseShirt,			ChestLoot.Condition.Halloween)
				.Add(ItemID.NursePants,			ChestLoot.Condition.Halloween)
				.Add(ItemID.QuadBarrelShotgun,	ChestLoot.Condition.InGraveyard, ChestLoot.Condition.DownedSkeletron)
				.RegisterShop(NPCID.ArmsDealer);
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
			RegisterNpcShop(NPCID.Dryad, shop);
		}

		private static void RegisterBombGuy() {
			var dryBombCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Dry Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.DryBomb));
			var wetBombCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Wet Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.WetBomb));
			var lavaBombCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Lava Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.LavaBomb));
			var honeyBombCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Honey Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.HoneyBomb));

			new ChestLoot()
				.Add(ItemID.Grenade)
				.Add(ItemID.Bomb)
				.Add(ItemID.Dynamite)
				.Add(ItemID.HellfireArrow,		ChestLoot.Condition.Hardmode)
				.Add(ItemID.LandMine,			ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedPlantera, ChestLoot.Condition.DownedPirates)
				.Add(ItemID.ExplosivePowder,	ChestLoot.Condition.Hardmode)
				.Add(ItemID.DryBomb,			dryBombCondition)
				.Add(ItemID.WetBomb,			wetBombCondition)
				.Add(ItemID.LavaBomb,			lavaBombCondition)
				.Add(ItemID.HoneyBomb,			honeyBombCondition)
				.RegisterShop(NPCID.Demolitionist);
		}

		private static void RegisterClothier() {
			var taxCollectorPresent = new ChestLoot.Condition(NetworkText.FromLiteral("If the Tax Collector is present"), () => NPC.AnyNPCs(NPCID.TaxCollector));
			var golfScoreOf2000 = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has an accumulate golf score of 2000 or more"), () => Main.LocalPlayer.golferScoreAccumulated >= 2000);

			new ChestLoot()
				.Add(ItemID.BlackThread)
				.Add(ItemID.PinkThread)
				.Add(ItemID.PlacePainting)																			// r/Terraria
				.Add(ItemID.SummerHat,			ChestLoot.Condition.TimeDay)
				.Add(ItemID.PlumbersShirt,		ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.PlumbersPants,		ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.WhiteTuxedoShirt,	ChestLoot.Condition.IsMoonFull, ChestLoot.Condition.TimeNight)
				.Add(ItemID.WhiteTuxedoPants,	ChestLoot.Condition.IsMoonFull, ChestLoot.Condition.TimeNight)
				.Add(ItemID.TheDoctorsShirt,	ChestLoot.Condition.IsMoonWaningGibbous)
				.Add(ItemID.TheDoctorsPants,	ChestLoot.Condition.IsMoonWaningGibbous)
				.Add(ItemID.FamiliarShirt)
				.Add(ItemID.FamiliarPants)
				.Add(ItemID.FamiliarWig)
				.Add(ItemID.ClownHat,			ChestLoot.Condition.DownedClown)
				.Add(ItemID.ClownShirt,			ChestLoot.Condition.DownedClown)
				.Add(ItemID.ClownPants,			ChestLoot.Condition.DownedClown)
				.Add(ItemID.MimeMask,			ChestLoot.Condition.BloodMoon)
				.Add(ItemID.FallenTuxedoShirt,	ChestLoot.Condition.BloodMoon)
				.Add(ItemID.FallenTuxedoPants,	ChestLoot.Condition.BloodMoon)
				.Add(ItemID.WhiteLunaticHood,	ChestLoot.Condition.TimeDay, ChestLoot.Condition.DownedCultist)		// Solar Lunatic Hood
				.Add(ItemID.WhiteLunaticRobe,	ChestLoot.Condition.TimeDay, ChestLoot.Condition.DownedCultist)		// Solar Lunatic Robe
				.Add(ItemID.BlueLunaticHood,	ChestLoot.Condition.TimeNight, ChestLoot.Condition.DownedCultist)	// Lunar Cultist Hood
				.Add(ItemID.BlueLunaticRobe,	ChestLoot.Condition.TimeNight, ChestLoot.Condition.DownedCultist)	// Lunat Cultist Robe
				.Add(ItemID.TaxCollectorHat,	taxCollectorPresent)
				.Add(ItemID.TaxCollectorSuit,	taxCollectorPresent)
				.Add(ItemID.TaxCollectorPants,	taxCollectorPresent)
				.Add(ItemID.UndertakerHat,		ChestLoot.Condition.InGraveyard)									// Gravedigger Hat
				.Add(ItemID.UndertakerCoat,		ChestLoot.Condition.InGraveyard)									// Gravedigger Coat
				.Add(ItemID.FuneralHat,			ChestLoot.Condition.InGraveyard)
				.Add(ItemID.FuneralCoat,		ChestLoot.Condition.InGraveyard)
				.Add(ItemID.FuneralPants,		ChestLoot.Condition.InGraveyard)
				.Add(ItemID.TragicUmbrella,		ChestLoot.Condition.InGraveyard)
				.Add(ItemID.VictorianGothHat,	ChestLoot.Condition.InGraveyard)
				.Add(ItemID.VictorianGothDress, ChestLoot.Condition.InGraveyard)
				.Add(ItemID.Beanie,				ChestLoot.Condition.InSnowBiome)
				.Add(ItemID.GuyFawkesMask,		ChestLoot.Condition.Halloween)
				.Add(ItemID.TamOShanter,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonThirdQuarter)
				.Add(ItemID.GraduationCapBlue,	ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaningCrescent)
				.Add(ItemID.GraduationGownBlue, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaningCrescent)
				.Add(ItemID.Tiara,				ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonNew)
				.Add(ItemID.PrincessDress,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonNew)
				.Add(ItemID.GraduationCapMaroon,ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingCrescent)
				.Add(ItemID.GraduationGownMaroon,ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingCrescent) // we have to ignore this menace 
				.Add(ItemID.CowboyHat,			ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter)
				.Add(ItemID.CowboyJacket,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter)
				.Add(ItemID.CowboyPants,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonFirstQuarter)
				.Add(ItemID.GraduationCapBlack, ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingGibbous)
				.Add(ItemID.GraduationGownBlack,ChestLoot.Condition.Hardmode, ChestLoot.Condition.IsMoonWaxingGibbous)
				.Add(ItemID.BallaHat,			ChestLoot.Condition.DownedFrost)
				.Add(ItemID.GangstaHat,			ChestLoot.Condition.DownedFrost)
				.Add(ItemID.ClothierJacket,		ChestLoot.Condition.Halloween)
				.Add(ItemID.ClothierPants,		ChestLoot.Condition.Halloween)
				.Add(ItemID.PartyBundleOfBalloonsAccessory,	ChestLoot.Condition.BirthdayPartyIsUp) // why
				.Add(ItemID.PartyBalloonAnimal, ChestLoot.Condition.BirthdayPartyIsUp)
				.Add(ItemID.FlowerBoyHat,		ChestLoot.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
				.Add(ItemID.FlowerBoyShirt,		ChestLoot.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
				.Add(ItemID.FlowerBoyPants,		ChestLoot.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
				.Add(ItemID.HunterCloak,		golfScoreOf2000)
				.RegisterShop(NPCID.Clothier);
		}

		private static void RegisterGoblin() {
			new ChestLoot()
				.Add(ItemID.RocketBoots)
				.Add(ItemID.Ruler)
				.Add(ItemID.TinkerersWorkshop)
				.Add(ItemID.GrapplingHook)
				.Add(ItemID.Toolbelt)
				.Add(ItemID.SpikyBall)
				.Add(ItemID.RubblemakerSmall,	ChestLoot.Condition.Hardmode)
				.RegisterShop(NPCID.GoblinTinkerer);
		}

		private static void RegisterWizard() {
			new ChestLoot()
				.Add(ItemID.CrystalBall)
				.Add(ItemID.IceRod)
				.Add(ItemID.GreaterManaPotion)
				.Add(ItemID.Bell)
				.Add(ItemID.Harp)
				.Add(ItemID.SpellTome)
				.Add(ItemID.Book)
				.Add(ItemID.MusicBox)
				.Add(ItemID.EmptyDropper)
				.Add(ItemID.WizardsHat,			ChestLoot.Condition.Halloween)
				.RegisterShop(NPCID.Wizard);
		}

		private static void RegisterMechanic() {
			var mechanicsRodCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During Moon Phases 2, 4, 6, and 8, when the Angler is present."), () => NPC.AnyNPCs(NPCID.Angler) && Main.moonPhase % 2 == 1);

			new ChestLoot()
				.Add(ItemID.Wrench)
				.Add(ItemID.BlueWrench)
				.Add(ItemID.GreenWrench)
				.Add(ItemID.YellowWrench)
				.Add(ItemID.WireCutter)
				.Add(ItemID.Wire)
				.Add(ItemID.Lever)
				.Add(ItemID.Switch)
				.Add(ItemID.RedPressurePlate)
				.Add(ItemID.GreenPressurePlate)
				.Add(ItemID.GrayPressurePlate)
				.Add(ItemID.BrownPressurePlate)
				.Add(ItemID.BluePressurePlate)
				.Add(ItemID.YellowPressurePlate)
				.Add(ItemID.OrangePressurePlate)
				.Add(ItemID.ProjectilePressurePad)
				.Add(ItemID.BoosterTrack)
				.Add(ItemID.ActuationAccessory)
				.Add(ItemID.Teleporter)
				.Add(ItemID.WirePipe)                                                                               // Junction Box
				.Add(ItemID.LaserRuler)                                                                             // Mechanical Ruler
				.Add(ItemID.MechanicalLens)
				.Add(ItemID.EngineeringHelmet)
				.Add(ItemID.WireBulb)
				.Add(ItemID.MechanicsRod,		mechanicsRodCondition)
				.Add(ItemID.Timer1Second)
				.Add(ItemID.Timer3Second)
				.Add(ItemID.Timer5Second)
				.Add(ItemID.TimerOneHalfSecond)
				.Add(ItemID.TimerOneFourthSecond)
				.RegisterShop(NPCID.Mechanic);
		}

		private static void RegisterSantaClaws() {
			var shop = new ChestLoot()
				.Add(ItemID.SantaHat)
				.Add(ItemID.SantaShirt)
				.Add(ItemID.SantaPants)
				.Add(ItemID.RedLight)
				.Add(ItemID.GreenLight)
				.Add(ItemID.BlueLight);
			for (int i = 1873; i < 1906; i++) {
				shop.Add(i);
			}
			shop.RegisterShop(NPCID.SantaClaus);
		}

		private static void RegisterTruffle() {
			new ChestLoot()
				.Add(ItemID.MushroomCap)
				.Add(ItemID.StrangeGlowingMushroom)
				.Add(ItemID.MySon)
				.Add(ItemID.DarkBlueSolution)
				.Add(ItemID.MushroomSpear,		ChestLoot.Condition.DownedMechBossAny)
				.Add(ItemID.Hammush,			ChestLoot.Condition.DownedMechBossAny)
				.Add(ItemID.Autohammer,			ChestLoot.Condition.DownedPlantera)
				.RegisterShop(NPCID.Truffle);
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
			RegisterNpcShop(NPCID.Steampunker, shop);
		}

		private static void RegisterDyeTrader() {
			var mpServer = new ChestLoot.Condition(NetworkText.FromLiteral("On a multiplayer server"), () => Main.netMode == NetmodeID.MultiplayerClient);

			new ChestLoot()
				.Add(ItemID.DyeVat)
				.Add(ItemID.SilverDye)
				.Add(ItemID.TeamDye,			mpServer)
				.Add(ItemID.DyeTraderRobe,		ChestLoot.Condition.Halloween)
				.Add(ItemID.DyeTraderTurban,	ChestLoot.Condition.Halloween)
				.Add(ItemID.ShadowDye,			ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.NegativeDye,		ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.BrownDye)
				.Add(ItemID.FogboundDye,		ChestLoot.Condition.InGraveyard)
				.Add(ItemID.BloodbathDye,		ChestLoot.Condition.BloodMoon)
				.RegisterShop(NPCID.DyeTrader);
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
			RegisterNpcShop(NPCID.PartyGirl, shop);
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
			RegisterNpcShop(NPCID.Cyborg, shop);
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
			RegisterNpcShop(NPCID.Painter, shop);
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
			RegisterNpcShop(NPCID.WitchDoctor, shop);
		}

		private static void RegisterPirate() {
			var bunnyCannonCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When Party Girl is present, world is in Hardmode and one mechanical boss is defeated."), () => Main.hardMode && NPC.downedMechBossAny && NPC.AnyNPCs(208));

			new ChestLoot()
				.Add(ItemID.Cannon)
				.Add(ItemID.Cannonball)
				.Add(ItemID.PirateHat)
				.Add(ItemID.PirateShirt)
				.Add(ItemID.PiratePants)
				.Add(ItemID.Sail)
				.Add(ItemID.ParrotCracker,		new ChestLoot.Condition(NetworkText.FromLiteral("When spoken to in an Ocean biome"), () => {
					int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
					return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
				}))
				.Add(ItemID.BunnyCannon,		bunnyCannonCondition)
				.RegisterShop(NPCID.Pirate);
		}

		private static void RegisterStylist() {
			var maxLife = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has at least 400 maximum life"), () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
			var maxMana = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has at least 200 maximum mana"), () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
			var moneyHair = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has at least 1 platinum coin in their inventory."), () => {
				long coinValue = 0L;
				for (int i = 0; i < Main.InventoryAmmoSlotsStart; i++) {
					if (Main.LocalPlayer.inventory[i].type == ItemID.CopperCoin)
						coinValue += Main.LocalPlayer.inventory[i].stack;
					else if (Main.LocalPlayer.inventory[i].type == ItemID.SilverCoin)
						coinValue += Main.LocalPlayer.inventory[i].stack * 100;
					else if (Main.LocalPlayer.inventory[i].type == ItemID.GoldCoin)
						coinValue += Main.LocalPlayer.inventory[i].stack * 10000;
					else if (Main.LocalPlayer.inventory[i].type == ItemID.PlatinumCoin)
						coinValue += Main.LocalPlayer.inventory[i].stack * 1000000;
					if (coinValue >= 1000000) {
						return true;
					}
				}
				return false;
			});
			var timeHair = new ChestLoot.Condition(NetworkText.FromLiteral("During the night and following day of moon phases Waning Gibbous, Waning Crescent, Waxing Crescent, Waxing Gibbous"), () => Main.moonPhase % 2 == (!Main.dayTime).ToInt());
			var teamHair = new ChestLoot.Condition(NetworkText.FromLiteral("If the player is on a team in multiplayer"), () => Main.LocalPlayer.team != 0);
			var partyHair = new ChestLoot.Condition(NetworkText.FromLiteral("If there is a Party Girl in the world"), () => NPC.AnyNPCs(NPCID.PartyGirl));

			new ChestLoot()
				.Add(ItemID.WilsonBeardShort)																		// Gentleman's Beard
				.Add(ItemID.HairDyeRemover)
				.Add(ItemID.DepthHairDye)
				.Add(ItemID.LifeHairDye,		maxLife)
				.Add(ItemID.ManaHairDye,		maxMana)
				.Add(ItemID.MoneyHairDye,		moneyHair)
				.Add(ItemID.TimeHairDye,		timeHair)
				.Add(ItemID.TeamHairDye,		teamHair)
				.Add(ItemID.PartyHairDye,		partyHair)
				.Add(ItemID.BiomeHairDye,		ChestLoot.Condition.Hardmode)
				.Add(ItemID.SpeedHairDye,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMechBossAny)
				.Add(ItemID.RainbowHairDye,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedTwins, ChestLoot.Condition.DownedSkeletronPrime, ChestLoot.Condition.DownedDestroyer)
				.Add(ItemID.MartianHairDye,		ChestLoot.Condition.DownedMartians)
				.Add(ItemID.TwilightHairDye,	ChestLoot.Condition.DownedMartians)
				.RegisterShop(NPCID.Stylist);
		}

		private static void RegisterSkeletonMerchant() {
			var wandOfSparkingCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Third Quarter moon in regular worlds"), () => Main.moonPhase == 2 && !Main.remixWorld);
			var magicDaggerCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Third Quarter moon in Don't dig up and Get fixed boi worlds"), () => Main.moonPhase == 2 && Main.remixWorld);
			var lesHealPotCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Waning Gibbous, Waning Crescent, Waxing Crescent or Waxing Gibbous moon"), () => Main.moonPhase % 2 == 0);
			var strangeBrewCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Full, Third Quarter, New or First Quarter moon"), () => Main.moonPhase % 2 != 0);
			var spelunkerGlowCondition = new ChestLoot.Condition(NetworkText.FromLiteral("At night, or all day during a Full Moon"), () => !Main.dayTime || Main.moonPhase == 0);
			var spelunkerFlareCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Flare Gun in their inventory at night, or all day during a Full Moon"), () => (!Main.dayTime || Main.moonPhase == 0) && Main.LocalPlayer.HasItem(ItemID.FlareGun));
			var glowstickCondition = new ChestLoot.Condition(NetworkText.FromLiteral("At daytime, during any moon phase except full"), () => Main.dayTime && Main.moonPhase != 0);
			var boneTorchCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During the first half of every in game minute (every real life second)"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
			var torchCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During the second half of every in game minute (every real life second)"), () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0);
			var boneArrowCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, New or Waxing Crescent moon"), () => Main.moonPhase == 0 || Main.moonPhase == 1 || Main.moonPhase == 4 || Main.moonPhase == 5);
			var woodArrowCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Third Quarter, Waning Crescent, First Quarter or Waxing Gibbous moon"), () => Main.moonPhase == 2 || Main.moonPhase == 3 || Main.moonPhase == 6 || Main.moonPhase == 7);
			var blueCounterCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Full or New moon"), () => Main.moonPhase % 4 == 0);
			var redCounterCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Waning Gibbous or Waxing Crescent moon"), () => Main.moonPhase % 4 == 1);
			var purpleCounterCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Third Quarter or First Quarter moon"), () => Main.moonPhase % 4 == 2);
			var greenCounterCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Waning Crescent or Waxing Gibbous moon"), () => Main.moonPhase % 4 == 3);
			var gradientCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, Third Quarter or Waning Crescent moon"), () => Main.moonPhase < 4);
			var formatcCondition = new ChestLoot.Condition(NetworkText.FromLiteral("During a New, Waxing Crescent, First Quarter or Waxing Gibbous moon"), () => Main.moonPhase >= 4);
			var artisanCondition = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has not consumed an Artisan Loaf before, during the Waning Crescent, New or Waxing Crescent moon phase"), () => !Main.LocalPlayer.ateArtisanBread && Main.moonPhase >= 3 && Main.moonPhase <= 5);

			new ChestLoot()
				.Add(ItemID.WoodenBoomerang,	ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.Umbrella,			ChestLoot.Condition.IsMoonWaningGibbous)
				.Add(ItemID.WandofSparking,		wandOfSparkingCondition)
				.Add(ItemID.MagicDagger,		magicDaggerCondition)
				.Add(ItemID.PortableStool,		ChestLoot.Condition.IsMoonWaningCrescent)							// Step Stool
				.Add(ItemID.Aglet,				ChestLoot.Condition.IsMoonNew)
				.Add(ItemID.ClimbingClaws,		ChestLoot.Condition.IsMoonWaxingCrescent)
				.Add(ItemID.CordageGuide,		ChestLoot.Condition.IsMoonFirstQuarter)								// Guide to Plant Fiber Cordage
				.Add(ItemID.Radar,				ChestLoot.Condition.IsMoonWaxingGibbous)
				.Add(ItemID.StrangeBrew,		strangeBrewCondition)
				.Add(ItemID.LesserHealingPotion,lesHealPotCondition)
				.Add(ItemID.HealingPotion,		ChestLoot.Condition.Hardmode, lesHealPotCondition)
				.Add(ItemID.SpelunkerGlowstick, spelunkerGlowCondition)
				.Add(ItemID.SpelunkerFlare,		spelunkerFlareCondition)
				.Add(ItemID.Glowstick,			glowstickCondition)
				.Add(ItemID.BoneTorch,			boneTorchCondition)
				.Add(ItemID.Torch,				torchCondition)
				.Add(ItemID.BoneArrow,			boneArrowCondition)
				.Add(ItemID.WoodenArrow,		woodArrowCondition)
				.Add(ItemID.BlueCounterweight,  blueCounterCondition)
				.Add(ItemID.RedCounterweight,   redCounterCondition)
				.Add(ItemID.PurpleCounterweight,purpleCounterCondition)
				.Add(ItemID.GreenCounterweight, greenCounterCondition)
				.Add(ItemID.Bomb)
				.Add(ItemID.Rope)
				.Add(ItemID.Gradient,			ChestLoot.Condition.Hardmode, gradientCondition)
				.Add(ItemID.FormatC,			ChestLoot.Condition.Hardmode, formatcCondition)
				.Add(ItemID.YoYoGlove,			ChestLoot.Condition.Hardmode)
				.Add(ItemID.SlapHand,			ChestLoot.Condition.Hardmode, ChestLoot.Condition.BloodMoon)
				.Add(ItemID.MagicLantern,		ChestLoot.Condition.TimeNight, ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.ArtisanLoaf,		artisanCondition)
				.RegisterShop(NPCID.SkeletonMerchant);
		}

		private static void RegisterBartender() {
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

			for (int i = 0; i < items.Length; i++) {
				for (int j = 0; j < items[i].Length; j++) {
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
			shop.RegisterShop(NPCID.DD2Bartender);
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
			RegisterNpcShop(NPCID.Golfer, chestLoot);
		}

		private static void RegisterZoologist() {
			var fairyGlowstick = new ChestLoot.Condition(NetworkText.FromLiteral("Once the player fills out all three Fairies in the Bestiary"), () => Chest.BestiaryGirl_IsFairyTorchAvailable());
			var solarPillarDead = new ChestLoot.Condition(NetworkText.FromLiteral("After beating the Solar Pillar"), () => NPC.downedTowerSolar);

			var moonIsFullOrWaningGibbous = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is full or waning gibbous"), () => Main.moonPhase <= 1);
			var moonIsThirdOrWaningCrescent = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is third quarter or waning crescent"), () => Main.moonPhase >= 2 && Main.moonPhase <= 3);
			var moonIsNewOrWaxingCrescent = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is new or waxing crescent"), () => Main.moonPhase >= 4 && Main.moonPhase <= 5);
			var moonIsFirstOrWaxingGibbous = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is first quarter or waxing gibbous"), () => Main.moonPhase >= 6);

			var bestiaryFilledBy10 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 10%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
			var bestiaryFilledBy25 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 25%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
			var bestiaryFilledBy30 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 30%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
			var bestiaryFilledBy35 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 35%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.35f);
			var bestiaryFilledBy40 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 40%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f);
			var bestiaryFilledBy45 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 45%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f);
			var bestiaryFilledBy50 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 50%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
			var bestiaryFilledBy70 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 70%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f);
			var bestiaryFilledBy100 = new ChestLoot.Condition(NetworkText.FromLiteral("When the Bestiary has been filled completely"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);

			new ChestLoot()
				.Add(ItemID.DontHurtCrittersBook)																	// Guide to Critter Companionship
				.Add(ItemID.SquirrelHook)
				.Add(ItemID.TheWerewolf,		ChestLoot.Condition.IsMoonFull)
				.Add(ItemID.BlandWhip)																				// Leather Whip
				.Add(ItemID.MolluskWhistle,		bestiaryFilledBy25)
				.Add(ItemID.CritterShampoo,		bestiaryFilledBy30)
				.Add(ItemID.DogEars,			moonIsFullOrWaningGibbous)
				.Add(ItemID.DogTail,			moonIsFullOrWaningGibbous)
				.Add(ItemID.FoxEars,			moonIsThirdOrWaningCrescent)
				.Add(ItemID.FoxTail,			moonIsThirdOrWaningCrescent)
				.Add(ItemID.LizardEars,			moonIsNewOrWaxingCrescent)
				.Add(ItemID.LizardTail,			moonIsNewOrWaxingCrescent)
				.Add(ItemID.BunnyEars,			moonIsFirstOrWaxingGibbous)
				.Add(ItemID.BunnyTail,			moonIsFirstOrWaxingGibbous)
				.Add(ItemID.FullMoonSqueakyToy, ChestLoot.Condition.Hardmode, ChestLoot.Condition.BloodMoon)
				.Add(ItemID.MudBud,				ChestLoot.Condition.DownedPlantera)
				.Add(ItemID.LicenseCat)
				.Add(ItemID.LicenseDog,			bestiaryFilledBy25)
				.Add(ItemID.LicenseBunny,		bestiaryFilledBy45)
				.Add(ItemID.KiteKoi,			bestiaryFilledBy10)
				.Add(ItemID.KiteCrawltipede,	solarPillarDead)
				.Add(ItemID.PaintedHorseSaddle, bestiaryFilledBy30)
				.Add(ItemID.MajesticHorseSaddle,bestiaryFilledBy30)
				.Add(ItemID.DarkHorseSaddle,	bestiaryFilledBy30)
				.Add(ItemID.VanityTreeSakuraSeed, bestiaryFilledBy30)                                               // Sakura Sapling
				.Add(ItemID.VanityTreeYellowWillowSeed, bestiaryFilledBy30)                                         // Yellow Willow Sapling
				.Add(ItemID.RabbitOrder,		bestiaryFilledBy40)
				.Add(ItemID.JoustingLance,		bestiaryFilledBy30)
				.Add(ItemID.FairyGlowstick,		fairyGlowstick)
				.Add(ItemID.WorldGlobe,			bestiaryFilledBy50)
				.Add(ItemID.MoonGlobe,			bestiaryFilledBy50)
				.Add(ItemID.TreeGlobe,			bestiaryFilledBy50)
				.Add(ItemID.LightningCarrot,	bestiaryFilledBy50)
				.Add(ItemID.DiggingMoleMinecart,bestiaryFilledBy35)
				.Add(ItemID.BallOfFuseWire,		bestiaryFilledBy70)
				.Add(ItemID.TeleportationPylonVictory, bestiaryFilledBy100)                                         // Universal Pylon
				.RegisterShop(NPCID.BestiaryGirl);
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
			RegisterNpcShop(NPCID.Princess, shop);
		}
	}
}