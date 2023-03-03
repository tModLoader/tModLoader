using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

public static class NPCShopDatabase
{
	private static Dictionary<string, NPCShop> npcShopByName = new();
	private static List<NPCShop.Entry> globalNpcShopEntries = new();

	public static void RegisterNpcShop(int npcId, NPCShop chestLoot, string shopName = "Shop") {
		npcShopByName.Add(GetNPCShopName(npcId, shopName), chestLoot);
	}

	public static void RegisterGlobalNpcShop(NPCShop.Entry entry) => globalNpcShopEntries.Add(entry);
	public static List<NPCShop.Entry> GetGlobalNpcShopEntries() => globalNpcShopEntries;

	public static NPCShop GetNPCShop(string fullName) {
		if (npcShopByName.TryGetValue(fullName, out NPCShop chestLoot))
			return chestLoot;

		return null;
	}

	public static string GetNPCShopName(int npcId, string shopName = "Shop") {
		return $"{(npcId < NPCID.Count ? $"Terraria/{NPCID.Search.GetName(npcId)}" : NPCLoader.GetNPC(npcId).FullName)}/{shopName}";
	}

	public static Dictionary<string, NPCShop> GetNpcShopsOf(int npcId) {
		return npcShopByName.Where(x => {
			var split = x.Key.Split('/');
			return split[0] == (npcId < NPCID.Count ? "Terraria" : NPCLoader.GetNPC(npcId).Mod.Name)
			&& split[1] == (npcId < NPCID.Count ? NPCID.Search.GetName(npcId) : NPCLoader.GetNPC(npcId).Name);
		}).ToDictionary(x => x.Key, x => x.Value);
	}

	public static string GetNpcShopByIndex(int index)
	{
		int npcType = NPCLoader.shopToNPC[index];
		if (index == 25) { // Painter 2 Shop Special Case
			return GetNPCShopName(npcType, "Decor");
		}
		return GetNPCShopName(npcType);
	}

	public static void Initialize() {
		npcShopByName.Clear();
		globalNpcShopEntries.Clear();

		NPCShops();
	}

	public static void NPCShops() {
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
		foreach (var shop in npcShopByName) {
			NPCLoader.ModifyShop(shop.Value);
		}

		RegisterGlobalNpcShop();

		var globalShopEntries = GetGlobalNpcShopEntries().ToArray();
		foreach (var shop in npcShopByName) {
			shop.Value.Add(globalShopEntries);
		}
	}

	private static void RegisterGlobalNpcShop() {
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
		var pylonMainCondition = new NPCShop.Condition(NetworkText.FromLiteral("When happy enough to sell pylons"), () =>
			Main.LocalPlayer.talkNPC != -1 && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[19] && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[20]
			&& (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421 || Main.remixWorld)
			&& TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16())
			&& !Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneCrimson
			);
		var purityPylonCondition = new NPCShop.Condition(NetworkText.FromLiteral(""), () => { // im having struggles localizing these
			if (Main.remixWorld) {
				return Main.LocalPlayer.Center.Y / 16.0 > Main.rockLayer && Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 350;
			}
			return Main.LocalPlayer.Center.Y / 16.0 < Main.worldSurface;
		});
		var cavernPylonCondition = new NPCShop.Condition(NetworkText.FromLiteral(""), () => {
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
		var glowshroomPylonCondtiion = new NPCShop.Condition(NetworkText.FromLiteral("Underground, when in remix world"),
			() => !Main.remixWorld || Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 200);

		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonPurity,	pylonMainCondition, NPCShop.Condition.InPurityBiome, purityPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonSnow,	pylonMainCondition, NPCShop.Condition.InSnowBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonDesert,	pylonMainCondition, NPCShop.Condition.InDesertBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonUnderground, pylonMainCondition, cavernPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonOcean, pylonMainCondition, oceanPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonJungle, pylonMainCondition, NPCShop.Condition.InJungleBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonHallow, pylonMainCondition, NPCShop.Condition.InHallowBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonMushroom, pylonMainCondition, NPCShop.Condition.InGlowshroomBiome, glowshroomPylonCondtiion));

