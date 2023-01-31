using System;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.Localization;

namespace Terraria.ModLoader;

public partial class ChestLoot {
	public interface ICondition {
		string Description { get; }

		bool IsAvailable();
	}

	public sealed class Condition : ICondition {
		#region Common conditions
		public static readonly Condition Christmas = new(NetworkText.FromLiteral("During Christmas"), () => Main.xMas);
		public static readonly Condition Halloween = new(NetworkText.FromLiteral("During Halloween"), () => Main.halloween);
		public static readonly Condition Hardmode = new(NetworkText.FromLiteral("In Hardmode"), () => Main.hardMode);
		public static readonly Condition PreHardmode = new(NetworkText.FromLiteral("Before Hardmode"), () => !Main.hardMode);
		public static readonly Condition TimeDay = new(NetworkText.FromLiteral("During day"), () => Main.dayTime);
		public static readonly Condition TimeNight = new(NetworkText.FromLiteral("During night"), () => !Main.dayTime);
		public static readonly Condition NotBloodMoon = new(NetworkText.FromLiteral("During normal night"), () => !Main.bloodMoon);
		public static readonly Condition BloodMoon = new(NetworkText.FromLiteral("During blood moon"), () => Main.bloodMoon);
		public static readonly Condition Eclipse = new(NetworkText.FromLiteral("During solar eclipse"), () => Main.eclipse);
		public static readonly Condition NotEclipse = new(NetworkText.FromLiteral("During normal day"), () => !Main.eclipse);
		public static readonly Condition Thunderstorm = new(NetworkText.FromLiteral("During thunderstorm"), () => Main.IsItStorming);
		public static readonly Condition ForTheWorthy = new(NetworkText.FromLiteral("In For The Worthy world"), () => Main.getGoodWorld);
		public static readonly Condition RemixWorld = new(NetworkText.FromLiteral("In Remix world"), () => Main.remixWorld);
		public static readonly Condition TenthAnniversary = new(NetworkText.FromLiteral("In Tenth Anniversary world"), () => Main.tenthAnniversaryWorld);
		public static readonly Condition NotForTheWorthy = new(NetworkText.FromLiteral("Not in For The Worthy world"), () => !Main.getGoodWorld);
		public static readonly Condition NotRemixWorld = new(NetworkText.FromLiteral("Not in Remix world"), () => !Main.remixWorld);
		public static readonly Condition NotTenthAnniversary = new(NetworkText.FromLiteral("Not in Tenth Anniversary world"), () => !Main.tenthAnniversaryWorld);
		public static readonly Condition BirthdayPartyIsUp = new(NetworkText.FromLiteral("During birthday party"), () => BirthdayParty.PartyIsUp);
		public static readonly Condition NightLanternsUp = new(NetworkText.FromLiteral("While night lanterns are up"), () => LanternNight.LanternsUp);
		public static readonly Condition IsMoonFull = new(NetworkText.FromLiteral("During full moon"), () => Main.GetMoonPhase() == MoonPhase.Full);
		public static readonly Condition IsMoonWaningGibbous = new(NetworkText.FromLiteral("During waning gibbous moon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
		public static readonly Condition IsMoonThirdQuarter = new(NetworkText.FromLiteral("During third quarter moon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
		public static readonly Condition IsMoonWaningCrescent = new(NetworkText.FromLiteral("During waning crescent moon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
		public static readonly Condition IsMoonNew = new(NetworkText.FromLiteral("During new moon"), () => Main.GetMoonPhase() == MoonPhase.Empty);
		public static readonly Condition IsMoonWaxingCrescent = new(NetworkText.FromLiteral("During waxing crescent moon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
		public static readonly Condition IsMoonFirstQuarter = new(NetworkText.FromLiteral("During first quarter moon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
		public static readonly Condition IsMoonWaxingGibbous = new(NetworkText.FromLiteral("During waxing gibbous moon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);
		public static readonly Condition HappyWindyDay = new(NetworkText.FromLiteral("During windy day"), () => Main.IsItAHappyWindyDay);
		public static readonly Condition InShoppingForestBiome = new(NetworkText.FromLiteral("In Forest"), () => Main.LocalPlayer.ShoppingZone_Forest);
		public static readonly Condition InForestBiome = new(NetworkText.FromLiteral("In Forest"), () => Main.LocalPlayer.ZoneForest);
		public static readonly Condition InPurityBiome = new(NetworkText.FromLiteral("In Purity"), () => Main.LocalPlayer.ZonePurity);
		public static readonly Condition InSnowBiome = new(NetworkText.FromLiteral("In Snow"), () => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InJungleBiome = new(NetworkText.FromLiteral("In Jungle"), () => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InCorruptBiome = new(NetworkText.FromLiteral("In Corruption"), () => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InCrimsonBiome = new(NetworkText.FromLiteral("In Crimson"), () => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InHallowBiome = new(NetworkText.FromLiteral("In Hallow"), () => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InDesertBiome = new(NetworkText.FromLiteral("In Desert"), () => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGraveyard = new(NetworkText.FromLiteral("In Graveyard"), () => Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition InGlowshroomBiome = new(NetworkText.FromLiteral("In Glowing Mushroom"), () => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InBeachBiome = new(NetworkText.FromLiteral("In Ocean"), () => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InUnderworld = new(NetworkText.FromLiteral("In Underworld"), () => Main.LocalPlayer.ZoneUnderworldHeight);
		public static readonly Condition InDungeonBiome = new(NetworkText.FromLiteral("In Dungeon"), () => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition InSpace = new(NetworkText.FromLiteral("In Space"), () => Main.LocalPlayer.ZoneSkyHeight);
		public static readonly Condition NotInForestBiome = new(NetworkText.FromLiteral("Not in Forest"), () => !Main.LocalPlayer.ZoneForest);
		public static readonly Condition NotInPurityBiome = new(NetworkText.FromLiteral("Not in Purity"), () => !Main.LocalPlayer.ZonePurity);
		public static readonly Condition NotInSnowBiome = new(NetworkText.FromLiteral("Not in Snow"), () => !Main.LocalPlayer.ZoneSnow);
		public static readonly Condition NotInJungleBiome = new(NetworkText.FromLiteral("Not in Jungle"), () => !Main.LocalPlayer.ZoneJungle);
		public static readonly Condition NotInCorruptBiome = new(NetworkText.FromLiteral("Not in Corruption"), () => !Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition NotInCrimsonBiome = new(NetworkText.FromLiteral("Not in Crimson"), () => !Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition NotInHallowBiome = new(NetworkText.FromLiteral("Not in Hallow"), () => !Main.LocalPlayer.ZoneHallow);
		public static readonly Condition NotInDesertBiome = new(NetworkText.FromLiteral("Not in Desert"), () => !Main.LocalPlayer.ZoneDesert);
		public static readonly Condition NotInGraveyard = new(NetworkText.FromLiteral("Not in Graveyard"), () => !Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition NotInGlowshroomBiome = new(NetworkText.FromLiteral("Not in Glowing Mushroom"), () => !Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition NotInBeachBiome = new(NetworkText.FromLiteral("Not in Ocean"), () => !Main.LocalPlayer.ZoneBeach);
		public static readonly Condition NotInUnderworld = new(NetworkText.FromLiteral("Not in Underworld"), () => !Main.LocalPlayer.ZoneUnderworldHeight);
		public static readonly Condition NotInDungeonBiome = new(NetworkText.FromLiteral("Not in Dungeon"), () => !Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition NotInSpace = new(NetworkText.FromLiteral("Not in Space"), () => Main.LocalPlayer.ZoneSkyHeight);
		public static readonly Condition CorruptionWorld = new(NetworkText.FromLiteral("In Corruption world"), () => !WorldGen.crimson);
		public static readonly Condition CrimsonWorld = new(NetworkText.FromLiteral("In Crimson world"), () => WorldGen.crimson);
		public static readonly Condition DownedKingSlime = new(NetworkText.FromLiteral("King Slime is slain"), () => NPC.downedSlimeKing);
		public static readonly Condition DownedEyeOfCthulhu = new(NetworkText.FromLiteral("Eye of Cthulhu is slain"), () => NPC.downedBoss1);
		public static readonly Condition DownedEowOrBoc = new(NetworkText.FromLiteral("Boss of Evil is slain"), () => NPC.downedBoss2);
		public static readonly Condition DownedEaterOfWorlds = new(NetworkText.FromLiteral("Eater of Worlds is slain"), () => NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition DownedBrainOfCthulhu = new(NetworkText.FromLiteral("Brain of Cthulhu is slain"), () => NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition DownedQueenBee = new(NetworkText.FromLiteral("Queen Bee is slain"), () => NPC.downedQueenBee);
		public static readonly Condition DownedSkeletron = new(NetworkText.FromLiteral("Skeletron is slain"), () => NPC.downedBoss3);
		public static readonly Condition DownedQueenSlime = new(NetworkText.FromLiteral("Queen Slime is slain"), () => NPC.downedQueenSlime);
		public static readonly Condition DownedMechBossAny = new(NetworkText.FromLiteral("Any Mechanical Boss is slain"), () => NPC.downedMechBossAny);
		public static readonly Condition DownedTwins = new(NetworkText.FromLiteral("The Twins are slain"), () => NPC.downedMechBoss2);
		public static readonly Condition DownedDestroyer = new(NetworkText.FromLiteral("The Destroyer is slain"), () => NPC.downedMechBoss1);
		public static readonly Condition DownedSkeletronPrime = new(NetworkText.FromLiteral("Skeletron Prime is slain"), () => NPC.downedMechBoss3);
		public static readonly Condition DownedPlantera = new(NetworkText.FromLiteral("Plantera is slain"), () => NPC.downedPlantBoss);
		public static readonly Condition DownedEmpressOfLight = new(NetworkText.FromLiteral("Empress of Light is slain"), () => NPC.downedEmpressOfLight);
		public static readonly Condition DownedGolem = new(NetworkText.FromLiteral("Golem is slain"), () => NPC.downedGolemBoss);
		public static readonly Condition DownedCultist = new(NetworkText.FromLiteral("Lunatic Cultist is slain"), () => NPC.downedAncientCultist);
		public static readonly Condition DownedMoonLord = new(NetworkText.FromLiteral("Moon Lord is slain"), () => NPC.downedMoonlord);
		public static readonly Condition DownedClown = new(NetworkText.FromLiteral("Clown is slain"), () => NPC.downedClown);
		public static readonly Condition DownedPirates = new(NetworkText.FromLiteral("Pirates are defeated"), () => NPC.downedPirates);
		public static readonly Condition DownedMartians = new(NetworkText.FromLiteral("Martians are defeated"), () => NPC.downedMartians);
		public static readonly Condition DownedFrost = new(NetworkText.FromLiteral("Frost Legion is defeated"), () => NPC.downedFrost);
		public static readonly Condition HappyEnough = new(NetworkText.FromLiteral("Is Happy enough"), () => Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421);

		public static readonly Condition InExpertMode = new(NetworkText.FromLiteral("In Expert mode"), () => Main.expertMode);
		public static readonly Condition InMasterMode = new(NetworkText.FromLiteral("In Master mode"), () => Main.masterMode);
		#endregion

		private readonly NetworkText DescriptionText;
		private readonly Func<bool> Predicate;

		public string Description => DescriptionText.ToString();

		public Condition(NetworkText description, Func<bool> predicate) {
			DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public bool IsAvailable() => Predicate();
	}
}
