using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

using static Terraria.ModLoader.NPCShop;

namespace Terraria.ModLoader;

public static class NPCShopDatabase
{
	private static readonly Dictionary<string, NPCShop> npcShopByName = new();

	public static readonly Dictionary<string, bool> NoPylons = new();

	public static IEnumerable<NPCShop> AllShops => npcShopByName.Values;

	public static void RegisterNpcShop(int npcId, NPCShop chestLoot, string shopName = "Shop") {
		npcShopByName.Add(GetNPCShopName(npcId, shopName), chestLoot);
	}

	public static NPCShop GetNPCShop(string fullName) {
		if (npcShopByName.TryGetValue(fullName, out NPCShop chestLoot))
			return chestLoot;

		return null;
	}

	/// <summary>
	/// Gets a shop name (identifier) in the format matching <see cref="NPCShop.FullName"/> <br/>
	/// Can be used with <see cref="GetNPCShop(string)"/> and <see cref="GlobalNPC.ModifyActiveShop(NPC, string, Item[])"/>
	/// </summary>
	/// <param name="npcId"></param>
	/// <param name="shopName"></param>
	/// <returns></returns>
	public static string GetNPCShopName(int npcId, string shopName = "Shop") {
		return $"{(npcId < NPCID.Count ? $"Terraria/{NPCID.Search.GetName(npcId)}" : NPCLoader.GetNPC(npcId).FullName)}/{shopName}";
	}

	public static string GetVanillaShop(int index)
	{
		int npcType = NPCLoader.shopToNPC[index];
		if (index == 25) { // Painter 2 Shop Special Case
			return GetNPCShopName(npcType, "Decor");
		}
		return GetNPCShopName(npcType);
	}

	public static void Initialize() {
		npcShopByName.Clear();
		NoPylons.Clear();

		NPCShops();
	}

