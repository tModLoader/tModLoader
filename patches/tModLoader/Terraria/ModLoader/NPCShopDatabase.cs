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

	internal static void AddShop(NPCShop shop)
	{
		npcShopByName.Add(shop.FullName, shop);
	}

	public static NPCShop GetNPCShop(string fullName)
	{
		if (npcShopByName.TryGetValue(fullName, out NPCShop shop))
			return shop;

		return null;
	}

	/// <summary>
	/// Gets a shop name (identifier) in the format matching <see cref="FullName"/> <br/>
	/// Can be used with <see cref="GetNPCShop(string)"/> and <see cref="GlobalNPC.ModifyActiveShop(NPC, string, Item[])"/>
	/// </summary>
	/// <param name="npcType"></param>
	/// <param name="shopName"></param>
	/// <returns></returns>
	public static string GetShopName(int npcType, string shopName = "Shop")
	{
		return $"{(npcType < NPCID.Count ? $"Terraria/{NPCID.Search.GetName(npcType)}" : NPCLoader.GetNPC(npcType).FullName)}/{shopName}";
	}

	public static string GetShopNameFromVanillaIndex(int index)
	{
		int npcType = NPCLoader.shopToNPC[index];
		if (index == 25) { // Painter 2 Shop Special Case
			return GetShopName(npcType, "Decor");
		}
		return GetShopName(npcType);
	}

	public static void Initialize()
	{
		npcShopByName.Clear();
		NoPylons.Clear();

		NPCShops();
	}

	public static void NPCShops()
	{
		NoPylons[GetShopName(NPCID.SkeletonMerchant)] = true;
		NoPylons[GetShopName(NPCID.DD2Bartender)] = true;

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

	public static Entry[] GetVanillaPylonEntries()
	{
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
		var pylonMainCondition = new Condition(NetworkText.FromKey("ShopConditions.HappyEnoughForPylons"), () =>
			Main.LocalPlayer.talkNPC != -1 && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[19] && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[20]
			&& (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421 || Main.remixWorld)
			&& TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16())
			&& !Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneCrimson
			);
		var purityPylonCondition = new Condition(NetworkText.FromKey("ShopConditions.InPurity"), () => { // im having struggles localizing these
			if (Main.remixWorld) {
				return Main.LocalPlayer.Center.Y / 16.0 > Main.rockLayer && Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 350;
			}
			return Main.LocalPlayer.Center.Y / 16.0 < Main.worldSurface;
		});
		var cavernPylonCondition = new Condition(NetworkText.FromKey("ShopConditions.InRockLayerHeight"), () => {
			return !Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneDesert
			&& !Main.LocalPlayer.ZoneBeach && !Main.LocalPlayer.ZoneJungle
			&& !Main.LocalPlayer.ZoneHallow && (!Main.remixWorld || !Main.LocalPlayer.ZoneGlowshroom)
			&& (double)(Main.LocalPlayer.Center.Y / 16f) >= Main.worldSurface;
		});
		var oceanPylonCondition = new Condition(NetworkText.FromKey("RecipeConditions.InBeach"), () => {
			bool flag4 = Main.LocalPlayer.ZoneBeach && Main.LocalPlayer.position.Y < Main.worldSurface * 16.0;
			if (Main.remixWorld) {
				double num13 = Main.LocalPlayer.position.X / 16.0;
				double num14 = Main.LocalPlayer.position.Y / 16.0;
				flag4 |= (num13 < Main.maxTilesX * 0.43 || num13 > Main.maxTilesX * 0.57) && num14 > Main.rockLayer && num14 < Main.maxTilesY - 350;
			}
			return flag4;
		});
		var glowshroomPylonCondtiion = new Condition(NetworkText.FromKey("ShopConditions.UndergroundOrRemix"),
			() => !Main.remixWorld || Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 200);

		return new Entry[] {
			new(ItemID.TeleportationPylonPurity,        pylonMainCondition, Condition.InPurityBiome, purityPylonCondition),
			new(ItemID.TeleportationPylonSnow,          pylonMainCondition, Condition.InSnowBiome),
			new(ItemID.TeleportationPylonDesert,        pylonMainCondition, Condition.InDesertBiome),
			new(ItemID.TeleportationPylonUnderground,   pylonMainCondition, cavernPylonCondition),
			new(ItemID.TeleportationPylonOcean,         pylonMainCondition, oceanPylonCondition),
			new(ItemID.TeleportationPylonJungle,        pylonMainCondition, Condition.InJungleBiome),
			new(ItemID.TeleportationPylonHallow,        pylonMainCondition, Condition.InHallowBiome),
			new(ItemID.TeleportationPylonMushroom,      pylonMainCondition, Condition.InGlowshroomBiome, glowshroomPylonCondtiion)
		};
	}

	private static void RegisterMerchant()
	{
		var flareGunCondition = Condition.PlayerCarriesItem(ItemID.FlareGun);
		var drumSetCondition = new Condition(NetworkText.FromKey("ShopConditions.DownedB2B3HM"), () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
		
		new NPCShop(NPCID.Merchant)
			.Add(ItemID.MiningHelmet)
			.Add(ItemID.PiggyBank)
			.Add(ItemID.IronAnvil)
			.Add(ItemID.BugNet)
			.Add(ItemID.CopperPickaxe)
			.Add(ItemID.CopperAxe)
			.Add(ItemID.Torch)
			.Add(ItemID.LesserHealingPotion)
			.Add(ItemID.HealingPotion,		Condition.Hardmode)
			.Add(ItemID.LesserManaPotion)
			.Add(ItemID.ManaPotion,			Condition.Hardmode)
			.Add(ItemID.WoodenArrow)
			.Add(ItemID.Shuriken)
			.Add(ItemID.Rope)
			.Add(ItemID.Marshmallow,		Condition.InSnowBiome)
			.Add(ItemID.Furnace,			Condition.InJungleBiome)
			.Add(ItemID.PinWheel,			Condition.TimeDay, Condition.HappyWindyDay)
			.Add(ItemID.ThrowingKnife,		Condition.BloodMoon)
			.Add(ItemID.Glowstick,			Condition.TimeNight)
			.Add(ItemID.SharpeningStation,	Condition.Hardmode)
			.Add(ItemID.Safe,				Condition.DownedSkeletron)
			.Add(ItemID.DiscoBall,			Condition.Hardmode)
			.Add(ItemID.Flare,				flareGunCondition)
			.Add(ItemID.BlueFlare,			flareGunCondition)
			.Add(ItemID.Sickle)
			.Add(ItemID.GoldDust,			Condition.Hardmode)
			.Add(ItemID.DrumSet,			drumSetCondition)
			.Add(ItemID.DrumStick,			drumSetCondition)
			.Add(ItemID.NailGun,			Condition.PlayerCarriesItem(ItemID.NailGun))
			.Register();
	}

	private static void RegisterArmsDealer()
	{
		var silverBulletCondition = new Condition(NetworkText.FromKey("ShopConditions.BloodOrHMInSilver"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Silver);
		var tungstenBulletCondition = new Condition(NetworkText.FromKey("ShopConditions.BloodOrHMInTungst"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Tungsten);
		var unholyArrowCondition = new Condition(NetworkText.FromKey("ShopConditions.NightAfterEvil"), () => (NPC.downedBoss2 && !Main.dayTime) || Main.hardMode);

		new NPCShop(NPCID.ArmsDealer)
			.Add(ItemID.MusketBall)
			.Add(ItemID.SilverBullet,		silverBulletCondition)
			.Add(ItemID.TungstenBullet,		tungstenBulletCondition)
			.Add(ItemID.UnholyArrow,		unholyArrowCondition)
			.Add(ItemID.FlintlockPistol)
			.Add(ItemID.Minishark)
			.Add(ItemID.IllegalGunParts,	Condition.TimeNight)
			.Add(ItemID.AmmoBox,			Condition.Hardmode)
			.Add(ItemID.Shotgun,			Condition.Hardmode)
			.Add(ItemID.EmptyBullet,		Condition.Hardmode)
			.Add(ItemID.StyngerBolt,		Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Stake,				Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.Nail,				Condition.PlayerCarriesItem(ItemID.NailGun))
			.Add(ItemID.CandyCorn,			Condition.PlayerCarriesItem(ItemID.CandyCornRifle))
			.Add(ItemID.ExplosiveJackOLantern, Condition.PlayerCarriesItem(ItemID.JackOLanternLauncher))
			.Add(ItemID.NurseHat,			Condition.Halloween)
			.Add(ItemID.NurseShirt,			Condition.Halloween)
			.Add(ItemID.NursePants,			Condition.Halloween)
			.Add(ItemID.QuadBarrelShotgun,	Condition.InGraveyard, Condition.DownedSkeletron)
			.Register();
	}

	private static void RegisterDryad()
	{
		var shop = new NPCShop(NPCID.Dryad);
		var corruptSeedsCondition = new Condition(NetworkText.FromKey("ShopConditions.CorruOrGravCrim"), () => !WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && WorldGen.crimson);
		var crimsonSeedsCondition = new Condition(NetworkText.FromKey("ShopConditions.CrimOrGravCorru"), () => WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && !WorldGen.crimson);

		shop.Add(ItemID.VilePowder,			Condition.BloodMoon, Condition.CrimsonWorld, Condition.NotRemixWorld)
			.Add(ItemID.ViciousPowder,		Condition.BloodMoon, Condition.CorruptionWorld, Condition.NotRemixWorld)
			.Add(ItemID.PurificationPowder, Condition.NotBloodMoon, Condition.NotRemixWorld)
			.Add(ItemID.GrassSeeds,			Condition.NotBloodMoon)
			.Add(ItemID.AshGrassSeeds,		Condition.InUnderworld)
			.Add(ItemID.CorruptSeeds,		Condition.BloodMoon, corruptSeedsCondition)
			.Add(ItemID.CrimsonSeeds,		Condition.BloodMoon, crimsonSeedsCondition)
			.Add(ItemID.Sunflower,			Condition.NotBloodMoon)
			.Add(ItemID.Acorn)
			.Add(ItemID.DontHurtNatureBook)																	// Guide to Environmental Preservation
			.Add(ItemID.DirtRod)
			.Add(ItemID.PumpkinSeed)
			.Add(ItemID.CorruptGrassEcho,	Condition.BloodMoon, Condition.CorruptionWorld) // Crimosn Grass Wall
			.Add(ItemID.CrimsonGrassEcho,	Condition.BloodMoon, Condition.CrimsonWorld)    // Corrupt Grass Wall
			.Add(ItemID.GrassWall,			Condition.NotBloodMoon)
			.Add(ItemID.FlowerWall)
			.Add(ItemID.JungleWall,			Condition.Hardmode)
			.Add(ItemID.HallowedSeeds,		Condition.Hardmode)
			.Add(ItemID.HallowedGrassEcho,	Condition.Hardmode)										// Hallowed Grass Wall
			.Add(ItemID.MushroomGrassSeeds, Condition.InGlowshroomBiome)
			.Add(ItemID.DryadCoverings,		Condition.Halloween)
			.Add(ItemID.DryadLoincloth,		Condition.Halloween)
			.Add(ItemID.DayBloomPlanterBox,	Condition.DownedKingSlime)
			.Add(ItemID.MoonglowPlanterBox, Condition.DownedQueenBee)
			.Add(ItemID.BlinkrootPlanterBox, Condition.DownedEyeOfCthulhu)
			.Add(ItemID.PotSuspendedDeathweedCorrupt, Condition.DownedEaterOfWorlds)
			.Add(ItemID.PotSuspendedDeathweedCrimson, Condition.DownedBrainOfCthulhu)
			.Add(ItemID.WaterleafPlanterBox, Condition.DownedSkeletron)
			.Add(ItemID.ShiverthornPlanterBox, Condition.DownedSkeletron)
			.Add(ItemID.FireBlossomPlanterBox, Condition.Hardmode)
			.Add(ItemID.FlowerPacketWhite)																	// White Flower Seeds
			.Add(ItemID.FlowerPacketYellow)																	// Yellows Flower Seeds
			.Add(ItemID.FlowerPacketRed)																	// Red Flower Seeds
			.Add(ItemID.FlowerPacketPink)																	// Pink Flower Seeds
			.Add(ItemID.FlowerPacketMagenta)																// Magenta Flower Seeds
			.Add(ItemID.FlowerPacketViolet)																	// Violet Flower Seeds
			.Add(ItemID.FlowerPacketBlue)																	// Blue Flower Seeds
			.Add(ItemID.FlowerPacketWild)																	// Wild Flower Seeds
			.Add(ItemID.FlowerPacketTallGrass)																// Tall Grass Seeds
			.Add(ItemID.PottedForestCedar,	Condition.Hardmode, Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedJungleCedar,	Condition.Hardmode, Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedHallowCedar,	Condition.Hardmode, Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.PottedForestTree,	Condition.Hardmode, Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedJungleTree,	Condition.Hardmode, Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedHallowTree,	Condition.Hardmode, Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.PottedForestPalm,	Condition.Hardmode, Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedJunglePalm,	Condition.Hardmode, Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedHallowPalm,	Condition.Hardmode, Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.PottedForestBamboo, Condition.Hardmode, Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.PottedJungleBamboo, Condition.Hardmode, Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.PottedHallowBamboo, Condition.Hardmode, Condition.IsMoonPhasesQuarter3)
			.Register();
	}

	private static void RegisterBombGuy()
	{
		new NPCShop(NPCID.Demolitionist)
			.Add(ItemID.Grenade)
			.Add(ItemID.Bomb)
			.Add(ItemID.Dynamite)
			.Add(ItemID.HellfireArrow,		Condition.Hardmode)
			.Add(ItemID.LandMine,			Condition.Hardmode, Condition.DownedPlantera, Condition.DownedPirates)
			.Add(ItemID.ExplosivePowder,	Condition.Hardmode)
			.Add(ItemID.DryBomb,			Condition.PlayerCarriesItem(ItemID.DryBomb))
			.Add(ItemID.WetBomb,			Condition.PlayerCarriesItem(ItemID.WetBomb))
			.Add(ItemID.LavaBomb,			Condition.PlayerCarriesItem(ItemID.LavaBomb))
			.Add(ItemID.HoneyBomb,			Condition.PlayerCarriesItem(ItemID.HoneyBomb))
			.Register();
	}

	private static void RegisterClothier()
	{
		var taxCollectorPresent = Condition.NpcIsPresent(NPCID.TaxCollector);

		new NPCShop(NPCID.Clothier)
			.Add(ItemID.BlackThread)
			.Add(ItemID.PinkThread)
			.Add(ItemID.PlacePainting)																		// r/Terraria
			.Add(ItemID.SummerHat,			Condition.TimeDay)
			.Add(ItemID.PlumbersShirt,		Condition.IsMoonFull)
			.Add(ItemID.PlumbersPants,		Condition.IsMoonFull)
			.Add(ItemID.WhiteTuxedoShirt,	Condition.IsMoonFull, Condition.TimeNight)
			.Add(ItemID.WhiteTuxedoPants,	Condition.IsMoonFull, Condition.TimeNight)
			.Add(ItemID.TheDoctorsShirt,	Condition.IsMoonWaningGibbous)
			.Add(ItemID.TheDoctorsPants,	Condition.IsMoonWaningGibbous)
			.Add(ItemID.FamiliarShirt)
			.Add(ItemID.FamiliarPants)
			.Add(ItemID.FamiliarWig)
			.Add(ItemID.ClownHat,			Condition.DownedClown)
			.Add(ItemID.ClownShirt,			Condition.DownedClown)
			.Add(ItemID.ClownPants,			Condition.DownedClown)
			.Add(ItemID.MimeMask,			Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoShirt,	Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoPants,	Condition.BloodMoon)
			.Add(ItemID.WhiteLunaticHood,	Condition.TimeDay, Condition.DownedCultist)		// Solar Lunatic Hood
			.Add(ItemID.WhiteLunaticRobe,	Condition.TimeDay, Condition.DownedCultist)		// Solar Lunatic Robe
			.Add(ItemID.BlueLunaticHood,	Condition.TimeNight, Condition.DownedCultist)	// Lunar Cultist Hood
			.Add(ItemID.BlueLunaticRobe,	Condition.TimeNight, Condition.DownedCultist)	// Lunat Cultist Robe
			.Add(ItemID.TaxCollectorHat,	taxCollectorPresent)
			.Add(ItemID.TaxCollectorSuit,	taxCollectorPresent)
			.Add(ItemID.TaxCollectorPants,	taxCollectorPresent)
			.Add(ItemID.UndertakerHat,		Condition.InGraveyard)									// Gravedigger Hat
			.Add(ItemID.UndertakerCoat,		Condition.InGraveyard)									// Gravedigger Coat
			.Add(ItemID.FuneralHat,			Condition.InGraveyard)
			.Add(ItemID.FuneralCoat,		Condition.InGraveyard)
			.Add(ItemID.FuneralPants,		Condition.InGraveyard)
			.Add(ItemID.TragicUmbrella,		Condition.InGraveyard)
			.Add(ItemID.VictorianGothHat,	Condition.InGraveyard)
			.Add(ItemID.VictorianGothDress, Condition.InGraveyard)
			.Add(ItemID.Beanie,				Condition.InSnowBiome)
			.Add(ItemID.GuyFawkesMask,		Condition.Halloween)
			.Add(ItemID.TamOShanter,		Condition.Hardmode, Condition.IsMoonThirdQuarter)
			.Add(ItemID.GraduationCapBlue,	Condition.Hardmode, Condition.IsMoonWaningCrescent)
			.Add(ItemID.GraduationGownBlue, Condition.Hardmode, Condition.IsMoonWaningCrescent)
			.Add(ItemID.Tiara,				Condition.Hardmode, Condition.IsMoonNew)
			.Add(ItemID.PrincessDress,		Condition.Hardmode, Condition.IsMoonNew)
			.Add(ItemID.GraduationCapMaroon,Condition.Hardmode, Condition.IsMoonWaxingCrescent)
			.Add(ItemID.GraduationGownMaroon,Condition.Hardmode, Condition.IsMoonWaxingCrescent) // we have to ignore this menace 
			.Add(ItemID.CowboyHat,			Condition.Hardmode, Condition.IsMoonFirstQuarter)
			.Add(ItemID.CowboyJacket,		Condition.Hardmode, Condition.IsMoonFirstQuarter)
			.Add(ItemID.CowboyPants,		Condition.Hardmode, Condition.IsMoonFirstQuarter)
			.Add(ItemID.GraduationCapBlack, Condition.Hardmode, Condition.IsMoonWaxingGibbous)
			.Add(ItemID.GraduationGownBlack,Condition.Hardmode, Condition.IsMoonWaxingGibbous)
			.Add(ItemID.BallaHat,			Condition.DownedFrost)
			.Add(ItemID.GangstaHat,			Condition.DownedFrost)
			.Add(ItemID.ClothierJacket,		Condition.Halloween)
			.Add(ItemID.ClothierPants,		Condition.Halloween)
			.Add(ItemID.PartyBundleOfBalloonsAccessory,	Condition.BirthdayPartyIsUp) // why
			.Add(ItemID.PartyBalloonAnimal, Condition.BirthdayPartyIsUp)
			.Add(ItemID.FlowerBoyHat,		Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.FlowerBoyShirt,		Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.FlowerBoyPants,		Condition.BirthdayPartyIsUp)                              // Silly Sunflower Petals
			.Add(ItemID.HunterCloak,		Condition.GolfScoreOver(2000))
			.Register();
	}

	private static void RegisterGoblin()
	{
		new NPCShop(NPCID.GoblinTinkerer)
			.Add(ItemID.RocketBoots)
			.Add(ItemID.Ruler)
			.Add(ItemID.TinkerersWorkshop)
			.Add(ItemID.GrapplingHook)
			.Add(ItemID.Toolbelt)
			.Add(ItemID.SpikyBall)
			.Add(ItemID.RubblemakerSmall,	Condition.Hardmode)
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
			.Add(ItemID.WizardsHat,			Condition.Halloween)
			.Register();
	}

	private static void RegisterMechanic()
	{
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
			.Add(ItemID.MechanicsRod,		Condition.NpcIsPresent(NPCID.Angler), Condition.IsMoonPhasesOddQuarters)
			.Add(ItemID.Timer1Second)
			.Add(ItemID.Timer3Second)
			.Add(ItemID.Timer5Second)
			.Add(ItemID.TimerOneHalfSecond)
			.Add(ItemID.TimerOneFourthSecond)
			.Add(ItemID.PixelBox)
			.Register();
	}

	private static void RegisterSantaClaws()
	{
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
			.Add(ItemID.MushroomSpear,		Condition.DownedMechBossAny)
			.Add(ItemID.Hammush,			Condition.DownedMechBossAny)
			.Add(ItemID.Autohammer,			Condition.DownedPlantera)
			.Register();
	}

	private static void RegisterSteampunker()
	{
		var eclipseOrBloodMoon = Condition.EclipseOrBloodMoon;
		var notEclipseOrBloodMoon = Condition.NotEclipseAndNotBloodMoon;
		var blendOMaticCondition = new Condition(NetworkText.FromKey("ShopConditions.HardmodeFTW"), () => Main.hardMode || !Main.getGoodWorld);

		new NPCShop(NPCID.Steampunker)
			.Add(ItemID.Clentaminator,		Condition.NotRemixWorld)
			.Add(ItemID.SteampunkHat,		Condition.IsMoonPhasesHalf0)
			.Add(ItemID.SteampunkShirt,		Condition.IsMoonPhasesHalf0)
			.Add(ItemID.SteampunkPants,		Condition.IsMoonPhasesHalf0)
			.Add(ItemID.Jetpack,			Condition.Hardmode, Condition.IsMoonPhasesHalf1)
			.Add(ItemID.BlendOMatic,		blendOMaticCondition)
			.Add(ItemID.FleshCloningVaat,	Condition.CrimsonWorld)
			.Add(ItemID.LesionStation,		Condition.CorruptionWorld)                             // Decay Chamber
			.Add(ItemID.IceMachine,			Condition.InSnowBiome)
			.Add(ItemID.SkyMill,			Condition.InSpace)
			.Add(ItemID.HoneyDispenser,		Condition.InJungleBiome)
			.Add(ItemID.BoneWelder,			Condition.InGraveyard)
			.Add(ItemID.LivingLoom,			Condition.PlayerCarriesItem(ItemID.LivingWoodWand))
			.Add(ItemID.SteampunkBoiler,	Condition.DownedEyeOfCthulhu, Condition.DownedEowOrBoc, Condition.DownedSkeletron)
			.Add(ItemID.RedSolution,		Condition.NotRemixWorld, eclipseOrBloodMoon, Condition.CrimsonWorld)
			.Add(ItemID.PurpleSolution,		Condition.NotRemixWorld, eclipseOrBloodMoon, Condition.CorruptionWorld)
			.Add(ItemID.BlueSolution,		Condition.NotRemixWorld, notEclipseOrBloodMoon, Condition.InHallowBiome)
			.Add(ItemID.GreenSolution,		Condition.NotRemixWorld, notEclipseOrBloodMoon, Condition.InPurityBiome)
			.Add(ItemID.SandSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.SnowSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.DirtSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.Cog)
			.Add(ItemID.SteampunkMinecart)
			.Add(ItemID.SteampunkGoggles,	Condition.Halloween)
			.Add(ItemID.LogicGate_AND)
			.Add(ItemID.LogicGate_OR)
			.Add(ItemID.LogicGate_XOR)
			.Add(ItemID.LogicGate_NAND)
			.Add(ItemID.LogicGate_NOR)
			.Add(ItemID.LogicGate_NXOR)
			.Add(ItemID.LogicGateLamp_Off)
			.Add(ItemID.LogicGateLamp_On)
			.Add(ItemID.LogicGateLamp_Faulty)
			.Add(ItemID.StaticHook,			Condition.Hardmode)
			.Add(ItemID.ConveyorBeltLeft)
			.Add(ItemID.ConveyorBeltRight)
			.Register();
	}

	private static void RegisterDyeTrader()
	{
		var mpServer = new Condition(NetworkText.FromKey("ShopConditions.MultiplayerServer"), () => Main.netMode == NetmodeID.MultiplayerClient);

		new NPCShop(NPCID.DyeTrader)
			.Add(ItemID.DyeVat)
			.Add(ItemID.SilverDye)
			.Add(ItemID.TeamDye,			mpServer)
			.Add(ItemID.DyeTraderRobe,		Condition.Halloween)
			.Add(ItemID.DyeTraderTurban,	Condition.Halloween)
			.Add(ItemID.ShadowDye,			Condition.IsMoonFull)
			.Add(ItemID.NegativeDye,		Condition.IsMoonFull)
			.Add(ItemID.BrownDye)
			.Add(ItemID.FogboundDye,		Condition.InGraveyard)
			.Add(ItemID.BloodbathDye,		Condition.BloodMoon)
			.Register();
	}

	private static void RegisterPartyGirl()
	{
		new NPCShop(NPCID.PartyGirl)
			.Add(ItemID.ConfettiGun)
			.Add(ItemID.Confetti)
			.Add(ItemID.SmokeBomb)
			.Add(ItemID.BubbleMachine,		Condition.TimeDay)
			.Add(ItemID.FogMachine,			Condition.TimeNight)
			.Add(ItemID.BubbleWand)
			.Add(ItemID.BeachBall)
			.Add(ItemID.LavaLamp)
			.Add(ItemID.PlasmaLamp)
			.Add(ItemID.FireworksBox)
			.Add(ItemID.FireworkFountain)
			.Add(ItemID.PartyMinecart)																			// Party Wagon
			.Add(ItemID.KiteSpectrum)																			// Spectrum Kite
			.Add(ItemID.PogoStick)
			.Add(ItemID.RedRocket,			Condition.Hardmode)
			.Add(ItemID.GreenRocket,		Condition.Hardmode)
			.Add(ItemID.BlueRocket,			Condition.Hardmode)
			.Add(ItemID.YellowRocket,		Condition.Hardmode)
			.Add(ItemID.PartyGirlGrenade,	Condition.PlayerCarriesItem(ItemID.PartyGirlGrenade))		// Happy Grenade
			.Add(ItemID.ConfettiCannon,		Condition.NpcIsPresent(NPCID.Pirate))
			.Add(ItemID.Bubble,				Condition.Hardmode)
			.Add(ItemID.SmokeBlock,			Condition.Hardmode)
			.Add(ItemID.PartyMonolith)																			// Party Center
			.Add(ItemID.PartyHat)
			.Add(ItemID.SillyBalloonMachine)
			.Add(ItemID.PartyPresent,		Condition.BirthdayPartyIsUp)
			.Add(ItemID.Pigronata,			Condition.BirthdayPartyIsUp)
			.Add(ItemID.SillyStreamerBlue,	Condition.BirthdayPartyIsUp)								// Blue Streamer
			.Add(ItemID.SillyStreamerGreen, Condition.BirthdayPartyIsUp)								// Green Streamer
			.Add(ItemID.SillyStreamerPink,	Condition.BirthdayPartyIsUp)								// Pink Streamer
			.Add(ItemID.SillyBalloonPurple, Condition.BirthdayPartyIsUp)								// Silly Purple Balloon
			.Add(ItemID.SillyBalloonGreen,	Condition.BirthdayPartyIsUp)                              // Silly Green Balloon
			.Add(ItemID.SillyBalloonPink,	Condition.BirthdayPartyIsUp)                              // Silly Pink Balloon
			.Add(ItemID.SillyBalloonTiedGreen, Condition.BirthdayPartyIsUp)							// Silly Tied Balloon (Green)
			.Add(ItemID.SillyBalloonTiedPurple, Condition.BirthdayPartyIsUp)                          // Silly Tied Balloon (Purple)
			.Add(ItemID.SillyBalloonTiedPink, Condition.BirthdayPartyIsUp)                            // Silly Tied Balloon (Pink)
			.Add(ItemID.FireworksLauncher,	Condition.DownedGolem)											// Celebration
			.Add(ItemID.ReleaseDoves,		Condition.InGraveyard)
			.Add(ItemID.ReleaseLantern,		Condition.NightLanternsUp)
			.Add(ItemID.Football,			Condition.GolfScoreOver(500))
			.Register();
	}

	private static void RegisterCyborg()
	{
		var portalGunStation = new Condition(NetworkText.FromKey("ShopConditions.PlayerCarriesItem2", Lang.GetItemName(ItemID.PortalGun), Lang.GetItemName(ItemID.PortalGunStation)),
			() => Main.LocalPlayer.HasItem(ItemID.PortalGun) || Main.LocalPlayer.HasItem(ItemID.PortalGunStation));

		new NPCShop(NPCID.Cyborg)
			.Add(ItemID.RocketI)
			.Add(ItemID.RocketII,			Condition.BloodMoon)
			.Add(ItemID.RocketIII,			Condition.EclipseOrNight)
			.Add(ItemID.RocketIV,			Condition.Eclipse)
			.Add(ItemID.DryRocket)
			.Add(ItemID.ProximityMineLauncher)
			.Add(ItemID.Nanites)
			.Add(ItemID.ClusterRocketI,		Condition.DownedMartians)
			.Add(ItemID.ClusterRocketII,	Condition.DownedMartians, Condition.EclipseOrBloodMoon)
			.Add(ItemID.CyborgHelmet,		Condition.Halloween)
			.Add(ItemID.CyborgShirt,		Condition.Halloween)
			.Add(ItemID.CyborgPants,		Condition.Halloween)
			.Add(ItemID.HiTekSunglasses,	Condition.DownedMartians)
			.Add(ItemID.NightVisionHelmet,  Condition.DownedMartians)
			.Add(ItemID.PortalGunStation,	portalGunStation)
			.Add(ItemID.EchoBlock,			Condition.InGraveyard)
			.Add(ItemID.SpectreGoggles,		Condition.InGraveyard)
			.Add(ItemID.JimsDrone)
			.Add(ItemID.JimsDroneVisor)
			.Register();
	}

	private static void RegisterPainter()
	{
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
			.Add(ItemID.ShadowPaint,		Condition.Hardmode)
			.Add(ItemID.NegativePaint,		Condition.Hardmode)
			.Add(ItemID.GlowPaint,			Condition.InGraveyard)									// Illuminant Coating
			.Add(ItemID.EchoCoating,		Condition.InGraveyard, Condition.DownedPlantera)
			.Register();

		new NPCShop(NPCID.Painter, "Decor") // Decor shop
			.Add(ItemID.Daylight)
			.Add(ItemID.FirstEncounter,		Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.GoodMorning,		Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.UndergroundReward,	Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.ThroughtheWindow,	Condition.IsMoonPhasesQuarter3)
			.Add(ItemID.Purity,				Condition.InShoppingForestBiome)
			.Add(ItemID.DeadlandComesAlive, Condition.InCrimsonBiome)
			.Add(ItemID.LightlessChasms,	Condition.InCorruptBiome)
			.Add(ItemID.TheLandofDeceivingLooks, Condition.InHallowBiome)
			.Add(ItemID.DoNotStepontheGrass, Condition.InJungleBiome)
			.Add(ItemID.ColdWatersintheWhiteLand, Condition.InSnowBiome)
			.Add(ItemID.SecretoftheSands,	Condition.InDesertBiome)
			.Add(ItemID.EvilPresence,		Condition.BloodMoon)
			.Add(ItemID.PlaceAbovetheClouds,Condition.InSpace)
			.Add(ItemID.SkyGuardian,		Condition.Hardmode, Condition.InSpace)
			.Add(ItemID.Thunderbolt,		Condition.Thunderstorm)
			.Add(ItemID.Nevermore,			Condition.InGraveyard)
			.Add(ItemID.Reborn,				Condition.InGraveyard)
			.Add(ItemID.Graveyard,			Condition.InGraveyard)
			.Add(ItemID.GhostManifestation, Condition.InGraveyard)
			.Add(ItemID.WickedUndead,		Condition.InGraveyard)
			.Add(ItemID.HailtotheKing,		Condition.InGraveyard)
			.Add(ItemID.BloodyGoblet,		Condition.InGraveyard)
			.Add(ItemID.StillLife,			Condition.InGraveyard)
			.Add(ItemID.ChristmasTreeWallpaper, Condition.Christmas)
			.Add(ItemID.CandyCaneWallpaper, Condition.Christmas)
			.Add(ItemID.StarsWallpaper,		Condition.Christmas)
			.Add(ItemID.SnowflakeWallpaper, Condition.Christmas)
			.Add(ItemID.BluegreenWallpaper, Condition.Christmas)
			.Add(ItemID.OrnamentWallpaper,  Condition.Christmas)
			.Add(ItemID.FestiveWallpaper,	Condition.Christmas)
			.Add(ItemID.SquigglesWallpaper, Condition.Christmas)
			.Add(ItemID.KrampusHornWallpaper, Condition.Christmas)
			.Add(ItemID.GrinchFingerWallpaper, Condition.Christmas)
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

	private static void RegisterWitchDoctor()
	{
		new NPCShop(NPCID.WitchDoctor)
			.Add(ItemID.ImbuingStation)
			.Add(ItemID.Blowgun)
			.Add(ItemID.StyngerBolt,		Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Stake,				Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.Cauldron,			Condition.Halloween)
			.Add(ItemID.TikiTotem,			Condition.Hardmode, Condition.InJungleBiome)
			.Add(ItemID.LeafWings,			Condition.Hardmode, Condition.InJungleBiome, Condition.TimeNight, Condition.DownedPlantera)
			.Add(ItemID.VialofVenom,		Condition.DownedPlantera)
			.Add(ItemID.TikiMask,			Condition.DownedPlantera)
			.Add(ItemID.TikiShirt,			Condition.DownedPlantera)
			.Add(ItemID.TikiPants,			Condition.DownedPlantera)
			.Add(ItemID.PygmyNecklace,		Condition.TimeNight)
			.Add(ItemID.HerculesBeetle,		Condition.Hardmode, Condition.DownedPlantera, Condition.InJungleBiome)
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
			.Add(ItemID.BewitchingTable,	Condition.NpcIsPresent(NPCID.Wizard))
			.Register();
	}

	private static void RegisterPirate()
	{
		new NPCShop(NPCID.Pirate)
			.Add(ItemID.Cannon)
			.Add(ItemID.Cannonball)
			.Add(ItemID.PirateHat)
			.Add(ItemID.PirateShirt)
			.Add(ItemID.PiratePants)
			.Add(ItemID.Sail)
			.Add(ItemID.ParrotCracker,		new Condition(NetworkText.FromLiteral("RecipeConditions.InBeach"), () => {
				int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
				return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
			}))
			.Add(ItemID.BunnyCannon,		Condition.NpcIsPresent(NPCID.PartyGirl),
											Condition.Hardmode,
											Condition.DownedMechBossAny)
			.Register();
	}

	private static void RegisterStylist()
	{
		var maxLife = new Condition(NetworkText.FromKey("ShopConditions.AtleastXHealth", 400), () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
		var maxMana = new Condition(NetworkText.FromKey("ShopConditions.AtleastXHealth", 200), () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
		var moneyHair = new Condition(NetworkText.FromKey("ShopConditions.PlatinumCoin"), () => {
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
		var timeHair = new Condition(NetworkText.FromKey("ShopCondition.StyleMoon"), () => Main.moonPhase % 2 == (!Main.dayTime).ToInt());
		var teamHair = new Condition(NetworkText.FromKey("ShopConditions.OnTeam"), () => Main.LocalPlayer.team != 0);
		var partyHair = new Condition(NetworkText.FromKey("ShopConditions.NpcIsPresent", Lang.GetNPCName(NPCID.PartyGirl)), () => NPC.AnyNPCs(NPCID.PartyGirl));

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
			.Add(ItemID.BiomeHairDye,		Condition.Hardmode)
			.Add(ItemID.SpeedHairDye,		Condition.Hardmode, Condition.DownedMechBossAny)
			.Add(ItemID.RainbowHairDye,		Condition.Hardmode, Condition.DownedTwins, Condition.DownedSkeletronPrime, Condition.DownedDestroyer)
			.Add(ItemID.MartianHairDye,		Condition.DownedMartians)
			.Add(ItemID.TwilightHairDye,	Condition.DownedMartians)
			.Register();
	}

	private static void RegisterSkeletonMerchant()
	{
		Condition[] wandOfSparkingCondition = { Condition.IsMoonThirdQuarter, Condition.NotRemixWorld };
		Condition[] magicDaggerCondition = { Condition.IsMoonThirdQuarter, Condition.RemixWorld };
		var spelunkerGlowCondition = new Condition(NetworkText.FromKey("ShopConditions.NightDayFullMoon"), () => !Main.dayTime || Main.moonPhase == 0);
		var glowstickCondition = new Condition(NetworkText.FromKey("ShopConditions.DaytimeSkinnyMoon"), () => Main.dayTime && Main.moonPhase != 0);
		var boneTorchCondition = new Condition(NetworkText.FromKey("ShopConditions.FirstHalfSecond"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
		var torchCondition = new Condition(NetworkText.FromKey("ShopConditions.SecondHalfSecond"), () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0);
		var artisanCondition = new Condition(NetworkText.FromKey("ShopConditions.NoAteLoaf"), () => !Main.LocalPlayer.ateArtisanBread);

		new NPCShop(NPCID.SkeletonMerchant)
			.Add(ItemID.WoodenBoomerang,	Condition.IsMoonFull)
			.Add(ItemID.Umbrella,			Condition.IsMoonWaningGibbous)
			.Add(ItemID.WandofSparking,		wandOfSparkingCondition)
			.Add(ItemID.MagicDagger,		magicDaggerCondition)
			.Add(ItemID.PortableStool,		Condition.IsMoonWaningCrescent)							// Step Stool
			.Add(ItemID.Aglet,				Condition.IsMoonNew)
			.Add(ItemID.ClimbingClaws,		Condition.IsMoonWaxingCrescent)
			.Add(ItemID.CordageGuide,		Condition.IsMoonFirstQuarter)								// Guide to Plant Fiber Cordage
			.Add(ItemID.Radar,				Condition.IsMoonWaxingGibbous)
			.Add(ItemID.StrangeBrew,		Condition.IsMoonPhasesEven)
			.Add(ItemID.LesserHealingPotion, Condition.IsMoonPhasesOdd)
			.Add(ItemID.HealingPotion,		Condition.Hardmode, Condition.IsMoonPhasesOdd)
			.Add(ItemID.SpelunkerGlowstick, spelunkerGlowCondition)
			.Add(ItemID.SpelunkerFlare,		spelunkerGlowCondition, Condition.PlayerCarriesItem(ItemID.FlareGun))
			.Add(ItemID.Glowstick,			glowstickCondition)
			.Add(ItemID.BoneTorch,			boneTorchCondition)
			.Add(ItemID.Torch,				torchCondition)
			.Add(ItemID.BoneArrow,			Condition.IsMoonPhasesEvenQuarters)
			.Add(ItemID.WoodenArrow,		Condition.IsMoonPhasesOddQuarters)
			.Add(ItemID.BlueCounterweight,	Condition.IsMoonPhases04)
			.Add(ItemID.RedCounterweight,	Condition.IsMoonPhases15)
			.Add(ItemID.PurpleCounterweight, Condition.IsMoonPhases26)
			.Add(ItemID.GreenCounterweight, Condition.IsMoonPhases37)
			.Add(ItemID.Bomb)
			.Add(ItemID.Rope)
			.Add(ItemID.Gradient,			Condition.Hardmode, Condition.IsMoonPhasesHalf0)
			.Add(ItemID.FormatC,			Condition.Hardmode, Condition.IsMoonPhasesHalf1)
			.Add(ItemID.YoYoGlove,			Condition.Hardmode)
			.Add(ItemID.SlapHand,			Condition.Hardmode, Condition.BloodMoon)
			.Add(ItemID.MagicLantern,		Condition.TimeNight, Condition.IsMoonFull)
			.Add(ItemID.ArtisanLoaf,		artisanCondition, Condition.IsMoonPhasesNearNew)
			.Register();
	}

	private static void RegisterBartender()
	{
		var shop = new NPCShop(NPCID.DD2Bartender).AllowFillingLastSlot();

		void AddEntry(int id, int price, params Condition[] conditions) =>
			shop.Add(
				new Entry(
					new Item(id) { shopCustomPrice = price, shopSpecialCurrency = CustomCurrencyID.DefenderMedals },
					conditions
				).ReserveSlot());

		// 1st row
		shop.Add(ItemID.Ale);
		shop.Add(new Entry(ItemID.DD2ElderCrystal).AddShopOpenedCallback((item, npc) => {        // Eternia Crystal
			if (NPC.downedGolemBoss) {
				item.shopCustomPrice = Item.buyPrice(gold: 4);
			}
			else if (NPC.downedMechBossAny) {
				item.shopCustomPrice = Item.buyPrice(gold: 1);
			}
			else {
				item.shopCustomPrice = Item.buyPrice(silver: 25);
			}
		}));
		shop.Add(ItemID.DD2ElderCrystalStand);                                          // Eternia Crystal Stand
		AddEntry(ItemID.DefendersForge, 50);

		AddEntry(ItemID.SquireGreatHelm,			15,	Condition.DownedMechBossAny);
		AddEntry(ItemID.SquirePlating,				15,	Condition.DownedMechBossAny);
		AddEntry(ItemID.SquireGreaves,				15,	Condition.DownedMechBossAny);
		AddEntry(ItemID.SquireAltHead,				50,	Condition.DownedGolem);          // Valhalla Knight's Helm
		AddEntry(ItemID.SquireAltShirt,				50,	Condition.DownedGolem);          // Valhalla Knight's Breastplate
		AddEntry(ItemID.SquireAltPants,				50,	Condition.DownedGolem);          // Valhalla Knight's Greaves

		// 2nd row
		AddEntry(ItemID.DD2FlameburstTowerT1Popper,	5);                                  // Flameburst Rod
		AddEntry(ItemID.DD2BallistraTowerT1Popper,	5);                                  // Ballista Rod
		AddEntry(ItemID.DD2ExplosiveTrapT1Popper,	5);                                  // Explosive Trap Rod
		AddEntry(ItemID.DD2LightningAuraT1Popper,	5);                                  // Lightning Aura Rod
		AddEntry(ItemID.ApprenticeHat,				15, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeRobe,				15, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeTrousers,			15, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeAltHead,			50, Condition.DownedGolem);          // Dark Atrist's Hat
		AddEntry(ItemID.ApprenticeAltShirt,			50, Condition.DownedGolem);          // Dark Atrist's Robes
		AddEntry(ItemID.ApprenticeAltPants,			50, Condition.DownedGolem);          // Dark Atrist's Leggings

		// 3rd row
		AddEntry(ItemID.DD2FlameburstTowerT2Popper, 15, Condition.DownedMechBossAny);    // Flameburst Cane
		AddEntry(ItemID.DD2BallistraTowerT2Popper,	15, Condition.DownedMechBossAny);    // Ballista Cane
		AddEntry(ItemID.DD2ExplosiveTrapT2Popper,	15, Condition.DownedMechBossAny);    // Explosive Trap Cane
		AddEntry(ItemID.DD2LightningAuraT2Popper,	15, Condition.DownedMechBossAny);    // Lightning Aura Cane
		AddEntry(ItemID.HuntressWig,				15, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressJerkin,				15, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressPants,				15, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressAltHead,			50, Condition.DownedGolem);          // Red Riding Hood
		AddEntry(ItemID.HuntressAltShirt,			50, Condition.DownedGolem);          // Red Riding Dress
		AddEntry(ItemID.HuntressAltPants,			50, Condition.DownedGolem);          // Red Riding Leggings

		// 4th row
		AddEntry(ItemID.DD2FlameburstTowerT3Popper, 60, Condition.DownedGolem);          // Flameburst Staff
		AddEntry(ItemID.DD2BallistraTowerT3Popper,	60, Condition.DownedGolem);          // Ballista Staff
		AddEntry(ItemID.DD2ExplosiveTrapT3Popper,	60, Condition.DownedGolem);          // Explosive Trap Staff
		AddEntry(ItemID.DD2LightningAuraT3Popper,	60, Condition.DownedGolem);          // Lightning Aura Staff
		AddEntry(ItemID.MonkBrows,					15, Condition.DownedMechBossAny);    // Monk's Bushy Brow Bald Cap
		AddEntry(ItemID.MonkShirt,					15, Condition.DownedMechBossAny);    // Monk's Shirt
		AddEntry(ItemID.MonkPants,					15, Condition.DownedMechBossAny);    // Monk's Pants
		AddEntry(ItemID.MonkAltHead,				50, Condition.DownedGolem);          // Shinobi Infiltrator's Helmet
		AddEntry(ItemID.MonkAltShirt,				50, Condition.DownedGolem);          // Shinobi Infiltrator's Torso
		AddEntry(ItemID.MonkAltPants,				50, Condition.DownedGolem);          // Shinobi Infiltrator's Pants

		shop.Register();
	}

	private static void RegisterGolfer()
	{
		var scoreOver500 = Condition.GolfScoreOver(500);
		var scoreOver1000 = Condition.GolfScoreOver(1000);
		var scoreOver2000 = Condition.GolfScoreOver(2000);

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
			.Add(ItemID.GolfCart,			scoreOver2000, Condition.DownedSkeletron)					// Golf Cart Keys
			.Add(ItemID.GolfPainting1,		scoreOver2000, Condition.IsMoonPhasesQuarter0)				// The Rolling Greens
			.Add(ItemID.GolfPainting2,		scoreOver2000, Condition.IsMoonPhasesQuarter1)				// Study of a Ball at Rest
			.Add(ItemID.GolfPainting3,		scoreOver2000, Condition.IsMoonPhasesQuarter2)				// Fore!
			.Add(ItemID.GolfPainting4,		scoreOver2000, Condition.IsMoonPhasesQuarter3)				// The Duplicity of Reflections
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

	private static void RegisterZoologist()
	{
		var fairyGlowstick = new Condition(NetworkText.FromKey("ShopConditions.BestiaryWinx"), () => Chest.BestiaryGirl_IsFairyTorchAvailable());
		var solarPillarDead = Condition.DownedSolarPillar;

		var moonIsFullOrWaningGibbous = Condition.IsMoonPhasesQuarter0;
		var moonIsThirdOrWaningCrescent = Condition.IsMoonPhasesQuarter1;
		var moonIsNewOrWaxingCrescent = Condition.IsMoonPhasesQuarter2;
		var moonIsFirstOrWaxingGibbous = Condition.IsMoonPhasesQuarter3;

		var bestiaryFilledBy10 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 10), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
		var bestiaryFilledBy25 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 25), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
		var bestiaryFilledBy30 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 30), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
		var bestiaryFilledBy35 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 35), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.35f);
		var bestiaryFilledBy40 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 40), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f);
		var bestiaryFilledBy45 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 45), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f);
		var bestiaryFilledBy50 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 50), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
		var bestiaryFilledBy70 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryPercentage", 70), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f);
		var bestiaryFilledBy100 = new Condition(NetworkText.FromKey("ShopConditions.BestiaryFull"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);

		new NPCShop(NPCID.BestiaryGirl)
			.Add(ItemID.DontHurtCrittersBook)																	// Guide to Critter Companionship
			.Add(ItemID.SquirrelHook)
			.Add(ItemID.TheWerewolf,		Condition.IsMoonFull)
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
			.Add(ItemID.FullMoonSqueakyToy, Condition.Hardmode, Condition.BloodMoon)
			.Add(ItemID.MudBud,				Condition.DownedPlantera)
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

	private static void RegisterPrincess()
	{
		var goodsCondition = new Condition(NetworkText.FromKey("ShopConditions.InCelebrationMk10"), () => Main.tenthAnniversaryWorld);

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
			.Add(ItemID.RoyalRomance,		Condition.DownedKingSlime, Condition.DownedKingSlime)
			.Add(ItemID.MusicBoxCredits,	Condition.Hardmode, Condition.DownedMoonLord)   // Music Box (Journey's End)
			.Add(ItemID.SlimeStaff,			goodsCondition)
			.Add(ItemID.HeartLantern,		goodsCondition)
			.Add(ItemID.FlaskofParty,		goodsCondition)
			.Add(ItemID.SandstorminaBottle, goodsCondition, Condition.InDesertBiome)
			.Add(ItemID.Terragrim,			goodsCondition, Condition.BloodMoon)
			.Add(ItemID.PirateStaff,		goodsCondition, Condition.Hardmode, Condition.DownedPirates, Condition.IsMoonPhasesQuarter0)
			.Add(ItemID.DiscountCard,		goodsCondition, Condition.Hardmode, Condition.DownedPirates, Condition.IsMoonPhasesQuarter1)
			.Add(ItemID.LuckyCoin,			goodsCondition, Condition.Hardmode, Condition.DownedPirates, Condition.IsMoonPhasesQuarter2)
			.Add(ItemID.CoinGun,			goodsCondition, Condition.Hardmode, Condition.DownedPirates, Condition.IsMoonPhasesQuarter3)
			.Register();
	}

	internal static void SortAllShops()
	{
		foreach (var shop in AllShops) {
			shop.Sort();
		}
	}
}