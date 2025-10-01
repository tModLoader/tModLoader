using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

using static Terraria.ModLoader.NPCShop;

namespace Terraria.ModLoader;

public static partial class NPCShopDatabase
{
	private static readonly Dictionary<string, AbstractNPCShop> npcShopByName = new();
	public static IEnumerable<AbstractNPCShop> AllShops => npcShopByName.Values;

	public static readonly ISet<string> NoPylons = new HashSet<string>();

	internal static void AddShop(AbstractNPCShop shop)
	{
		npcShopByName.Add(shop.FullName, shop);
	}

	public static bool TryGetNPCShop(string fullName, out AbstractNPCShop shop) => npcShopByName.TryGetValue(fullName, out shop);

	/// <summary>
	/// Gets a shop name (identifier) in the format matching <see cref="AbstractNPCShop.FullName"/> <br/>
	/// Can be used with <see cref="TryGetNPCShop"/> and <see cref="GlobalNPC.ModifyActiveShop(NPC, string, Item[])"/>
	/// </summary>
	/// <param name="npcType"></param>
	/// <param name="shopName"></param>
	/// <returns></returns>
	public static string GetShopName(int npcType, string shopName = "Shop")
	{
		return $"{(npcType < NPCID.Count ? $"Terraria/{NPCID.Search.GetName(npcType)}" : NPCLoader.GetNPC(npcType).FullName)}/{shopName}";
	}

	private static string[] _vanillaShopNames = new[] {
		null,
		GetShopName(NPCID.Merchant),
		GetShopName(NPCID.ArmsDealer),
		GetShopName(NPCID.Dryad),
		GetShopName(NPCID.Demolitionist),
		GetShopName(NPCID.Clothier),
		GetShopName(NPCID.GoblinTinkerer),
		GetShopName(NPCID.Wizard),
		GetShopName(NPCID.Mechanic),
		GetShopName(NPCID.SantaClaus),
		GetShopName(NPCID.Truffle),
		GetShopName(NPCID.Steampunker),
		GetShopName(NPCID.DyeTrader),
		GetShopName(NPCID.PartyGirl),
		GetShopName(NPCID.Cyborg),
		GetShopName(NPCID.Painter),
		GetShopName(NPCID.WitchDoctor),
		GetShopName(NPCID.Pirate),
		GetShopName(NPCID.Stylist),
		GetShopName(NPCID.TravellingMerchant),
		GetShopName(NPCID.SkeletonMerchant),
		GetShopName(NPCID.DD2Bartender),
		GetShopName(NPCID.Golfer),
		GetShopName(NPCID.BestiaryGirl),
		GetShopName(NPCID.Princess),
		GetShopName(NPCID.Painter, "Decor"),
	};

	public static string GetShopNameFromVanillaIndex(int index) => _vanillaShopNames[index];

	public static void Initialize()
	{
		npcShopByName.Clear();
		NoPylons.Clear();

		RegisterVanillaNPCShops();

		for (int i = 0; i < NPCLoader.NPCCount; i++) {
			NPCLoader.AddShops(i);
		}
		foreach (var shop in AllShops.OfType<NPCShop>()) {
			NPCLoader.ModifyShop(shop);
		}
	}

	internal static void FinishSetup()
	{
		foreach (var shop in AllShops) {
			shop.FinishSetup();
			// NPCShopDatabase.Initialize(); seems intentionally run before SetupRecipes, where IsAMaterial is populated, so we need to fix entries here.
			foreach (var entry in shop.ActiveEntries) {
				entry.Item.Refresh(onlyIfVariantChanged: false);
			}
		}

		InitShopTestSystem();
	}

	private static void RegisterVanillaNPCShops()
	{
		NoPylons.Add(GetShopName(NPCID.TravellingMerchant));
		NoPylons.Add(GetShopName(NPCID.SkeletonMerchant));
		NoPylons.Add(GetShopName(NPCID.DD2Bartender)); // Bartender sometimes can't fit pylons, but sometimes can. Special hack in PylonShopNPC to match vanilla for this
		NoPylons.Add(GetShopName(NPCID.SantaClaus)); // Got no space

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
		RegisterTravellingMerchant();
	}

	public static IEnumerable<Entry> GetPylonEntries()
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

		// These zones/pylons have extra conditions beyond just being in a certain biome.
		var forestPylonCondition = new Condition("Conditions.ForestPylon", () => { // im having struggles localizing these
			if (Main.LocalPlayer.ZoneSnow || Main.LocalPlayer.ZoneDesert || Main.LocalPlayer.ZoneBeach || Main.LocalPlayer.ZoneJungle || Main.LocalPlayer.ZoneHallow || Main.LocalPlayer.ZoneGlowshroom)
				return false;

			return Main.remixWorld
				? Main.LocalPlayer.Center.Y / 16.0 > Main.rockLayer && Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 350
				: Main.LocalPlayer.Center.Y / 16.0 < Main.worldSurface;
		});

		var cavernPylonCondition = new Condition("Conditions.UndergroundPylon", () =>
			!Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneDesert && !Main.LocalPlayer.ZoneBeach && !Main.LocalPlayer.ZoneJungle && !Main.LocalPlayer.ZoneHallow && (Main.remixWorld || !Main.LocalPlayer.ZoneGlowshroom)
			&& Main.LocalPlayer.Center.Y / 16.0 >= Main.worldSurface
		);

		var oceanPylonCondition = new Condition("Conditions.InBeach", () => {
			bool flag4 = Main.LocalPlayer.ZoneBeach && Main.LocalPlayer.position.Y < Main.worldSurface * 16.0;
			if (Main.remixWorld) {
				double num13 = Main.LocalPlayer.position.X / 16.0;
				double num14 = Main.LocalPlayer.position.Y / 16.0;
				flag4 |= (num13 < Main.maxTilesX * 0.43 || num13 > Main.maxTilesX * 0.57) && num14 > Main.rockLayer && num14 < Main.maxTilesY - 350;
			}
			return flag4;
		});

		var mushroomPylonCondition = new Condition("Conditions.InGlowshroom", () => Main.LocalPlayer.ZoneGlowshroom && (!Main.remixWorld || !Main.LocalPlayer.ZoneUnderworldHeight));