	public static void NPCShops() {
		NoPylons[GetNPCShopName(NPCID.SkeletonMerchant)] = true;
		NoPylons[GetNPCShopName(NPCID.DD2Bartender)] = true;

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
			NPCLoader.AddShops(i);
		}
		foreach (var shop in AllShops) {
			NPCLoader.ModifyShop(shop);
		}
	}

	public static NPCShop.Entry[] GetVanillaPylonEntries() {
		/*
		bool num12 = type != 19 && type != 20;
		bool flag3 = TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16());
		if (num12 && (flag || Main.remixWorld) && flag3 && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson) {
			if (!Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && !Main.player[Main.myPlayer].ZoneGlowshroom) {
				if (Main.remixWorld) {
					if ((double)(Main.player[Main.myPlayer].Center.Y / 16f) > Main.rockLayer && Main.player[Main.myPlayer].Center.Y / 16f < (float)(Main.maxTilesY - 350) && num < 39)
						array[num++].SetDefaults(4876);
				}
				else if ((double)(Main.player[Main.myPlayer].Center.Y / 16f) < Main.worldSurface && num < 39) {
					array[num++].SetDefaults(4876);
				}
			}

			if (Main.player[Main.myPlayer].ZoneSnow && num < 39)
				array[num++].SetDefaults(4920);

			if (Main.player[Main.myPlayer].ZoneDesert && num < 39)
				array[num++].SetDefaults(4919);

			if (Main.remixWorld) {
				if (!Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && (double)(Main.player[Main.myPlayer].Center.Y / 16f) >= Main.worldSurface && num < 39)
					array[num++].SetDefaults(4917);
			}
			else if (!Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneHallow && !Main.player[Main.myPlayer].ZoneGlowshroom && (double)(Main.player[Main.myPlayer].Center.Y / 16f) >= Main.worldSurface && num < 39) {
				array[num++].SetDefaults(4917);
			}

			bool flag4 = Main.player[Main.myPlayer].ZoneBeach && (double)Main.player[Main.myPlayer].position.Y < Main.worldSurface * 16.0;
			if (Main.remixWorld) {
				float num13 = Main.player[Main.myPlayer].position.X / 16f;
				float num14 = Main.player[Main.myPlayer].position.Y / 16f;
				flag4 |= ((double)num13 < (double)Main.maxTilesX * 0.43 || (double)num13 > (double)Main.maxTilesX * 0.57) && (double)num14 > Main.rockLayer && num14 < (float)(Main.maxTilesY - 350);
			}

			if (flag4 && num < 39)
				array[num++].SetDefaults(4918);

			if (Main.player[Main.myPlayer].ZoneJungle && num < 39)
				array[num++].SetDefaults(4875);

			if (Main.player[Main.myPlayer].ZoneHallow && num < 39)
				array[num++].SetDefaults(4916);

			if (Main.player[Main.myPlayer].ZoneGlowshroom && (!Main.remixWorld || Main.player[Main.myPlayer].Center.Y / 16f < (float)(Main.maxTilesY - 200)) && num < 39)
				array[num++].SetDefaults(4921);
		}*/
		var pylonMainCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.HappyEnoughForPylons"), () =>
			Main.LocalPlayer.talkNPC != -1 && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[19] && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[20]
			&& (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421 || Main.remixWorld)
			&& TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16())
			&& !Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneCrimson
			);
		var purityPylonCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.InPurity"), () => { // im having struggles localizing these
			if (Main.remixWorld) {
				return Main.LocalPlayer.Center.Y / 16.0 > Main.rockLayer && Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 350;
			}
			return Main.LocalPlayer.Center.Y / 16.0 < Main.worldSurface;
		});
		var cavernPylonCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.InRockLayerHeight"), () => {
			return !Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneDesert
			&& !Main.LocalPlayer.ZoneBeach && !Main.LocalPlayer.ZoneJungle
			&& !Main.LocalPlayer.ZoneHallow && (!Main.remixWorld || !Main.LocalPlayer.ZoneGlowshroom)
			&& (double)(Main.LocalPlayer.Center.Y / 16f) >= Main.worldSurface;
		});
		var oceanPylonCondition = new NPCShop.Condition(NetworkText.FromKey("RecipeConditions.InBeach"), () => {
			bool flag4 = Main.LocalPlayer.ZoneBeach && Main.LocalPlayer.position.Y < Main.worldSurface * 16.0;
			if (Main.remixWorld) {
				double num13 = Main.LocalPlayer.position.X / 16.0;
				double num14 = Main.LocalPlayer.position.Y / 16.0;
				flag4 |= (num13 < Main.maxTilesX * 0.43 || num13 > Main.maxTilesX * 0.57) && num14 > Main.rockLayer && num14 < Main.maxTilesY - 350;
			}
			return flag4;
		});
		var glowshroomPylonCondtiion = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.UndergroundOrRemix"),
			() => !Main.remixWorld || Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 200);

		return new NPCShop.Entry[] {
			new(ItemID.TeleportationPylonPurity,        pylonMainCondition, NPCShop.Condition.InPurityBiome, purityPylonCondition),
			new(ItemID.TeleportationPylonSnow,          pylonMainCondition, NPCShop.Condition.InSnowBiome),
			new(ItemID.TeleportationPylonDesert,        pylonMainCondition, NPCShop.Condition.InDesertBiome),
			new(ItemID.TeleportationPylonUnderground,   pylonMainCondition, cavernPylonCondition),
			new(ItemID.TeleportationPylonOcean,         pylonMainCondition, oceanPylonCondition),
			new(ItemID.TeleportationPylonJungle,        pylonMainCondition, NPCShop.Condition.InJungleBiome),
			new(ItemID.TeleportationPylonHallow,        pylonMainCondition, NPCShop.Condition.InHallowBiome),
			new(ItemID.TeleportationPylonMushroom,      pylonMainCondition, NPCShop.Condition.InGlowshroomBiome, glowshroomPylonCondtiion)
		};
	}

	private static void RegisterMerchant() {
		var flareGunCondition = NPCShop.Condition.PlayerCarriesItem(ItemID.FlareGun);
		var drumSetCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.DownedB2B3HM"), () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
		
		new NPCShop(NPCID.Merchant)
			.Add(ItemID.MiningHelmet)
			.Add(ItemID.PiggyBank)
			.Add(ItemID.IronAnvil)
			.Add(ItemID.BugNet)
			.Add(ItemID.CopperPickaxe)
			.Add(ItemID.CopperAxe)
			.Add(ItemID.Torch)
			.Add(ItemID.LesserHealingPotion)
			.Add(ItemID.HealingPotion,		NPCShop.Condition.Hardmode)
			.Add(ItemID.LesserManaPotion)
			.Add(ItemID.ManaPotion,			NPCShop.Condition.Hardmode)
			.Add(ItemID.WoodenArrow)
			.Add(ItemID.Shuriken)
			.Add(ItemID.Rope)
			.Add(ItemID.Marshmallow,		NPCShop.Condition.InSnowBiome)
			.Add(ItemID.Furnace,			NPCShop.Condition.InJungleBiome)
			.Add(ItemID.PinWheel,			NPCShop.Condition.TimeDay, NPCShop.Condition.HappyWindyDay)
			.Add(ItemID.ThrowingKnife,		NPCShop.Condition.BloodMoon)
			.Add(ItemID.Glowstick,			NPCShop.Condition.TimeNight)
			.Add(ItemID.SharpeningStation,	NPCShop.Condition.Hardmode)
			.Add(ItemID.Safe,				NPCShop.Condition.DownedSkeletron)
			.Add(ItemID.DiscoBall,			NPCShop.Condition.Hardmode)
			.Add(ItemID.Flare,				flareGunCondition)
			.Add(ItemID.BlueFlare,			flareGunCondition)
			.Add(ItemID.Sickle)
			.Add(ItemID.GoldDust,			NPCShop.Condition.Hardmode)
			.Add(ItemID.DrumSet,			drumSetCondition)
			.Add(ItemID.DrumStick,			drumSetCondition)
			.Add(ItemID.NailGun,			NPCShop.Condition.PlayerCarriesItem(ItemID.NailGun))
			.Register();
	}

	private static void RegisterArmsDealer() {
		var silverBulletCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BloodOrHMInSilver"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Silver);
		var tungstenBulletCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BloodOrHMInTungst"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Tungsten);
		var unholyArrowCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.NightAfterEvil"), () => (NPC.downedBoss2 && !Main.dayTime) || Main.hardMode);

		new NPCShop(NPCID.ArmsDealer)
			.Add(ItemID.MusketBall)
			.Add(ItemID.SilverBullet,		silverBulletCondition)
			.Add(ItemID.TungstenBullet,		tungstenBulletCondition)
			.Add(ItemID.UnholyArrow,		unholyArrowCondition)
			.Add(ItemID.FlintlockPistol)
			.Add(ItemID.Minishark)
			.Add(ItemID.IllegalGunParts,	NPCShop.Condition.TimeNight)
			.Add(ItemID.AmmoBox,			NPCShop.Condition.Hardmode)
			.Add(ItemID.Shotgun,			NPCShop.Condition.Hardmode)
			.Add(ItemID.EmptyBullet,		NPCShop.Condition.Hardmode)
			.Add(ItemID.StyngerBolt,		NPCShop.Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Stake,				NPCShop.Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.Nail,				NPCShop.Condition.PlayerCarriesItem(ItemID.NailGun))
			.Add(ItemID.CandyCorn,			NPCShop.Condition.PlayerCarriesItem(ItemID.CandyCornRifle))
			.Add(ItemID.ExplosiveJackOLantern, NPCShop.Condition.PlayerCarriesItem(ItemID.JackOLanternLauncher))
			.Add(ItemID.NurseHat,			NPCShop.Condition.Halloween)
			.Add(ItemID.NurseShirt,			NPCShop.Condition.Halloween)
			.Add(ItemID.NursePants,			NPCShop.Condition.Halloween)
			.Add(ItemID.QuadBarrelShotgun,	NPCShop.Condition.InGraveyard, NPCShop.Condition.DownedSkeletron)
			.Register();
	}

	private static void RegisterDryad() {
		var shop = new NPCShop(NPCID.Dryad);
		var corruptSeedsCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.CorruOrGravCrim"), () => !WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && WorldGen.crimson);
		var crimsonSeedsCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.CrimOrGravCorru"), () => WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && !WorldGen.crimson);

		shop.Add(ItemID.VilePowder,			NPCShop.Condition.BloodMoon, NPCShop.Condition.CrimsonWorld, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.ViciousPowder,		NPCShop.Condition.BloodMoon, NPCShop.Condition.CorruptionWorld, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.PurificationPowder, NPCShop.Condition.NotBloodMoon, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.GrassSeeds,			NPCShop.Condition.NotBloodMoon)
			.Add(ItemID.AshGrassSeeds,		NPCShop.Condition.InUnderworld)
			.Add(ItemID.CorruptSeeds,		NPCShop.Condition.BloodMoon, corruptSeedsCondition)
			.Add(ItemID.CrimsonSeeds,		NPCShop.Condition.BloodMoon, crimsonSeedsCondition)
			.Add(ItemID.Sunflower,			NPCShop.Condition.NotBloodMoon)
			.Add(ItemID.Acorn)
			.Add(ItemID.DontHurtNatureBook)																	// Guide to Environmental Preservation
			.Add(ItemID.DirtRod)
			.Add(ItemID.PumpkinSeed)
			.Add(ItemID.CorruptGrassEcho,	NPCShop.Condition.BloodMoon, NPCShop.Condition.CorruptionWorld) // Crimosn Grass Wall
			.Add(ItemID.CrimsonGrassEcho,	NPCShop.Condition.BloodMoon, NPCShop.Condition.CrimsonWorld)    // Corrupt Grass Wall
			.Add(ItemID.GrassWall,			NPCShop.Condition.NotBloodMoon)
			.Add(ItemID.FlowerWall)
			.Add(ItemID.JungleWall,			NPCShop.Condition.Hardmode)
			.Add(ItemID.HallowedSeeds,		NPCShop.Condition.Hardmode)
			.Add(ItemID.HallowedGrassEcho,	NPCShop.Condition.Hardmode)										// Hallowed Grass Wall
			.Add(ItemID.MushroomGrassSeeds, NPCShop.Condition.InGlowshroomBiome)
			.Add(ItemID.DryadCoverings,		NPCShop.Condition.Halloween)
			.Add(ItemID.DryadLoincloth,		NPCShop.Condition.Halloween)
			.Add(ItemID.DayBloomPlanterBox,	NPCShop.Condition.DownedKingSlime)
			.Add(ItemID.MoonglowPlanterBox, NPCShop.Condition.DownedQueenBee)
			.Add(ItemID.BlinkrootPlanterBox, NPCShop.Condition.DownedEyeOfCthulhu)
			.Add(ItemID.PotSuspendedDeathweedCorrupt, NPCShop.Condition.DownedEaterOfWorlds)
			.Add(ItemID.PotSuspendedDeathweedCrimson, NPCShop.Condition.DownedBrainOfCthulhu)
			.Add(ItemID.WaterleafPlanterBox, NPCShop.Condition.DownedSkeletron)
			.Add(ItemID.ShiverthornPlanterBox, NPCShop.Condition.DownedSkeletron)
			.Add(ItemID.FireBlossomPlanterBox, NPCShop.Condition.Hardmode)
			.Add(ItemID.FlowerPacketWhite)																	// White Flower Seeds
			.Add(ItemID.FlowerPacketYellow)																	// Yellows Flower Seeds
			.Add(ItemID.FlowerPacketRed)																	// Red Flower Seeds
			.Add(ItemID.FlowerPacketPink)																	// Pink Flower Seeds
			.Add(ItemID.FlowerPacketMagenta)																// Magenta Flower Seeds
			.Add(ItemID.FlowerPacketViolet)																	// Violet Flower Seeds
			.Add(ItemID.FlowerPacketBlue)																	// Blue Flower Seeds
			.Add(ItemID.FlowerPacketWild)																	// Wild Flower Seeds
			.Add(ItemID.FlowerPacketTallGrass)																// Tall Grass Seeds
			.Add(ItemID.PottedForestCedar,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedJungleCedar,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedHallowCedar,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedForestTree,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedJungleTree,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedHallowTree,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedForestPalm,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedJunglePalm,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedHallowPalm,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedForestBamboo, NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.PottedJungleBamboo, NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.PottedHallowBamboo, NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesQuarter3)
			.Register();
	}

	private static void RegisterBombGuy() {
		new NPCShop(NPCID.Demolitionist)
			.Add(ItemID.Grenade)
			.Add(ItemID.Bomb)
			.Add(ItemID.Dynamite)
			.Add(ItemID.HellfireArrow,		NPCShop.Condition.Hardmode)
			.Add(ItemID.LandMine,			NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPlantera, NPCShop.Condition.DownedPirates)
			.Add(ItemID.ExplosivePowder,	NPCShop.Condition.Hardmode)
			.Add(ItemID.DryBomb,			NPCShop.Condition.PlayerCarriesItem(ItemID.DryBomb))
			.Add(ItemID.WetBomb,			NPCShop.Condition.PlayerCarriesItem(ItemID.WetBomb))
			.Add(ItemID.LavaBomb,			NPCShop.Condition.PlayerCarriesItem(ItemID.LavaBomb))
			.Add(ItemID.HoneyBomb,			NPCShop.Condition.PlayerCarriesItem(ItemID.HoneyBomb))
			.Register();
	}

	private static void RegisterClothier() {
		var taxCollectorPresent = NPCShop.Condition.NpcIsPresent(NPCID.TaxCollector);

		new NPCShop(NPCID.Clothier)
			.Add(ItemID.BlackThread)
			.Add(ItemID.PinkThread)
			.Add(ItemID.PlacePainting)																		// r/Terraria
			.Add(ItemID.SummerHat,			NPCShop.Condition.TimeDay)
			.Add(ItemID.PlumbersShirt,		NPCShop.Condition.IsMoonFull)
			.Add(ItemID.PlumbersPants,		NPCShop.Condition.IsMoonFull)
			.Add(ItemID.WhiteTuxedoShirt,	NPCShop.Condition.IsMoonFull, NPCShop.Condition.TimeNight)
			.Add(ItemID.WhiteTuxedoPants,	NPCShop.Condition.IsMoonFull, NPCShop.Condition.TimeNight)
			.Add(ItemID.TheDoctorsShirt,	NPCShop.Condition.IsMoonWaningGibbous)
			.Add(ItemID.TheDoctorsPants,	NPCShop.Condition.IsMoonWaningGibbous)
			.Add(ItemID.FamiliarShirt)
			.Add(ItemID.FamiliarPants)
			.Add(ItemID.FamiliarWig)
			.Add(ItemID.ClownHat,			NPCShop.Condition.DownedClown)
			.Add(ItemID.ClownShirt,			NPCShop.Condition.DownedClown)
			.Add(ItemID.ClownPants,			NPCShop.Condition.DownedClown)
			.Add(ItemID.MimeMask,			NPCShop.Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoShirt,	NPCShop.Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoPants,	NPCShop.Condition.BloodMoon)
			.Add(ItemID.WhiteLunaticHood,	NPCShop.Condition.TimeDay, NPCShop.Condition.DownedCultist)		// Solar Lunatic Hood
			.Add(ItemID.WhiteLunaticRobe,	NPCShop.Condition.TimeDay, NPCShop.Condition.DownedCultist)		// Solar Lunatic Robe
			.Add(ItemID.BlueLunaticHood,	NPCShop.Condition.TimeNight, NPCShop.Condition.DownedCultist)	// Lunar Cultist Hood
			.Add(ItemID.BlueLunaticRobe,	NPCShop.Condition.TimeNight, NPCShop.Condition.DownedCultist)	// Lunat Cultist Robe
			.Add(ItemID.TaxCollectorHat,	taxCollectorPresent)
			.Add(ItemID.TaxCollectorSuit,	taxCollectorPresent)
			.Add(ItemID.TaxCollectorPants,	taxCollectorPresent)
			.Add(ItemID.UndertakerHat,		NPCShop.Condition.InGraveyard)									// Gravedigger Hat
			.Add(ItemID.UndertakerCoat,		NPCShop.Condition.InGraveyard)									// Gravedigger Coat
			.Add(ItemID.FuneralHat,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.FuneralCoat,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.FuneralPants,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.TragicUmbrella,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.VictorianGothHat,	NPCShop.Condition.InGraveyard)
			.Add(ItemID.VictorianGothDress, NPCShop.Condition.InGraveyard)
			.Add(ItemID.Beanie,				NPCShop.Condition.InSnowBiome)
			.Add(ItemID.GuyFawkesMask,		NPCShop.Condition.Halloween)
			.Add(ItemID.TamOShanter,		NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonThirdQuarter)
			.Add(ItemID.GraduationCapBlue,	NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaningCrescent)
			.Add(ItemID.GraduationGownBlue, NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaningCrescent)
			.Add(ItemID.Tiara,				NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonNew)
			.Add(ItemID.PrincessDress,		NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonNew)
			.Add(ItemID.GraduationCapMaroon,NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaxingCrescent)
			.Add(ItemID.GraduationGownMaroon,NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaxingCrescent) // we have to ignore this menace 
			.Add(ItemID.CowboyHat,			NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonFirstQuarter)
			.Add(ItemID.CowboyJacket,		NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonFirstQuarter)
			.Add(ItemID.CowboyPants,		NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonFirstQuarter)
			.Add(ItemID.GraduationCapBlack, NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaxingGibbous)
			.Add(ItemID.GraduationGownBlack,NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonWaxingGibbous)
			.Add(ItemID.BallaHat,			NPCShop.Condition.DownedFrost)
			.Add(ItemID.GangstaHat,			NPCShop.Condition.DownedFrost)
			.Add(ItemID.ClothierJacket,		NPCShop.Condition.Halloween)
			.Add(ItemID.ClothierPants,		NPCShop.Condition.Halloween)
			.Add(ItemID.PartyBundleOfBalloonsAccessory,	NPCShop.Condition.BirthdayPartyIsUp) // why
			.Add(ItemID.PartyBalloonAnimal, NPCShop.Condition.BirthdayPartyIsUp)
			.Add(ItemID.FlowerBoyHat,		NPCShop.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.FlowerBoyShirt,		NPCShop.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.FlowerBoyPants,		NPCShop.Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.HunterCloak,		NPCShop.Condition.GolfScoreOver(2000))
			.Register();
	}

	private static void RegisterGoblin() {
		new NPCShop(NPCID.GoblinTinkerer)
			.Add(ItemID.RocketBoots)
			.Add(ItemID.Ruler)
			.Add(ItemID.TinkerersWorkshop)
			.Add(ItemID.GrapplingHook)
			.Add(ItemID.Toolbelt)
			.Add(ItemID.SpikyBall)
			.Add(ItemID.RubblemakerSmall,	NPCShop.Condition.Hardmode)
			.Register();
	}

	private static void RegisterWizard()
	{
		new NPCShop(NPCID.Wizard)
			.Add(ItemID.CrystalBall)
			.Add(ItemID.IceRod)
			.Add(ItemID.GreaterManaPotion)
			.Add(ItemID.Bell)
			.Add(ItemID.Harp)
			.Add(ItemID.SpellTome)
			.Add(ItemID.Book)
			.Add(ItemID.MusicBox)
			.Add(ItemID.EmptyDropper)
			.Add(ItemID.WizardsHat,			NPCShop.Condition.Halloween)
			.Register();
	}

	private static void RegisterMechanic() {
		new NPCShop(NPCID.Mechanic)
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
			.Add(ItemID.Actuator)
			.Add(ItemID.Teleporter)
			.Add(ItemID.WirePipe)                                                                               // Junction Box
			.Add(ItemID.LaserRuler)                                                                             // Mechanical Ruler
			.Add(ItemID.MechanicalLens)
			.Add(ItemID.EngineeringHelmet)
			.Add(ItemID.WireBulb)
			.Add(ItemID.MechanicsRod,		NPCShop.Condition.NpcIsPresent(NPCID.Angler), NPCShop.Condition.IsMoonPhasesOddQuarters)
			.Add(ItemID.Timer1Second)
			.Add(ItemID.Timer3Second)
			.Add(ItemID.Timer5Second)
			.Add(ItemID.TimerOneHalfSecond)
			.Add(ItemID.TimerOneFourthSecond)
			.Add(ItemID.PixelBox)
			.Register();
	}

	private static void RegisterSantaClaws() {
		var shop = new NPCShop(NPCID.SantaClaus)
			.Add(ItemID.SantaHat)
			.Add(ItemID.SantaShirt)
			.Add(ItemID.SantaPants)
			.Add(ItemID.RedLight)
			.Add(ItemID.GreenLight)
			.Add(ItemID.BlueLight);
		for (int i = 1873; i < 1906; i++) {
			shop.Add(i);
		}
		shop.Register();
	}

	private static void RegisterTruffle()
	{
		new NPCShop(NPCID.Truffle)
			.Add(ItemID.MushroomCap)
			.Add(ItemID.StrangeGlowingMushroom)
			.Add(ItemID.MySon)
			.Add(ItemID.DarkBlueSolution)
			.Add(ItemID.MushroomSpear,		NPCShop.Condition.DownedMechBossAny)
			.Add(ItemID.Hammush,			NPCShop.Condition.DownedMechBossAny)
			.Add(ItemID.Autohammer,			NPCShop.Condition.DownedPlantera)
			.Register();
	}

	private static void RegisterSteampunker() {
		var eclipseOrBloodMoon = NPCShop.Condition.EclipseOrBloodMoon;
		var notEclipseOrBloodMoon = NPCShop.Condition.NotEclipseAndNotBloodMoon;
		var blendOMaticCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.HardmodeFTW"), () => Main.hardMode || !Main.getGoodWorld);

		new NPCShop(NPCID.Steampunker)
			.Add(ItemID.Clentaminator,		NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.SteampunkHat,		NPCShop.Condition.IsMoonPhasesHalf0)
			.Add(ItemID.SteampunkShirt,		NPCShop.Condition.IsMoonPhasesHalf0)
			.Add(ItemID.SteampunkPants,		NPCShop.Condition.IsMoonPhasesHalf0)
			.Add(ItemID.Jetpack,			NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesHalf1)
			.Add(ItemID.BlendOMatic,		blendOMaticCondition)
			.Add(ItemID.FleshCloningVaat,	NPCShop.Condition.CrimsonWorld)
			.Add(ItemID.LesionStation,		NPCShop.Condition.CorruptionWorld)                             // Decay Chamber
			.Add(ItemID.IceMachine,			NPCShop.Condition.InSnowBiome)
			.Add(ItemID.SkyMill,			NPCShop.Condition.InSpace)
			.Add(ItemID.HoneyDispenser,		NPCShop.Condition.InJungleBiome)
			.Add(ItemID.BoneWelder,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.LivingLoom,			NPCShop.Condition.PlayerCarriesItem(ItemID.LivingWoodWand))
			.Add(ItemID.SteampunkBoiler,	NPCShop.Condition.DownedEyeOfCthulhu, NPCShop.Condition.DownedEowOrBoc, NPCShop.Condition.DownedSkeletron)
			.Add(ItemID.RedSolution,		NPCShop.Condition.NotRemixWorld, eclipseOrBloodMoon, NPCShop.Condition.CrimsonWorld)
			.Add(ItemID.PurpleSolution,		NPCShop.Condition.NotRemixWorld, eclipseOrBloodMoon, NPCShop.Condition.CorruptionWorld)
			.Add(ItemID.BlueSolution,		NPCShop.Condition.NotRemixWorld, notEclipseOrBloodMoon, NPCShop.Condition.InHallowBiome)
			.Add(ItemID.GreenSolution,		NPCShop.Condition.NotRemixWorld, notEclipseOrBloodMoon, NPCShop.Condition.InPurityBiome)
			.Add(ItemID.SandSolution,		NPCShop.Condition.NotRemixWorld, NPCShop.Condition.DownedMoonLord)
			.Add(ItemID.SnowSolution,		NPCShop.Condition.NotRemixWorld, NPCShop.Condition.DownedMoonLord)
			.Add(ItemID.DirtSolution,		NPCShop.Condition.NotRemixWorld, NPCShop.Condition.DownedMoonLord)
			.Add(ItemID.Cog)
			.Add(ItemID.SteampunkMinecart)
			.Add(ItemID.SteampunkGoggles,	NPCShop.Condition.Halloween)
			.Add(ItemID.LogicGate_AND)
			.Add(ItemID.LogicGate_OR)
			.Add(ItemID.LogicGate_XOR)
			.Add(ItemID.LogicGate_NAND)
			.Add(ItemID.LogicGate_NOR)
			.Add(ItemID.LogicGate_NXOR)
			.Add(ItemID.LogicGateLamp_Off)
			.Add(ItemID.LogicGateLamp_On)
			.Add(ItemID.LogicGateLamp_Faulty)
			.Add(ItemID.StaticHook,			NPCShop.Condition.Hardmode)
			.Add(ItemID.ConveyorBeltLeft)
			.Add(ItemID.ConveyorBeltRight)
			.Register();
	}

	private static void RegisterDyeTrader() {
		var mpServer = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.MultiplayerServer"), () => Main.netMode == NetmodeID.MultiplayerClient);

		new NPCShop(NPCID.DyeTrader)
			.Add(ItemID.DyeVat)
			.Add(ItemID.SilverDye)
			.Add(ItemID.TeamDye,			mpServer)
			.Add(ItemID.DyeTraderRobe,		NPCShop.Condition.Halloween)
			.Add(ItemID.DyeTraderTurban,	NPCShop.Condition.Halloween)
			.Add(ItemID.ShadowDye,			NPCShop.Condition.IsMoonFull)
			.Add(ItemID.NegativeDye,		NPCShop.Condition.IsMoonFull)
			.Add(ItemID.BrownDye)
			.Add(ItemID.FogboundDye,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.BloodbathDye,		NPCShop.Condition.BloodMoon)
			.Register();
	}

	private static void RegisterPartyGirl() {
		new NPCShop(NPCID.PartyGirl)
			.Add(ItemID.ConfettiGun)
			.Add(ItemID.Confetti)
			.Add(ItemID.SmokeBomb)
			.Add(ItemID.BubbleMachine,		NPCShop.Condition.TimeDay)
			.Add(ItemID.FogMachine,			NPCShop.Condition.TimeNight)
			.Add(ItemID.BubbleWand)
			.Add(ItemID.BeachBall)
			.Add(ItemID.LavaLamp)
			.Add(ItemID.PlasmaLamp)
			.Add(ItemID.FireworksBox)
			.Add(ItemID.FireworkFountain)
			.Add(ItemID.PartyMinecart)																			// Party Wagon
			.Add(ItemID.KiteSpectrum)																			// Spectrum Kite
			.Add(ItemID.PogoStick)
			.Add(ItemID.RedRocket,			NPCShop.Condition.Hardmode)
			.Add(ItemID.GreenRocket,		NPCShop.Condition.Hardmode)
			.Add(ItemID.BlueRocket,			NPCShop.Condition.Hardmode)
			.Add(ItemID.YellowRocket,		NPCShop.Condition.Hardmode)
			.Add(ItemID.PartyGirlGrenade,	NPCShop.Condition.PlayerCarriesItem(ItemID.PartyGirlGrenade))		// Happy Grenade
			.Add(ItemID.ConfettiCannon,		NPCShop.Condition.NpcIsPresent(NPCID.Pirate))
			.Add(ItemID.Bubble,				NPCShop.Condition.Hardmode)
			.Add(ItemID.SmokeBlock,			NPCShop.Condition.Hardmode)
			.Add(ItemID.PartyMonolith)																			// Party Center
			.Add(ItemID.PartyHat)
			.Add(ItemID.SillyBalloonMachine)
			.Add(ItemID.PartyPresent,		NPCShop.Condition.BirthdayPartyIsUp)
			.Add(ItemID.Pigronata,			NPCShop.Condition.BirthdayPartyIsUp)
			.Add(ItemID.SillyStreamerBlue,	NPCShop.Condition.BirthdayPartyIsUp)								// Blue Streamer
			.Add(ItemID.SillyStreamerGreen, NPCShop.Condition.BirthdayPartyIsUp)								// Green Streamer
			.Add(ItemID.SillyStreamerPink,	NPCShop.Condition.BirthdayPartyIsUp)								// Pink Streamer
			.Add(ItemID.SillyBalloonPurple, NPCShop.Condition.BirthdayPartyIsUp)								// Silly Purple Balloon
			.Add(ItemID.SillyBalloonGreen,	NPCShop.Condition.BirthdayPartyIsUp)                              // Silly Green Balloon
			.Add(ItemID.SillyBalloonPink,	NPCShop.Condition.BirthdayPartyIsUp)                              // Silly Pink Balloon
			.Add(ItemID.SillyBalloonTiedGreen, NPCShop.Condition.BirthdayPartyIsUp)							// Silly Tied Balloon (Green)
			.Add(ItemID.SillyBalloonTiedPurple, NPCShop.Condition.BirthdayPartyIsUp)                          // Silly Tied Balloon (Purple)
			.Add(ItemID.SillyBalloonTiedPink, NPCShop.Condition.BirthdayPartyIsUp)                            // Silly Tied Balloon (Pink)
			.Add(ItemID.FireworksLauncher,	NPCShop.Condition.DownedGolem)											// Celebration
			.Add(ItemID.ReleaseDoves,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.ReleaseLantern,		NPCShop.Condition.NightLanternsUp)
			.Add(ItemID.Football,			NPCShop.Condition.GolfScoreOver(500))
			.Register();
	}

	private static void RegisterCyborg() {
		var portalGunStation = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.PlayerCarriesItem2", Lang.GetItemName(ItemID.PortalGun), Lang.GetItemName(ItemID.PortalGunStation)),
			() => Main.LocalPlayer.HasItem(ItemID.PortalGun) || Main.LocalPlayer.HasItem(ItemID.PortalGunStation));

		new NPCShop(NPCID.Cyborg)
			.Add(ItemID.RocketI)
			.Add(ItemID.RocketII,			NPCShop.Condition.BloodMoon)
			.Add(ItemID.RocketIII,			NPCShop.Condition.EclipseOrNight)
			.Add(ItemID.RocketIV,			NPCShop.Condition.Eclipse)
			.Add(ItemID.DryRocket)
			.Add(ItemID.ProximityMineLauncher)
			.Add(ItemID.Nanites)
			.Add(ItemID.ClusterRocketI,		NPCShop.Condition.DownedMartians)
			.Add(ItemID.ClusterRocketII,	NPCShop.Condition.DownedMartians, NPCShop.Condition.EclipseOrBloodMoon)
			.Add(ItemID.CyborgHelmet,		NPCShop.Condition.Halloween)
			.Add(ItemID.CyborgShirt,		NPCShop.Condition.Halloween)
			.Add(ItemID.CyborgPants,		NPCShop.Condition.Halloween)
			.Add(ItemID.HiTekSunglasses,	NPCShop.Condition.DownedMartians)
			.Add(ItemID.NightVisionHelmet,  NPCShop.Condition.DownedMartians)
			.Add(ItemID.PortalGunStation,	portalGunStation)
			.Add(ItemID.EchoBlock,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.SpectreGoggles,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.JimsDrone)
			.Add(ItemID.JimsDroneVisor)
			.Register();
	}

	private static void RegisterPainter() {
		new NPCShop(NPCID.Painter) // Default shop
			.Add(ItemID.Paintbrush)
			.Add(ItemID.PaintRoller)
			.Add(ItemID.PaintScraper)
			.Add(ItemID.RedPaint)
			.Add(ItemID.OrangePaint)
			.Add(ItemID.YellowPaint)
			.Add(ItemID.LimePaint)
			.Add(ItemID.GreenPaint)
			.Add(ItemID.TealPaint)
			.Add(ItemID.CyanPaint)
			.Add(ItemID.SkyBluePaint)
			.Add(ItemID.BluePaint)
			.Add(ItemID.PurplePaint)
			.Add(ItemID.VioletPaint)
			.Add(ItemID.PinkPaint)
			.Add(ItemID.BlackPaint)
			.Add(ItemID.GrayPaint)
			.Add(ItemID.WhitePaint)
			.Add(ItemID.BrownPaint)
			.Add(ItemID.ShadowPaint,		NPCShop.Condition.Hardmode)
			.Add(ItemID.NegativePaint,		NPCShop.Condition.Hardmode)
			.Add(ItemID.GlowPaint,			NPCShop.Condition.InGraveyard)									// Illuminant Coating
			.Add(ItemID.EchoCoating,		NPCShop.Condition.InGraveyard, NPCShop.Condition.DownedPlantera)
			.Register();

		new NPCShop(NPCID.Painter, "Decor") // Decor shop
			.Add(ItemID.Daylight)
			.Add(ItemID.FirstEncounter,		NPCShop.Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.GoodMorning,		NPCShop.Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.UndergroundReward,	NPCShop.Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.ThroughtheWindow,	NPCShop.Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.Purity,				NPCShop.Condition.InShoppingForestBiome)
			.Add(ItemID.DeadlandComesAlive, NPCShop.Condition.InCrimsonBiome)
			.Add(ItemID.LightlessChasms,	NPCShop.Condition.InCorruptBiome)
			.Add(ItemID.TheLandofDeceivingLooks, NPCShop.Condition.InHallowBiome)
			.Add(ItemID.DoNotStepontheGrass, NPCShop.Condition.InJungleBiome)
			.Add(ItemID.ColdWatersintheWhiteLand, NPCShop.Condition.InSnowBiome)
			.Add(ItemID.SecretoftheSands,	NPCShop.Condition.InDesertBiome)
			.Add(ItemID.EvilPresence,		NPCShop.Condition.BloodMoon)
			.Add(ItemID.PlaceAbovetheClouds,NPCShop.Condition.InSpace)
			.Add(ItemID.SkyGuardian,		NPCShop.Condition.Hardmode, NPCShop.Condition.InSpace)
			.Add(ItemID.Thunderbolt,		NPCShop.Condition.Thunderstorm)
			.Add(ItemID.Nevermore,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.Reborn,				NPCShop.Condition.InGraveyard)
			.Add(ItemID.Graveyard,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.GhostManifestation, NPCShop.Condition.InGraveyard)
			.Add(ItemID.WickedUndead,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.HailtotheKing,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.BloodyGoblet,		NPCShop.Condition.InGraveyard)
			.Add(ItemID.StillLife,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.ChristmasTreeWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.CandyCaneWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.StarsWallpaper,		NPCShop.Condition.Christmas)
			.Add(ItemID.SnowflakeWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.BluegreenWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.OrnamentWallpaper,  NPCShop.Condition.Christmas)
			.Add(ItemID.FestiveWallpaper,	NPCShop.Condition.Christmas)
			.Add(ItemID.SquigglesWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.KrampusHornWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.GrinchFingerWallpaper, NPCShop.Condition.Christmas)
			.Add(ItemID.BubbleWallpaper)
			.Add(ItemID.CopperPipeWallpaper)
			.Add(ItemID.DuckyWallpaper)
			.Add(ItemID.FancyGreyWallpaper)
			.Add(ItemID.IceFloeWallpaper)
			.Add(ItemID.MusicWallpaper)
			.Add(ItemID.PurpleRainWallpaper)
			.Add(ItemID.RainbowWallpaper)
			.Add(ItemID.SparkleStoneWallpaper)
			.Add(ItemID.StarlitHeavenWallpaper)
			.Register();
	}

	private static void RegisterWitchDoctor() {
		new NPCShop(NPCID.WitchDoctor)
			.Add(ItemID.ImbuingStation)
			.Add(ItemID.Blowgun)
			.Add(ItemID.StyngerBolt,		NPCShop.Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Stake,				NPCShop.Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.Cauldron,			NPCShop.Condition.Halloween)
			.Add(ItemID.TikiTotem,			NPCShop.Condition.Hardmode, NPCShop.Condition.InJungleBiome)
			.Add(ItemID.LeafWings,			NPCShop.Condition.Hardmode, NPCShop.Condition.InJungleBiome, NPCShop.Condition.TimeNight, NPCShop.Condition.DownedPlantera)
			.Add(ItemID.VialofVenom,		NPCShop.Condition.DownedPlantera)
			.Add(ItemID.TikiMask,			NPCShop.Condition.DownedPlantera)
			.Add(ItemID.TikiShirt,			NPCShop.Condition.DownedPlantera)
			.Add(ItemID.TikiPants,			NPCShop.Condition.DownedPlantera)
			.Add(ItemID.PygmyNecklace,		NPCShop.Condition.TimeNight)
			.Add(ItemID.HerculesBeetle,		NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPlantera, NPCShop.Condition.InJungleBiome)
			.Add(ItemID.PureWaterFountain)
			.Add(ItemID.DesertWaterFountain)
			.Add(ItemID.JungleWaterFountain)
			.Add(ItemID.IcyWaterFountain)
			.Add(ItemID.CorruptWaterFountain)
			.Add(ItemID.CrimsonWaterFountain)
			.Add(ItemID.HallowedWaterFountain)
			.Add(ItemID.BloodWaterFountain)
			.Add(ItemID.CavernFountain)																			// Cavern Water Fountain
			.Add(ItemID.OasisFountain)																			// Oasis Water Fountain
			.Add(ItemID.BewitchingTable,	NPCShop.Condition.NpcIsPresent(NPCID.Wizard))
			.Register();
	}

	private static void RegisterPirate() {
		new NPCShop(NPCID.Pirate)
			.Add(ItemID.Cannon)
			.Add(ItemID.Cannonball)
			.Add(ItemID.PirateHat)
			.Add(ItemID.PirateShirt)
			.Add(ItemID.PiratePants)
			.Add(ItemID.Sail)
			.Add(ItemID.ParrotCracker,		new NPCShop.Condition(NetworkText.FromLiteral("RecipeConditions.InBeach"), () => {
				int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
				return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
			}))
			.Add(ItemID.BunnyCannon,		NPCShop.Condition.NpcIsPresent(NPCID.PartyGirl),
											NPCShop.Condition.Hardmode,
											NPCShop.Condition.DownedMechBossAny)
			.Register();
	}

	private static void RegisterStylist() {
		var maxLife = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.AtleastXHealth", 400), () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
		var maxMana = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.AtleastXHealth", 200), () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
		var moneyHair = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.PlatinumCoin"), () => {
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
		var timeHair = new NPCShop.Condition(NetworkText.FromKey("ShopCondition.StyleMoon"), () => Main.moonPhase % 2 == (!Main.dayTime).ToInt());
		var teamHair = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.OnTeam"), () => Main.LocalPlayer.team != 0);
		var partyHair = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.NpcIsPresent", Lang.GetNPCName(NPCID.PartyGirl)), () => NPC.AnyNPCs(NPCID.PartyGirl));

		new NPCShop(NPCID.Stylist)
			.Add(ItemID.WilsonBeardShort)																		// Gentleman's Beard
			.Add(ItemID.HairDyeRemover)
			.Add(ItemID.DepthHairDye)
			.Add(ItemID.LifeHairDye,		maxLife)
			.Add(ItemID.ManaHairDye,		maxMana)
			.Add(ItemID.MoneyHairDye,		moneyHair)
			.Add(ItemID.TimeHairDye,		timeHair)
			.Add(ItemID.TeamHairDye,		teamHair)
			.Add(ItemID.PartyHairDye,		partyHair)
			.Add(ItemID.BiomeHairDye,		NPCShop.Condition.Hardmode)
			.Add(ItemID.SpeedHairDye,		NPCShop.Condition.Hardmode, NPCShop.Condition.DownedMechBossAny)
			.Add(ItemID.RainbowHairDye,		NPCShop.Condition.Hardmode, NPCShop.Condition.DownedTwins, NPCShop.Condition.DownedSkeletronPrime, NPCShop.Condition.DownedDestroyer)
			.Add(ItemID.MartianHairDye,		NPCShop.Condition.DownedMartians)
			.Add(ItemID.TwilightHairDye,	NPCShop.Condition.DownedMartians)
			.Register();
	}

	private static void RegisterSkeletonMerchant() {
		NPCShop.Condition[] wandOfSparkingCondition = { NPCShop.Condition.IsMoonThirdQuarter, NPCShop.Condition.NotRemixWorld };
		NPCShop.Condition[] magicDaggerCondition = { NPCShop.Condition.IsMoonThirdQuarter, NPCShop.Condition.RemixWorld };
		var spelunkerGlowCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.NightDayFullMoon"), () => !Main.dayTime || Main.moonPhase == 0);
		var glowstickCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.DaytimeSkinnyMoon"), () => Main.dayTime && Main.moonPhase != 0);
		var boneTorchCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.FirstHalfSecond"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
		var torchCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.SecondHalfSecond"), () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0);
		var artisanCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.NoAteLoaf"), () => !Main.LocalPlayer.ateArtisanBread);

		new NPCShop(NPCID.SkeletonMerchant)
			.Add(ItemID.WoodenBoomerang,	NPCShop.Condition.IsMoonFull)
			.Add(ItemID.Umbrella,			NPCShop.Condition.IsMoonWaningGibbous)
			.Add(ItemID.WandofSparking,		wandOfSparkingCondition)
			.Add(ItemID.MagicDagger,		magicDaggerCondition)
			.Add(ItemID.PortableStool,		NPCShop.Condition.IsMoonWaningCrescent)							// Step Stool
			.Add(ItemID.Aglet,				NPCShop.Condition.IsMoonNew)
			.Add(ItemID.ClimbingClaws,		NPCShop.Condition.IsMoonWaxingCrescent)
			.Add(ItemID.CordageGuide,		NPCShop.Condition.IsMoonFirstQuarter)								// Guide to Plant Fiber Cordage
			.Add(ItemID.Radar,				NPCShop.Condition.IsMoonWaxingGibbous)
			.Add(ItemID.StrangeBrew,		NPCShop.Condition.IsMoonPhasesEven)
			.Add(ItemID.LesserHealingPotion, NPCShop.Condition.IsMoonPhasesOdd)
			.Add(ItemID.HealingPotion,		NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesOdd)
			.Add(ItemID.SpelunkerGlowstick, spelunkerGlowCondition)
			.Add(ItemID.SpelunkerFlare,		spelunkerGlowCondition, NPCShop.Condition.PlayerCarriesItem(ItemID.FlareGun))
			.Add(ItemID.Glowstick,			glowstickCondition)
			.Add(ItemID.BoneTorch,			boneTorchCondition)
			.Add(ItemID.Torch,				torchCondition)
			.Add(ItemID.BoneArrow,			NPCShop.Condition.IsMoonPhasesEvenQuarters)
			.Add(ItemID.WoodenArrow,		NPCShop.Condition.IsMoonPhasesOddQuarters)
			.Add(ItemID.BlueCounterweight,	NPCShop.Condition.IsMoonPhases04)
			.Add(ItemID.RedCounterweight,	NPCShop.Condition.IsMoonPhases15)
			.Add(ItemID.PurpleCounterweight, NPCShop.Condition.IsMoonPhases26)
			.Add(ItemID.GreenCounterweight, NPCShop.Condition.IsMoonPhases37)
			.Add(ItemID.Bomb)
			.Add(ItemID.Rope)
			.Add(ItemID.Gradient,			NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesHalf0)
			.Add(ItemID.FormatC,			NPCShop.Condition.Hardmode, NPCShop.Condition.IsMoonPhasesHalf1)
			.Add(ItemID.YoYoGlove,			NPCShop.Condition.Hardmode)
			.Add(ItemID.SlapHand,			NPCShop.Condition.Hardmode, NPCShop.Condition.BloodMoon)
			.Add(ItemID.MagicLantern,		NPCShop.Condition.TimeNight, NPCShop.Condition.IsMoonFull)
			.Add(ItemID.ArtisanLoaf,		artisanCondition, NPCShop.Condition.IsMoonPhasesNearNew)
			.Register();
	}

	private static void RegisterBartender()
	{
		// Woah! Whole nothing here!
		// Look in ModLoader.Default.BartenderShopNPC for actual implementation.
		var shop = new NPCShop(NPCID.DD2Bartender);

		static Item MakeItem(int id, int currencyPrice) =>
			new(id) { shopCustomPrice = currencyPrice, shopSpecialCurrency = CustomCurrencyID.DefenderMedals };

		// 1st row
		shop.Add(ItemID.Ale);
		shop.Add(ItemID.DD2ElderCrystal);                                                                                   // Eternia Crystal
		shop.Add(ItemID.DD2ElderCrystalStand);                                                                              // Eternia Crystal Stand
		shop.Add(MakeItem(ItemID.DefendersForge, 50));

		shop.Add(new Entry(MakeItem(ItemID.SquireGreatHelm, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquirePlating, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquireGreaves, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquireAltHead, 50), Condition.DownedGolem).ReserveSlot());                       // Valhalla Knight's Helm
		shop.Add(new Entry(MakeItem(ItemID.SquireAltShirt, 50), Condition.DownedGolem).ReserveSlot());                      // Valhalla Knight's Breastplate
		shop.Add(new Entry(MakeItem(ItemID.SquireAltPants, 50), Condition.DownedGolem).ReserveSlot());                      // Valhalla Knight's Greaves

		// 2nd row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT1Popper, 5)).ReserveSlot());                                  // Flameburst Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT1Popper, 5)).ReserveSlot());                                   // Ballista Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT1Popper, 5)).ReserveSlot());                                    // Explosive Trap Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT1Popper, 5)).ReserveSlot());                                    // Lightning Aura Rod
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeHat, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeRobe, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeTrousers, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltHead, 50), Condition.DownedGolem).ReserveSlot());                   // Dark Atrist's Hat
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltShirt, 50), Condition.DownedGolem).ReserveSlot());                  // Dark Atrist's Robes
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltPants, 50), Condition.DownedGolem).ReserveSlot());                  // Dark Atrist's Leggings

		// 3rd row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT2Popper, 15), Condition.DownedMechBossAny).ReserveSlot());    // Flameburst Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT2Popper, 15), Condition.DownedMechBossAny).ReserveSlot());    // Ballista Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT2Popper, 15), Condition.DownedMechBossAny).ReserveSlot());    // Explosive Trap Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT2Popper, 15), Condition.DownedMechBossAny).ReserveSlot());    // Lightning Aura Cane
		shop.Add(new Entry(MakeItem(ItemID.HuntressWig, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressJerkin, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressPants, 15), Condition.DownedMechBossAny).ReserveSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltHead, 50), Condition.DownedGolem).ReserveSlot());                     // Red Riding Hood
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltShirt, 50), Condition.DownedGolem).ReserveSlot());                    // Red Riding Dress
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltPants, 50), Condition.DownedGolem).ReserveSlot());                    // Red Riding Leggings

		// 4th row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT3Popper, 60), Condition.DownedGolem).ReserveSlot());          // Flameburst Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT3Popper, 60), Condition.DownedGolem).ReserveSlot());           // Ballista Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT3Popper, 60), Condition.DownedGolem).ReserveSlot());          // Explosive Trap Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT3Popper, 60), Condition.DownedGolem).ReserveSlot());          // Lightning Aura Staff
		shop.Add(new Entry(MakeItem(ItemID.MonkBrows, 15), Condition.DownedMechBossAny).ReserveSlot());                // Monk's Bushy Brow Bald Cap
		shop.Add(new Entry(MakeItem(ItemID.MonkShirt, 15), Condition.DownedMechBossAny).ReserveSlot());             // Monk's Shirt
		shop.Add(new Entry(MakeItem(ItemID.MonkPants, 15), Condition.DownedMechBossAny).ReserveSlot());             // Monk's Pants
		shop.Add(new Entry(MakeItem(ItemID.MonkAltHead, 50), Condition.DownedGolem).ReserveSlot());                         // Shinobi Infiltrator's Helmet
		shop.Add(new Entry(MakeItem(ItemID.MonkAltShirt, 50), Condition.DownedGolem).ReserveSlot());                        // Shinobi Infiltrator's Torso
		shop.Add(new Entry(MakeItem(ItemID.MonkAltPants, 50), Condition.DownedGolem).ReserveSlot());                        // Shinobi Infiltrator's Pants

		shop.Register();
	}

	private static void RegisterGolfer() {
		var scoreOver500 = NPCShop.Condition.GolfScoreOver(500);
		var scoreOver1000 = NPCShop.Condition.GolfScoreOver(1000);
		var scoreOver2000 = NPCShop.Condition.GolfScoreOver(2000);

		new NPCShop(NPCID.Golfer)
			.Add(ItemID.GolfClubStoneIron)                                                                      // Worn Golf Club (Iron)
			.Add(ItemID.GolfClubWoodDriver)                                                                     // Worn Golf Club (Driver)
			.Add(ItemID.GolfClubBronzeWedge)                                                                    // Worn Golf Club (Wedge)
			.Add(ItemID.GolfClubRustyPutter)                                                                    // Worn Golf Club (Putter)
			.Add(ItemID.GolfCupFlagWhite)                                                                       // White Pin Flag
			.Add(ItemID.GolfCupFlagRed)																			// Red Pin Flag
			.Add(ItemID.GolfCupFlagGreen)                                                                       // Green Pin Flag
			.Add(ItemID.GolfCupFlagBlue)																		// Blue Pin Flag
			.Add(ItemID.GolfCupFlagYellow)																		// Yellow Pin Flag
			.Add(ItemID.GolfCupFlagPurple)                                                                      // Purple Pin Flag
			.Add(ItemID.GolfTee)
			.Add(ItemID.GolfBall)
			.Add(ItemID.GolfWhistle)
			.Add(ItemID.GolfCup)
			.Add(ItemID.ArrowSign)
			.Add(ItemID.PaintedArrowSign)
			.Add(ItemID.GolfHat)																				// Country Club Cap
			.Add(ItemID.GolfVisor)                                                                              // Country Club Visor
			.Add(ItemID.GolfShirt)                                                                              // Country Club Vest
			.Add(ItemID.GolfPants)                                                                              // Country Club Trousers
			.Add(ItemID.LawnMower)
			.Add(ItemID.GolfCart,			scoreOver2000, NPCShop.Condition.DownedSkeletron)					// Golf Cart Keys
			.Add(ItemID.GolfPainting1,		scoreOver2000, NPCShop.Condition.IsMoonPhasesQuarter0)				// The Rolling Greens
			.Add(ItemID.GolfPainting2,		scoreOver2000, NPCShop.Condition.IsMoonPhasesQuarter1)				// Study of a Ball at Rest
			.Add(ItemID.GolfPainting3,		scoreOver2000, NPCShop.Condition.IsMoonPhasesQuarter2)				// Fore!
			.Add(ItemID.GolfPainting4,		scoreOver2000, NPCShop.Condition.IsMoonPhasesQuarter3)				// The Duplicity of Reflections
			.Add(ItemID.GolfClubIron,		scoreOver500)                                                       // Golf Club (Iron)
			.Add(ItemID.GolfClubDriver,		scoreOver500)                                                       // Golf Club (Driver)
			.Add(ItemID.GolfClubWedge,		scoreOver500)                                                       // Golf Club (Wedge)
			.Add(ItemID.GolfClubPutter,		scoreOver500)                                                       // Golf Club (Putter)
			.Add(ItemID.GolfChest,			scoreOver500)
			.Add(ItemID.GolfTrophyBronze,	scoreOver500)														// Bronze Golf Trophy
			.Add(ItemID.GolfClubMythrilIron,scoreOver1000)                                                      // Fancy Golf Club (Iron)
			.Add(ItemID.GolfClubPearlwoodDriver, scoreOver1000)                                                 // Fancy Golf Club (Driver)
			.Add(ItemID.GolfClubGoldWedge,	scoreOver1000)                                                      // Fancy Golf Club (Wedge)
			.Add(ItemID.GolfClubLeadPutter, scoreOver1000)                                                      // Fancy Golf Club (Putter)
			.Add(ItemID.GolfTrophySilver,	scoreOver1000)														// Silver Golf Trophy
			.Add(ItemID.GolfClubTitaniumIron,scoreOver2000)                                                     // Premium Golf Club (Iron)
			.Add(ItemID.GolfClubChlorophyteDriver, scoreOver2000)                                               // Premium Golf Club (Driver)
			.Add(ItemID.GolfClubDiamondWedge, scoreOver2000)                                                    // Premium Golf Club (Wedge)
			.Add(ItemID.GolfClubShroomitePutter, scoreOver2000)                                                 // Premium Golf Club (Putter)
			.Add(ItemID.GolfTrophyGold,		scoreOver2000)														// Gold Golf Trophy
			.Register();
	}

	private static void RegisterZoologist() {
		var fairyGlowstick = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryWinx"), () => Chest.BestiaryGirl_IsFairyTorchAvailable());
		var solarPillarDead = NPCShop.Condition.DownedSolarPillar;

		var moonIsFullOrWaningGibbous = NPCShop.Condition.IsMoonPhasesQuarter0;
		var moonIsThirdOrWaningCrescent = NPCShop.Condition.IsMoonPhasesQuarter1;
		var moonIsNewOrWaxingCrescent = NPCShop.Condition.IsMoonPhasesQuarter2;
		var moonIsFirstOrWaxingGibbous = NPCShop.Condition.IsMoonPhasesQuarter3;

		var bestiaryFilledBy10 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 10), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
		var bestiaryFilledBy25 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 25), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
		var bestiaryFilledBy30 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 30), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
		var bestiaryFilledBy35 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 35), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.35f);
		var bestiaryFilledBy40 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 40), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f);
		var bestiaryFilledBy45 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 45), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f);
		var bestiaryFilledBy50 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 50), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
		var bestiaryFilledBy70 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 70), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f);
		var bestiaryFilledBy100 = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.BestiaryFull"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);

		new NPCShop(NPCID.BestiaryGirl)
			.Add(ItemID.DontHurtCrittersBook)																	// Guide to Critter Companionship
			.Add(ItemID.SquirrelHook)
			.Add(ItemID.TheWerewolf,		NPCShop.Condition.IsMoonFull)
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
			.Add(ItemID.FullMoonSqueakyToy, NPCShop.Condition.Hardmode, NPCShop.Condition.BloodMoon)
			.Add(ItemID.MudBud,				NPCShop.Condition.DownedPlantera)
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
			.Register();
	}

	private static void RegisterPrincess() {
		var goodsCondition = new NPCShop.Condition(NetworkText.FromKey("ShopConditions.InCelebrationMk10"), () => Main.tenthAnniversaryWorld);

		var shop = new NPCShop(NPCID.Princess)
			.Add(ItemID.RoyalTiara)
			.Add(ItemID.RoyalDressTop)																			// Royal Blouse
			.Add(ItemID.RoyalDressBottom);																		// Royal Dress
		for (int i = 5076; i <= 5087; i++) {
			shop.Add(i);
		}
		shop.Add(ItemID.PrincessStyle)
			.Add(ItemID.SuspiciouslySparkly)
			.Add(ItemID.TerraBladeChronicles)
			.Add(ItemID.BerniePetItem)                                                                          // Bernie's Button
			.Add(ItemID.RoyalRomance,		NPCShop.Condition.DownedKingSlime, NPCShop.Condition.DownedKingSlime)
			.Add(ItemID.MusicBoxCredits,	NPCShop.Condition.Hardmode, NPCShop.Condition.DownedMoonLord)   // Music Box (Journey's End)
			.Add(ItemID.SlimeStaff,			goodsCondition)
			.Add(ItemID.HeartLantern,		goodsCondition)
			.Add(ItemID.FlaskofParty,		goodsCondition)
			.Add(ItemID.SandstorminaBottle, goodsCondition, NPCShop.Condition.InDesertBiome)
			.Add(ItemID.Terragrim,			goodsCondition, NPCShop.Condition.BloodMoon)
			.Add(ItemID.PirateStaff,		goodsCondition, NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPirates, NPCShop.Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.DiscountCard,		goodsCondition, NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPirates, NPCShop.Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.LuckyCoin,			goodsCondition, NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPirates, NPCShop.Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.CoinGun,			goodsCondition, NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPirates, NPCShop.Condition.IsMoonPhasesQuarter3)
			.Register();
	}

	internal static void SortAllShops()
	{
		foreach (var shop in AllShops) {
			shop.Sort();
		}
	}
}