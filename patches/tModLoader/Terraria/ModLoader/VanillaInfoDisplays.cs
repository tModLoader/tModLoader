using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public abstract class VanillaInfoDisplay : InfoDisplay
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue(LangKey);

		protected abstract string LangKey { get; }
	}

	public class WatchesInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_0";

		protected override string LangKey => "LegacyInterface.95";

		public override bool Active() {
			return Main.player[Main.myPlayer].accWatch > 0;
		}

		public override string DisplayValue() {
			string textValue = Language.GetTextValue("GameUI.TimeAtMorning");
			double num5 = Main.time;
			if (!Main.dayTime)
				num5 += 54000.0;

			num5 = num5 / 86400.0 * 24.0;
			double num6 = 7.5;
			num5 = num5 - num6 - 12.0;
			if (num5 < 0.0)
				num5 += 24.0;

			if (num5 >= 12.0)
				textValue = Language.GetTextValue("GameUI.TimePastMorning");

			int num7 = (int)num5;
			double num8 = num5 - (double)num7;
			num8 = (int)(num8 * 60.0);
			string text4 = string.Concat(num8);
			if (num8 < 10.0)
				text4 = "0" + text4;

			if (num7 > 12)
				num7 -= 12;

			if (num7 == 0)
				num7 = 12;

			if (Main.player[Main.myPlayer].accWatch == 1)
				text4 = "00";
			else if (Main.player[Main.myPlayer].accWatch == 2)
				text4 = ((!(num8 < 30.0)) ? "30" : "00");

			return num7 + ":" + text4 + " " + textValue;
		}
	}

	public class WeatherRadioInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_1";

		protected override string LangKey => "LegacyInterface.96";

		public override bool Active() {
			return Main.player[Main.myPlayer].accWeatherRadio;
		}

		public override string DisplayValue() {
			string weather = Main.IsItStorming
						? Language.GetTextValue("GameUI.Storm")
						: (Main.maxRaining > 0.6
							? Language.GetTextValue("GameUI.HeavyRain")
							: (Main.maxRaining >= 0.2
								? Language.GetTextValue("GameUI.Rain")
								: (Main.maxRaining > 0f
									? Language.GetTextValue("GameUI.LightRain")
									: (Main.cloudBGActive > 0f
										? Language.GetTextValue("GameUI.Overcast")
										: (Main.numClouds > 90
											? Language.GetTextValue("GameUI.MostlyCloudy")
											: (Main.numClouds > 55
												? Language.GetTextValue("GameUI.Cloudy")
												: (Main.numClouds <= 15
													? Language.GetTextValue("GameUI.Clear")
													: Language.GetTextValue("GameUI.PartlyCloudy"))))))));
			int windSpeed = (int)(Main.windSpeedCurrent * 50f);
			if (windSpeed < 0)
				weather += Language.GetTextValue("GameUI.EastWind", Math.Abs(windSpeed));
			else if (windSpeed > 0)
				weather += Language.GetTextValue("GameUI.WestWind", windSpeed);
			return weather;
		}
	}

	public class SextantInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture {
			get {
				int index = 7;
				if ((Main.bloodMoon && !Main.dayTime) || (Main.eclipse && Main.dayTime))
					index = 8;
				return $"Terraria/Images/UI/InfoIcon_" + index;
			}
		}

		protected override string LangKey => "LegacyInterface.102";

		public override bool Active() {
			return Main.player[Main.myPlayer].accCalendar;
		}

		public override string DisplayValue() {
			if (Main.moonPhase == 0)
				return Language.GetTextValue("GameUI.FullMoon");
			else if (Main.moonPhase == 1)
				return Language.GetTextValue("GameUI.WaningGibbous");
			else if (Main.moonPhase == 2)
				return Language.GetTextValue("GameUI.ThirdQuarter");
			else if (Main.moonPhase == 3)
				return Language.GetTextValue("GameUI.WaningCrescent");
			else if (Main.moonPhase == 4)
				return Language.GetTextValue("GameUI.NewMoon");
			else if (Main.moonPhase == 5)
				return Language.GetTextValue("GameUI.WaxingCrescent");
			else if (Main.moonPhase == 6)
				return Language.GetTextValue("GameUI.FirstQuarter");
			else if (Main.moonPhase == 7)
				return Language.GetTextValue("GameUI.WaxingGibbous");

			return "How did we get here?"; // tML: This is an indication that something has gone wrong. Only ignore this if you know what you're doing.
		}
	}

	public class FishFinderInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_2";

		protected override string LangKey => "LegacyInterface.97";

		public override bool Active() {
			return Main.player[Main.myPlayer].accFishFinder;
		}

		public override string DisplayValue() {
			bool currentlyFishing = false;
			for (int j = 0; j < 1000; j++) {
				if (Main.projectile[j].active && Main.projectile[j].owner == Main.myPlayer && Main.projectile[j].bobber) {
					currentlyFishing = true;
					break;
				}
			}

			if (!currentlyFishing) {
				PlayerFishingConditions fishingConditions = Main.player[Main.myPlayer].GetFishingConditions();
				Main.player[Main.myPlayer].displayedFishingInfo = Language.GetTextValue("GameUI.FishingPower", fishingConditions.FinalFishingLevel);
				if (fishingConditions.BaitItemType == ItemID.TruffleWorm)
					Main.player[Main.myPlayer].displayedFishingInfo = Language.GetTextValue("GameUI.FishingWarning");
			}

			return Main.player[Main.myPlayer].displayedFishingInfo;
		}
	}

	public class MetalDetectorInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_10";

		protected override string LangKey => "LegacyInterface.104";

		public override bool Active() {
			return Main.player[Main.myPlayer].accOreFinder;
		}

		public override string DisplayValue() {
			string oresFound;
			if (Main.SceneMetrics.bestOre <= 0) {
				oresFound = Language.GetTextValue("GameUI.NoTreasureNearby");
			}
			else {
				int baseOption = 0;
				int num10 = Main.SceneMetrics.bestOre;
				if (Main.SceneMetrics.ClosestOrePosition.HasValue) {
					Microsoft.Xna.Framework.Point value = Main.SceneMetrics.ClosestOrePosition.Value;
					Tile tileSafely = Framing.GetTileSafely(value);
					if (tileSafely.active()) {
						MapHelper.GetTileBaseOption(value.Y, tileSafely, ref baseOption);
						num10 = tileSafely.type;
						if (TileID.Sets.BasicChest[num10] || TileID.Sets.BasicChestFake[num10])
							baseOption = 0;
					}
				}

				oresFound = Language.GetTextValue("GameUI.OreDetected", Lang.GetMapObjectName(MapHelper.TileToLookup(num10, baseOption)));
			}

			return oresFound;
		}
	}

	public class LifeformAnalyzerInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_11";

		protected override string LangKey => "LegacyInterface.105";

		public override bool Active() {
			return Main.player[Main.myPlayer].accCritterGuide;
		}

		public override string DisplayValue() {
			int num11 = 1300;
			int num12 = 0;
			int num13 = -1;
			if (Main.player[Main.myPlayer].accCritterGuideCounter <= 0) {
				Main.player[Main.myPlayer].accCritterGuideCounter = 15;
				for (int k = 0; k < 200; k++) {
					if (Main.npc[k].active && Main.npc[k].rarity > num12 && (Main.npc[k].Center - Main.player[Main.myPlayer].Center).Length() < (float)num11) {
						num13 = k;
						num12 = Main.npc[k].rarity;
					}
				}

				Main.player[Main.myPlayer].accCritterGuideNumber = (byte)num13;
			}
			else {
				Main.player[Main.myPlayer].accCritterGuideCounter--;
				num13 = Main.player[Main.myPlayer].accCritterGuideNumber;
			}

			return (num13 < 0 || num13 >= 200 || !Main.npc[num13].active || Main.npc[num13].rarity <= 0) ? Language.GetTextValue("GameUI.NoRareCreatures") : Main.npc[num13].GivenOrTypeName;
		}
	}

	public class RadarInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_5";

		protected override string LangKey => "LegacyInterface.100";

		public override bool Active() {
			return Main.player[Main.myPlayer].accThirdEye;
		}

		public override string DisplayValue() {
			int num14 = 2000;
			if (Main.player[Main.myPlayer].accThirdEyeCounter == 0) {
				Main.player[Main.myPlayer].accThirdEyeNumber = 0;
				Main.player[Main.myPlayer].accThirdEyeCounter = 15;
				for (int l = 0; l < 200; l++) {
					if (Main.npc[l].active && !Main.npc[l].friendly && Main.npc[l].damage > 0 && Main.npc[l].lifeMax > 5 && !Main.npc[l].dontCountMe && (Main.npc[l].Center - Main.player[Main.myPlayer].Center).Length() < (float)num14)
						Main.player[Main.myPlayer].accThirdEyeNumber++;
				}
			}
			else {
				Main.player[Main.myPlayer].accThirdEyeCounter--;
			}

			return (Main.player[Main.myPlayer].accThirdEyeNumber == 0)
				? Language.GetTextValue("GameUI.NoEnemiesNearby")
				: ((Main.player[Main.myPlayer].accThirdEyeNumber != 1)
					? Language.GetTextValue("GameUI.EnemiesNearby", Main.player[Main.myPlayer].accThirdEyeNumber)
					: Language.GetTextValue("GameUI.OneEnemyNearby"));
		}
	}

	public class TallyCounterInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_6";

		protected override string LangKey => "LegacyInterface.101";

		public override bool Active() {
			return Main.player[Main.myPlayer].accJarOfSouls;
		}

		public override string DisplayValue() {
			int lastCreatureHit = Main.player[Main.myPlayer].lastCreatureHit;
			return ((lastCreatureHit > 0)
				? (Lang.GetNPCNameValue(Item.BannerToNPC(lastCreatureHit)) + ": " + NPC.killCount[lastCreatureHit])
				: Language.GetTextValue("GameUI.NoKillCount"));
		}
	}

	public class DummyInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_8";

		protected override string LangKey => "LegacyInterface.101";

		public override bool Active() {
			return false;
		}

		public override string DisplayValue() {
			return "";
		}
	}

	public class DPSMeterInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_12";

		protected override string LangKey => "LegacyInterface.106";

		public override bool Active() {
			return Main.player[Main.myPlayer].accDreamCatcher;
		}

		public override string DisplayValue() {
			Main.player[Main.myPlayer].checkDPSTime();
			int dPS = Main.player[Main.myPlayer].getDPS();
			return (dPS != 0) ? Language.GetTextValue("GameUI.DPS", Main.player[Main.myPlayer].getDPS()) : Language.GetTextValue("GameUI.NoDPS");
		}
	}

	public class StopwatchInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_9";

		protected override string LangKey => "LegacyInterface.103";

		public override bool Active() {
			return Main.player[Main.myPlayer].accStopwatch;
		}

		public override string DisplayValue() {
			Vector2 vector = Main.player[Main.myPlayer].velocity + Main.player[Main.myPlayer].instantMovementAccumulatedThisFrame;
			if (Main.player[Main.myPlayer].mount.Active && Main.player[Main.myPlayer].mount.IsConsideredASlimeMount && Main.player[Main.myPlayer].velocity.Y != 0f && !Main.player[Main.myPlayer].SlimeDontHyperJump)
				vector.Y += Main.player[Main.myPlayer].velocity.Y;

			int num15 = (int)(1f + vector.Length() * 6f);
			if (num15 > Main.player[Main.myPlayer].speedSlice.Length)
				num15 = Main.player[Main.myPlayer].speedSlice.Length;

			float num16 = 0f;
			for (int num17 = num15 - 1; num17 > 0; num17--) {
				Main.player[Main.myPlayer].speedSlice[num17] = Main.player[Main.myPlayer].speedSlice[num17 - 1];
			}

			Main.player[Main.myPlayer].speedSlice[0] = vector.Length();
			for (int m = 0; m < Main.player[Main.myPlayer].speedSlice.Length; m++) {
				if (m < num15)
					num16 += Main.player[Main.myPlayer].speedSlice[m];
				else
					Main.player[Main.myPlayer].speedSlice[m] = num16 / (float)num15;
			}

			num16 /= (float)num15;
			int num18 = 42240;
			int num19 = 216000;
			float num20 = num16 * (float)num19 / (float)num18;
			if (!Main.player[Main.myPlayer].merman && !Main.player[Main.myPlayer].ignoreWater) {
				if (Main.player[Main.myPlayer].honeyWet)
					num20 /= 4f;
				else if (Main.player[Main.myPlayer].wet)
					num20 /= 2f;
			}

			return Language.GetTextValue("GameUI.Speed", Math.Round(num20));
		}
	}

	public class CompassInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_3";

		protected override string LangKey => "LegacyInterface.98";

		public override bool Active() {
			return Main.player[Main.myPlayer].accCompass > 0;
		}

		public override string DisplayValue() {
			int num21 = (int)((Main.player[Main.myPlayer].position.X + (float)(Main.player[Main.myPlayer].width / 2)) * 2f / 16f - (float)Main.maxTilesX);
			return (num21 > 0)
				? Language.GetTextValue("GameUI.CompassEast", num21)
				: ((num21 >= 0)
					? Language.GetTextValue("GameUI.CompassCenter")
					: Language.GetTextValue("GameUI.CompassWest", -num21));
		}
	}

	public class DepthMeterInfoDisplay : VanillaInfoDisplay
	{
		public override string Texture => $"Terraria/Images/UI/InfoIcon_4";

		protected override string LangKey => "LegacyInterface.99";

		public override bool Active() {
			return Main.player[Main.myPlayer].accDepthMeter > 0;
		}

		public override string DisplayValue() {
			int num22 = (int)((double)((Main.player[Main.myPlayer].position.Y + (float)Main.player[Main.myPlayer].height) * 2f / 16f) - Main.worldSurface * 2.0);
			string text6 = "";
			float num23 = Main.maxTilesX / 4200;
			num23 *= num23;
			int num24 = 1200;
			float num25 = (float)((double)(Main.player[Main.myPlayer].Center.Y / 16f - (65f + 10f * num23)) / (Main.worldSurface / 5.0));
			text6 = (Main.player[Main.myPlayer].position.Y > (float)((Main.maxTilesY - 204) * 16))
				? Language.GetTextValue("GameUI.LayerUnderworld")
				: (((double)Main.player[Main.myPlayer].position.Y > Main.rockLayer * 16.0 + (double)(num24 / 2) + 16.0)
					? Language.GetTextValue("GameUI.LayerCaverns")
					: ((num22 > 0)
						? Language.GetTextValue("GameUI.LayerUnderground")
						: ((!(num25 >= 1f))
							? Language.GetTextValue("GameUI.LayerSpace")
							: Language.GetTextValue("GameUI.LayerSurface"))));
			string text7 = "";
			num22 = Math.Abs(num22);
			text7 = ((num22 != 0) ? Language.GetTextValue("GameUI.Depth", num22) : Language.GetTextValue("GameUI.DepthLevel"));
			return text7 + " " + text6;
		}
	}
}
