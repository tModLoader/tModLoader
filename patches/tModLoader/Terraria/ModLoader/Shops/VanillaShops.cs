using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Shops
{
public class ZoologistShop : NPCShop
	{
		public override int NPCType => NPCID.BestiaryGirl;

		public override void SetDefaults() {
			CreateEntry(ItemID.FairyGlowstick).AddCondition(NetworkText.FromKey("ShopConditions.FairyTorch"), () => {
				static bool DidDiscoverBestiaryEntry(int npcId) => Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;

				return DidDiscoverBestiaryEntry(585) && DidDiscoverBestiaryEntry(584) && DidDiscoverBestiaryEntry(583);
			});

			CreateEntry(ItemID.DontHurtCrittersBook);
			CreateEntry(ItemID.SquirrelHook);

			CreateEntry(ItemID.BlandWhip).AddCondition(Condition.BestiaryCompletion(0.1f));
			CreateEntry(ItemID.LicenseCat).AddCondition(NetworkText.FromKey("ShopConditions.NotBoughtCat"), () => !NPC.boughtCat);
			CreateEntry(ItemID.LicenseDog).AddCondition(new SimpleCondition(NetworkText.FromKey("ShopConditions.NotBoughtDog"), () => !NPC.boughtDog), Condition.BestiaryCompletion(0.25f));
			CreateEntry(ItemID.LicenseBunny).AddCondition(new SimpleCondition(NetworkText.FromKey("ShopConditions.NotBoughtBunny"), () => !NPC.boughtBunny), Condition.BestiaryCompletion(0.45f));

			CreateEntry(ItemID.VanityTreeSakuraSeed).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(ItemID.VanityTreeYellowWillowSeed).AddCondition(Condition.BestiaryCompletion(0.4f));

			CreateEntry(ItemID.KiteCrawltipede).AddCondition(Condition.DownedSolarTower);

			CreateEntry(ItemID.KiteKoi).AddCondition(Condition.BestiaryCompletion(0.1f));
			CreateEntry(ItemID.CritterShampoo).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(ItemID.MolluskWhistle).AddCondition(Condition.BestiaryCompletion(0.25f));
			CreateEntry(ItemID.PaintedHorseSaddle).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(ItemID.MajesticHorseSaddle).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(ItemID.DarkHorseSaddle).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(ItemID.JoustingLance).AddCondition(Condition.BestiaryCompletion(0.3f), Condition.Hardmode);
			CreateEntry(ItemID.RabbitOrder).AddCondition(Condition.BestiaryCompletion(0.4f));

			CreateEntry(ItemID.FullMoonSqueakyToy).AddCondition(Condition.Hardmode, Condition.BloodMoon);
			CreateEntry(ItemID.MudBud).AddCondition(Condition.DownedPlantera);

			CreateEntry(ItemID.TreeGlobe).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(ItemID.WorldGlobe).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(ItemID.LightningCarrot).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(ItemID.DiggingMoleMinecart).AddCondition(Condition.BestiaryCompletion(0.6f));
			CreateEntry(ItemID.BallOfFuseWire).AddCondition(Condition.BestiaryCompletion(0.7f));
			CreateEntry(ItemID.FoodBarbarianHelm).AddCondition(Condition.BestiaryCompletion(1f));

			CreateEntry(ItemID.DogEars).AddCondition(Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.DogTail).AddCondition(Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(ItemID.FoxEars).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.FoxTail).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);

			CreateEntry(ItemID.LizardEars).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.LizardTail).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);

			CreateEntry(ItemID.BunnyEars).AddCondition(Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.BunnyTail).AddCondition(Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class GolferShop : NPCShop
	{
		public override int NPCType => NPCID.Golfer;

		public override void SetDefaults() {
			CreateEntry(ItemID.GolfClubStoneIron);
			CreateEntry(ItemID.GolfClubWoodDriver);
			CreateEntry(ItemID.GolfClubBronzeWedge);
			CreateEntry(ItemID.GolfClubRustyPutter);
			CreateEntry(ItemID.GolfCupFlagWhite);
			CreateEntry(ItemID.GolfCupFlagRed);
			CreateEntry(ItemID.GolfCupFlagGreen);
			CreateEntry(ItemID.GolfCupFlagBlue);
			CreateEntry(ItemID.GolfCupFlagYellow);
			CreateEntry(ItemID.GolfCupFlagPurple);

			CreateEntry(ItemID.GolfClubIron).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(ItemID.GolfClubDriver).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(ItemID.GolfClubWedge).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(ItemID.DeepSkyBluePaint).AddCondition(Condition.GolfScore(500, Operation.Greater));

			CreateEntry(ItemID.GolfTee);
			CreateEntry(ItemID.WoodenCrateHard);
			CreateEntry(ItemID.GolfWhistle);
			CreateEntry(ItemID.GolfCup);
			CreateEntry(ItemID.ArrowSign);
			CreateEntry(ItemID.PaintedArrowSign);

			CreateEntry(ItemID.GolfClubMythrilIron).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(ItemID.GolfClubPearlwoodDriver).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(ItemID.GolfClubGoldWedge).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(ItemID.GolfClubLeadPutter).AddCondition(Condition.GolfScore(1000, Operation.Greater));

			CreateEntry(ItemID.GolfHat);
			CreateEntry(ItemID.GolfVisor);
			CreateEntry(ItemID.GolfShirt);
			CreateEntry(ItemID.GolfPants);
			CreateEntry(ItemID.LawnMower);

			CreateEntry(ItemID.GolfChest).AddCondition(Condition.GolfScore(500, Operation.Greater));

			CreateEntry(ItemID.GolfClubTitaniumIron).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(ItemID.GolfClubChlorophyteDriver).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(ItemID.GolfClubDiamondWedge).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(ItemID.GolfClubShroomitePutter).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(ItemID.GolfCart).AddCondition(Condition.GolfScore(2000, Operation.Greater), Condition.DownedSkeletron);

			CreateEntry(ItemID.GolfTrophyBronze).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(ItemID.GolfTrophySilver).AddCondition(Condition.GolfScore(999, Operation.Greater));
			CreateEntry(ItemID.GolfTrophyGold).AddCondition(Condition.GolfScore(1999, Operation.Greater));

			CreateEntry(ItemID.GolfPainting1).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.GolfPainting2).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.GolfPainting3).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.GolfPainting4).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class PrincessShop : NPCShop
	{
		public override int NPCType => NPCID.Princess;

		public override void SetDefaults() {
			CreateEntry(ItemID.RoyalTiara);
			CreateEntry(ItemID.RoyalDressTop);
			CreateEntry(ItemID.RoyalDressBottom);
			CreateEntry(ItemID.RoyalScepter);
			CreateEntry(ItemID.GlassSlipper);
			CreateEntry(ItemID.PrinceUniform);
			CreateEntry(ItemID.PrincePants);
			CreateEntry(ItemID.PrinceCape);
			CreateEntry(ItemID.PottedCrystalPlantFern);
			CreateEntry(ItemID.PottedCrystalPlantSpiral);
			CreateEntry(ItemID.PottedCrystalPlantTeardrop);
			CreateEntry(ItemID.PottedCrystalPlantTree);
			CreateEntry(ItemID.Princess64);
			CreateEntry(ItemID.PaintingOfALass);
			CreateEntry(ItemID.DarkSideHallow);
			CreateEntry(ItemID.MusicBoxCredits).AddCondition(Condition.Hardmode, Condition.DownedMoonLord);
		}
	}

	public class BartenderShop : NPCShop
	{
		public override int NPCType => NPCID.DD2Bartender;

		public override void SetDefaults() {
			Condition condition1 = Condition.Hardmode & Condition.DownedAnyMechBoss;
			Condition condition2 = Condition.Hardmode & Condition.DownedGolem;

			CreateEntry(ItemID.Ale);

			CreateEntry(ItemID.DD2ElderCrystal).SetPrice(Item.buyPrice(gold: 4)).AddCondition(condition1, condition2);
			CreateEntry(ItemID.DD2ElderCrystal).SetPrice(Item.buyPrice(gold: 1)).AddCondition(condition1, !condition2);
			CreateEntry(ItemID.DD2ElderCrystal).SetPrice(Item.buyPrice(silver: 25)).AddCondition(!condition1, !condition2);

			CreateEntry(ItemID.DD2ElderCrystalStand);

			CreateEntry(ItemID.DefendersForge).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75);
			CreateEntry(ItemID.DD2FlameburstTowerT1Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(ItemID.DD2BallistraTowerT1Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(ItemID.DD2ExplosiveTrapT1Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(ItemID.DD2LightningAuraT1Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);

			CreateEntry(ItemID.DD2FlameburstTowerT2Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.DD2BallistraTowerT2Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.DD2ExplosiveTrapT2Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.DD2LightningAuraT2Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);

			CreateEntry(ItemID.DD2FlameburstTowerT3Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(ItemID.DD2BallistraTowerT3Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(ItemID.DD2ExplosiveTrapT3Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(ItemID.DD2LightningAuraT3Popper).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);

			CreateEntry(ItemID.SquireGreatHelm).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.SquirePlating).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.SquireGreaves).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.ApprenticeHat).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.ApprenticeRobe).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.ApprenticeTrousers).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.HuntressWig).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.HuntressJerkin).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.HuntressPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.MonkBrows).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.MonkShirt).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(ItemID.MonkPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);

			CreateEntry(ItemID.SquireAltHead).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.SquireAltShirt).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.SquireAltPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.ApprenticeAltHead).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.ApprenticeAltShirt).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.ApprenticeAltPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.HuntressAltHead).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.HuntressAltShirt).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.HuntressAltPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.MonkAltHead).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.MonkAltShirt).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(ItemID.MonkAltPants).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
		}
	}

	public class SkeletonMerchantShop : NPCShop
	{
		public override int NPCType => NPCID.SkeletonMerchant;

		public override void SetDefaults() {
			CreateEntry(ItemID.StrangeBrew).AddCondition(Condition.PhaseFull | Condition.PhaseHalfAtLeft | Condition.PhaseEmpty | Condition.PhaseHalfAtRight);
			CreateEntry(ItemID.StrangeBrew).AddCondition(Condition.PhaseQuarterAtLeft | Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtRight);

			CreateEntry(ItemID.SpelunkerGlowstick).AddCondition(Condition.TimeNight | Condition.PhaseFull);
			CreateEntry(ItemID.Glowstick).AddCondition(!(Condition.TimeNight | Condition.PhaseFull));

			Condition cond = new SimpleCondition(NetworkText.FromLiteral("ShopConditions.MinuteTime"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
			CreateEntry(ItemID.BoneTorch).AddCondition(cond);
			CreateEntry(ItemID.Torch).AddCondition(!cond);

			cond = Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseEmpty | Condition.PhaseQuarterAtRight;
			CreateEntry(ItemID.BoneArrow).AddCondition(cond);
			CreateEntry(ItemID.WoodenArrow).AddCondition(!cond);

			CreateEntry(ItemID.BlueCounterweight).AddCondition(Condition.PhaseFull | Condition.PhaseEmpty);
			CreateEntry(ItemID.RedCounterweight).AddCondition(Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.PurpleCounterweight).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseHalfAtRight);
			CreateEntry(ItemID.GreenCounterweight).AddCondition(Condition.PhaseQuarterAtLeft | Condition.PhaseThreeQuartersAtRight);

			CreateEntry(ItemID.Bomb);
			CreateEntry(ItemID.Rope);

			CreateEntry(ItemID.Gradient).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.FormatC).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight | Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.YoYoGlove).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SlapHand).AddCondition(Condition.Hardmode, Condition.BloodMoon);
			CreateEntry(ItemID.MagicLantern).AddCondition(Condition.PhaseFull, Condition.TimeNight);
		}
	}

	public class StylistShop : NPCShop
	{
		public override int NPCType => NPCID.Stylist;

		public override void SetDefaults() {
			CreateEntry(ItemID.HairDyeRemover);
			CreateEntry(ItemID.DepthHairDye);
			CreateEntry(ItemID.LifeHairDye).AddCondition(NetworkText.FromKey("ShopConditions.Health400"), () => Main.LocalPlayer.statLifeMax >= 400);
			CreateEntry(ItemID.ManaHairDye).AddCondition(NetworkText.FromKey("ShopConditions.Mana200"), () => Main.LocalPlayer.statManaMax >= 200);
			CreateEntry(ItemID.MoneyHairDye).AddCondition(Condition.HasMoney(1000000, Operation.GreaterEqual));

			CreateEntry(ItemID.TimeHairDye).AddCondition(
				(Condition.TimeNight & (Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtRight)) |
				(Condition.TimeDay & (Condition.PhaseFull | Condition.PhaseHalfAtLeft | Condition.PhaseEmpty | Condition.PhaseHalfAtRight)));
			CreateEntry(ItemID.TeamHairDye).AddCondition(NetworkText.FromKey("ShopConditions.TeamPlayer"), () => Main.LocalPlayer.team != 0);
			CreateEntry(ItemID.BiomeHairDye).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.PartyHairDye).AddCondition(Condition.NPCExists(208));
			CreateEntry(ItemID.RainbowHairDye).AddCondition(Condition.Hardmode, Condition.DownedDestroyer, Condition.DownedTwins, Condition.DownedSkeletronPrime);
			CreateEntry(ItemID.SpeedHairDye).AddCondition(Condition.Hardmode, Condition.DownedAnyMechBoss);
			CreateEntry(ItemID.MartianHairDye).AddCondition(Condition.Hardmode, Condition.DownedMartians);
			CreateEntry(ItemID.TwilightHairDye).AddCondition(Condition.Hardmode, Condition.DownedMartians);
		}
	}

	public class PirateShop : NPCShop
	{
		public override int NPCType => NPCID.Pirate;

		public override void SetDefaults() {
			CreateEntry(ItemID.Cannon);
			CreateEntry(ItemID.Cannonball);
			CreateEntry(ItemID.PirateHat);
			CreateEntry(ItemID.PirateShirt);
			CreateEntry(ItemID.PiratePants);
			CreateEntry(ItemID.Sail);
			CreateEntry(ItemID.ParrotCracker).AddCondition(Condition.InOcean);
			CreateEntry(ItemID.BunnyCannon).AddCondition(Condition.Hardmode, Condition.DownedAnyMechBoss, Condition.NPCExists(208));
		}
	}

	public class WitchDoctorShop : NPCShop
	{
		public override int NPCType => NPCID.WitchDoctor;

		public override void SetDefaults() {
			CreateEntry(ItemID.ImbuingStation);
			CreateEntry(ItemID.Blowgun);

			CreateEntry(ItemID.BewitchingTable).AddCondition(Condition.NPCExists(108));
			CreateEntry(ItemID.PygmyNecklace).AddCondition(Condition.TimeNight);
			CreateEntry(ItemID.TikiMask).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(ItemID.TikiShirt).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(ItemID.TikiPants).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(ItemID.HerculesBeetle).AddCondition(Condition.Hardmode, Condition.DownedPlantera, Condition.InJungle);
			CreateEntry(ItemID.VialofVenom).AddCondition(Condition.Hardmode, Condition.DownedPlantera);

			CreateEntry(ItemID.TikiTotem).AddCondition(Condition.Hardmode, Condition.InJungle);
			CreateEntry(ItemID.LeafWings).AddCondition(Condition.Hardmode, Condition.InJungle, Condition.TimeNight);

			CreateEntry(ItemID.PureWaterFountain);
			CreateEntry(ItemID.DesertWaterFountain);
			CreateEntry(ItemID.JungleWaterFountain);
			CreateEntry(ItemID.IcyWaterFountain);
			CreateEntry(ItemID.CorruptWaterFountain);
			CreateEntry(ItemID.CrimsonWaterFountain);
			CreateEntry(ItemID.HallowedWaterFountain);
			CreateEntry(ItemID.BloodWaterFountain);
			CreateEntry(ItemID.CavernFountain);
			CreateEntry(ItemID.TeleportationPylonUnderground);
			CreateEntry(ItemID.Stake).AddCondition(Condition.HasItem(1835));
			CreateEntry(ItemID.StyngerBolt).AddCondition(Condition.HasItem(1258));
			CreateEntry(ItemID.Cauldron).AddCondition(Condition.Halloween);
		}
	}

	public class PainterShop : NPCShop
	{
		public override int NPCType => NPCID.Painter;

		public override void SetDefaults() {
			CreateEntry(ItemID.Paintbrush);
			CreateEntry(ItemID.PaintRoller);
			CreateEntry(ItemID.RedPaint);

			for (int i = ItemID.RedPaint; i <= ItemID.PinkPaint; i++) CreateEntry(i);

			CreateEntry(ItemID.BlackPaint);
			CreateEntry(ItemID.GrayPaint);
			CreateEntry(ItemID.WhitePaint);
			CreateEntry(ItemID.BrownPaint);
			CreateEntry(ItemID.GlowPaint).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.ShadowPaint).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.NegativePaint).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.Daylight).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.FirstEncounter).AddCondition(Condition.InGraveyardBiome, Condition.PhaseFull, Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.GoodMorning).AddCondition(Condition.InGraveyardBiome, Condition.PhaseHalfAtLeft, Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.UndergroundReward).AddCondition(Condition.InGraveyardBiome, Condition.PhaseEmpty, Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.ThroughtheWindow).AddCondition(Condition.InGraveyardBiome, Condition.PhaseHalfAtRight, Condition.PhaseThreeQuartersAtRight);

			CreateEntry(ItemID.DeadlandComesAlive).AddCondition(Condition.InCrimson);
			CreateEntry(ItemID.LightlessChasms).AddCondition(Condition.InCorrupt);
			CreateEntry(ItemID.TheLandofDeceivingLooks).AddCondition(Condition.InHallow);
			CreateEntry(ItemID.DoNotStepontheGrass).AddCondition(Condition.InJungle);
			CreateEntry(ItemID.ColdWatersintheWhiteLand).AddCondition(Condition.InSnow);
			CreateEntry(ItemID.SecretoftheSands).AddCondition(Condition.InDesert);
			CreateEntry(ItemID.EvilPresence).AddCondition(Condition.BloodMoon);

			CreateEntry(ItemID.PlaceAbovetheClouds).AddCondition(!Condition.InGraveyardBiome, Condition.InSpace, Condition.BloodMoon);
			CreateEntry(ItemID.SkyGuardian).AddCondition(!Condition.InGraveyardBiome, Condition.InSpace, Condition.Hardmode);

			CreateEntry(ItemID.Nevermore).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.Reborn).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.Graveyard).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.GhostManifestation).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.WickedUndead).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.BloodyGoblet).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.StillLife).AddCondition(Condition.InGraveyardBiome);

			for (int i = ItemID.ChristmasTreeWallpaper; i <= ItemID.GrinchFingerWallpaper; i++) CreateEntry(i).AddCondition(Condition.Christmas);
			for (int i = ItemID.BubbleWallpaper; i <= ItemID.DuckyWallpaper; i++) CreateEntry(i);
			for (int i = ItemID.FancyGreyWallpaper; i <= ItemID.StarlitHeavenWallpaper; i++) CreateEntry(i);
		}
	}

	public class CyborgShop : NPCShop
	{
		public override int NPCType => NPCID.Cyborg;

		public override void SetDefaults() {
			CreateEntry(ItemID.RocketI);
			CreateEntry(ItemID.RocketII).AddCondition(Condition.BloodMoon);
			CreateEntry(ItemID.RocketIII).AddCondition(Condition.TimeNight | Condition.SolarEclipse);
			CreateEntry(ItemID.RocketIV).AddCondition(Condition.SolarEclipse);
			CreateEntry(ItemID.ClusterRocketI).AddCondition(Condition.DownedMartians);
			CreateEntry(ItemID.ClusterRocketII).AddCondition(Condition.DownedMartians, Condition.BloodMoon | Condition.SolarEclipse);
			CreateEntry(ItemID.DryRocket).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.ProximityMineLauncher).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.Nanites).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SpectreGoggles).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.EchoBlock).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.CyborgHelmet).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.CyborgShirt).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.CyborgPants).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.HiTekSunglasses).AddCondition(Condition.DownedMartians);
			CreateEntry(ItemID.NightVisionHelmet).AddCondition(Condition.DownedMartians);
			CreateEntry(ItemID.PortalGunStation).AddCondition(Condition.HasItem(33840) | Condition.HasItem(3664));
		}
	}

	public class PartyGirlShop : NPCShop
	{
		public override int NPCType => NPCID.PartyGirl;

		public override void SetDefaults() {
			CreateEntry(ItemID.BeachBall);
			CreateEntry(ItemID.Football).AddCondition(Condition.GolfScore(500, Operation.GreaterEqual));
			CreateEntry(ItemID.ConfettiGun);
			CreateEntry(ItemID.SmokeBomb);
			CreateEntry(ItemID.BubbleMachine).AddCondition(Condition.TimeDay);
			CreateEntry(ItemID.FogMachine).AddCondition(Condition.TimeNight);
			CreateEntry(ItemID.Confetti);
			CreateEntry(ItemID.BubbleWand);
			CreateEntry(ItemID.LavaLamp);
			CreateEntry(ItemID.PlasmaLamp);
			CreateEntry(ItemID.FireworksBox);
			CreateEntry(ItemID.FireworkFountain);
			CreateEntry(ItemID.PartyMinecart);
			CreateEntry(ItemID.KiteSpectrum);

			CreateEntry(ItemID.ReleaseDoves).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.ReleaseLantern).AddCondition(Condition.Lanterns);
			CreateEntry(ItemID.PartyGirlGrenade).AddCondition(Condition.HasItem(3548));
			CreateEntry(ItemID.ConfettiCannon).AddCondition(Condition.NPCExists(229));
			CreateEntry(ItemID.FireworksLauncher).AddCondition(Condition.DownedGolem);

			CreateEntry(ItemID.Bubble).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SmokeBlock).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.RedRocket).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.GreenRocket).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.BlueRocket).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.YellowRocket).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.PogoStick);
			CreateEntry(ItemID.PartyMonolith);
			CreateEntry(ItemID.PartyHat);
			CreateEntry(ItemID.SillyBalloonMachine);

			CreateEntry(ItemID.PartyPresent).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.Pigronata).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyStreamerBlue).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyStreamerGreen).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyStreamerPink).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonPurple).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonGreen).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonPink).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonTiedGreen).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonTiedPurple).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.SillyBalloonTiedPink).AddCondition(Condition.PartyTime);
		}
	}

	public class DyeTraderShop : NPCShop
	{
		public override int NPCType => NPCID.DyeTrader;

		public override void SetDefaults() {
			CreateEntry(ItemID.SilverDye);
			CreateEntry(ItemID.BrownDye);
			CreateEntry(ItemID.DyeVat);
			CreateEntry(ItemID.TeamDye).AddCondition(NetworkText.FromKey("ShopConditions.Multiplayer"), () => Main.netMode == NetmodeID.MultiplayerClient);
			CreateEntry(ItemID.DyeTraderTurban).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.DyeTraderRobe).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.ShadowDye).AddCondition(Condition.PhaseFull);
			CreateEntry(ItemID.NegativeDye).AddCondition(Condition.PhaseFull);
			CreateEntry(ItemID.BloodbathDye).AddCondition(Condition.TimeNight, Condition.BloodMoon);
			CreateEntry(ItemID.FogboundDye).AddCondition(Condition.InGraveyardBiome);
		}
	}

	public class SteampunkerShop : NPCShop
	{
		public override int NPCType => NPCID.Steampunker;

		public override void SetDefaults() {
			CreateEntry(ItemID.Clentaminator);

			CreateEntry(ItemID.Jetpack).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight | Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.SteampunkHat).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.SteampunkShirt).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.SteampunkPants).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(ItemID.SteampunkWings).AddCondition(Condition.DownedGolem);
			CreateEntry(ItemID.StaticHook);
			CreateEntry(ItemID.LogicGate_AND);
			CreateEntry(ItemID.LogicGate_OR);
			CreateEntry(ItemID.LogicGate_XOR);
			CreateEntry(ItemID.LogicGate_NAND);
			CreateEntry(ItemID.LogicGate_NOR);
			CreateEntry(ItemID.LogicGate_NXOR);
			CreateEntry(ItemID.LogicGateLamp_On);
			CreateEntry(ItemID.LogicGateLamp_Off);
			CreateEntry(ItemID.LogicGateLamp_Faulty);
			CreateEntry(ItemID.ConveyorBeltLeft);
			CreateEntry(ItemID.ConveyorBeltRight);
			CreateEntry(ItemID.BlendOMatic);

			CreateEntry(ItemID.SteampunkBoiler).AddCondition(Condition.DownedEoC, Condition.DownedEvilBoss, Condition.DownedSkeletron);

			CreateEntry(ItemID.FleshCloningVaat).AddCondition(Condition.Crimson);
			CreateEntry(ItemID.LesionStation).AddCondition(Condition.Corruption);
			CreateEntry(ItemID.BoneWelder).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.HoneyDispenser).AddCondition(Condition.InJungle);
			CreateEntry(ItemID.IceMachine).AddCondition(Condition.InSnow);

			CreateEntry(ItemID.SkyMill).AddCondition(Condition.InSpace);
			CreateEntry(ItemID.LivingLoom).AddCondition(Condition.HasItem(832));
			CreateEntry(ItemID.Teleporter);
			CreateEntry(ItemID.RedSolution).AddCondition(Condition.AstrologicalEvent, Condition.Crimson);
			CreateEntry(ItemID.PurpleSolution).AddCondition(Condition.AstrologicalEvent, Condition.Corruption);

			CreateEntry(ItemID.BlueSolution).AddCondition(!Condition.AstrologicalEvent, Condition.InHallow);
			CreateEntry(ItemID.GreenSolution).AddCondition(!Condition.AstrologicalEvent, !Condition.InHallow);

			CreateEntry(ItemID.Cog).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SteampunkMinecart).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SteampunkGoggles).AddCondition(Condition.Halloween);
		}
	}

	public class TruffleShop : NPCShop
	{
		public override int NPCType => NPCID.Truffle;

		public override void SetDefaults() {
			CreateEntry(ItemID.MushroomSpear).AddCondition(Condition.DownedAnyMechBoss);
			CreateEntry(ItemID.Hammush).AddCondition(Condition.DownedAnyMechBoss);

			CreateEntry(ItemID.MushroomCap);
			CreateEntry(ItemID.Autohammer).AddCondition(Condition.DownedPlantera);
			CreateEntry(ItemID.StrangeGlowingMushroom);
			CreateEntry(ItemID.DarkBlueSolution);
		}
	}

	public class SantaClausShop : NPCShop
	{
		public override int NPCType => NPCID.SantaClaus;

		public override void SetDefaults() {
			CreateEntry(ItemID.SantaHat);
			CreateEntry(ItemID.SantaShirt);
			CreateEntry(ItemID.SantaPants);
			CreateEntry(ItemID.RedLight);
			CreateEntry(ItemID.GreenLight);
			CreateEntry(ItemID.BlueLight);

			for (int i = ItemID.ChristmasTree; i < ItemID.GiantBow; i++) CreateEntry(i);
		}
	}

	public class MechanicShop : NPCShop
	{
		public override int NPCType => NPCID.Mechanic;

		public override void SetDefaults() {
			CreateEntry(ItemID.Wrench);
			CreateEntry(ItemID.BlueWrench);
			CreateEntry(ItemID.GreenWrench);
			CreateEntry(ItemID.YellowWrench);
			CreateEntry(ItemID.WireCutter);
			CreateEntry(ItemID.Wire);
			CreateEntry(ItemID.Lever);
			CreateEntry(ItemID.Switch);
			CreateEntry(ItemID.RedPressurePlate);
			CreateEntry(ItemID.GreenPressurePlate);
			CreateEntry(ItemID.GrayPressurePlate);
			CreateEntry(ItemID.BrownPressurePlate);
			CreateEntry(ItemID.BluePressurePlate);
			CreateEntry(ItemID.YellowPressurePlate);
			CreateEntry(ItemID.OrangePressurePlate);
			CreateEntry(ItemID.ProjectilePressurePad);
			CreateEntry(ItemID.BoosterTrack);
			CreateEntry(ItemID.Actuator);
			CreateEntry(ItemID.WirePipe);
			CreateEntry(ItemID.LaserRuler);
			CreateEntry(ItemID.MechanicalLens);
			CreateEntry(ItemID.EngineeringHelmet);
			CreateEntry(ItemID.WireBulb);
			CreateEntry(ItemID.Timer5Second);
			CreateEntry(ItemID.Timer3Second);
			CreateEntry(ItemID.Timer1Second);
			CreateEntry(ItemID.TimerOneHalfSecond);
			CreateEntry(ItemID.TimerOneFourthSecond);
			CreateEntry(ItemID.MechanicsRod).AddCondition(Condition.NPCExists(369), Condition.PhaseQuarterAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtLeft | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class WizardShop : NPCShop
	{
		public override int NPCType => NPCID.Wizard;

		public override void SetDefaults() {
			CreateEntry(ItemID.CrystalBall);
			CreateEntry(ItemID.IceRod);
			CreateEntry(ItemID.GreaterManaPotion);
			CreateEntry(ItemID.Bell);
			CreateEntry(ItemID.Harp);
			CreateEntry(ItemID.SpellTome);
			CreateEntry(ItemID.Book);
			CreateEntry(ItemID.MusicBox);
			CreateEntry(ItemID.EmptyDropper);
			CreateEntry(ItemID.WizardsHat).AddCondition(Condition.Halloween);
		}
	}

	public class GoblinTinkererShop : NPCShop
	{
		public override int NPCType => NPCID.GoblinTinkerer;

		public override void SetDefaults() {
			CreateEntry(ItemID.RocketBoots);
			CreateEntry(ItemID.Ruler);
			CreateEntry(ItemID.TinkerersWorkshop);
			CreateEntry(ItemID.GrapplingHook);
			CreateEntry(ItemID.Toolbelt);
			CreateEntry(ItemID.SpikyBall);
		}
	}

	public class ClothierShop : NPCShop
	{
		public override int NPCType => NPCID.Clothier;

		public override void SetDefaults() {
			CreateEntry(ItemID.BlackThread);
			CreateEntry(ItemID.PinkThread);
			CreateEntry(ItemID.SummerHat).AddCondition(Condition.TimeDay);
			CreateEntry(ItemID.PlumbersShirt).AddCondition(Condition.PhaseFull);
			CreateEntry(ItemID.PlumbersPants).AddCondition(Condition.PhaseFull);
			CreateEntry(ItemID.WhiteTuxedoShirt).AddCondition(Condition.PhaseFull, Condition.TimeNight);
			CreateEntry(ItemID.SpookyHook).AddCondition(Condition.PhaseFull, Condition.TimeNight);
			CreateEntry(ItemID.TheDoctorsShirt).AddCondition(Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.TheDoctorsPants).AddCondition(Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.FamiliarShirt);
			CreateEntry(ItemID.FamiliarPants);
			CreateEntry(ItemID.FamiliarWig);
			CreateEntry(ItemID.ClownHat).AddCondition(Condition.DownedClown);
			CreateEntry(ItemID.ClownShirt).AddCondition(Condition.DownedClown);
			CreateEntry(ItemID.ClownPants).AddCondition(Condition.DownedClown);

			CreateEntry(ItemID.MimeMask).AddCondition(Condition.BloodMoon);
			CreateEntry(ItemID.FallenTuxedoShirt).AddCondition(Condition.BloodMoon, Condition.TimeNight);
			CreateEntry(ItemID.FallenTuxedoPants).AddCondition(Condition.BloodMoon, Condition.TimeNight);

			CreateEntry(ItemID.WhiteLunaticHood).AddCondition(Condition.DownedLunaticCultist, Condition.TimeDay);
			CreateEntry(ItemID.WhiteLunaticRobe).AddCondition(Condition.DownedLunaticCultist, Condition.TimeDay);
			CreateEntry(ItemID.BlueLunaticHood).AddCondition(Condition.DownedLunaticCultist, Condition.TimeNight);
			CreateEntry(ItemID.BlueLunaticRobe).AddCondition(Condition.DownedLunaticCultist, Condition.TimeNight);

			CreateEntry(ItemID.TaxCollectorHat).AddCondition(Condition.NPCExists(441));
			CreateEntry(ItemID.TaxCollectorSuit).AddCondition(Condition.NPCExists(441));
			CreateEntry(ItemID.TaxCollectorPants).AddCondition(Condition.NPCExists(441));

			CreateEntry(ItemID.UndertakerHat).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.UndertakerCoat).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.FuneralHat).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.FuneralCoat).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.FuneralPants).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.TragicUmbrella).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.VictorianGothHat).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(ItemID.VictorianGothDress).AddCondition(Condition.InGraveyardBiome);

			CreateEntry(ItemID.Beanie).AddCondition(Condition.InSnow);
			CreateEntry(ItemID.GuyFawkesMask).AddCondition(Condition.Halloween);

			CreateEntry(ItemID.TamOShanter).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft);
			CreateEntry(ItemID.GraduationCapBlue).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.GraduationGownBlue).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.Tiara).AddCondition(Condition.Hardmode, Condition.PhaseEmpty);
			CreateEntry(ItemID.PrincessDress).AddCondition(Condition.Hardmode, Condition.PhaseEmpty);
			CreateEntry(ItemID.GraduationCapMaroon).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.GraduationGownMaroon).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.CowboyHat).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(ItemID.CowboyJacket).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(ItemID.CowboyPants).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(ItemID.GraduationCapBlack).AddCondition(Condition.Hardmode, Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.GraduationGownBlack).AddCondition(Condition.Hardmode, Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.BallaHat).AddCondition(Condition.DownedFrostLegion);
			CreateEntry(ItemID.GangstaHat).AddCondition(Condition.DownedFrostLegion);
			CreateEntry(ItemID.ClothierJacket).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.ClothierPants).AddCondition(Condition.Halloween);

			CreateEntry(ItemID.PartyBundleOfBalloonsAccessory).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.PartyBalloonAnimal).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.FlowerBoyHat).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.FlowerBoyShirt).AddCondition(Condition.PartyTime);
			CreateEntry(ItemID.FlowerBoyPants).AddCondition(Condition.PartyTime);

			CreateEntry(ItemID.HunterCloak).AddCondition(Condition.GolfScore(2000, Operation.GreaterEqual));
		}
	}

	public class DemolitionistShop : NPCShop
	{
		public override int NPCType => NPCID.Demolitionist;

		public override void SetDefaults() {
			CreateEntry(ItemID.Grenade);
			CreateEntry(ItemID.Bomb);
			CreateEntry(ItemID.Dynamite);

			CreateEntry(ItemID.HellfireArrow).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.LandMine).AddCondition(Condition.Hardmode, Condition.DownedPlantera, Condition.DownedPirates);
			CreateEntry(ItemID.ExplosivePowder).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.DryBomb).AddCondition(Condition.HasItem(4827));
			CreateEntry(ItemID.WetBomb).AddCondition(Condition.HasItem(4824));
			CreateEntry(ItemID.LavaBomb).AddCondition(Condition.HasItem(4825));
			CreateEntry(ItemID.HoneyBomb).AddCondition(Condition.HasItem(4826));
		}
	}

	public class DryadShop : NPCShop
	{
		public override int NPCType => NPCID.Dryad;

		public override void SetDefaults() {
			CreateEntry(ItemID.ViciousPowder).AddCondition(Condition.BloodMoon, Condition.Crimson);
			CreateEntry(ItemID.CrimsonSeeds).AddCondition(Condition.BloodMoon, Condition.Crimson);
			CreateEntry(ItemID.CrimsonGrassEcho).AddCondition(Condition.BloodMoon, Condition.Crimson);

			CreateEntry(ItemID.VilePowder).AddCondition(Condition.BloodMoon, Condition.Corruption);
			CreateEntry(ItemID.CorruptSeeds).AddCondition(Condition.BloodMoon, Condition.Corruption);
			CreateEntry(ItemID.CorruptGrassEcho).AddCondition(Condition.BloodMoon, Condition.Corruption);

			CreateEntry(ItemID.PurificationPowder).AddCondition(!Condition.BloodMoon);
			CreateEntry(ItemID.GrassSeeds).AddCondition(!Condition.BloodMoon);
			CreateEntry(ItemID.Sunflower).AddCondition(!Condition.BloodMoon);
			CreateEntry(ItemID.GrassWall).AddCondition(!Condition.BloodMoon);

			CreateEntry(ItemID.CorruptSeeds).AddCondition(Condition.Hardmode, Condition.InGraveyardBiome, Condition.Crimson);
			CreateEntry(ItemID.CrimsonSeeds).AddCondition(Condition.Hardmode, Condition.InGraveyardBiome, Condition.Corruption);

			CreateEntry(ItemID.Acorn);
			CreateEntry(ItemID.DirtRod);
			CreateEntry(ItemID.PumpkinSeed);
			CreateEntry(ItemID.FlowerWall);

			CreateEntry(ItemID.JungleWall).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.HallowedSeeds).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.HallowedGrassEcho).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.MushroomGrassSeeds).AddCondition(Condition.InGlowshroom);

			CreateEntry(ItemID.DryadCoverings).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.DryadLoincloth).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.DayBloomPlanterBox).AddCondition(Condition.DownedKingSlime);
			CreateEntry(ItemID.MoonglowPlanterBox).AddCondition(Condition.DownedQueenBee);
			CreateEntry(ItemID.BlinkrootPlanterBox).AddCondition(Condition.DownedEoC);
			CreateEntry(ItemID.CorruptPlanterBox).AddCondition(Condition.DownedEoW);
			CreateEntry(ItemID.CrimsonPlanterBox).AddCondition(Condition.DownedBoC);
			CreateEntry(ItemID.WaterleafPlanterBox).AddCondition(Condition.DownedSkeletron);
			CreateEntry(ItemID.ShiverthornPlanterBox).AddCondition(Condition.DownedSkeletron);

			CreateEntry(ItemID.FireBlossomPlanterBox).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.FlowerPacketWhite);
			CreateEntry(ItemID.FlowerPacketYellow);
			CreateEntry(ItemID.FlowerPacketRed);
			CreateEntry(ItemID.FlowerPacketPink);
			CreateEntry(ItemID.FlowerPacketMagenta);
			CreateEntry(ItemID.FlowerPacketViolet);
			CreateEntry(ItemID.FlowerPacketBlue);
			CreateEntry(ItemID.FlowerPacketWild);
			CreateEntry(ItemID.FlowerPacketTallGrass);

			CreateEntry(ItemID.PottedForestCedar).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.PottedJungleCedar).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(ItemID.PottedHallowCedar).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(ItemID.PottedForestTree).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.PottedJungleTree).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(ItemID.PottedHallowTree).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);

			CreateEntry(ItemID.PottedForestPalm).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.PottedJunglePalm).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(ItemID.PottedHallowPalm).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);

			CreateEntry(ItemID.PottedForestBamboo).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.PottedJungleBamboo).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(ItemID.PottedHallowBamboo).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class ArmsDealerShop : NPCShop
	{
		public override int NPCType => NPCID.ArmsDealer;

		public override void SetDefaults() {
			CreateEntry(ItemID.MusketBall);

			CreateEntry(ItemID.TungstenBullet).AddCondition(Condition.Hardmode | (Condition.BloodMoon & new SimpleCondition(NetworkText.FromKey("WorldSilverOre"), () => WorldGen.SavedOreTiers.Silver == 168)));
			CreateEntry(ItemID.SilverBullet).AddCondition(Condition.Hardmode | (Condition.BloodMoon & new SimpleCondition(NetworkText.FromKey("WorldTungstenOre"), () => WorldGen.SavedOreTiers.Silver != 168)));
			CreateEntry(ItemID.UnholyArrow).AddCondition((Condition.DownedEvilBoss & Condition.TimeNight) | Condition.Hardmode);

			CreateEntry(ItemID.FlintlockPistol);
			CreateEntry(ItemID.Minishark);

			CreateEntry(ItemID.QuadBarrelShotgun).AddCondition(Condition.InGraveyardBiome, Condition.DownedSkeletron);
			CreateEntry(ItemID.IllegalGunParts).AddCondition(Condition.TimeNight);
			CreateEntry(ItemID.Shotgun).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.EmptyBullet).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.StyngerBolt).AddCondition(Condition.HasItem(1258));
			CreateEntry(ItemID.Stake).AddCondition(Condition.HasItem(1835));
			CreateEntry(ItemID.Nail).AddCondition(Condition.HasItem(3107));
			CreateEntry(ItemID.CandyCorn).AddCondition(Condition.HasItem(1782));
			CreateEntry(ItemID.ExplosiveJackOLantern).AddCondition(Condition.HasItem(1784));

			CreateEntry(ItemID.NurseHat).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.NurseShirt).AddCondition(Condition.Halloween);
			CreateEntry(ItemID.NursePants).AddCondition(Condition.Halloween);
		}
	}

	public class MerchantShop : NPCShop
	{
		public override int NPCType => NPCID.Merchant;

		public override void SetDefaults() {
			CreateEntry(ItemID.MiningHelmet);
			CreateEntry(ItemID.PiggyBank);
			CreateEntry(ItemID.IronAnvil);
			CreateEntry(ItemID.BugNet);
			CreateEntry(ItemID.CopperPickaxe);
			CreateEntry(ItemID.CopperAxe);
			CreateEntry(ItemID.Torch);
			CreateEntry(ItemID.LesserHealingPotion);
			CreateEntry(ItemID.LesserManaPotion);
			CreateEntry(ItemID.WoodenArrow);
			CreateEntry(ItemID.Shuriken);
			CreateEntry(ItemID.Rope);

			CreateEntry(ItemID.Marshmallow).AddCondition(Condition.InSnow);
			CreateEntry(ItemID.Furnace).AddCondition(Condition.InJungle);
			CreateEntry(ItemID.PinWheel).AddCondition(Condition.TimeDay, new SimpleCondition(NetworkText.FromLiteral("HappyWind"), () => Main.IsItAHappyWindyDay));
			CreateEntry(ItemID.ThrowingKnife).AddCondition(Condition.BloodMoon);
			CreateEntry(ItemID.Glowstick).AddCondition(Condition.TimeNight);
			CreateEntry(ItemID.Safe).AddCondition(Condition.DownedSkeletron);
			CreateEntry(ItemID.DiscoBall).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.Flare).AddCondition(Condition.HasItem(930));
			CreateEntry(ItemID.BlueFlare).AddCondition(Condition.HasItem(930));

			CreateEntry(ItemID.Sickle);
			CreateEntry(ItemID.GoldDust).AddCondition(Condition.Hardmode);
			CreateEntry(ItemID.SharpeningStation).AddCondition(Condition.Hardmode);

			CreateEntry(ItemID.Nail).AddCondition(Condition.HasItem(3107));
			CreateEntry(ItemID.DrumSet).AddCondition(Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.Hardmode);
			CreateEntry(ItemID.DrumStick).AddCondition(Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.Hardmode);
		}
	}

	public class TravellingMerchantShop : NPCShop
	{
		private class TravellingMerchantList : EntryList
		{
			public override IEnumerable<Item> GetItems(bool checkRequirements = true) {
				Player player = null;
				for (int i = 0; i < 255; i++)
				{
					Player player2 = Main.player[i];
					if (player2.active && (player == null || player.luck < player2.luck)) player = player2;
				}

				player ??= new Player();

				int toPick = Main.rand.Next(4, 7);
				if (player.RollLuck(4) == 0) toPick++;
				if (player.RollLuck(8) == 0) toPick++;
				if (player.RollLuck(16) == 0) toPick++;
				if (player.RollLuck(32) == 0) toPick++;
				if (Main.expertMode && player.RollLuck(2) == 0) toPick++;

				List<Entry> evaluated = new List<Entry>();

				int roll = 0;
				while (roll < toPick)
				{
					Entry entry = entries.LastOrDefault(entry => Conditions.All(condition => {
						condition.SetCustomData(player);
						return condition.Evaluate();
					}));

					if (entry == null || evaluated.Contains(entry)) continue;
					evaluated.Add(entry);

					roll++;

					foreach (Item item in entry.GetItems(false)) yield return item;
				}
			}
		}

		private class LuckCondition : Condition
		{
			private int range;
			private Player player;

			public LuckCondition(int range) : base(NetworkText.FromKey("ShopConditions.PlayerLuck")) {
				this.range = range;
			}

			public override bool Evaluate() => player?.RollLuck(range) == 0;

			public override void SetCustomData(object obj) {
				if (obj is Player player) this.player = player;
			}
		}

		public override int NPCType => NPCID.TravellingMerchant;

		public override bool EvaluateOnOpen => false;

		public override void SetDefaults() {
			TravellingMerchantList list = new TravellingMerchantList();
			AddEntry(list);

			int[] array = { 100, 200, 300, 400, 500, 600 };

			list.CreateEntry(ItemID.BlackCounterweight).AddCondition(new LuckCondition(array[4]));
			list.CreateEntry(ItemID.YellowCounterweight).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.AngelHalo).AddCondition(new LuckCondition(array[5]));
			list.CreateEntry(ItemID.Gatligator).AddCondition(new LuckCondition(array[4]), Condition.Hardmode);
			list.CreateEntry(ItemID.Kimono).AddCondition(new LuckCondition(array[4]));
			list.CreateEntry(ItemID.ArcaneRuneWall).AddCondition(new LuckCondition(array[4]));

			list.CreateEntry(ItemID.ZapinatorGray).AddCondition(new LuckCondition(array[4]), Condition.DownedEoC | Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.DownedQueenBee);
			list.CreateEntry(ItemID.ZapinatorOrange).AddCondition(new LuckCondition(array[4]), Condition.Hardmode);

			list.CreateEntry(ItemID.PulseBow).AddCondition(new LuckCondition(array[3]), Condition.Hardmode, Condition.DownedPlantera);
			list.CreateEntry(ItemID.WaterGun).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.CelestialMagnet).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.DiamondRing).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.CrimsonCloak).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.MysteriousCape).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.RedCape).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.WinterCape).AddCondition(new LuckCondition(array[3]));

			list.CreateEntry(ItemID.HunterCloak).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.SittingDucksFishingRod).AddCondition(new LuckCondition(array[3]), Condition.DownedSkeletron);
			list.CreateEntry(ItemID.CompanionCube).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.AntiPortalBlock).AddCondition(new LuckCondition(array[3]), Condition.Hardmode);
			list.CreateEntry(ItemID.BirdieRattle).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.ExoticEasternChewToy).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.BedazzledNectar).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.BambooLeaf).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(ItemID.BambooLeaf).AddCondition(new LuckCondition(array[3]));

			list.CreateEntry(ItemID.Revolver).AddCondition(new LuckCondition(array[2]), Condition.OrbSmashed);
			list.CreateEntry(ItemID.AmmoBox).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.Fez).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.MagicHat).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.GypsyRobe).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.Gi).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.ChefHat).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(ItemID.ChefHat),
				new EntryItem(ItemID.ChefShirt),
				new EntryItem(ItemID.ChefPants)
			}).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(ItemID.GameMasterShirt),
				new EntryItem(ItemID.GameMasterPants)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(ItemID.StarPrincessCrown).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(ItemID.StarPrincessCrown),
				new EntryItem(ItemID.StarPrincessDress),
				new EntryItem(ItemID.CelestialWand)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(ItemID.DemonHorns).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.DevilHorns).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.PandaEars).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.Fedora).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.StarHairpin).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.HeartHairpin).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(ItemID.UnicornHornHat).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(ItemID.PrettyPinkRibbon),
				new EntryItem(ItemID.PrettyPinkDressPants),
				new EntryItem(ItemID.PrettyPinkDressSkirt)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(ItemID.Code1).AddCondition(new LuckCondition(array[2]), Condition.DownedEoC);
			list.CreateEntry(ItemID.Code2).AddCondition(new LuckCondition(array[2]), Condition.DownedAnyMechBoss);
			list.CreateEntry(ItemID.MoonLordPainting).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMoonLord);
			list.CreateEntry(ItemID.PaintingCastleMarsberg).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(ItemID.PaintingMartiaLisa).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(ItemID.PaintingTheTruthIsUpThere).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(ItemID.PaintingAcorns).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(ItemID.PaintingColdSnap).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(ItemID.PaintingCursedSaint).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(ItemID.PaintingSnowfellas).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(ItemID.PaintingTheSeason).AddCondition(new LuckCondition(array[2]), Condition.Christmas);

			list.CreateEntry(ItemID.BrickLayer).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.ExtendoGrip).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.PaintSprayer).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.PortableCementMixer).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.ActuationAccessory).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.Katana).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(ItemID.UltrabrightTorch).AddCondition(new LuckCondition(array[1]));

			list.CreateEntry(ItemID.Sake).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.PadThai).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.Pho).AddCondition(new LuckCondition(array[0]));

			list.AddEntry(new EntryListRandom
			{
				new EntryItem(ItemID.TigerSkin),
				new EntryItem(ItemID.LeopardSkin),
				new EntryItem(ItemID.ZebraSkin)
			}).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(ItemID.UltrabrightTorch).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(ItemID.SteampunkCup).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.FancyDishes).AddCondition(new LuckCondition(array[0]));

			list.AddEntry(new EntryList
			{
				new EntryItem(ItemID.DynastyWood),
				new EntryItem(ItemID.RedDynastyShingles),
				new EntryItem(ItemID.BlueDynastyShingles)
			}).AddCondition(new LuckCondition(array[0]));

			EntryListRandom teamBlocks = new EntryListRandom
			{
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockWhite),
					new EntryItem(ItemID.TeamBlockWhitePlatform)
				},
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockRed),
					new EntryItem(ItemID.TeamBlockRedPlatform)
				},
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockBlue),
					new EntryItem(ItemID.TeamBlockBluePlatform)
				},
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockGreen),
					new EntryItem(ItemID.TeamBlockGreenPlatform)
				},
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockYellow),
					new EntryItem(ItemID.TeamBlockYellowPlatform)
				},
				new EntryList
				{
					new EntryItem(ItemID.TeamBlockPink),
					new EntryItem(ItemID.TeamBlockPinkPlatform)
				}
			};
			list.AddEntry(teamBlocks).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(ItemID.LawnFlamingo).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.DPSMeter).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.LifeformAnalyzer).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(ItemID.Stopwatch).AddCondition(new LuckCondition(array[0]));
		}
	}
}