		yield return new Entry(ItemID.TeleportationPylonPurity,        Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, forestPylonCondition).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonSnow,          Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, Condition.InSnow).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonDesert,        Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, Condition.InDesert).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonUnderground,   Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, cavernPylonCondition).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonOcean,         Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, oceanPylonCondition).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonJungle,        Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, Condition.InJungle).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonHallow,        Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, Condition.InHallow).OrderLast();
		yield return new Entry(ItemID.TeleportationPylonMushroom,      Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome, mushroomPylonCondition).OrderLast();


		foreach (ModPylon pylon in PylonLoader.modPylons) {
			if (pylon.GetNPCShopEntry() is { } entry)
				yield return entry.OrderLast();
		}
	}

	private static void RegisterMerchant()
	{
		var carriesFlareGun = Condition.PlayerCarriesItem(ItemID.FlareGun);
		var drumSetCondition = new Condition("Conditions.DownedB2B3HM", () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);

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
			.Add(ItemID.Marshmallow,		Condition.InSnow)
			.Add(ItemID.Furnace,			Condition.InJungle)
			.Add(ItemID.PinWheel,			Condition.TimeDay, Condition.HappyWindyDay)
			.Add(ItemID.ThrowingKnife,		Condition.BloodMoon)
			.Add(ItemID.Glowstick,			Condition.TimeNight)
			.Add(ItemID.Safe,				Condition.DownedSkeletron)
			.Add(ItemID.DiscoBall,			Condition.Hardmode)
			.Add(ItemID.Flare,				carriesFlareGun)
			.Add(ItemID.BlueFlare,			carriesFlareGun)
			.Add(ItemID.Sickle)
			.Add(ItemID.GoldDust,			Condition.Hardmode)
			.Add(ItemID.SharpeningStation,	Condition.Hardmode)
			.Add(ItemID.DrumSet,			drumSetCondition)
			.Add(ItemID.DrumStick,			drumSetCondition)
			.Add(ItemID.Nail,				Condition.PlayerCarriesItem(ItemID.NailGun))
			.Register();
	}

	private static void RegisterArmsDealer()
	{
		new NPCShop(NPCID.ArmsDealer)
			.Add(ItemID.MusketBall)
			.Add(ItemID.SilverBullet,			Condition.BloodMoonOrHardmode, new Condition("Conditions.WorldGenSilver", () => WorldGen.SavedOreTiers.Silver == TileID.Silver))
			.Add(ItemID.TungstenBullet,			Condition.BloodMoonOrHardmode, new Condition("Conditions.WorldGenTungsten", () => WorldGen.SavedOreTiers.Silver == TileID.Tungsten))
			.Add(ItemID.UnholyArrow,			new Condition("Conditions.NightAfterEvilOrHardmode", () => (NPC.downedBoss2 && !Main.dayTime) || Main.hardMode))
			.Add(ItemID.FlintlockPistol)
			.Add(ItemID.Minishark)
			.Add(ItemID.QuadBarrelShotgun,		Condition.InGraveyard, Condition.DownedSkeletron)
			.Add(ItemID.IllegalGunParts,		Condition.TimeNight)
			.Add(ItemID.Shotgun,				Condition.Hardmode)
			.Add(ItemID.EmptyBullet,			Condition.Hardmode)
			.Add(ItemID.AmmoBox,				Condition.Hardmode)
			.Add(ItemID.StyngerBolt,			Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Stake,					Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.Nail,					Condition.PlayerCarriesItem(ItemID.NailGun))
			.Add(ItemID.CandyCorn,				Condition.PlayerCarriesItem(ItemID.CandyCornRifle))
			.Add(ItemID.ExplosiveJackOLantern,	Condition.PlayerCarriesItem(ItemID.JackOLanternLauncher))
			.Add(ItemID.NurseHat,				Condition.Halloween)
			.Add(ItemID.NurseShirt,				Condition.Halloween)
			.Add(ItemID.NursePants,				Condition.Halloween)
			.Register();
	}

	private static void RegisterDryad()
	{
		new NPCShop(NPCID.Dryad)
			.Add(ItemID.ViciousPowder,					Condition.BloodMoon, Condition.CrimsonWorld, Condition.NotRemixWorld)
			.Add(ItemID.CrimsonSeeds,					Condition.BloodMoon, Condition.CrimsonWorld)
			.Add(ItemID.CrimsonGrassEcho,				Condition.BloodMoon, Condition.CrimsonWorld)
			.Add(ItemID.VilePowder,						Condition.BloodMoon, Condition.CorruptWorld, Condition.NotRemixWorld)
			.Add(ItemID.CorruptSeeds,					Condition.BloodMoon, Condition.CorruptWorld)
			.Add(ItemID.CorruptGrassEcho,				Condition.BloodMoon, Condition.CorruptWorld)
			.Add(ItemID.PurificationPowder,				Condition.NotBloodMoon, Condition.NotRemixWorld)
			.Add(ItemID.GrassSeeds,						Condition.NotBloodMoon)
			.Add(ItemID.Sunflower,						Condition.NotBloodMoon)
			.Add(ItemID.GrassWall,						Condition.NotBloodMoon)
			.Add(ItemID.CrimsonSeeds,					Condition.Hardmode, Condition.InGraveyard, Condition.CorruptWorld)
			.Add(ItemID.CorruptSeeds,					Condition.Hardmode, Condition.InGraveyard, Condition.CrimsonWorld)
			.Add(ItemID.Acorn)
			.Add(ItemID.DontHurtNatureBook)
			.Add(ItemID.DirtRod)
			.Add(ItemID.PumpkinSeed)
			.Add(ItemID.FlowerWall)
			.Add(ItemID.JungleWall,						Condition.Hardmode)
			.Add(ItemID.HallowedSeeds,					Condition.Hardmode)
			.Add(ItemID.HallowedGrassEcho,				Condition.Hardmode)
			.Add(ItemID.AshGrassSeeds,					Condition.InUnderworld)
			.Add(ItemID.MushroomGrassSeeds,				Condition.NotInUnderworld, Condition.InGlowshroom)
			.Add(ItemID.DryadCoverings,					Condition.Halloween)
			.Add(ItemID.DryadLoincloth,					Condition.Halloween)
			.Add(ItemID.DayBloomPlanterBox,				Condition.DownedKingSlime)
			.Add(ItemID.MoonglowPlanterBox,				Condition.DownedQueenBee)
			.Add(ItemID.BlinkrootPlanterBox,			Condition.DownedEyeOfCthulhu)
			.Add(ItemID.CorruptPlanterBox,				Condition.DownedEaterOfWorlds)
			.Add(ItemID.CrimsonPlanterBox,				Condition.DownedBrainOfCthulhu)
			.Add(ItemID.WaterleafPlanterBox,			Condition.DownedSkeletron)
			.Add(ItemID.ShiverthornPlanterBox,			Condition.DownedSkeletron)
			.Add(ItemID.FireBlossomPlanterBox,			Condition.Hardmode)
			.Add(ItemID.FlowerPacketWhite)
			.Add(ItemID.FlowerPacketYellow)
			.Add(ItemID.FlowerPacketRed)
			.Add(ItemID.FlowerPacketPink)
			.Add(ItemID.FlowerPacketMagenta)
			.Add(ItemID.FlowerPacketViolet)
			.Add(ItemID.FlowerPacketBlue)
			.Add(ItemID.FlowerPacketWild)
			.Add(ItemID.FlowerPacketTallGrass)
			.Add(ItemID.PottedForestCedar,				Condition.Hardmode, Condition.MoonPhasesQuarter0)
			.Add(ItemID.PottedJungleCedar,				Condition.Hardmode, Condition.MoonPhasesQuarter0)
			.Add(ItemID.PottedHallowCedar,				Condition.Hardmode, Condition.MoonPhasesQuarter0)
			.Add(ItemID.PottedForestTree,				Condition.Hardmode, Condition.MoonPhasesQuarter1)
			.Add(ItemID.PottedJungleTree,				Condition.Hardmode, Condition.MoonPhasesQuarter1)
			.Add(ItemID.PottedHallowTree,				Condition.Hardmode, Condition.MoonPhasesQuarter1)
			.Add(ItemID.PottedForestPalm,				Condition.Hardmode, Condition.MoonPhasesQuarter2)
			.Add(ItemID.PottedJunglePalm,				Condition.Hardmode, Condition.MoonPhasesQuarter2)
			.Add(ItemID.PottedHallowPalm,				Condition.Hardmode, Condition.MoonPhasesQuarter2)
			.Add(ItemID.PottedForestBamboo,				Condition.Hardmode, Condition.MoonPhasesQuarter3)
			.Add(ItemID.PottedJungleBamboo,				Condition.Hardmode, Condition.MoonPhasesQuarter3)
			.Add(ItemID.PottedHallowBamboo,				Condition.Hardmode, Condition.MoonPhasesQuarter3)
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
		var taxCollectorIsPresent = Condition.NpcIsPresent(NPCID.TaxCollector);

		new NPCShop(NPCID.Clothier)
			.Add(ItemID.BlackThread)
			.Add(ItemID.PinkThread)
			.Add(ItemID.SummerHat,						Condition.TimeDay)
			.Add(ItemID.PlumbersShirt,					Condition.MoonPhaseFull)
			.Add(ItemID.PlumbersPants,					Condition.MoonPhaseFull)
			.Add(ItemID.WhiteTuxedoShirt,				Condition.MoonPhaseFull, Condition.TimeNight)
			.Add(ItemID.WhiteTuxedoPants,				Condition.MoonPhaseFull, Condition.TimeNight)
			.Add(ItemID.TheDoctorsShirt,				Condition.MoonPhaseWaningGibbous)
			.Add(ItemID.TheDoctorsPants,				Condition.MoonPhaseWaningGibbous)
			.Add(ItemID.FamiliarShirt)
			.Add(ItemID.FamiliarPants)
			.Add(ItemID.FamiliarWig)
			.Add(ItemID.ClownHat,						Condition.DownedClown)
			.Add(ItemID.ClownShirt,						Condition.DownedClown)
			.Add(ItemID.ClownPants,						Condition.DownedClown)
			.Add(ItemID.MimeMask,						Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoShirt,				Condition.BloodMoon)
			.Add(ItemID.FallenTuxedoPants,				Condition.BloodMoon)
			.Add(ItemID.WhiteLunaticHood,				Condition.TimeDay, Condition.DownedCultist)
			.Add(ItemID.WhiteLunaticRobe,				Condition.TimeDay, Condition.DownedCultist)
			.Add(ItemID.BlueLunaticHood,				Condition.TimeNight, Condition.DownedCultist)
			.Add(ItemID.BlueLunaticRobe,				Condition.TimeNight, Condition.DownedCultist)
			.Add(ItemID.TaxCollectorHat,				taxCollectorIsPresent)
			.Add(ItemID.TaxCollectorSuit,				taxCollectorIsPresent)
			.Add(ItemID.TaxCollectorPants,				taxCollectorIsPresent)
			.Add(ItemID.UndertakerHat,					Condition.InGraveyard)
			.Add(ItemID.UndertakerCoat,					Condition.InGraveyard)
			.Add(ItemID.FuneralHat,						Condition.InGraveyard)
			.Add(ItemID.FuneralCoat,					Condition.InGraveyard)
			.Add(ItemID.FuneralPants,					Condition.InGraveyard)
			.Add(ItemID.TragicUmbrella,					Condition.InGraveyard)
			.Add(ItemID.VictorianGothHat,				Condition.InGraveyard)
			.Add(ItemID.VictorianGothDress,				Condition.InGraveyard)
			.Add(ItemID.Beanie,							Condition.InSnow)
			.Add(ItemID.GuyFawkesMask,					Condition.Halloween)
			.Add(ItemID.TamOShanter,					Condition.Hardmode, Condition.MoonPhaseThirdQuarter)
			.Add(ItemID.GraduationCapBlue,				Condition.Hardmode, Condition.MoonPhaseWaningCrescent)
			.Add(ItemID.GraduationGownBlue,				Condition.Hardmode, Condition.MoonPhaseWaningCrescent)
			.Add(ItemID.Tiara,							Condition.Hardmode, Condition.MoonPhaseNew)
			.Add(ItemID.PrincessDress,					Condition.Hardmode, Condition.MoonPhaseNew)
			.Add(ItemID.GraduationCapMaroon,			Condition.Hardmode, Condition.MoonPhaseWaxingCrescent)
			.Add(ItemID.GraduationGownMaroon,			Condition.Hardmode, Condition.MoonPhaseWaxingCrescent)
			.Add(ItemID.CowboyHat,						Condition.Hardmode, Condition.MoonPhaseFirstQuarter)
			.Add(ItemID.CowboyJacket,					Condition.Hardmode, Condition.MoonPhaseFirstQuarter)
			.Add(ItemID.CowboyPants,					Condition.Hardmode, Condition.MoonPhaseFirstQuarter)
			.Add(ItemID.GraduationCapBlack,				Condition.Hardmode, Condition.MoonPhaseWaxingGibbous)
			.Add(ItemID.GraduationGownBlack,			Condition.Hardmode, Condition.MoonPhaseWaxingGibbous)
			.Add(ItemID.BallaHat,						Condition.DownedFrostLegion, Condition.TimeDay)
			.Add(ItemID.GangstaHat,						Condition.DownedFrostLegion, Condition.TimeNight)
			.Add(ItemID.ClothierJacket,					Condition.Halloween)
			.Add(ItemID.ClothierPants,					Condition.Halloween)
			.Add(ItemID.PartyBundleOfBalloonsAccessory,	Condition.BirthdayParty)
			.Add(ItemID.PartyBalloonAnimal,				Condition.BirthdayParty)
			.Add(ItemID.FlowerBoyHat,					Condition.BirthdayParty)
			.Add(ItemID.FlowerBoyShirt,					Condition.BirthdayParty)
			.Add(ItemID.FlowerBoyPants,					Condition.BirthdayParty)
			.Add(ItemID.HunterCloak,					Condition.GolfScoreOver(2000))
			.Add(ItemID.PlacePainting)
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
			.Add(ItemID.WirePipe)
			.Add(ItemID.PixelBox)
			.Add(ItemID.LaserRuler)
			.Add(ItemID.MechanicalLens)
			.Add(ItemID.EngineeringHelmet)
			.Add(ItemID.WireBulb)
			.Add(ItemID.Timer5Second)
			.Add(ItemID.Timer3Second)
			.Add(ItemID.Timer1Second)
			.Add(ItemID.TimerOneHalfSecond)
			.Add(ItemID.TimerOneFourthSecond)
			.Add(ItemID.MechanicsRod, Condition.NpcIsPresent(NPCID.Angler), Condition.MoonPhasesOdd)
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

		for (int i = ItemID.ChristmasTree; i <= ItemID.BlueAndYellowLights; i++) {
			shop.Add(i);
		}

		shop.Register();
	}

	private static void RegisterTruffle()
	{
		new NPCShop(NPCID.Truffle)
			.Add(ItemID.MushroomSpear,		Condition.DownedMechBossAny)
			.Add(ItemID.Hammush,			Condition.DownedMechBossAny)
			.Add(ItemID.MushroomCap)
			.Add(ItemID.Autohammer,			Condition.DownedPlantera)
			.Add(ItemID.StrangeGlowingMushroom)
			.Add(ItemID.MySon)
			.Add(ItemID.DarkBlueSolution,	Condition.NotRemixWorld)
			.Register();
	}

	private static void RegisterSteampunker()
	{
		var steampunkerOutfitCondition = new Condition("Conditions.MoonPhasesHalf0OrPreHardmode", () => Condition.PreHardmode.IsMet() || Condition.MoonPhasesHalf0.IsMet());
		new NPCShop(NPCID.Steampunker)
			.Add(ItemID.Clentaminator,		Condition.NotRemixWorld)
			.Add(ItemID.SteampunkHat,		steampunkerOutfitCondition)
			.Add(ItemID.SteampunkShirt,		steampunkerOutfitCondition)
			.Add(ItemID.SteampunkPants,		steampunkerOutfitCondition)
			.Add(ItemID.Jetpack,			Condition.Hardmode, Condition.MoonPhasesHalf1)
			.Add(ItemID.SteampunkWings,		Condition.DownedGolem)
			.Add(ItemID.StaticHook,			Condition.Hardmode)
			.Add(ItemID.LogicGate_AND)
			.Add(ItemID.LogicGate_OR)
			.Add(ItemID.LogicGate_XOR)
			.Add(ItemID.LogicGate_NAND)
			.Add(ItemID.LogicGate_NOR)
			.Add(ItemID.LogicGate_NXOR)
			.Add(ItemID.LogicGateLamp_On)
			.Add(ItemID.LogicGateLamp_Off)
			.Add(ItemID.LogicGateLamp_Faulty)
			.Add(ItemID.ConveyorBeltLeft)
			.Add(ItemID.ConveyorBeltRight)
			.Add(ItemID.BlendOMatic,		new Condition("Conditions.HardmodeFTW", () => Main.hardMode || !Main.getGoodWorld))
			.Add(ItemID.SteampunkBoiler,	Condition.DownedEyeOfCthulhu, Condition.DownedEowOrBoc, Condition.DownedSkeletron)
			.Add(ItemID.FleshCloningVaat,	Condition.CrimsonWorld)
			.Add(ItemID.LesionStation,		Condition.CorruptWorld)
			.Add(ItemID.BoneWelder,			Condition.InGraveyard)
			.Add(ItemID.HoneyDispenser,		Condition.InJungle)
			.Add(ItemID.IceMachine,			Condition.InSnow)
			.Add(ItemID.SkyMill,			Condition.InSpace)
			.Add(ItemID.LivingLoom,			Condition.PlayerCarriesItem(ItemID.LivingWoodWand))
			.Add(ItemID.RedSolution,		Condition.NotRemixWorld, Condition.EclipseOrBloodMoon, Condition.CrimsonWorld)
			.Add(ItemID.PurpleSolution,		Condition.NotRemixWorld, Condition.EclipseOrBloodMoon, Condition.CorruptWorld)
			.Add(ItemID.BlueSolution,		Condition.NotRemixWorld, Condition.NotEclipseAndNotBloodMoon, Condition.InHallow)
			.Add(ItemID.GreenSolution,		Condition.NotRemixWorld, Condition.NotEclipseAndNotBloodMoon, Condition.NotInHallow)
			.Add(ItemID.SandSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.SnowSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.DirtSolution,		Condition.NotRemixWorld, Condition.DownedMoonLord)
			.Add(ItemID.Cog,				Condition.Hardmode)
			.Add(ItemID.SteampunkMinecart,	Condition.Hardmode)
			.Add(ItemID.SteampunkGoggles,	Condition.Halloween)
			.Register();
	}

	private static void RegisterDyeTrader()
	{
		new NPCShop(NPCID.DyeTrader)
			.Add(ItemID.SilverDye)
			.Add(ItemID.BrownDye)
			.Add(ItemID.DyeVat)
			.Add(ItemID.TeamDye,			Condition.Multiplayer)
			.Add(ItemID.DyeTraderTurban,	Condition.Halloween)
			.Add(ItemID.DyeTraderRobe,		Condition.Halloween)
			.Add(ItemID.ShadowDye,			Condition.MoonPhaseFull)
			.Add(ItemID.NegativeDye,		Condition.MoonPhaseFull)
			.Add(ItemID.BloodbathDye,		Condition.BloodMoon)
			.Add(ItemID.FogboundDye,		Condition.InGraveyard)
			.Register();
	}

	private static void RegisterPartyGirl()
	{
		new NPCShop(NPCID.PartyGirl)
			.Add(ItemID.BeachBall)
			.Add(ItemID.Football,				Condition.GolfScoreOver(500))
			.Add(ItemID.ConfettiGun)
			.Add(ItemID.SmokeBomb)
			.Add(ItemID.BubbleMachine,			Condition.TimeDay)
			.Add(ItemID.FogMachine,				Condition.TimeNight)
			.Add(ItemID.Confetti)
			.Add(ItemID.BubbleWand)
			.Add(ItemID.LavaLamp)
			.Add(ItemID.PlasmaLamp)
			.Add(ItemID.FireworksBox)
			.Add(ItemID.FireworkFountain)
			.Add(ItemID.PartyMinecart)
			.Add(ItemID.KiteSpectrum)
			.Add(ItemID.ReleaseDoves,			Condition.InGraveyard)
			.Add(ItemID.ReleaseLantern,			Condition.LanternNight)
			.Add(ItemID.PartyGirlGrenade,		Condition.PlayerCarriesItem(ItemID.PartyGirlGrenade))
			.Add(ItemID.ConfettiCannon,			Condition.NpcIsPresent(NPCID.Pirate))
			.Add(ItemID.FireworksLauncher,		Condition.DownedGolem)
			.Add(ItemID.Bubble,					Condition.Hardmode)
			.Add(ItemID.SmokeBlock,				Condition.Hardmode)
			.Add(ItemID.RedRocket,				Condition.Hardmode)
			.Add(ItemID.GreenRocket,			Condition.Hardmode)
			.Add(ItemID.BlueRocket,				Condition.Hardmode)
			.Add(ItemID.YellowRocket,			Condition.Hardmode)
			.Add(ItemID.PogoStick)
			.Add(ItemID.PartyMonolith)
			.Add(ItemID.PartyHat)
			.Add(ItemID.SillyBalloonMachine)
			.Add(ItemID.PartyPresent,			Condition.BirthdayParty)
			.Add(ItemID.Pigronata,				Condition.BirthdayParty)
			.Add(ItemID.SillyStreamerBlue,		Condition.BirthdayParty)
			.Add(ItemID.SillyStreamerGreen,		Condition.BirthdayParty)
			.Add(ItemID.SillyStreamerPink,		Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonPurple,		Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonGreen,		Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonPink,		Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonTiedGreen,	Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonTiedPurple, Condition.BirthdayParty)
			.Add(ItemID.SillyBalloonTiedPink,	Condition.BirthdayParty)
			.Register();
	}

	private static void RegisterCyborg()
	{
		var portalGunStation = new Condition(Language.GetText("Conditions.PlayerCarriesItem2").WithFormatArgs(Lang.GetItemName(ItemID.PortalGun), Lang.GetItemName(ItemID.PortalGunStation)),
			() => Main.LocalPlayer.HasItem(ItemID.PortalGun) || Main.LocalPlayer.HasItem(ItemID.PortalGunStation));

		new NPCShop(NPCID.Cyborg)
			.Add(ItemID.RocketI)
			.Add(ItemID.RocketII,				Condition.BloodMoon)
			.Add(ItemID.RocketIII,				Condition.NightOrEclipse)
			.Add(ItemID.RocketIV,				Condition.Eclipse)
			.Add(ItemID.ClusterRocketI,			Condition.DownedMartians)
			.Add(ItemID.ClusterRocketII,		Condition.DownedMartians, Condition.EclipseOrBloodMoon)
			.Add(ItemID.DryRocket,				Condition.Hardmode)
			.Add(ItemID.ProximityMineLauncher,	Condition.Hardmode)
			.Add(ItemID.Nanites,				Condition.Hardmode)
			.Add(ItemID.JimsDrone,				Condition.Hardmode)
			.Add(ItemID.JimsDroneVisor,			Condition.Hardmode)
			.Add(ItemID.SpectreGoggles,			Condition.InGraveyard)
			.Add(ItemID.EchoBlock,				Condition.InGraveyard)
			.Add(ItemID.CyborgHelmet,			Condition.Halloween)
			.Add(ItemID.CyborgShirt,			Condition.Halloween)
			.Add(ItemID.CyborgPants,			Condition.Halloween)
			.Add(ItemID.HiTekSunglasses,		Condition.DownedMartians)
			.Add(ItemID.NightVisionHelmet,		Condition.DownedMartians)
			.Add(ItemID.PortalGunStation,		portalGunStation)
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
			.Add(ItemID.GlowPaint,			Condition.InGraveyard)
			.Add(ItemID.EchoCoating,		Condition.InGraveyard, Condition.DownedPlantera)
			.Register();

		new NPCShop(NPCID.Painter, "Decor") // Decor shop
			.Add(ItemID.ChristmasTreeWallpaper,		Condition.Christmas)
			.Add(ItemID.OrnamentWallpaper,			Condition.Christmas)
			.Add(ItemID.CandyCaneWallpaper,			Condition.Christmas)
			.Add(ItemID.FestiveWallpaper,			Condition.Christmas)
			.Add(ItemID.StarsWallpaper,				Condition.Christmas)
			.Add(ItemID.SquigglesWallpaper,			Condition.Christmas)
			.Add(ItemID.SnowflakeWallpaper,			Condition.Christmas)
			.Add(ItemID.KrampusHornWallpaper,		Condition.Christmas)
			.Add(ItemID.BluegreenWallpaper,			Condition.Christmas)
			.Add(ItemID.GrinchFingerWallpaper,		Condition.Christmas)
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
			.Add(ItemID.Daylight,					Condition.NotInGraveyard)
			.Add(ItemID.FirstEncounter,				Condition.NotInGraveyard, Condition.MoonPhasesQuarter0)
			.Add(ItemID.GoodMorning,				Condition.NotInGraveyard, Condition.MoonPhasesQuarter1)
			.Add(ItemID.UndergroundReward,			Condition.NotInGraveyard, Condition.MoonPhasesQuarter2)
			.Add(ItemID.ThroughtheWindow,			Condition.NotInGraveyard, Condition.MoonPhasesQuarter3)
			.Add(ItemID.Purity,						Condition.InShoppingZoneForest)
			.Add(ItemID.DeadlandComesAlive,			Condition.InCrimson)
			.Add(ItemID.LightlessChasms,			Condition.InCorrupt)
			.Add(ItemID.TheLandofDeceivingLooks,	Condition.InHallow)
			.Add(ItemID.DoNotStepontheGrass,		Condition.InJungle)
			.Add(ItemID.ColdWatersintheWhiteLand,	Condition.InSnow)
			.Add(ItemID.SecretoftheSands,			Condition.InDesert)
			.Add(ItemID.EvilPresence,				Condition.BloodMoon)
			.Add(ItemID.PlaceAbovetheClouds,		Condition.NotInGraveyard, Condition.InSpace)
			.Add(ItemID.SkyGuardian,				Condition.NotInGraveyard, Condition.Hardmode, Condition.InSpace)
			.Add(ItemID.Thunderbolt,				Condition.Thunderstorm)
			.Add(ItemID.Nevermore,					Condition.InGraveyard)
			.Add(ItemID.Reborn,						Condition.InGraveyard)
			.Add(ItemID.Graveyard,					Condition.InGraveyard)
			.Add(ItemID.GhostManifestation,			Condition.InGraveyard)
			.Add(ItemID.WickedUndead,				Condition.InGraveyard)
			.Add(ItemID.HailtotheKing,				Condition.InGraveyard)
			.Add(ItemID.BloodyGoblet,				Condition.InGraveyard)
			.Add(ItemID.StillLife,					Condition.InGraveyard)
			.Register();
	}

	private static void RegisterWitchDoctor()
	{
		new NPCShop(NPCID.WitchDoctor)
			.Add(ItemID.ImbuingStation)
			.Add(ItemID.Blowgun)
			.Add(ItemID.BewitchingTable,	Condition.NpcIsPresent(NPCID.Wizard))
			.Add(ItemID.PygmyNecklace,		Condition.TimeNight)
			.Add(ItemID.TikiMask,			Condition.DownedPlantera)
			.Add(ItemID.TikiShirt,			Condition.DownedPlantera)
			.Add(ItemID.TikiPants,			Condition.DownedPlantera)
			.Add(ItemID.HerculesBeetle,		Condition.DownedPlantera, Condition.InJungle)
			.Add(ItemID.VialofVenom,		Condition.DownedPlantera)
			.Add(ItemID.TikiTotem,			Condition.Hardmode, Condition.InJungle)
			.Add(ItemID.LeafWings,			Condition.Hardmode, Condition.InJungle, Condition.TimeNight, Condition.DownedPlantera)
			.Add(ItemID.PureWaterFountain)
			.Add(ItemID.DesertWaterFountain)
			.Add(ItemID.JungleWaterFountain)
			.Add(ItemID.IcyWaterFountain)
			.Add(ItemID.CorruptWaterFountain)
			.Add(ItemID.CrimsonWaterFountain)
			.Add(ItemID.HallowedWaterFountain)
			.Add(ItemID.BloodWaterFountain)
			.Add(ItemID.CavernFountain)
			.Add(ItemID.OasisFountain)
			.Add(ItemID.Stake,				Condition.PlayerCarriesItem(ItemID.StakeLauncher))
			.Add(ItemID.StyngerBolt,		Condition.PlayerCarriesItem(ItemID.Stynger))
			.Add(ItemID.Cauldron,			Condition.Halloween)
			.Register();
	}

	private static void RegisterPirate()
	{
		var beachCondition = new Condition("Conditions.InBeach", () => {
			int num6 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
			return (double)(Main.screenPosition.Y / 16f) < Main.worldSurface + 10.0 && (num6 < 380 || num6 > Main.maxTilesX - 380);
		});

		new NPCShop(NPCID.Pirate)
			.Add(ItemID.Cannon)
			.Add(ItemID.Cannonball)
			.Add(ItemID.PirateHat)
			.Add(ItemID.PirateShirt)
			.Add(ItemID.PiratePants)
			.Add(ItemID.Sail)
			.Add(ItemID.ParrotCracker,	beachCondition)
			.Add(ItemID.BunnyCannon,	Condition.NpcIsPresent(NPCID.PartyGirl), Condition.Hardmode, Condition.DownedMechBossAny)
			.Register();
	}

	private static void RegisterStylist()
	{
		var maxLife = new Condition(Language.GetText("Conditions.AtleastXHealth").WithFormatArgs(400), () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
		var maxMana = new Condition(Language.GetText("Conditions.AtleastXMana").WithFormatArgs(200), () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
		var moneyHair = new Condition("Conditions.PlatinumCoin", () => {
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
		var timeHair = new Condition("Conditions.StyleMoon", () => Main.moonPhase % 2 == (!Main.dayTime).ToInt());
		var teamHair = new Condition("Conditions.OnTeam", () => Main.LocalPlayer.team != 0);

		new NPCShop(NPCID.Stylist)
			.Add(ItemID.HairDyeRemover)
			.Add(ItemID.DepthHairDye)
			.Add(ItemID.LifeHairDye,		maxLife)
			.Add(ItemID.ManaHairDye,		maxMana)
			.Add(ItemID.MoneyHairDye,		moneyHair)
			.Add(ItemID.TimeHairDye,		timeHair)
			.Add(ItemID.TeamHairDye,		teamHair)
			.Add(ItemID.BiomeHairDye,		Condition.Hardmode)
			.Add(ItemID.PartyHairDye,		Condition.NpcIsPresent(NPCID.PartyGirl))
			.Add(ItemID.RainbowHairDye,		Condition.Hardmode, Condition.DownedTwins, Condition.DownedSkeletronPrime, Condition.DownedDestroyer)
			.Add(ItemID.SpeedHairDye,		Condition.Hardmode, Condition.DownedMechBossAny)
			.Add(ItemID.MartianHairDye,		Condition.DownedMartians)
			.Add(ItemID.TwilightHairDye,	Condition.DownedMartians)
			.Add(ItemID.WilsonBeardShort)
			.Register();
	}

	private static void RegisterSkeletonMerchant()
	{
		var spelunkerGlowCondition = new Condition("Conditions.NightDayFullMoon", () => !Main.dayTime || Main.moonPhase == 0);
		var glowstickCondition = new Condition("Conditions.DaytimeNotFullMoon", () => Main.dayTime && Main.moonPhase != 0);
		var artisanCondition = new Condition("Conditions.NoAteLoaf", () => !Main.LocalPlayer.ateArtisanBread);

		// these two are probably a bug, meant to cycle every 3 minutes
		var boneTorchCondition = new Condition("Conditions.Periodically", () => Main.time % 60 <= 30);
		var torchCondition = new Condition("Conditions.Periodically", () => Main.time % 60 > 30);

		new NPCShop(NPCID.SkeletonMerchant)
			.Add(ItemID.WoodenBoomerang,		Condition.MoonPhaseFull)
			.Add(ItemID.Umbrella,				Condition.MoonPhaseWaningGibbous)
			.Add(ItemID.WandofSparking,			Condition.MoonPhaseThirdQuarter, Condition.NotRemixWorld)
			.Add(ItemID.MagicDagger,			Condition.MoonPhaseThirdQuarter, Condition.RemixWorld)
			.Add(ItemID.PortableStool,			Condition.MoonPhaseWaningCrescent)
			.Add(ItemID.Aglet,					Condition.MoonPhaseNew)
			.Add(ItemID.ClimbingClaws,			Condition.MoonPhaseWaxingCrescent)
			.Add(ItemID.CordageGuide,			Condition.MoonPhaseFirstQuarter)
			.Add(ItemID.Radar,					Condition.MoonPhaseWaxingGibbous)
			.Add(ItemID.StrangeBrew,			Condition.MoonPhasesEven)
			.Add(ItemID.LesserHealingPotion,	Condition.MoonPhasesOdd)
			.Add(ItemID.HealingPotion,			Condition.Hardmode, Condition.MoonPhasesOdd)
			.Add(ItemID.SpelunkerGlowstick,		spelunkerGlowCondition)
			.Add(ItemID.SpelunkerFlare,			spelunkerGlowCondition, Condition.PlayerCarriesItem(ItemID.FlareGun))
			.Add(ItemID.Glowstick,				glowstickCondition)
			.Add(ItemID.BoneTorch,				boneTorchCondition)
			.Add(ItemID.Torch,					torchCondition)
			.Add(ItemID.BoneArrow,				Condition.MoonPhasesEvenQuarters)
			.Add(ItemID.WoodenArrow,			Condition.MoonPhasesOddQuarters)
			.Add(ItemID.BlueCounterweight,		Condition.MoonPhases04)
			.Add(ItemID.RedCounterweight,		Condition.MoonPhases15)
			.Add(ItemID.PurpleCounterweight,	Condition.MoonPhases26)
			.Add(ItemID.GreenCounterweight,		Condition.MoonPhases37)
			.Add(ItemID.Bomb)
			.Add(ItemID.Rope)
			.Add(ItemID.Gradient,				Condition.Hardmode, Condition.MoonPhasesHalf0)
			.Add(ItemID.FormatC,				Condition.Hardmode, Condition.MoonPhasesHalf1)
			.Add(ItemID.YoYoGlove,				Condition.Hardmode)
			.Add(ItemID.SlapHand,				Condition.Hardmode, Condition.BloodMoon)
			.Add(ItemID.MagicLantern,			Condition.TimeNight, Condition.MoonPhaseFull)
			.Add(ItemID.ArtisanLoaf,			artisanCondition, Condition.MoonPhasesNearNew)
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
		shop.Add(new Entry(ItemID.DD2ElderCrystal).AddShopOpenedCallback((item, npc) => {					// Eternia Crystal
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
		shop.Add(ItemID.DD2ElderCrystalStand);																// Eternia Crystal Stand
		AddEntry(ItemID.DefendersForge, 50);

		AddEntry(ItemID.SquireGreatHelm,			15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.SquirePlating,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.SquireGreaves,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.SquireAltHead,				50, Condition.Hardmode, Condition.DownedGolem);			// Valhalla Knight's Helm
		AddEntry(ItemID.SquireAltShirt,				50, Condition.Hardmode, Condition.DownedGolem);         // Valhalla Knight's Breastplate
		AddEntry(ItemID.SquireAltPants,				50, Condition.Hardmode, Condition.DownedGolem);         // Valhalla Knight's Greaves

		// 2nd row
		AddEntry(ItemID.DD2FlameburstTowerT1Popper,	5);														// Flameburst Rod
		AddEntry(ItemID.DD2BallistraTowerT1Popper,	5);														// Ballista Rod
		AddEntry(ItemID.DD2ExplosiveTrapT1Popper,	5);														// Explosive Trap Rod
		AddEntry(ItemID.DD2LightningAuraT1Popper,	5);														// Lightning Aura Rod
		AddEntry(ItemID.ApprenticeHat,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeRobe,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeTrousers,			15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.ApprenticeAltHead,			50, Condition.Hardmode, Condition.DownedGolem);         // Dark Atrist's Hat
		AddEntry(ItemID.ApprenticeAltShirt,			50, Condition.Hardmode, Condition.DownedGolem);         // Dark Atrist's Robes
		AddEntry(ItemID.ApprenticeAltPants,			50, Condition.Hardmode, Condition.DownedGolem);         // Dark Atrist's Leggings

		// 3rd row
		AddEntry(ItemID.DD2FlameburstTowerT2Popper, 15, Condition.Hardmode, Condition.DownedMechBossAny);   // Flameburst Cane
		AddEntry(ItemID.DD2BallistraTowerT2Popper,	15, Condition.Hardmode, Condition.DownedMechBossAny);   // Ballista Cane
		AddEntry(ItemID.DD2ExplosiveTrapT2Popper,	15, Condition.Hardmode, Condition.DownedMechBossAny);   // Explosive Trap Cane
		AddEntry(ItemID.DD2LightningAuraT2Popper,	15, Condition.Hardmode, Condition.DownedMechBossAny);   // Lightning Aura Cane
		AddEntry(ItemID.HuntressWig,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressJerkin,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressPants,				15, Condition.Hardmode, Condition.DownedMechBossAny);
		AddEntry(ItemID.HuntressAltHead,			50, Condition.Hardmode, Condition.DownedGolem);         // Red Riding Hood
		AddEntry(ItemID.HuntressAltShirt,			50, Condition.Hardmode, Condition.DownedGolem);         // Red Riding Dress
		AddEntry(ItemID.HuntressAltPants,			50, Condition.Hardmode, Condition.DownedGolem);         // Red Riding Leggings

		// 4th row
		AddEntry(ItemID.DD2FlameburstTowerT3Popper, 60, Condition.Hardmode, Condition.DownedGolem);         // Flameburst Staff
		AddEntry(ItemID.DD2BallistraTowerT3Popper,	60, Condition.Hardmode, Condition.DownedGolem);         // Ballista Staff
		AddEntry(ItemID.DD2ExplosiveTrapT3Popper,	60, Condition.Hardmode, Condition.DownedGolem);         // Explosive Trap Staff
		AddEntry(ItemID.DD2LightningAuraT3Popper,	60, Condition.Hardmode, Condition.DownedGolem);         // Lightning Aura Staff
		AddEntry(ItemID.MonkBrows,					15, Condition.Hardmode, Condition.DownedMechBossAny);   // Monk's Bushy Brow Bald Cap
		AddEntry(ItemID.MonkShirt,					15, Condition.Hardmode, Condition.DownedMechBossAny);   // Monk's Shirt
		AddEntry(ItemID.MonkPants,					15, Condition.Hardmode, Condition.DownedMechBossAny);   // Monk's Pants
		AddEntry(ItemID.MonkAltHead,				50, Condition.Hardmode, Condition.DownedGolem);         // Shinobi Infiltrator's Helmet
		AddEntry(ItemID.MonkAltShirt,				50, Condition.Hardmode, Condition.DownedGolem);         // Shinobi Infiltrator's Torso
		AddEntry(ItemID.MonkAltPants,				50, Condition.Hardmode, Condition.DownedGolem);         // Shinobi Infiltrator's Pants

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
			.Add(ItemID.GolfClubIron,				scoreOver500)                                               // Golf Club (Iron)
			.Add(ItemID.GolfClubDriver,				scoreOver500)                                               // Golf Club (Driver)
			.Add(ItemID.GolfClubWedge,				scoreOver500)                                               // Golf Club (Wedge)
			.Add(ItemID.GolfClubPutter,				scoreOver500)                                               // Golf Club (Putter)
			.Add(ItemID.GolfTee)
			.Add(ItemID.GolfBall)
			.Add(ItemID.GolfWhistle)
			.Add(ItemID.GolfCup)
			.Add(ItemID.ArrowSign)
			.Add(ItemID.PaintedArrowSign)
			.Add(ItemID.GolfClubMythrilIron,		scoreOver1000)                                              // Fancy Golf Club (Iron)
			.Add(ItemID.GolfClubPearlwoodDriver,	scoreOver1000)                                              // Fancy Golf Club (Driver)
			.Add(ItemID.GolfClubGoldWedge,			scoreOver1000)                                              // Fancy Golf Club (Wedge)
			.Add(ItemID.GolfClubLeadPutter,			scoreOver1000)                                              // Fancy Golf Club (Putter)
			.Add(ItemID.GolfHat)																				// Country Club Cap
			.Add(ItemID.GolfVisor)                                                                              // Country Club Visor
			.Add(ItemID.GolfShirt)                                                                              // Country Club Vest
			.Add(ItemID.GolfPants)                                                                              // Country Club Trousers
			.Add(ItemID.LawnMower)
			.Add(ItemID.GolfChest,					scoreOver500)
			.Add(ItemID.GolfClubTitaniumIron,		scoreOver2000)                                              // Premium Golf Club (Iron)
			.Add(ItemID.GolfClubChlorophyteDriver,	scoreOver2000)                                              // Premium Golf Club (Driver)
			.Add(ItemID.GolfClubDiamondWedge,		scoreOver2000)                                              // Premium Golf Club (Wedge)
			.Add(ItemID.GolfClubShroomitePutter,	scoreOver2000)                                              // Premium Golf Club (Putter)
			.Add(ItemID.GolfCart,					scoreOver2000, Condition.DownedSkeletron)					// Golf Cart Keys
			.Add(ItemID.GolfTrophyBronze,			scoreOver500)												// Bronze Golf Trophy
			.Add(ItemID.GolfTrophySilver,			scoreOver1000)												// Silver Golf Trophy
			.Add(ItemID.GolfTrophyGold,				scoreOver2000)												// Gold Golf Trophy
			.Add(ItemID.GolfPainting1,				scoreOver2000, Condition.MoonPhasesQuarter0)				// The Rolling Greens
			.Add(ItemID.GolfPainting2,				scoreOver2000, Condition.MoonPhasesQuarter1)				// Study of a Ball at Rest
			.Add(ItemID.GolfPainting3,				scoreOver2000, Condition.MoonPhasesQuarter2)				// Fore!
			.Add(ItemID.GolfPainting4,				scoreOver2000, Condition.MoonPhasesQuarter3)				// The Duplicity of Reflections
			.Register();
	}

	private static void RegisterZoologist()
	{
		new NPCShop(NPCID.BestiaryGirl)
			.Add(ItemID.FairyGlowstick,				new Condition("Conditions.BestiaryWinx", () => Chest.BestiaryGirl_IsFairyTorchAvailable()))
			.Add(ItemID.DontHurtCrittersBook)
			.Add(ItemID.SquirrelHook)
			.Add(ItemID.TheWerewolf,				Condition.MoonPhaseFull, Condition.TimeNight)
			.Add(ItemID.BlandWhip,					Condition.BestiaryFilledPercent(10))
			.Add(ItemID.LicenseCat)
			.Add(ItemID.LicenseDog,					Condition.BestiaryFilledPercent(25))
			.Add(ItemID.LicenseBunny,				Condition.BestiaryFilledPercent(45))
			.Add(ItemID.VanityTreeSakuraSeed,		Condition.BestiaryFilledPercent(30))
			.Add(ItemID.VanityTreeYellowWillowSeed, Condition.BestiaryFilledPercent(30))
			.Add(ItemID.KiteCrawltipede,			Condition.DownedSolarPillar)
			.Add(ItemID.KiteKoi,					Condition.BestiaryFilledPercent(10))
			.Add(ItemID.CritterShampoo,				Condition.BestiaryFilledPercent(30))
			.Add(ItemID.MolluskWhistle,				Condition.BestiaryFilledPercent(25))
			.Add(ItemID.PaintedHorseSaddle,			Condition.BestiaryFilledPercent(30))
			.Add(ItemID.MajesticHorseSaddle,		Condition.BestiaryFilledPercent(30))
			.Add(ItemID.DarkHorseSaddle,			Condition.BestiaryFilledPercent(30))
			.Add(ItemID.JoustingLance,				Condition.BestiaryFilledPercent(30), Condition.Hardmode)
			.Add(ItemID.DiggingMoleMinecart,		Condition.BestiaryFilledPercent(35))
			.Add(ItemID.RabbitOrder,				Condition.BestiaryFilledPercent(40))
			.Add(ItemID.FullMoonSqueakyToy,			Condition.Hardmode, Condition.BloodMoon)
			.Add(ItemID.MudBud,						Condition.DownedPlantera)
			.Add(ItemID.TreeGlobe,					Condition.BestiaryFilledPercent(50))
			.Add(ItemID.WorldGlobe,					Condition.BestiaryFilledPercent(50))
			.Add(ItemID.MoonGlobe,					Condition.BestiaryFilledPercent(50))
			.Add(ItemID.LightningCarrot,			Condition.BestiaryFilledPercent(50))
			.Add(ItemID.BallOfFuseWire,				Condition.BestiaryFilledPercent(70))
			.Add(ItemID.TeleportationPylonVictory,	Condition.BestiaryFilledPercent(100))
			.Add(ItemID.DogEars,					Condition.MoonPhasesQuarter0)
			.Add(ItemID.DogTail,					Condition.MoonPhasesQuarter0)
			.Add(ItemID.FoxEars,					Condition.MoonPhasesQuarter1)
			.Add(ItemID.FoxTail,					Condition.MoonPhasesQuarter1)
			.Add(ItemID.LizardEars,					Condition.MoonPhasesQuarter2)
			.Add(ItemID.LizardTail,					Condition.MoonPhasesQuarter2)
			.Add(ItemID.BunnyEars,					Condition.MoonPhasesQuarter3)
			.Add(ItemID.BunnyTail,					Condition.MoonPhasesQuarter3)
			.Register();
	}

	private static void RegisterPrincess()
	{
		var shop = new NPCShop(NPCID.Princess)
			.Add(ItemID.RoyalTiara)
			.Add(ItemID.RoyalDressTop)
			.Add(ItemID.RoyalDressBottom);
		for (int i = ItemID.RoyalScepter; i <= ItemID.DarkSideHallow; i++) {
			shop.Add(i);
		}
		shop.Add(ItemID.PrincessStyle)
			.Add(ItemID.SuspiciouslySparkly)
			.Add(ItemID.TerraBladeChronicles)
			.Add(ItemID.RoyalRomance,		Condition.DownedKingSlime, Condition.DownedQueenSlime)
			.Add(ItemID.MusicBoxCredits,	Condition.Hardmode, Condition.DownedMoonLord)
			.Add(ItemID.SlimeStaff,			Condition.TenthAnniversaryWorld)
			.Add(ItemID.HeartLantern,		Condition.TenthAnniversaryWorld)
			.Add(ItemID.FlaskofParty,		Condition.TenthAnniversaryWorld)
			.Add(ItemID.SandstorminaBottle, Condition.TenthAnniversaryWorld, Condition.InDesert)
			.Add(ItemID.Terragrim,			Condition.TenthAnniversaryWorld, Condition.BloodMoon)
			.Add(ItemID.PirateStaff,		Condition.TenthAnniversaryWorld, Condition.Hardmode, Condition.DownedPirates, Condition.MoonPhasesQuarter0)
			.Add(ItemID.DiscountCard,		Condition.TenthAnniversaryWorld, Condition.Hardmode, Condition.DownedPirates, Condition.MoonPhasesQuarter1)
			.Add(ItemID.LuckyCoin,			Condition.TenthAnniversaryWorld, Condition.Hardmode, Condition.DownedPirates, Condition.MoonPhasesQuarter2)
			.Add(ItemID.CoinGun,			Condition.TenthAnniversaryWorld, Condition.Hardmode, Condition.DownedPirates, Condition.MoonPhasesQuarter3)
			.Add(ItemID.BerniePetItem)
			.Register();
	}

	private static void RegisterTravellingMerchant()
	{
		new TravellingMerchantShop(NPCID.TravellingMerchant)
			.AddInfoEntry(ItemID.BlackCounterweight)
			.AddInfoEntry(ItemID.YellowCounterweight)
			.AddInfoEntry(ItemID.AngelHalo)
			.AddInfoEntry(ItemID.Gatligator, Condition.Hardmode)
			.AddInfoEntry(ItemID.BouncingShield, Condition.Hardmode)
			.AddInfoEntry(ItemID.Kimono)
			.AddInfoEntry(ItemID.ArcaneRuneWall)
			.AddInfoEntry(ItemID.PulseBow, Condition.DownedDestroyer, Condition.DownedTwins, Condition.DownedSkeletronPrime)
			.AddInfoEntry(ItemID.WaterGun)
			.AddInfoEntry(ItemID.DiamondRing)
			.AddInfoEntry(ItemID.CrimsonCloak)
			.AddInfoEntry(ItemID.MysteriousCape)
			.AddInfoEntry(ItemID.RedCape)
			.AddInfoEntry(ItemID.WinterCape)
			.AddInfoEntry(ItemID.HunterCloak)
			.AddInfoEntry(ItemID.SittingDucksFishingRod, Condition.DownedSkeletron)
			.AddInfoEntry(ItemID.CompanionCube)
			.AddInfoEntry(ItemID.AntiPortalBlock, Condition.Hardmode)
			.AddInfoEntry(ItemID.BirdieRattle)
			.AddInfoEntry(ItemID.ExoticEasternChewToy)
			.AddInfoEntry(ItemID.BlueEgg)
			.AddInfoEntry(ItemID.BedazzledNectar)
			.AddInfoEntry(ItemID.BambooLeaf)
			.AddInfoEntry(ItemID.Pho)
			.AddInfoEntry(ItemID.Revolver, Condition.SmashedShadowOrb)
			.AddInfoEntry(ItemID.Fez)
			.AddInfoEntry(ItemID.MagicHat)
			.AddInfoEntry(ItemID.GypsyRobe)
			.AddInfoEntry(ItemID.Gi)
			.AddInfoEntry(ItemID.ChefHat)
			.AddInfoEntry(ItemID.GameMasterShirt)
			.AddInfoEntry(ItemID.StarPrincessCrown)
			.AddInfoEntry(ItemID.LincolnsHood)
			.AddInfoEntry(ItemID.DemonHorns)
			.AddInfoEntry(ItemID.DevilHorns)
			.AddInfoEntry(ItemID.PandaEars)
			.AddInfoEntry(ItemID.VulkelfEar)
			.AddInfoEntry(ItemID.GoblorcEar)
			.AddInfoEntry(ItemID.Fedora)
			.AddInfoEntry(ItemID.StarHairpin)
			.AddInfoEntry(ItemID.HeartHairpin)
			.AddInfoEntry(ItemID.UnicornHornHat)
			.AddInfoEntry(ItemID.PrettyPinkRibbon)
			.AddInfoEntry(ItemID.ZapinatorGray, Condition.DownedEarlygameBoss)
			.AddInfoEntry(ItemID.ZapinatorOrange, Condition.Hardmode)
			.AddInfoEntry(ItemID.Code1, Condition.DownedEyeOfCthulhu)
			.AddInfoEntry(ItemID.Code2, Condition.DownedMechBossAny)
			.AddInfoEntry(ItemID.PadThai)
			.AddInfoEntry(ItemID.BrickLayer)
			.AddInfoEntry(ItemID.ExtendoGrip)
			.AddInfoEntry(ItemID.PaintSprayer)
			.AddInfoEntry(ItemID.PortableCementMixer)
			.AddInfoEntry(ItemID.ActuationAccessory)
			.AddInfoEntry(ItemID.Keybrand, Condition.RemixWorld)
			.AddInfoEntry(ItemID.Katana, Condition.NotRemixWorld)
			.AddInfoEntry(ItemID.UltrabrightTorch)
			.AddInfoEntry(ItemID.Sake)
			.AddInfoEntry(ItemID.TigerSkin)
			.AddInfoEntry(ItemID.LeopardSkin)
			.AddInfoEntry(ItemID.ZebraSkin)
			.AddInfoEntry(ItemID.SteampunkCup)
			.AddInfoEntry(ItemID.FancyDishes)
			.AddInfoEntry(ItemID.DynastyWood)
			.AddInfoEntry(ItemID.TeamBlockWhite)
			.AddInfoEntry(ItemID.LawnFlamingo)
			.AddInfoEntry(ItemID.DPSMeter)
			.AddInfoEntry(ItemID.LifeformAnalyzer)
			.AddInfoEntry(ItemID.Stopwatch)
			.Register();
	}
}

public class TravellingMerchantShop : AbstractNPCShop
{
	private new record Entry(Item Item, IEnumerable<Condition> Conditions) : AbstractNPCShop.Entry { }

	private List<Entry> _entries = new();

	public override IEnumerable<AbstractNPCShop.Entry> ActiveEntries => _entries;

	public TravellingMerchantShop(int npcType) : base(npcType) { }

	public TravellingMerchantShop AddInfoEntry(Item item, params Condition[] conditions)
	{
		_entries.Add(new Entry(item, conditions.ToList()));
		return this;
	}

	public TravellingMerchantShop AddInfoEntry(int item, params Condition[] conditions) => AddInfoEntry(ContentSamples.ItemsByType[item], conditions);

	public override void FillShop(ICollection<Item> items, NPC npc)
	{
		foreach (var itemId in Main.travelShop) {
			if (itemId != 0)
				items.Add(new Item(itemId));
		}
	}

	public override void FillShop(Item[] items, NPC npc, out bool overflow)
	{
		overflow = false;

		int i = 0;
		foreach (var itemId in Main.travelShop) {
			if (itemId == 0)
				continue;

			items[i++] = new Item(itemId);
		}
	}
}