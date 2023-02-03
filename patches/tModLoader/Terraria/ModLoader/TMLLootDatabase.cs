using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

// can and/or will be used for other tml loots later. (ie pots loot)
public class TMLLootDatabase
{
	private Dictionary<string, ChestLoot> npcShopByName = new();
	private List<ChestLoot.Entry> globalNpcShopEntries = new();

	public void RegisterNpcShop(int npcId, ChestLoot chestLoot, string shopName = "Shop") {
		npcShopByName.Add(GetNPCShopName(npcId, shopName), chestLoot);
	}

	public void RegisterGlobalNpcShop(ChestLoot.Entry entry) => globalNpcShopEntries.Add(entry);

	public ChestLoot GetNPCShop(string fullName) {
		if (npcShopByName.TryGetValue(fullName, out ChestLoot chestLoot))
			return chestLoot;

		return null;
	}

	public static string GetNPCShopName(int npcId, string shopName = "Shop") {
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

	public void NPCShops() {
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
		foreach (var shop in npcShopByName) {
			NPCLoader.SetupShop(shop.Key, shop.Value);
		}

		RegisterGlobalNpcShop();

		for (int i = 0; i < NPCLoader.NPCCount; i++) {
			NPCLoader.PostSetupShop(i);
		}

		var globalShopEntries = GetGlobalNpcShopEntries().ToArray();
		foreach (var shop in npcShopByName) {
			shop.Value.Add(globalShopEntries);
			NPCLoader.PostSetupShop(shop.Key, shop.Value);
		}
	}

	private void RegisterGlobalNpcShop() {
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
		var pylonMainCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When happy enough to sell pylons"), () =>
			Main.LocalPlayer.talkNPC != -1 && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[19] && Main.npc[Main.LocalPlayer.talkNPC].type != NPCLoader.shopToNPC[20]
			&& (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421 || Main.remixWorld)
			&& TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16())
			&& !Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneCrimson
			);
		var purityPylonCondition = new ChestLoot.Condition(NetworkText.FromLiteral(""), () => { // im having struggles localizing these
			if (Main.remixWorld) {
				return Main.LocalPlayer.Center.Y / 16.0 > Main.rockLayer && Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 350;
			}
			return Main.LocalPlayer.Center.Y / 16.0 < Main.worldSurface;
		});
		var cavernPylonCondition = new ChestLoot.Condition(NetworkText.FromLiteral(""), () => {
			return !Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneDesert
			&& !Main.LocalPlayer.ZoneBeach && !Main.LocalPlayer.ZoneJungle
			&& !Main.LocalPlayer.ZoneHallow && (!Main.remixWorld || !Main.LocalPlayer.ZoneGlowshroom)
			&& (double)(Main.LocalPlayer.Center.Y / 16f) >= Main.worldSurface;
		});
		var oceanPylonCondition = new ChestLoot.Condition(NetworkText.FromKey("RecipeConditions.InBeach"), () => {
			bool flag4 = Main.LocalPlayer.ZoneBeach && Main.LocalPlayer.position.Y < Main.worldSurface * 16.0;
			if (Main.remixWorld) {
				double num13 = Main.LocalPlayer.position.X / 16.0;
				double num14 = Main.LocalPlayer.position.Y / 16.0;
				flag4 |= (num13 < Main.maxTilesX * 0.43 || num13 > Main.maxTilesX * 0.57) && num14 > Main.rockLayer && num14 < Main.maxTilesY - 350;
			}
			return flag4;
		});
		var glowshroomPylonCondtiion = new ChestLoot.Condition(NetworkText.FromLiteral("Underground, when in remix world"),
			() => !Main.remixWorld || Main.LocalPlayer.Center.Y / 16f < Main.maxTilesY - 200);

		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonPurity,	pylonMainCondition, ChestLoot.Condition.InPurityBiome, purityPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonSnow,	pylonMainCondition, ChestLoot.Condition.InSnowBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonDesert,	pylonMainCondition, ChestLoot.Condition.InDesertBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonUnderground, pylonMainCondition, cavernPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonOcean, pylonMainCondition, oceanPylonCondition));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonJungle, pylonMainCondition, ChestLoot.Condition.InJungleBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonHallow, pylonMainCondition, ChestLoot.Condition.InHallowBiome));
		RegisterGlobalNpcShop(new(ItemID.TeleportationPylonMushroom, pylonMainCondition, ChestLoot.Condition.InGlowshroomBiome, glowshroomPylonCondtiion));

		foreach (ModPylon pylon in PylonLoader.modPylons) {
			if (pylon.ItemDrop == 0) {
				continue;
			}

			RegisterGlobalNpcShop(new ChestLoot.Entry(pylon.ItemDrop, new ChestLoot.Condition(NetworkText.Empty, () =>
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

	private static void RegisterDryad() {
		var shop = new ChestLoot();
		var mp1_2 = new ChestLoot.Condition(NetworkText.FromLiteral("During Full or Waning Gibbous moon phase"), () => Main.moonPhase / 2 == 0);
		var mp3_4 = new ChestLoot.Condition(NetworkText.FromLiteral("During Third Quarter or Waning Crescent moon phase"), () => Main.moonPhase / 2 == 1);
		var mp5_6 = new ChestLoot.Condition(NetworkText.FromLiteral("During New or Waxing Crescent moon phase"), () => Main.moonPhase / 2 == 2);
		var mp7_8 = new ChestLoot.Condition(NetworkText.FromLiteral("During First Quarter or Waxing Gibbous moon phase"), () => Main.moonPhase / 2 == 3);
		var corruptSeedsCondition = new ChestLoot.Condition(NetworkText.FromLiteral("In Corruption world or in Graveyrad in Crimson world"), () => !WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && WorldGen.crimson);
		var crimsonSeedsCondition = new ChestLoot.Condition(NetworkText.FromLiteral("In Crimson world or in Graveyrad in Corruption world"), () => WorldGen.crimson || Main.LocalPlayer.ZoneGraveyard && !WorldGen.crimson);

		shop.Add(ItemID.VilePowder,			ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CrimsonWorld, ChestLoot.Condition.NotRemixWorld)
			.Add(ItemID.ViciousPowder,		ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CorruptionWorld, ChestLoot.Condition.NotRemixWorld)
			.Add(ItemID.PurificationPowder, ChestLoot.Condition.NotBloodMoon, ChestLoot.Condition.NotRemixWorld)
			.Add(ItemID.GrassSeeds,			ChestLoot.Condition.NotBloodMoon)
			.Add(ItemID.AshGrassSeeds,		ChestLoot.Condition.InUnderworld)
			.Add(ItemID.CorruptSeeds,		ChestLoot.Condition.BloodMoon, corruptSeedsCondition)
			.Add(ItemID.CrimsonSeeds,		ChestLoot.Condition.BloodMoon, crimsonSeedsCondition)
			.Add(ItemID.Sunflower,			ChestLoot.Condition.NotBloodMoon)
			.Add(ItemID.Acorn)
			.Add(ItemID.DirtRod)
			.Add(ItemID.PumpkinSeed)
			.Add(ItemID.CorruptGrassEcho,	ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CorruptionWorld) // Crimosn Grass Wall
			.Add(ItemID.CrimsonGrassEcho,	ChestLoot.Condition.BloodMoon, ChestLoot.Condition.CrimsonWorld)    // Corrupt Grass Wall
			.Add(ItemID.GrassWall,			ChestLoot.Condition.NotBloodMoon)
			.Add(ItemID.FlowerWall)
			.Add(ItemID.JungleWall,			ChestLoot.Condition.Hardmode)
			.Add(ItemID.HallowedSeeds,		ChestLoot.Condition.Hardmode)
			.Add(ItemID.HallowedGrassEcho,	ChestLoot.Condition.Hardmode)										// Hallowed Grass Wall
			.Add(ItemID.MushroomGrassSeeds, ChestLoot.Condition.InGlowshroomBiome)
			.Add(ItemID.DryadCoverings,		ChestLoot.Condition.Halloween)
			.Add(ItemID.DryadLoincloth,		ChestLoot.Condition.Halloween)
			.Add(ItemID.DayBloomPlanterBox,	ChestLoot.Condition.DownedKingSlime)
			.Add(ItemID.MoonglowPlanterBox, ChestLoot.Condition.DownedQueenBee)
			.Add(ItemID.BlinkrootPlanterBox, ChestLoot.Condition.DownedEyeOfCthulhu)
			.Add(ItemID.PotSuspendedDeathweedCorrupt, ChestLoot.Condition.DownedEaterOfWorlds)
			.Add(ItemID.PotSuspendedDeathweedCrimson, ChestLoot.Condition.DownedBrainOfCthulhu)
			.Add(ItemID.WaterleafPlanterBox, ChestLoot.Condition.DownedSkeletron)
			.Add(ItemID.ShiverthornPlanterBox, ChestLoot.Condition.DownedSkeletron)
			.Add(ItemID.FireBlossomPlanterBox, ChestLoot.Condition.Hardmode)
			.Add(ItemID.FlowerPacketWhite)																		// White Flower Seeds
			.Add(ItemID.FlowerPacketYellow)																		// Yellows Flower Seeds
			.Add(ItemID.FlowerPacketRed)																		// Red Flower Seeds
			.Add(ItemID.FlowerPacketPink)																		// Pink Flower Seeds
			.Add(ItemID.FlowerPacketMagenta)																	// Magenta Flower Seeds
			.Add(ItemID.FlowerPacketViolet)																		// Violet Flower Seeds
			.Add(ItemID.FlowerPacketBlue)																		// Blue Flower Seeds
			.Add(ItemID.FlowerPacketWild)																		// Wild Flower Seeds
			.Add(ItemID.FlowerPacketTallGrass)																	// Tall Grass Seeds
			.Add(ItemID.PottedForestCedar,	ChestLoot.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedJungleCedar,	ChestLoot.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedHallowCedar,	ChestLoot.Condition.Hardmode, mp1_2)
			.Add(ItemID.PottedForestTree,	ChestLoot.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedJungleTree,	ChestLoot.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedHallowTree,	ChestLoot.Condition.Hardmode, mp3_4)
			.Add(ItemID.PottedForestPalm,	ChestLoot.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedJunglePalm,	ChestLoot.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedHallowPalm,	ChestLoot.Condition.Hardmode, mp5_6)
			.Add(ItemID.PottedForestBamboo, ChestLoot.Condition.Hardmode, mp7_8)
			.Add(ItemID.PottedJungleBamboo, ChestLoot.Condition.Hardmode, mp7_8)
			.Add(ItemID.PottedHallowBamboo, ChestLoot.Condition.Hardmode, mp7_8)
			.RegisterShop(NPCID.Dryad);
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

	private static void RegisterSteampunker() {
		var firstFourPhases = new ChestLoot.Condition(NetworkText.FromLiteral("During a Full, Waning Gibbous, Third Quarter or Waning Crescent moon phase"), () => Main.moonPhase < 4);
		var secondFourPhases = new ChestLoot.Condition(NetworkText.FromLiteral("During a New, Waxing Crescent, First Quarter, Waxing Gibbous moon phase"), () => Main.moonPhase >= 4);
		var livingWoodWandInInv = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has a Living Wood Wand in their inventory"), () => Main.LocalPlayer.HasItem(ItemID.LivingWoodWand));
		var eclipseOrBloodMoon = new ChestLoot.Condition(NetworkText.FromLiteral("During a Blood Moon or Solar Eclipse"), () => Main.bloodMoon || Main.eclipse);
		var notEclipseOrBloodMoon = new ChestLoot.Condition(NetworkText.FromLiteral("During a not Blood Moon and Solar Eclipse"), () => !Main.bloodMoon && !Main.eclipse);
		var blendOMaticCondition = new ChestLoot.Condition(NetworkText.FromLiteral("In Hardmode or in not For the Worthy world"), () => Main.hardMode || !Main.getGoodWorld);

		new ChestLoot()
			.Add(ItemID.Clentaminator,		ChestLoot.Condition.NotRemixWorld)
			.Add(ItemID.SteampunkHat,		firstFourPhases)
			.Add(ItemID.SteampunkShirt,		firstFourPhases)
			.Add(ItemID.SteampunkPants,		firstFourPhases)
			.Add(ItemID.Jetpack,			ChestLoot.Condition.Hardmode, secondFourPhases)
			.Add(ItemID.BlendOMatic,		blendOMaticCondition)
			.Add(ItemID.FleshCloningVaat,	ChestLoot.Condition.CrimsonWorld)
			.Add(ItemID.LesionStation,		ChestLoot.Condition.CorruptionWorld)                             // Decay Chamber
			.Add(ItemID.IceMachine,			ChestLoot.Condition.InSnowBiome)
			.Add(ItemID.SkyMill,			ChestLoot.Condition.InSpace)
			.Add(ItemID.HoneyDispenser,		ChestLoot.Condition.InJungleBiome)
			.Add(ItemID.BoneWelder,			ChestLoot.Condition.InGraveyard)
			.Add(ItemID.LivingLoom,			livingWoodWandInInv)
			.Add(ItemID.SteampunkBoiler,	ChestLoot.Condition.DownedEyeOfCthulhu, ChestLoot.Condition.DownedEowOrBoc, ChestLoot.Condition.DownedSkeletron)
			.Add(ItemID.RedSolution,		ChestLoot.Condition.NotRemixWorld, eclipseOrBloodMoon, ChestLoot.Condition.CrimsonWorld)
			.Add(ItemID.PurpleSolution,		ChestLoot.Condition.NotRemixWorld, eclipseOrBloodMoon, ChestLoot.Condition.CorruptionWorld)
			.Add(ItemID.BlueSolution,		ChestLoot.Condition.NotRemixWorld, notEclipseOrBloodMoon, ChestLoot.Condition.InHallowBiome)
			.Add(ItemID.GreenSolution,		ChestLoot.Condition.NotRemixWorld, notEclipseOrBloodMoon, ChestLoot.Condition.InPurityBiome)
			.Add(ItemID.SandSolution,		ChestLoot.Condition.NotRemixWorld, ChestLoot.Condition.DownedMoonLord)
			.Add(ItemID.SnowSolution,		ChestLoot.Condition.NotRemixWorld, ChestLoot.Condition.DownedMoonLord)
			.Add(ItemID.DirtSolution,		ChestLoot.Condition.NotRemixWorld, ChestLoot.Condition.DownedMoonLord)
			.Add(ItemID.Cog)
			.Add(ItemID.SteampunkMinecart)
			.Add(ItemID.SteampunkGoggles,	ChestLoot.Condition.Halloween)
			.Add(ItemID.LogicGate_AND)
			.Add(ItemID.LogicGate_OR)
			.Add(ItemID.LogicGate_XOR)
			.Add(ItemID.LogicGate_NAND)
			.Add(ItemID.LogicGate_NOR)
			.Add(ItemID.LogicGate_NXOR)
			.Add(ItemID.LogicGateLamp_Off)
			.Add(ItemID.LogicGateLamp_On)
			.Add(ItemID.LogicGateLamp_Faulty)
			.Add(ItemID.StaticHook,			ChestLoot.Condition.Hardmode)
			.Add(ItemID.ConveyorBeltLeft)
			.Add(ItemID.ConveyorBeltRight)
			.RegisterShop(NPCID.Steampunker);
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

	private static void RegisterPartyGirl() {
		var happyGrenadesInInv = new ChestLoot.Condition(NetworkText.FromLiteral("If the player has Happy Grenades in inventory"), () => Main.LocalPlayer.HasItem(ItemID.PartyGirlGrenade));
		var scoreOver500 = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 500"), () => Main.LocalPlayer.golferScoreAccumulated > 500);
		var piratePresent = new ChestLoot.Condition(NetworkText.FromLiteral("When Pirate is present"), () => NPC.AnyNPCs(NPCID.Pirate));

		new ChestLoot()
			.Add(ItemID.ConfettiGun)
			.Add(ItemID.Confetti)
			.Add(ItemID.SmokeBomb)
			.Add(ItemID.BubbleMachine,		ChestLoot.Condition.TimeDay)
			.Add(ItemID.FogMachine,			ChestLoot.Condition.TimeNight)
			.Add(ItemID.BubbleWand)
			.Add(ItemID.BeachBall)
			.Add(ItemID.LavaLamp)
			.Add(ItemID.PlasmaLamp)
			.Add(ItemID.FireworksBox)
			.Add(ItemID.FireworkFountain)
			.Add(ItemID.PartyMinecart)																			// Party Wagon
			.Add(ItemID.KiteSpectrum)																			// Spectrum Kite
			.Add(ItemID.PogoStick)
			.Add(ItemID.RedRocket,			ChestLoot.Condition.Hardmode)
			.Add(ItemID.GreenRocket,		ChestLoot.Condition.Hardmode)
			.Add(ItemID.BlueRocket,			ChestLoot.Condition.Hardmode)
			.Add(ItemID.YellowRocket,		ChestLoot.Condition.Hardmode)
			.Add(ItemID.PartyGirlGrenade,	happyGrenadesInInv)													// Happy Grenade
			.Add(ItemID.ConfettiCannon,		piratePresent)
			.Add(ItemID.Bubble,				ChestLoot.Condition.Hardmode)
			.Add(ItemID.SmokeBlock,			ChestLoot.Condition.Hardmode)
			.Add(ItemID.PartyMonolith)																			// Party Center
			.Add(ItemID.PartyHat)
			.Add(ItemID.SillyBalloonMachine)
			.Add(ItemID.PartyPresent,		ChestLoot.Condition.BirthdayPartyIsUp)
			.Add(ItemID.Pigronata,			ChestLoot.Condition.BirthdayPartyIsUp)
			.Add(ItemID.SillyStreamerBlue,	ChestLoot.Condition.BirthdayPartyIsUp)								// Blue Streamer
			.Add(ItemID.SillyStreamerGreen, ChestLoot.Condition.BirthdayPartyIsUp)								// Green Streamer
			.Add(ItemID.SillyStreamerPink,	ChestLoot.Condition.BirthdayPartyIsUp)								// Pink Streamer
			.Add(ItemID.SillyBalloonPurple, ChestLoot.Condition.BirthdayPartyIsUp)								// Silly Purple Balloon
			.Add(ItemID.SillyBalloonGreen,	ChestLoot.Condition.BirthdayPartyIsUp)                              // Silly Green Balloon
			.Add(ItemID.SillyBalloonPink,	ChestLoot.Condition.BirthdayPartyIsUp)                              // Silly Pink Balloon
			.Add(ItemID.SillyBalloonTiedGreen, ChestLoot.Condition.BirthdayPartyIsUp)							// Silly Tied Balloon (Green)
			.Add(ItemID.SillyBalloonTiedPurple, ChestLoot.Condition.BirthdayPartyIsUp)                          // Silly Tied Balloon (Purple)
			.Add(ItemID.SillyBalloonTiedPink, ChestLoot.Condition.BirthdayPartyIsUp)                            // Silly Tied Balloon (Pink)
			.Add(ItemID.FireworksLauncher,	ChestLoot.Condition.DownedGolem)											// Celebration
			.Add(ItemID.ReleaseDoves,		ChestLoot.Condition.InGraveyard)
			.Add(ItemID.ReleaseLantern,		ChestLoot.Condition.NightLanternsUp)
			.Add(ItemID.Football,			scoreOver500)
			.RegisterShop(NPCID.PartyGirl);
	}

	private static void RegisterCyborg() {
		var rocker3Cond = new ChestLoot.Condition(NetworkText.FromLiteral("At night or during Solar Eclipse"), () => !Main.dayTime || Main.eclipse);
		var clustRocketCond = new ChestLoot.Condition(NetworkText.FromLiteral("During a Blood Moon or Solar Eclipse"), () => Main.bloodMoon || Main.eclipse);
		var portalGunStation = new ChestLoot.Condition(NetworkText.FromLiteral("When the player has a Portal Gun or Portal Gun Station in the inventory"), () => Main.LocalPlayer.HasItem(ItemID.PortalGun) || Main.LocalPlayer.HasItem(ItemID.PortalGunStation));

		new ChestLoot()
			.Add(ItemID.RocketI)
			.Add(ItemID.RocketII,			ChestLoot.Condition.BloodMoon)
			.Add(ItemID.RocketIII,			rocker3Cond)
			.Add(ItemID.RocketIV,			ChestLoot.Condition.Eclipse)
			.Add(ItemID.DryRocket)
			.Add(ItemID.ProximityMineLauncher)
			.Add(ItemID.Nanites)
			.Add(ItemID.ClusterRocketI,		ChestLoot.Condition.DownedMartians)
			.Add(ItemID.ClusterRocketII,	ChestLoot.Condition.DownedMartians, clustRocketCond)
			.Add(ItemID.CyborgHelmet,		ChestLoot.Condition.Halloween)
			.Add(ItemID.CyborgShirt,		ChestLoot.Condition.Halloween)
			.Add(ItemID.CyborgPants,		ChestLoot.Condition.Halloween)
			.Add(ItemID.HiTekSunglasses,	ChestLoot.Condition.DownedMartians)
			.Add(ItemID.NightVisionHelmet,  ChestLoot.Condition.DownedMartians)
			.Add(ItemID.PortalGunStation,	portalGunStation)
			.Add(ItemID.EchoBlock,			ChestLoot.Condition.InGraveyard)
			.Add(ItemID.SpectreGoggles,		ChestLoot.Condition.InGraveyard)
			.Add(ItemID.JimsDrone)
			.Add(ItemID.JimsDroneVisor)
			.RegisterShop(NPCID.Cyborg);
	}

	private static void RegisterPainter() {
		var moonIsFullOrWaningGibbous = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is full or waning gibbous"), () => Main.moonPhase <= 1);
		var moonIsThirdOrWaningCrescent = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is third quarter or waning crescent"), () => Main.moonPhase >= 2 && Main.moonPhase <= 3);
		var moonIsNewOrWaxingCrescent = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is new or waxing crescent"), () => Main.moonPhase >= 4 && Main.moonPhase <= 5);
		var moonIsFirstOrWaxingGibbous = new ChestLoot.Condition(NetworkText.FromLiteral("If moon is first quarter or waxing gibbous"), () => Main.moonPhase >= 6);

		new ChestLoot()
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
			.Add(ItemID.ShadowPaint,		ChestLoot.Condition.Hardmode)
			.Add(ItemID.NegativePaint,		ChestLoot.Condition.Hardmode)
			.Add(ItemID.GlowPaint,			ChestLoot.Condition.InGraveyard)									// Illuminant Coating
			.Add(ItemID.EchoCoating,		ChestLoot.Condition.InGraveyard, ChestLoot.Condition.DownedPlantera)
			.RegisterShop(NPCID.Painter, "Shop");

		new ChestLoot()
			.Add(ItemID.Daylight)
			.Add(ItemID.FirstEncounter,		moonIsFullOrWaningGibbous)
			.Add(ItemID.GoodMorning,		moonIsThirdOrWaningCrescent)
			.Add(ItemID.UndergroundReward,	moonIsNewOrWaxingCrescent)
			.Add(ItemID.ThroughtheWindow,	moonIsFirstOrWaxingGibbous)
			.Add(ItemID.Purity,				ChestLoot.Condition.InShoppingForestBiome)
			.Add(ItemID.DeadlandComesAlive, ChestLoot.Condition.InCrimsonBiome)
			.Add(ItemID.LightlessChasms,	ChestLoot.Condition.InCorruptBiome)
			.Add(ItemID.TheLandofDeceivingLooks, ChestLoot.Condition.InHallowBiome)
			.Add(ItemID.DoNotStepontheGrass, ChestLoot.Condition.InJungleBiome)
			.Add(ItemID.ColdWatersintheWhiteLand, ChestLoot.Condition.InSnowBiome)
			.Add(ItemID.SecretoftheSands,	ChestLoot.Condition.InDesertBiome)
			.Add(ItemID.EvilPresence,		ChestLoot.Condition.BloodMoon)
			.Add(ItemID.PlaceAbovetheClouds,ChestLoot.Condition.InSpace)
			.Add(ItemID.SkyGuardian,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.InSpace)
			.Add(ItemID.Thunderbolt,		ChestLoot.Condition.Thunderstorm)
			.Add(ItemID.Nevermore,			ChestLoot.Condition.InGraveyard)
			.Add(ItemID.Reborn,				ChestLoot.Condition.InGraveyard)
			.Add(ItemID.Graveyard,			ChestLoot.Condition.InGraveyard)
			.Add(ItemID.GhostManifestation, ChestLoot.Condition.InGraveyard)
			.Add(ItemID.WickedUndead,		ChestLoot.Condition.InGraveyard)
			.Add(ItemID.HailtotheKing,		ChestLoot.Condition.InGraveyard)
			.Add(ItemID.BloodyGoblet,		ChestLoot.Condition.InGraveyard)
			.Add(ItemID.StillLife,			ChestLoot.Condition.InGraveyard)
			.Add(ItemID.ChristmasTreeWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.CandyCaneWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.StarsWallpaper,		ChestLoot.Condition.Christmas)
			.Add(ItemID.SnowflakeWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.BluegreenWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.OrnamentWallpaper,  ChestLoot.Condition.Christmas)
			.Add(ItemID.FestiveWallpaper,	ChestLoot.Condition.Christmas)
			.Add(ItemID.SquigglesWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.KrampusHornWallpaper, ChestLoot.Condition.Christmas)
			.Add(ItemID.GrinchFingerWallpaper, ChestLoot.Condition.Christmas)
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
			.RegisterShop(NPCID.Painter, "Decor");
	}

	private static void RegisterWitchDoctor() {
		var styngerBoltCond = new ChestLoot.Condition(NetworkText.FromLiteral("When player has a Stynger"), () => Main.LocalPlayer.HasItem(ItemID.Stynger));
		var stakeCondition = new ChestLoot.Condition(NetworkText.FromLiteral("When player has a Stake Launcher"), () => Main.LocalPlayer.HasItem(ItemID.StakeLauncher));
		var wizardCondition = new ChestLoot.Condition(NetworkText.FromLiteral("If Wizard is present"), () => NPC.AnyNPCs(NPCID.Wizard));

		new ChestLoot()
			.Add(ItemID.ImbuingStation)
			.Add(ItemID.Blowgun)
			.Add(ItemID.StyngerBolt,		styngerBoltCond)
			.Add(ItemID.Stake,				stakeCondition)
			.Add(ItemID.Cauldron,			ChestLoot.Condition.Halloween)
			.Add(ItemID.TikiTotem,			ChestLoot.Condition.Hardmode, ChestLoot.Condition.InJungleBiome)
			.Add(ItemID.LeafWings,			ChestLoot.Condition.Hardmode, ChestLoot.Condition.InJungleBiome, ChestLoot.Condition.TimeNight, ChestLoot.Condition.DownedPlantera)
			.Add(ItemID.VialofVenom,		ChestLoot.Condition.DownedPlantera)
			.Add(ItemID.TikiMask,			ChestLoot.Condition.DownedPlantera)
			.Add(ItemID.TikiShirt,			ChestLoot.Condition.DownedPlantera)
			.Add(ItemID.TikiPants,			ChestLoot.Condition.DownedPlantera)
			.Add(ItemID.PygmyNecklace,		ChestLoot.Condition.TimeNight)
			.Add(ItemID.HerculesBeetle,		ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedPlantera, ChestLoot.Condition.InJungleBiome)
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
			.RegisterShop(NPCID.WitchDoctor);
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

		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(gold: 4) }, ChestLoot.Condition.DownedGolem, ChestLoot.Condition.DownedMechBossAny);
		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(gold: 1) }, new ChestLoot.Condition(NetworkText.FromLiteral("Golem is not slain"), () => !NPC.downedGolemBoss), ChestLoot.Condition.DownedMechBossAny);
		shop.Add(new Item(3828) { shopCustomPrice = Item.buyPrice(silver: 25) }, new ChestLoot.Condition(NetworkText.FromLiteral("Golem is not slain"), () => !NPC.downedGolemBoss && !NPC.downedMechBossAny));

		shop.Add(3816);
		shop.Add(3813);
		shop.LastEntry.item.shopCustomPrice = 75;
		shop.LastEntry.item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;

		for (int i = 0; i < items.Length; i++) {
			for (int j = 0; j < items[i].Length; j++) {
				int condType = conditions[i][j];
				if ((condType & golem) > 0) {
					shop.Add(items[i][j], golemCond);
					shop.LastEntry.item.shopCustomPrice = prices[i][j];
					shop.LastEntry.item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
				}
				else if ((condType & mechBoss) > 0) {
					shop.Add(items[i][j], mechCond);
					shop.LastEntry.item.shopCustomPrice = prices[i][j];
					shop.LastEntry.item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
				}
				else {
					shop.Add(0);
				}
			}
		}
		shop.RegisterShop(NPCID.DD2Bartender);
	}

	private static void RegisterGolfer() {
		var scoreOver500 = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 500"), () => Main.LocalPlayer.golferScoreAccumulated > 500);
		var scoreOver1000 = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 1000"), () => Main.LocalPlayer.golferScoreAccumulated > 1000);
		var scoreOver2000 = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 2000"), () => Main.LocalPlayer.golferScoreAccumulated > 2000);
		var painting1Cond = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 2000, during Full or Waning Gibbous moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 0);
		var painting2Cond = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 2000, during Third Quarter or Waning Crescent moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 1);
		var painting3Cond = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 2000, during New or Waxing Crescent moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 2);
		var painting4Cond = new ChestLoot.Condition(NetworkText.FromLiteral("Golf score over 2000, during First Quarter or Waxing Gibbous moon phase"), () => Main.LocalPlayer.golferScoreAccumulated > 2000 && Main.moonPhase / 2 == 3);

		new ChestLoot()
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
			.Add(ItemID.GolfCart,			scoreOver2000, ChestLoot.Condition.DownedSkeletron)					// Golf Cart Keys
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
			.RegisterShop(NPCID.Golfer);
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

	private static void RegisterPrincess() {
		var goodsCondition = new ChestLoot.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds."), () => Main.tenthAnniversaryWorld);
		var pirateStaffCond = new ChestLoot.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the Full or Waning Gibbous moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 0);
		var discountCardCond = new ChestLoot.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the Third Quarter or Waning Crescent moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 1);
		var luckyCoinCond = new ChestLoot.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the New or Waxing Crescent moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 2);
		var coinGunCond = new ChestLoot.Condition(NetworkText.FromLiteral("In Celebrationmk10 and Get fixed boi worlds, in Hardmode, after the Pirate Invasion has been defeated, during the First Quarter or Waxing Gibbous moon phase"), () => Main.tenthAnniversaryWorld && NPC.downedPirates && Main.moonPhase / 2 == 3);

		var shop = new ChestLoot()
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
			.Add(ItemID.RoyalRomance,		ChestLoot.Condition.DownedKingSlime, ChestLoot.Condition.DownedKingSlime)
			.Add(ItemID.MusicBoxCredits,	ChestLoot.Condition.Hardmode, ChestLoot.Condition.DownedMoonLord)   // Music Box (Journey's End)
			.Add(ItemID.SlimeStaff,			goodsCondition)
			.Add(ItemID.HeartLantern,		goodsCondition)
			.Add(ItemID.FlaskofParty,		goodsCondition)
			.Add(ItemID.SandstorminaBottle, goodsCondition, ChestLoot.Condition.InDesertBiome)
			.Add(ItemID.Terragrim,			goodsCondition, ChestLoot.Condition.BloodMoon)
			.Add(ItemID.PirateStaff,		pirateStaffCond)
			.Add(ItemID.DiscountCard,		discountCardCond)
			.Add(ItemID.LuckyCoin,			luckyCoinCond)
			.Add(ItemID.CoinGun,			coinGunCond)
			.RegisterShop(NPCID.Princess);
	}
}