		foreach (ModPylon pylon in PylonLoader.modPylons) {
			if (pylon.ItemDrop == 0) {
				continue;
			}

			RegisterGlobalNpcShop(new NPCShop.Entry(pylon.ItemDrop, new NPCShop.Condition(NetworkText.Empty, () =>
				Main.LocalPlayer.talkNPC != -1 &&
				pylon.IsPylonForSale(
					Main.npc[Main.LocalPlayer.talkNPC].type,
					Main.LocalPlayer,
					Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421
				).HasValue
				)));
		}
	}

	private static void RegisterMerchant() {
		var flareGunCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player carries a Flare Gun in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.FlareGun));
		var drumSetCondition = new NPCShop.Condition(NetworkText.FromLiteral("When either the Eater of Worlds, Brain of Cthulhu, Skeletron, or Wall of Flesh have been defeated"), () => NPC.downedBoss2 || NPC.downedBoss3 || Main.hardMode);
		var nailGunCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Nail Gun"), () => Main.LocalPlayer.HasItem(ItemID.NailGun));

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
			.Add(ItemID.NailGun,			nailGunCondition)
			.Register();
	}

	private static void RegisterArmsDealer() {
		var silverBulletCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Blood Moon, in a world with Silver Ore. (Always available in Hardmode)"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Silver);
		var tungstenBulletCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Blood Moon, in a world with Tungsten Ore. (Always available in Hardmode)"), () => (Main.bloodMoon || Main.hardMode) && WorldGen.SavedOreTiers.Silver == TileID.Tungsten);
		var unholyArrowCondition = new NPCShop.Condition(NetworkText.FromLiteral("During the night after defeating the Eater of Worlds or Brain of Cthulhu. (Always available in Hardmode)"), () => (NPC.downedBoss2 && !Main.dayTime) || Main.hardMode);
		var styngerCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Stynger"), () => Main.LocalPlayer.HasItem(ItemID.Stynger));
		var stakeCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Stake Launcher"), () => Main.LocalPlayer.HasItem(ItemID.StakeLauncher));
		var nailCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Nail Gun"), () => Main.LocalPlayer.HasItem(ItemID.NailGun));
		var candyCornCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Candy Corn Rifle"), () => Main.LocalPlayer.HasItem(ItemID.CandyCornRifle));
		var jackCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player possesses a Jack 'O Lantern Launcher"), () => Main.LocalPlayer.HasItem(ItemID.JackOLanternLauncher));

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
			.Add(ItemID.StyngerBolt,		styngerCondition)
			.Add(ItemID.Stake,				stakeCondition)
			.Add(ItemID.Nail,				nailCondition)
			.Add(ItemID.CandyCorn,			candyCornCondition)
			.Add(ItemID.ExplosiveJackOLantern, jackCondition)
			.Add(ItemID.NurseHat,			NPCShop.Condition.Halloween)
			.Add(ItemID.NurseShirt,			NPCShop.Condition.Halloween)
			.Add(ItemID.NursePants,			NPCShop.Condition.Halloween)
			.Add(ItemID.QuadBarrelShotgun,	NPCShop.Condition.InGraveyard, NPCShop.Condition.DownedSkeletron)
			.Register();
	}

	private static void RegisterDryad() {
		var shop = new NPCShop(NPCID.Dryad);
		var mp1_2 = new NPCShop.Condition(NetworkText.FromLiteral("During Full or Waning Gibbous moon phase"), () => Main.moonPhase / 2 == 0);
		var mp3_4 = new NPCShop.Condition(NetworkText.FromLiteral("During Third Quarter or Waning Crescent moon phase"), () => Main.moonPhase / 2 == 1);
		var mp5_6 = new NPCShop.Condition(NetworkText.FromLiteral("During New or Waxing Crescent moon phase"), () => Main.moonPhase / 2 == 2);
		var mp7_8 = new NPCShop.Condition(NetworkText.FromLiteral("During First Quarter or Waxing Gibbous moon phase"), () => Main.moonPhase / 2 == 3);
		var corruptSeedsCondition = new NPCShop.Condition(NetworkText.FromLiteral("In Corruption world or in Graveyrad in Crimson world"), () => !WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && WorldGen.crimson);
		var crimsonSeedsCondition = new NPCShop.Condition(NetworkText.FromLiteral("In Crimson world or in Graveyrad in Corruption world"), () => WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && !WorldGen.crimson);

		shop.Add(ItemID.VilePowder,			NPCShop.Condition.BloodMoon, NPCShop.Condition.CrimsonWorld, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.ViciousPowder,		NPCShop.Condition.BloodMoon, NPCShop.Condition.CorruptionWorld, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.PurificationPowder, NPCShop.Condition.NotBloodMoon, NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.GrassSeeds,			NPCShop.Condition.NotBloodMoon)
			.Add(ItemID.AshGrassSeeds,		NPCShop.Condition.InUnderworld)
			.Add(ItemID.CorruptSeeds,		NPCShop.Condition.BloodMoon, corruptSeedsCondition)
			.Add(ItemID.CrimsonSeeds,		NPCShop.Condition.BloodMoon, crimsonSeedsCondition)
			.Add(ItemID.Sunflower,			NPCShop.Condition.NotBloodMoon)
			.Add(ItemID.Acorn)
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
			.Add(ItemID.FlowerPacketWhite)																		// White Flower Seeds
			.Add(ItemID.FlowerPacketYellow)																		// Yellows Flower Seeds
			.Add(ItemID.FlowerPacketRed)																		// Red Flower Seeds
			.Add(ItemID.FlowerPacketPink)																		// Pink Flower Seeds
			.Add(ItemID.FlowerPacketMagenta)																	// Magenta Flower Seeds
			.Add(ItemID.FlowerPacketViolet)																		// Violet Flower Seeds
			.Add(ItemID.FlowerPacketBlue)																		// Blue Flower Seeds
			.Add(ItemID.FlowerPacketWild)																		// Wild Flower Seeds
			.Add(ItemID.FlowerPacketTallGrass)																	// Tall Grass Seeds
			.Add(ItemID.PottedForestCedar,	NPCShop.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedJungleCedar,	NPCShop.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedHallowCedar,	NPCShop.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedForestTree,	NPCShop.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedJungleTree,	NPCShop.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedHallowTree,	NPCShop.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedForestPalm,	NPCShop.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedJunglePalm,	NPCShop.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedHallowPalm,	NPCShop.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedForestBamboo, NPCShop.Condition.Hardmode, mp7_8)
			.Add(ItemID.PottedJungleBamboo, NPCShop.Condition.Hardmode, mp7_8)
			.Add(ItemID.PottedHallowBamboo, NPCShop.Condition.Hardmode, mp7_8)
			.Register();
	}

	private static void RegisterBombGuy() {
		var dryBombCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Dry Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.DryBomb));
		var wetBombCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Wet Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.WetBomb));
		var lavaBombCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Lava Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.LavaBomb));
		var honeyBombCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Honey Bomb in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.HoneyBomb));

		new NPCShop(NPCID.Demolitionist)
			.Add(ItemID.Grenade)
			.Add(ItemID.Bomb)
			.Add(ItemID.Dynamite)
			.Add(ItemID.HellfireArrow,		NPCShop.Condition.Hardmode)
			.Add(ItemID.LandMine,			NPCShop.Condition.Hardmode, NPCShop.Condition.DownedPlantera, NPCShop.Condition.DownedPirates)
			.Add(ItemID.ExplosivePowder,	NPCShop.Condition.Hardmode)
			.Add(ItemID.DryBomb,			dryBombCondition)
			.Add(ItemID.WetBomb,			wetBombCondition)
			.Add(ItemID.LavaBomb,			lavaBombCondition)
			.Add(ItemID.HoneyBomb,			honeyBombCondition)
			.Register();
	}

	private static void RegisterClothier() {
		var taxCollectorPresent = new NPCShop.Condition(NetworkText.FromLiteral("If the Tax Collector is present"), () => NPC.AnyNPCs(NPCID.TaxCollector));
		var golfScoreOf2000 = new NPCShop.Condition(NetworkText.FromLiteral("When the player has an accumulate golf score of 2000 or more"), () => Main.LocalPlayer.golferScoreAccumulated >= 2000);

		new NPCShop(NPCID.Clothier)
			.Add(ItemID.BlackThread)
			.Add(ItemID.PinkThread)
			.Add(ItemID.PlacePainting)																			// r/Terraria
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
			.Add(ItemID.HunterCloak,		golfScoreOf2000)
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
		var mechanicsRodCondition = new NPCShop.Condition(NetworkText.FromLiteral("During Moon Phases 2, 4, 6, and 8, when the Angler is present."), () => NPC.AnyNPCs(NPCID.Angler) && Main.moonPhase % 2 == 1);

		new NPCShop(NPCID.Merchant)
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
		var firstFourPhases = new NPCShop.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, Third Quarter or Waning Crescent moon phase"), () => Main.moonPhase < 4);
		var secondFourPhases = new NPCShop.Condition(NetworkText.FromLiteral("During a New, Waxing Crescent, First Quarter, Waxing Gibbous moon phase"), () => Main.moonPhase >= 4);
		var livingWoodWandInInv = new NPCShop.Condition(NetworkText.FromLiteral("If the player has a Living Wood Wand in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.LivingWoodWand));
		var eclipseOrBloodMoon = new NPCShop.Condition(NetworkText.FromLiteral("During a Blood Moon or Solar Eclipse"), () => Main.bloodMoon || Main.eclipse);
		var notEclipseOrBloodMoon = new NPCShop.Condition(NetworkText.FromLiteral("During a not Blood Moon and Solar Eclipse"), () => !Main.bloodMoon && !Main.eclipse);
		var blendOMaticCondition = new NPCShop.Condition(NetworkText.FromLiteral("In Hardmode or in not For the Worthy world"), () => Main.hardMode || !Main.getGoodWorld);

		new NPCShop(NPCID.Steampunker)
			.Add(ItemID.Clentaminator,		NPCShop.Condition.NotRemixWorld)
			.Add(ItemID.SteampunkHat,		firstFourPhases)
			.Add(ItemID.SteampunkShirt,		firstFourPhases)
			.Add(ItemID.SteampunkPants,		firstFourPhases)
			.Add(ItemID.Jetpack,			NPCShop.Condition.Hardmode, secondFourPhases)
			.Add(ItemID.BlendOMatic,		blendOMaticCondition)
			.Add(ItemID.FleshCloningVaat,	NPCShop.Condition.CrimsonWorld)
			.Add(ItemID.LesionStation,		NPCShop.Condition.CorruptionWorld)                             // Decay Chamber
			.Add(ItemID.IceMachine,			NPCShop.Condition.InSnowBiome)
			.Add(ItemID.SkyMill,			NPCShop.Condition.InSpace)
			.Add(ItemID.HoneyDispenser,		NPCShop.Condition.InJungleBiome)
			.Add(ItemID.BoneWelder,			NPCShop.Condition.InGraveyard)
			.Add(ItemID.LivingLoom,			livingWoodWandInInv)
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
		var mpServer = new NPCShop.Condition(NetworkText.FromLiteral("On a multiplayer server"), () => Main.netMode == NetmodeID.MultiplayerClient);

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
		var happyGrenadesInInv = new NPCShop.Condition(NetworkText.FromLiteral("If the player has Happy Grenades in inventory"), () => Main.LocalPlayer.HasItem(ItemID.PartyGirlGrenade));
		var scoreOver500 = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 500"), () => Main.LocalPlayer.golferScoreAccumulated > 500);
		var piratePresent = new NPCShop.Condition(NetworkText.FromLiteral("When Pirate is present"), () => NPC.AnyNPCs(NPCID.Pirate));

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
			.Add(ItemID.PartyGirlGrenade,	happyGrenadesInInv)													// Happy Grenade
			.Add(ItemID.ConfettiCannon,		piratePresent)
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
			.Add(ItemID.Football,			scoreOver500)
			.Register();
	}

	private static void RegisterCyborg() {
		var rocker3Cond = new NPCShop.Condition(NetworkText.FromLiteral("At night or during Solar Eclipse"), () => !Main.dayTime || Main.eclipse);
		var clustRocketCond = new NPCShop.Condition(NetworkText.FromLiteral("During a Blood Moon or Solar Eclipse"), () => Main.bloodMoon || Main.eclipse);
		var portalGunStation = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Portal Gun or Portal Gun Station in the inventory"), () => Main.LocalPlayer.HasItem(ItemID.PortalGun) || Main.LocalPlayer.HasItem(ItemID.PortalGunStation));

		new NPCShop(NPCID.Cyborg)
			.Add(ItemID.RocketI)
			.Add(ItemID.RocketII,			NPCShop.Condition.BloodMoon)
			.Add(ItemID.RocketIII,			rocker3Cond)
			.Add(ItemID.RocketIV,			NPCShop.Condition.Eclipse)
			.Add(ItemID.DryRocket)
			.Add(ItemID.ProximityMineLauncher)
			.Add(ItemID.Nanites)
			.Add(ItemID.ClusterRocketI,		NPCShop.Condition.DownedMartians)
			.Add(ItemID.ClusterRocketII,	NPCShop.Condition.DownedMartians, clustRocketCond)
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
		var moonIsFullOrWaningGibbous = new NPCShop.Condition(NetworkText.FromLiteral("If moon is full or waning gibbous"), () => Main.moonPhase <= 1);
		var moonIsThirdOrWaningCrescent = new NPCShop.Condition(NetworkText.FromLiteral("If moon is third quarter or waning crescent"), () => Main.moonPhase >= 2 && Main.moonPhase <= 3);
		var moonIsNewOrWaxingCrescent = new NPCShop.Condition(NetworkText.FromLiteral("If moon is new or waxing crescent"), () => Main.moonPhase >= 4 && Main.moonPhase <= 5);
		var moonIsFirstOrWaxingGibbous = new NPCShop.Condition(NetworkText.FromLiteral("If moon is first quarter or waxing gibbous"), () => Main.moonPhase >= 6);

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
			.Add(ItemID.ShadowPaint,		NPCShop.Condition.Hardmode)
			.Add(ItemID.NegativePaint,		NPCShop.Condition.Hardmode)
			.Add(ItemID.GlowPaint,			NPCShop.Condition.InGraveyard)									// Illuminant Coating
			.Add(ItemID.EchoCoating,		NPCShop.Condition.InGraveyard, NPCShop.Condition.DownedPlantera)
			.Register();

		new NPCShop(NPCID.Painter, "Decor") // Decor shop
			.Add(ItemID.Daylight)
			.Add(ItemID.FirstEncounter,		moonIsFullOrWaningGibbous)
			.Add(ItemID.GoodMorning,		moonIsThirdOrWaningCrescent)
			.Add(ItemID.UndergroundReward,	moonIsNewOrWaxingCrescent)
			.Add(ItemID.ThroughtheWindow,	moonIsFirstOrWaxingGibbous)
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
		var styngerBoltCond = new NPCShop.Condition(NetworkText.FromLiteral("When player has a Stynger"), () => Main.LocalPlayer.HasItem(ItemID.Stynger));
		var stakeCondition = new NPCShop.Condition(NetworkText.FromLiteral("When player has a Stake Launcher"), () => Main.LocalPlayer.HasItem(ItemID.StakeLauncher));
		var wizardCondition = new NPCShop.Condition(NetworkText.FromLiteral("If Wizard is present"), () => NPC.AnyNPCs(NPCID.Wizard));

		new NPCShop(NPCID.WitchDoctor)
			.Add(ItemID.ImbuingStation)
			.Add(ItemID.Blowgun)
			.Add(ItemID.StyngerBolt,		styngerBoltCond)
			.Add(ItemID.Stake,				stakeCondition)
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
			.Add(ItemID.BewitchingTable,	wizardCondition)
			.Register();
	}

	private static void RegisterPirate() {
		var bunnyCannonCondition = new NPCShop.Condition(NetworkText.FromLiteral("When Party Girl is present, world is in Hardmode and one mechanical boss is defeated."), () => Main.hardMode && NPC.downedMechBossAny && NPC.AnyNPCs(208));

		new NPCShop(NPCID.Pirate)
			.Add(ItemID.Cannon)
			.Add(ItemID.Cannonball)
			.Add(ItemID.PirateHat)
			.Add(ItemID.PirateShirt)
			.Add(ItemID.PiratePants)
			.Add(ItemID.Sail)
			.Add(ItemID.ParrotCracker,		new NPCShop.Condition(NetworkText.FromLiteral("When spoken to in an Ocean biome"), () => {
				int num7 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
				return (double)(Main.screenPosition.Y / 16.0) < Main.worldSurface + 10.0 && (num7 < 380 || num7 > Main.maxTilesX - 380);
			}))
			.Add(ItemID.BunnyCannon,		bunnyCannonCondition)
			.Register();
	}

	private static void RegisterStylist() {
		var maxLife = new NPCShop.Condition(NetworkText.FromLiteral("If the player has at least 400 maximum life"), () => Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax);
		var maxMana = new NPCShop.Condition(NetworkText.FromLiteral("If the player has at least 200 maximum mana"), () => Main.LocalPlayer.ConsumedManaCrystals == Player.ManaCrystalMax);
		var moneyHair = new NPCShop.Condition(NetworkText.FromLiteral("If the player has at least 1 platinum coin in their inventory."), () => {
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
		var timeHair = new NPCShop.Condition(NetworkText.FromLiteral("During the night and following day of moon phases Waning Gibbous, Waning Crescent, Waxing Crescent, Waxing Gibbous"), () => Main.moonPhase % 2 == (!Main.dayTime).ToInt());
		var teamHair = new NPCShop.Condition(NetworkText.FromLiteral("If the player is on a team in multiplayer"), () => Main.LocalPlayer.team != 0);
		var partyHair = new NPCShop.Condition(NetworkText.FromLiteral("If there is a Party Girl in the world"), () => NPC.AnyNPCs(NPCID.PartyGirl));

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
		var wandOfSparkingCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Third Quarter moon in regular worlds"), () => Main.moonPhase == 2 && !Main.remixWorld);
		var magicDaggerCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Third Quarter moon in Don't dig up and Get fixed boi worlds"), () => Main.moonPhase == 2 && Main.remixWorld);
		var lesHealPotCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Waning Gibbous, Waning Crescent, Waxing Crescent or Waxing Gibbous moon"), () => Main.moonPhase % 2 == 0);
		var strangeBrewCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Full, Third Quarter, New or First Quarter moon"), () => Main.moonPhase % 2 != 0);
		var spelunkerGlowCondition = new NPCShop.Condition(NetworkText.FromLiteral("At night, or all day during a Full Moon"), () => !Main.dayTime || Main.moonPhase == 0);
		var spelunkerFlareCondition = new NPCShop.Condition(NetworkText.FromLiteral("When the player has a Flare Gun in their inventory at night, or all day during a Full Moon"), () => (!Main.dayTime || Main.moonPhase == 0) && Main.LocalPlayer.HasItem(ItemID.FlareGun));
		var glowstickCondition = new NPCShop.Condition(NetworkText.FromLiteral("At daytime, during any moon phase except full"), () => Main.dayTime && Main.moonPhase != 0);
		var boneTorchCondition = new NPCShop.Condition(NetworkText.FromLiteral("During the first half of every in game minute (every real life second)"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
		var torchCondition = new NPCShop.Condition(NetworkText.FromLiteral("During the second half of every in game minute (every real life second)"), () => Main.time % 60.0 * 60.0 * 6.0 > 10800.0);
		var boneArrowCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, New or Waxing Crescent moon"), () => Main.moonPhase == 0 || Main.moonPhase == 1 || Main.moonPhase == 4 || Main.moonPhase == 5);
		var woodArrowCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Third Quarter, Waning Crescent, First Quarter or Waxing Gibbous moon"), () => Main.moonPhase == 2 || Main.moonPhase == 3 || Main.moonPhase == 6 || Main.moonPhase == 7);
		var blueCounterCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Full or New moon"), () => Main.moonPhase % 4 == 0);
		var redCounterCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Waning Gibbous or Waxing Crescent moon"), () => Main.moonPhase % 4 == 1);
		var purpleCounterCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Third Quarter or First Quarter moon"), () => Main.moonPhase % 4 == 2);
		var greenCounterCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Waning Crescent or Waxing Gibbous moon"), () => Main.moonPhase % 4 == 3);
		var gradientCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, Third Quarter or Waning Crescent moon"), () => Main.moonPhase < 4);
		var formatcCondition = new NPCShop.Condition(NetworkText.FromLiteral("During a New, Waxing Crescent, First Quarter or Waxing Gibbous moon"), () => Main.moonPhase >= 4);
		var artisanCondition = new NPCShop.Condition(NetworkText.FromLiteral("If the player has not consumed an Artisan Loaf before, during the Waning Crescent, New or Waxing Crescent moon phase"), () => !Main.LocalPlayer.ateArtisanBread && Main.moonPhase >= 3 && Main.moonPhase <= 5);

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
			.Add(ItemID.StrangeBrew,		strangeBrewCondition)
			.Add(ItemID.LesserHealingPotion,lesHealPotCondition)
			.Add(ItemID.HealingPotion,		NPCShop.Condition.Hardmode, lesHealPotCondition)
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
			.Add(ItemID.Gradient,			NPCShop.Condition.Hardmode, gradientCondition)
			.Add(ItemID.FormatC,			NPCShop.Condition.Hardmode, formatcCondition)
			.Add(ItemID.YoYoGlove,			NPCShop.Condition.Hardmode)
			.Add(ItemID.SlapHand,			NPCShop.Condition.Hardmode, NPCShop.Condition.BloodMoon)
			.Add(ItemID.MagicLantern,		NPCShop.Condition.TimeNight, NPCShop.Condition.IsMoonFull)
			.Add(ItemID.ArtisanLoaf,		artisanCondition)
			.Register();
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

		NPCShop shop = new(NPCID.DD2Bartender);
		NPCShop.Condition[] mechCond = { NPCShop.Condition.Hardmode, NPCShop.Condition.DownedMechBossAny };
		NPCShop.Condition[] golemCond = { NPCShop.Condition.Hardmode, NPCShop.Condition.DownedGolem };

		shop.Add(353);

		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(gold: 4) }, NPCShop.Condition.DownedGolem, NPCShop.Condition.DownedMechBossAny);
		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(gold: 1) }, new NPCShop.Condition(NetworkText.FromLiteral("Golem is not slain"), () => !NPC.downedGolemBoss), NPCShop.Condition.DownedMechBossAny);
		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(silver: 25) }, new NPCShop.Condition(NetworkText.FromLiteral("Golem is not slain"), () => !NPC.downedGolemBoss && !NPC.downedMechBossAny));

		shop.Add(3816);
		shop.Add(new Item(3813) {
			shopCustomPrice = 75,
			shopSpecialCurrency = CustomCurrencyID.DefenderMedals
		});

		for (int i = 0; i < items.Length; i++) {
			for (int j = 0; j < items[i].Length; j++) {
				int condType = conditions[i][j];
				if ((condType & golem) > 0) {
					shop.Add(new Item(items[i][j]) {
						shopCustomPrice = prices[i][j],
						shopSpecialCurrency = CustomCurrencyID.DefenderMedals
					}, golemCond);
				}
				else if ((condType & mechBoss) > 0) {
					shop.Add(new Item(items[i][j]) {
						shopCustomPrice = prices[i][j],
						shopSpecialCurrency = CustomCurrencyID.DefenderMedals
					}, mechCond);
				}
				else {
					shop.Add(0);
				}
			}
		}
		shop.Register();
	}

	private static void RegisterGolfer() {
		var scoreOver500 = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 500"), () => Main.LocalPlayer.golferScoreAccumulated > 500);
		var scoreOver1000 = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 1000"), () => Main.LocalPlayer.golferScoreAccumulated > 1000);
		var scoreOver2000 = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 2000"), () => Main.LocalPlayer.golferScoreAccumulated > 2000);
		var painting1Cond = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 2000, during Full or Waning Gibbous moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 0);
		var painting2Cond = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 2000, during Third Quarter or Waning Crescent moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 1);
		var painting3Cond = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 2000, during New or Waxing Crescent moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 2);
		var painting4Cond = new NPCShop.Condition(NetworkText.FromLiteral("Golf score over 2000, during First Quarter or Waxing Gibbous moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 3);

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
			.Add(ItemID.GolfPainting1,		painting1Cond)														// The Rolling Greens
			.Add(ItemID.GolfPainting2,		painting2Cond)														// Study of a Ball at Rest
			.Add(ItemID.GolfPainting3,		painting3Cond)														// Fore!
			.Add(ItemID.GolfPainting4,		painting4Cond)														// The Duplicity of Reflections
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
		var fairyGlowstick = new NPCShop.Condition(NetworkText.FromLiteral("Once the player fills out all three Fairies in the Bestiary"), () => Chest.BestiaryGirl_IsFairyTorchAvailable());
		var solarPillarDead = new NPCShop.Condition(NetworkText.FromLiteral("After beating the Solar Pillar"), () => NPC.downedTowerSolar);

		var moonIsFullOrWaningGibbous = new NPCShop.Condition(NetworkText.FromLiteral("If moon is full or waning gibbous"), () => Main.moonPhase <= 1);
		var moonIsThirdOrWaningCrescent = new NPCShop.Condition(NetworkText.FromLiteral("If moon is third quarter or waning crescent"), () => Main.moonPhase >= 2 && Main.moonPhase <= 3);
		var moonIsNewOrWaxingCrescent = new NPCShop.Condition(NetworkText.FromLiteral("If moon is new or waxing crescent"), () => Main.moonPhase >= 4 && Main.moonPhase <= 5);
		var moonIsFirstOrWaxingGibbous = new NPCShop.Condition(NetworkText.FromLiteral("If moon is first quarter or waxing gibbous"), () => Main.moonPhase >= 6);

		var bestiaryFilledBy10 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 10%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f);
		var bestiaryFilledBy25 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 25%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.25f);
		var bestiaryFilledBy30 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 30%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.3f);
		var bestiaryFilledBy35 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 35%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.35f);
		var bestiaryFilledBy40 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 40%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.4f);
		var bestiaryFilledBy45 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 45%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.45f);
		var bestiaryFilledBy50 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 50%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.5f);
		var bestiaryFilledBy70 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled to at least 70%"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.7f);
		var bestiaryFilledBy100 = new NPCShop.Condition(NetworkText.FromLiteral("When the Bestiary has been filled completely"), () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);

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
		var goodsCondition = new NPCShop.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds."), () => Main.tenthAnniversaryWorld);
		var pirateStaffCond = new NPCShop.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the Full or Waning Gibbous moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 0);
		var discountCardCond = new NPCShop.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the Third Quarter or Waning Crescent moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 1);
		var luckyCoinCond = new NPCShop.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the New or Waxing Crescent moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 2);
		var coinGunCond = new NPCShop.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the First Quarter or Waxing Gibbous moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 3);

		var shop = new NPCShop(NPCID.Princess)
			.Add(ItemID.RoyalTiara)
			.Add(ItemID.RoyalDressTop)																			// Royal Blouse
			.Add(ItemID.RoyalDressBottom);																		// Royal Dress
		for (int i = 5076; i < 5087; i++) {
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
			.Add(ItemID.PirateStaff,		pirateStaffCond)
			.Add(ItemID.DiscountCard,		discountCardCond)
			.Add(ItemID.LuckyCoin,			luckyCoinCond)
			.Add(ItemID.CoinGun,			coinGunCond)
			.Register();
	}
}