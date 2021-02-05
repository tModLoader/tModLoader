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
			CreateEntry(4776).AddCondition(NetworkText.FromKey("ShopConditions.FairyTorch"), () => {
				static bool DidDiscoverBestiaryEntry(int npcId) => Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;

				return DidDiscoverBestiaryEntry(585) && DidDiscoverBestiaryEntry(584) && DidDiscoverBestiaryEntry(583);
			});

			CreateEntry(4767);
			CreateEntry(4759);

			CreateEntry(4672).AddCondition(Condition.BestiaryCompletion(0.1f));
			CreateEntry(4829).AddCondition(NetworkText.FromKey("ShopConditions.NotBoughtCat"), () => !NPC.boughtCat);
			CreateEntry(4830).AddCondition(new SimpleCondition(NetworkText.FromKey("ShopConditions.NotBoughtDog"), () => !NPC.boughtDog), Condition.BestiaryCompletion(0.25f));
			CreateEntry(4910).AddCondition(new SimpleCondition(NetworkText.FromKey("ShopConditions.NotBoughtBunny"), () => !NPC.boughtBunny), Condition.BestiaryCompletion(0.45f));

			CreateEntry(4871).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(4907).AddCondition(Condition.BestiaryCompletion(0.4f));

			CreateEntry(4677).AddCondition(Condition.DownedSolarTower);

			CreateEntry(4676).AddCondition(Condition.BestiaryCompletion(0.1f));
			CreateEntry(4762).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(4716).AddCondition(Condition.BestiaryCompletion(0.25f));
			CreateEntry(4785).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(4786).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(4787).AddCondition(Condition.BestiaryCompletion(0.3f));
			CreateEntry(4788).AddCondition(Condition.BestiaryCompletion(0.3f), Condition.Hardmode);
			CreateEntry(4955).AddCondition(Condition.BestiaryCompletion(0.4f));

			CreateEntry(4736).AddCondition(Condition.Hardmode, Condition.BloodMoon);
			CreateEntry(4701).AddCondition(Condition.DownedPlantera);

			CreateEntry(4765).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(4766).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(4777).AddCondition(Condition.BestiaryCompletion(0.5f));
			CreateEntry(4763).AddCondition(Condition.BestiaryCompletion(0.6f));
			CreateEntry(4735).AddCondition(Condition.BestiaryCompletion(0.7f));
			CreateEntry(4751).AddCondition(Condition.BestiaryCompletion(1f));

			CreateEntry(4768).AddCondition(Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(4769).AddCondition(Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(4770).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(4771).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);

			CreateEntry(4772).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(4773).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);

			CreateEntry(4560).AddCondition(Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(4775).AddCondition(Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class GolferShop : NPCShop
	{
		public override int NPCType => NPCID.Golfer;

		public override void SetDefaults() {
			CreateEntry(4587);
			CreateEntry(4590);
			CreateEntry(4589);
			CreateEntry(4588);
			CreateEntry(4083);
			CreateEntry(4084);
			CreateEntry(4085);
			CreateEntry(4086);
			CreateEntry(4087);
			CreateEntry(4088);

			CreateEntry(4039).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(4094).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(4093).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(1092).AddCondition(Condition.GolfScore(500, Operation.Greater));

			CreateEntry(4089);
			CreateEntry(3979);
			CreateEntry(4095);
			CreateEntry(4040);
			CreateEntry(4319);
			CreateEntry(4320);

			CreateEntry(4591).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(4594).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(4593).AddCondition(Condition.GolfScore(1000, Operation.Greater));
			CreateEntry(4592).AddCondition(Condition.GolfScore(1000, Operation.Greater));

			CreateEntry(4135);
			CreateEntry(4138);
			CreateEntry(4136);
			CreateEntry(4137);
			CreateEntry(4049);

			CreateEntry(4265).AddCondition(Condition.GolfScore(500, Operation.Greater));

			CreateEntry(4595).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(4598).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(4597).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(4596).AddCondition(Condition.GolfScore(2000, Operation.Greater));
			CreateEntry(4264).AddCondition(Condition.GolfScore(2000, Operation.Greater), Condition.DownedSkeletron);

			CreateEntry(4599).AddCondition(Condition.GolfScore(500, Operation.Greater));
			CreateEntry(4600).AddCondition(Condition.GolfScore(999, Operation.Greater));
			CreateEntry(4601).AddCondition(Condition.GolfScore(1999, Operation.Greater));

			CreateEntry(4658).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(4659).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(4660).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(4661).AddCondition(Condition.GolfScore(1999, Operation.Greater), Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class PrincessShop : NPCShop
	{
		public override int NPCType => NPCID.Princess;

		public override void SetDefaults() {
			CreateEntry(5071);
			CreateEntry(5072);
			CreateEntry(5073);
			CreateEntry(5076);
			CreateEntry(5077);
			CreateEntry(5078);
			CreateEntry(5079);
			CreateEntry(5080);
			CreateEntry(5081);
			CreateEntry(5082);
			CreateEntry(5083);
			CreateEntry(5084);
			CreateEntry(5085);
			CreateEntry(5086);
			CreateEntry(5087);
			CreateEntry(5044).AddCondition(Condition.Hardmode, Condition.DownedMoonLord);
		}
	}

	public class BartenderShop : NPCShop
	{
		public override int NPCType => NPCID.DD2Bartender;

		public override void SetDefaults() {
			Condition condition1 = Condition.Hardmode & Condition.DownedAnyMechBoss;
			Condition condition2 = Condition.Hardmode & Condition.DownedGolem;

			CreateEntry(353);

			CreateEntry(3828).SetPrice(Item.buyPrice(gold: 4)).AddCondition(condition1, condition2);
			CreateEntry(3828).SetPrice(Item.buyPrice(gold: 1)).AddCondition(condition1, !condition2);
			CreateEntry(3828).SetPrice(Item.buyPrice(silver: 25)).AddCondition(!condition1, !condition2);

			CreateEntry(3816);

			CreateEntry(3813).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75);
			CreateEntry(3818).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(3824).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(3832).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);
			CreateEntry(3829).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(5);

			CreateEntry(3819).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3825).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3833).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3830).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);

			CreateEntry(3820).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(3826).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(3834).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);
			CreateEntry(3831).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(100).AddCondition(condition2);

			CreateEntry(3800).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3801).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3802).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3797).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3798).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3799).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3803).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3804).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3805).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3806).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3807).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);
			CreateEntry(3808).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(25).AddCondition(condition1);

			CreateEntry(3871).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3872).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3873).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3874).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3875).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3876).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3877).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3878).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3879).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3880).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3881).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
			CreateEntry(3882).SetCurrency(CustomCurrencyID.DefenderMedals).SetPrice(75).AddCondition(condition2);
		}
	}

	public class SkeletonMerchantShop : NPCShop
	{
		public override int NPCType => NPCID.SkeletonMerchant;

		public override void SetDefaults() {
			CreateEntry(3001).AddCondition(Condition.PhaseFull | Condition.PhaseHalfAtLeft | Condition.PhaseEmpty | Condition.PhaseHalfAtRight);
			CreateEntry(3001).AddCondition(Condition.PhaseQuarterAtLeft | Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtRight);

			CreateEntry(3002).AddCondition(Condition.TimeNight | Condition.PhaseFull);
			CreateEntry(282).AddCondition(!(Condition.TimeNight | Condition.PhaseFull));

			Condition cond = new SimpleCondition(NetworkText.FromLiteral("ShopConditions.MinuteTime"), () => Main.time % 60.0 * 60.0 * 6.0 <= 10800.0);
			CreateEntry(3004).AddCondition(cond);
			CreateEntry(8).AddCondition(!cond);

			cond = Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseEmpty | Condition.PhaseQuarterAtRight;
			CreateEntry(3003).AddCondition(cond);
			CreateEntry(40).AddCondition(!cond);

			CreateEntry(3310).AddCondition(Condition.PhaseFull | Condition.PhaseEmpty);
			CreateEntry(3313).AddCondition(Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtRight);
			CreateEntry(3312).AddCondition(Condition.PhaseHalfAtLeft | Condition.PhaseHalfAtRight);
			CreateEntry(3311).AddCondition(Condition.PhaseQuarterAtLeft | Condition.PhaseThreeQuartersAtRight);

			CreateEntry(166);
			CreateEntry(965);

			CreateEntry(3316).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(3315).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight | Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(3334).AddCondition(Condition.Hardmode);
			CreateEntry(3258).AddCondition(Condition.Hardmode, Condition.BloodMoon);
			CreateEntry(3043).AddCondition(Condition.PhaseFull, Condition.TimeNight);
		}
	}

	public class StylistShop : NPCShop
	{
		public override int NPCType => NPCID.Stylist;

		public override void SetDefaults() {
			CreateEntry(1990);
			CreateEntry(1979);
			CreateEntry(1977).AddCondition(NetworkText.FromKey("ShopConditions.Health400"), () => Main.LocalPlayer.statLifeMax >= 400);
			CreateEntry(1978).AddCondition(NetworkText.FromKey("ShopConditions.Mana200"), () => Main.LocalPlayer.statManaMax >= 200);
			CreateEntry(1980).AddCondition(Condition.HasMoney(1000000, Operation.GreaterEqual));

			CreateEntry(1981).AddCondition(
				(Condition.TimeNight & (Condition.PhaseThreeQuartersAtLeft | Condition.PhaseQuarterAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtRight)) |
				(Condition.TimeDay & (Condition.PhaseFull | Condition.PhaseHalfAtLeft | Condition.PhaseEmpty | Condition.PhaseHalfAtRight)));
			CreateEntry(1982).AddCondition(NetworkText.FromKey("ShopConditions.TeamPlayer"), () => Main.LocalPlayer.team != 0);
			CreateEntry(1983).AddCondition(Condition.Hardmode);
			CreateEntry(1984).AddCondition(Condition.NPCExists(208));
			CreateEntry(1985).AddCondition(Condition.Hardmode, Condition.DownedDestroyer, Condition.DownedTwins, Condition.DownedSkeletronPrime);
			CreateEntry(1986).AddCondition(Condition.Hardmode, Condition.DownedAnyMechBoss);
			CreateEntry(2863).AddCondition(Condition.Hardmode, Condition.DownedMartians);
			CreateEntry(3259).AddCondition(Condition.Hardmode, Condition.DownedMartians);
		}
	}

	public class PirateShop : NPCShop
	{
		public override int NPCType => NPCID.Pirate;

		public override void SetDefaults() {
			CreateEntry(928);
			CreateEntry(929);
			CreateEntry(876);
			CreateEntry(877);
			CreateEntry(878);
			CreateEntry(2434);
			CreateEntry(1180).AddCondition(Condition.InOcean);
			CreateEntry(1337).AddCondition(Condition.Hardmode, Condition.DownedAnyMechBoss, Condition.NPCExists(208));
		}
	}

	public class WitchDoctorShop : NPCShop
	{
		public override int NPCType => NPCID.WitchDoctor;

		public override void SetDefaults() {
			CreateEntry(1430);
			CreateEntry(986);

			CreateEntry(2999).AddCondition(Condition.NPCExists(108));
			CreateEntry(1158).AddCondition(Condition.TimeNight);
			CreateEntry(1159).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(1160).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(1161).AddCondition(Condition.Hardmode, Condition.DownedPlantera);
			CreateEntry(1167).AddCondition(Condition.Hardmode, Condition.DownedPlantera, Condition.InJungle);
			CreateEntry(1339).AddCondition(Condition.Hardmode, Condition.DownedPlantera);

			CreateEntry(1171).AddCondition(Condition.Hardmode, Condition.InJungle);
			CreateEntry(1162).AddCondition(Condition.Hardmode, Condition.InJungle, Condition.TimeNight);

			CreateEntry(909);
			CreateEntry(910);
			CreateEntry(940);
			CreateEntry(941);
			CreateEntry(942);
			CreateEntry(943);
			CreateEntry(944);
			CreateEntry(945);
			CreateEntry(4922);
			CreateEntry(4917);
			CreateEntry(1836).AddCondition(Condition.HasItem(1835));
			CreateEntry(1261).AddCondition(Condition.HasItem(1258));
			CreateEntry(1791).AddCondition(Condition.Halloween);
		}
	}

	public class PainterShop : NPCShop
	{
		public override int NPCType => NPCID.Painter;

		public override void SetDefaults() {
			CreateEntry(1071);
			CreateEntry(1072);
			CreateEntry(1073);

			for (int i = 1073; i <= 1084; i++) CreateEntry(i);

			CreateEntry(1097);
			CreateEntry(1099);
			CreateEntry(1098);
			CreateEntry(1966);
			CreateEntry(4668).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(1967).AddCondition(Condition.Hardmode);
			CreateEntry(1968).AddCondition(Condition.Hardmode);
			CreateEntry(1490).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(1481).AddCondition(Condition.InGraveyardBiome, Condition.PhaseFull, Condition.PhaseQuarterAtLeft);
			CreateEntry(1482).AddCondition(Condition.InGraveyardBiome, Condition.PhaseHalfAtLeft, Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(1483).AddCondition(Condition.InGraveyardBiome, Condition.PhaseEmpty, Condition.PhaseQuarterAtRight);
			CreateEntry(1484).AddCondition(Condition.InGraveyardBiome, Condition.PhaseHalfAtRight, Condition.PhaseThreeQuartersAtRight);

			CreateEntry(1492).AddCondition(Condition.InCrimson);
			CreateEntry(1488).AddCondition(Condition.InCorrupt);
			CreateEntry(1489).AddCondition(Condition.InHallow);
			CreateEntry(1486).AddCondition(Condition.InJungle);
			CreateEntry(1487).AddCondition(Condition.InSnow);
			CreateEntry(1491).AddCondition(Condition.InDesert);
			CreateEntry(1493).AddCondition(Condition.BloodMoon);

			CreateEntry(1485).AddCondition(!Condition.InGraveyardBiome, Condition.InSpace, Condition.BloodMoon);
			CreateEntry(1494).AddCondition(!Condition.InGraveyardBiome, Condition.InSpace, Condition.Hardmode);

			CreateEntry(4723).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4724).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4725).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4726).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4727).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4728).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4729).AddCondition(Condition.InGraveyardBiome);

			for (int i = 1948; i <= 1957; i++) CreateEntry(i).AddCondition(Condition.Christmas);
			for (int i = 2158; i <= 2160; i++) CreateEntry(i);
			for (int i = 2008; i <= 2014; i++) CreateEntry(i);
		}
	}

	public class CyborgShop : NPCShop
	{
		public override int NPCType => NPCID.Cyborg;

		public override void SetDefaults() {
			CreateEntry(771);
			CreateEntry(772).AddCondition(Condition.BloodMoon);
			CreateEntry(773).AddCondition(Condition.TimeNight | Condition.SolarEclipse);
			CreateEntry(774).AddCondition(Condition.SolarEclipse);
			CreateEntry(4445).AddCondition(Condition.DownedMartians);
			CreateEntry(4446).AddCondition(Condition.DownedMartians, Condition.BloodMoon | Condition.SolarEclipse);
			CreateEntry(4459).AddCondition(Condition.Hardmode);
			CreateEntry(760).AddCondition(Condition.Hardmode);
			CreateEntry(1346).AddCondition(Condition.Hardmode);
			CreateEntry(4409).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4392).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(1743).AddCondition(Condition.Halloween);
			CreateEntry(1744).AddCondition(Condition.Halloween);
			CreateEntry(1745).AddCondition(Condition.Halloween);
			CreateEntry(2862).AddCondition(Condition.DownedMartians);
			CreateEntry(3109).AddCondition(Condition.DownedMartians);
			CreateEntry(3664).AddCondition(Condition.HasItem(33840) | Condition.HasItem(3664));
		}
	}

	public class PartyGirlShop : NPCShop
	{
		public override int NPCType => NPCID.PartyGirl;

		public override void SetDefaults() {
			CreateEntry(859);
			CreateEntry(4743).AddCondition(Condition.GolfScore(500, Operation.GreaterEqual));
			CreateEntry(1000);
			CreateEntry(1168);
			CreateEntry(1449).AddCondition(Condition.TimeDay);
			CreateEntry(4552).AddCondition(Condition.TimeNight);
			CreateEntry(1345);
			CreateEntry(1450);
			CreateEntry(3253);
			CreateEntry(4553);
			CreateEntry(2700);
			CreateEntry(2738);
			CreateEntry(4470);
			CreateEntry(4681);

			CreateEntry(4682).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4702).AddCondition(Condition.Lanterns);
			CreateEntry(3548).AddCondition(Condition.HasItem(3548));
			CreateEntry(3369).AddCondition(Condition.NPCExists(229));
			CreateEntry(3546).AddCondition(Condition.DownedGolem);

			CreateEntry(3214).AddCondition(Condition.Hardmode);
			CreateEntry(2868).AddCondition(Condition.Hardmode);
			CreateEntry(970).AddCondition(Condition.Hardmode);
			CreateEntry(971).AddCondition(Condition.Hardmode);
			CreateEntry(972).AddCondition(Condition.Hardmode);
			CreateEntry(973).AddCondition(Condition.Hardmode);

			CreateEntry(4791);
			CreateEntry(3747);
			CreateEntry(3732);
			CreateEntry(3742);

			CreateEntry(3749).AddCondition(Condition.PartyTime);
			CreateEntry(3746).AddCondition(Condition.PartyTime);
			CreateEntry(3739).AddCondition(Condition.PartyTime);
			CreateEntry(3740).AddCondition(Condition.PartyTime);
			CreateEntry(3741).AddCondition(Condition.PartyTime);
			CreateEntry(3737).AddCondition(Condition.PartyTime);
			CreateEntry(3738).AddCondition(Condition.PartyTime);
			CreateEntry(3736).AddCondition(Condition.PartyTime);
			CreateEntry(3745).AddCondition(Condition.PartyTime);
			CreateEntry(3744).AddCondition(Condition.PartyTime);
			CreateEntry(3743).AddCondition(Condition.PartyTime);
		}
	}

	public class DyeTraderShop : NPCShop
	{
		public override int NPCType => NPCID.DyeTrader;

		public override void SetDefaults() {
			CreateEntry(1037);
			CreateEntry(2874);
			CreateEntry(1120);
			CreateEntry(1969).AddCondition(NetworkText.FromKey("ShopConditions.Multiplayer"), () => Main.netMode == NetmodeID.MultiplayerClient);
			CreateEntry(3248).AddCondition(Condition.Halloween);
			CreateEntry(1741).AddCondition(Condition.Halloween);
			CreateEntry(2871).AddCondition(Condition.PhaseFull);
			CreateEntry(2872).AddCondition(Condition.PhaseFull);
			CreateEntry(4663).AddCondition(Condition.TimeNight, Condition.BloodMoon);
			CreateEntry(4662).AddCondition(Condition.InGraveyardBiome);
		}
	}

	public class SteampunkerShop : NPCShop
	{
		public override int NPCType => NPCID.Steampunker;

		public override void SetDefaults() {
			CreateEntry(779);

			CreateEntry(748).AddCondition(Condition.PhaseEmpty | Condition.PhaseQuarterAtRight | Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(839).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(840).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(841).AddCondition(Condition.PhaseFull | Condition.PhaseQuarterAtLeft | Condition.PhaseHalfAtLeft | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(948).AddCondition(Condition.DownedGolem);
			CreateEntry(3623);
			CreateEntry(3603);
			CreateEntry(3604);
			CreateEntry(3607);
			CreateEntry(3605);
			CreateEntry(3606);
			CreateEntry(3608);
			CreateEntry(3618);
			CreateEntry(3602);
			CreateEntry(3663);
			CreateEntry(3609);
			CreateEntry(3610);
			CreateEntry(995);

			CreateEntry(2203).AddCondition(Condition.DownedEoC, Condition.DownedEvilBoss, Condition.DownedSkeletron);

			CreateEntry(2193).AddCondition(Condition.Crimson);
			CreateEntry(4142).AddCondition(Condition.Corruption);
			CreateEntry(2192).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(2204).AddCondition(Condition.InJungle);
			CreateEntry(2198).AddCondition(Condition.InSnow);

			CreateEntry(2197).AddCondition(Condition.InSpace);
			CreateEntry(2196).AddCondition(Condition.HasItem(832));
			CreateEntry(1263);
			CreateEntry(784).AddCondition(Condition.AstrologicalEvent, Condition.Crimson);
			CreateEntry(782).AddCondition(Condition.AstrologicalEvent, Condition.Corruption);

			CreateEntry(781).AddCondition(!Condition.AstrologicalEvent, Condition.InHallow);
			CreateEntry(780).AddCondition(!Condition.AstrologicalEvent, !Condition.InHallow);

			CreateEntry(1344).AddCondition(Condition.Hardmode);
			CreateEntry(4472).AddCondition(Condition.Hardmode);
			CreateEntry(1742).AddCondition(Condition.Halloween);
		}
	}

	public class TruffleShop : NPCShop
	{
		public override int NPCType => NPCID.Truffle;

		public override void SetDefaults() {
			CreateEntry(756).AddCondition(Condition.DownedAnyMechBoss);
			CreateEntry(787).AddCondition(Condition.DownedAnyMechBoss);

			CreateEntry(868);
			CreateEntry(1551).AddCondition(Condition.DownedPlantera);
			CreateEntry(1181);
			CreateEntry(783);
		}
	}

	public class SantaClausShop : NPCShop
	{
		public override int NPCType => NPCID.SantaClaus;

		public override void SetDefaults() {
			CreateEntry(588);
			CreateEntry(589);
			CreateEntry(590);
			CreateEntry(597);
			CreateEntry(598);
			CreateEntry(596);

			for (int i = 1873; i < 1906; i++) CreateEntry(i);
		}
	}

	public class MechanicShop : NPCShop
	{
		public override int NPCType => NPCID.Mechanic;

		public override void SetDefaults() {
			CreateEntry(509);
			CreateEntry(850);
			CreateEntry(851);
			CreateEntry(3612);
			CreateEntry(510);
			CreateEntry(530);
			CreateEntry(513);
			CreateEntry(538);
			CreateEntry(529);
			CreateEntry(541);
			CreateEntry(542);
			CreateEntry(543);
			CreateEntry(852);
			CreateEntry(853);
			CreateEntry(4261);
			CreateEntry(3707);
			CreateEntry(2739);
			CreateEntry(849);
			CreateEntry(3616);
			CreateEntry(2799);
			CreateEntry(3619);
			CreateEntry(3627);
			CreateEntry(3629);
			CreateEntry(585);
			CreateEntry(584);
			CreateEntry(583);
			CreateEntry(4484);
			CreateEntry(4485);
			CreateEntry(2295).AddCondition(Condition.NPCExists(369), Condition.PhaseQuarterAtLeft | Condition.PhaseQuarterAtRight | Condition.PhaseThreeQuartersAtLeft | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class WizardShop : NPCShop
	{
		public override int NPCType => NPCID.Wizard;

		public override void SetDefaults() {
			CreateEntry(487);
			CreateEntry(496);
			CreateEntry(500);
			CreateEntry(507);
			CreateEntry(508);
			CreateEntry(531);
			CreateEntry(149);
			CreateEntry(576);
			CreateEntry(3186);
			CreateEntry(1739).AddCondition(Condition.Halloween);
		}
	}

	public class GoblinTinkererShop : NPCShop
	{
		public override int NPCType => NPCID.GoblinTinkerer;

		public override void SetDefaults() {
			CreateEntry(128);
			CreateEntry(486);
			CreateEntry(398);
			CreateEntry(84);
			CreateEntry(407);
			CreateEntry(161);
		}
	}

	public class ClothierShop : NPCShop
	{
		public override int NPCType => NPCID.Clothier;

		public override void SetDefaults() {
			CreateEntry(254);
			CreateEntry(981);
			CreateEntry(242).AddCondition(Condition.TimeDay);
			CreateEntry(245).AddCondition(Condition.PhaseFull);
			CreateEntry(246).AddCondition(Condition.PhaseFull);
			CreateEntry(1288).AddCondition(Condition.PhaseFull, Condition.TimeNight);
			CreateEntry(1829).AddCondition(Condition.PhaseFull, Condition.TimeNight);
			CreateEntry(325).AddCondition(Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(326).AddCondition(Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(269);
			CreateEntry(270);
			CreateEntry(271);
			CreateEntry(503).AddCondition(Condition.DownedClown);
			CreateEntry(504).AddCondition(Condition.DownedClown);
			CreateEntry(505).AddCondition(Condition.DownedClown);

			CreateEntry(322).AddCondition(Condition.BloodMoon);
			CreateEntry(3362).AddCondition(Condition.BloodMoon, Condition.TimeNight);
			CreateEntry(3363).AddCondition(Condition.BloodMoon, Condition.TimeNight);

			CreateEntry(2856).AddCondition(Condition.DownedLunaticCultist, Condition.TimeDay);
			CreateEntry(2858).AddCondition(Condition.DownedLunaticCultist, Condition.TimeDay);
			CreateEntry(2857).AddCondition(Condition.DownedLunaticCultist, Condition.TimeNight);
			CreateEntry(2859).AddCondition(Condition.DownedLunaticCultist, Condition.TimeNight);

			CreateEntry(3242).AddCondition(Condition.NPCExists(441));
			CreateEntry(3243).AddCondition(Condition.NPCExists(441));
			CreateEntry(3244).AddCondition(Condition.NPCExists(441));

			CreateEntry(4685).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4686).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4704).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4705).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4706).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4707).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4708).AddCondition(Condition.InGraveyardBiome);
			CreateEntry(4709).AddCondition(Condition.InGraveyardBiome);

			CreateEntry(1429).AddCondition(Condition.InSnow);
			CreateEntry(1740).AddCondition(Condition.Halloween);

			CreateEntry(869).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft);
			CreateEntry(4994).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtLeft);
			CreateEntry(4997).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtLeft);
			CreateEntry(864).AddCondition(Condition.Hardmode, Condition.PhaseEmpty);
			CreateEntry(865).AddCondition(Condition.Hardmode, Condition.PhaseEmpty);
			CreateEntry(4995).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtRight);
			CreateEntry(4998).AddCondition(Condition.Hardmode, Condition.PhaseQuarterAtRight);
			CreateEntry(873).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(874).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(875).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight);
			CreateEntry(4996).AddCondition(Condition.Hardmode, Condition.PhaseThreeQuartersAtRight);
			CreateEntry(4999).AddCondition(Condition.Hardmode, Condition.PhaseThreeQuartersAtRight);
			CreateEntry(1275).AddCondition(Condition.DownedFrostLegion);
			CreateEntry(1276).AddCondition(Condition.DownedFrostLegion);
			CreateEntry(3246).AddCondition(Condition.Halloween);
			CreateEntry(3247).AddCondition(Condition.Halloween);

			CreateEntry(3730).AddCondition(Condition.PartyTime);
			CreateEntry(3731).AddCondition(Condition.PartyTime);
			CreateEntry(3733).AddCondition(Condition.PartyTime);
			CreateEntry(3734).AddCondition(Condition.PartyTime);
			CreateEntry(3735).AddCondition(Condition.PartyTime);

			CreateEntry(4744).AddCondition(Condition.GolfScore(2000, Operation.GreaterEqual));
		}
	}

	public class DemolitionistShop : NPCShop
	{
		public override int NPCType => NPCID.Demolitionist;

		public override void SetDefaults() {
			CreateEntry(168);
			CreateEntry(166);
			CreateEntry(167);

			CreateEntry(265).AddCondition(Condition.Hardmode);
			CreateEntry(937).AddCondition(Condition.Hardmode, Condition.DownedPlantera, Condition.DownedPirates);
			CreateEntry(1347).AddCondition(Condition.Hardmode);

			CreateEntry(4827).AddCondition(Condition.HasItem(4827));
			CreateEntry(4824).AddCondition(Condition.HasItem(4824));
			CreateEntry(4825).AddCondition(Condition.HasItem(4825));
			CreateEntry(4826).AddCondition(Condition.HasItem(4826));
		}
	}

	public class DryadShop : NPCShop
	{
		public override int NPCType => NPCID.Dryad;

		public override void SetDefaults() {
			CreateEntry(2886).AddCondition(Condition.BloodMoon, Condition.Crimson);
			CreateEntry(2171).AddCondition(Condition.BloodMoon, Condition.Crimson);
			CreateEntry(4508).AddCondition(Condition.BloodMoon, Condition.Crimson);

			CreateEntry(67).AddCondition(Condition.BloodMoon, Condition.Corruption);
			CreateEntry(59).AddCondition(Condition.BloodMoon, Condition.Corruption);
			CreateEntry(4504).AddCondition(Condition.BloodMoon, Condition.Corruption);

			CreateEntry(66).AddCondition(!Condition.BloodMoon);
			CreateEntry(62).AddCondition(!Condition.BloodMoon);
			CreateEntry(63).AddCondition(!Condition.BloodMoon);
			CreateEntry(745).AddCondition(!Condition.BloodMoon);

			CreateEntry(59).AddCondition(Condition.Hardmode, Condition.InGraveyardBiome, Condition.Crimson);
			CreateEntry(2171).AddCondition(Condition.Hardmode, Condition.InGraveyardBiome, Condition.Corruption);

			CreateEntry(27);
			CreateEntry(114);
			CreateEntry(1828);
			CreateEntry(747);

			CreateEntry(746).AddCondition(Condition.Hardmode);
			CreateEntry(369).AddCondition(Condition.Hardmode);
			CreateEntry(4505).AddCondition(Condition.Hardmode);

			CreateEntry(194).AddCondition(Condition.InGlowshroom);

			CreateEntry(1853).AddCondition(Condition.Halloween);
			CreateEntry(1854).AddCondition(Condition.Halloween);
			CreateEntry(3215).AddCondition(Condition.DownedKingSlime);
			CreateEntry(3216).AddCondition(Condition.DownedQueenBee);
			CreateEntry(3219).AddCondition(Condition.DownedEoC);
			CreateEntry(3217).AddCondition(Condition.DownedEoW);
			CreateEntry(3218).AddCondition(Condition.DownedBoC);
			CreateEntry(3220).AddCondition(Condition.DownedSkeletron);
			CreateEntry(3221).AddCondition(Condition.DownedSkeletron);

			CreateEntry(3222).AddCondition(Condition.Hardmode);

			CreateEntry(4047);
			CreateEntry(4045);
			CreateEntry(4044);
			CreateEntry(4043);
			CreateEntry(4042);
			CreateEntry(4046);
			CreateEntry(4041);
			CreateEntry(4241);
			CreateEntry(4048);

			CreateEntry(4430).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(4431).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);
			CreateEntry(4432).AddCondition(Condition.Hardmode, Condition.PhaseFull | Condition.PhaseThreeQuartersAtLeft);

			CreateEntry(4433).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(4434).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);
			CreateEntry(4435).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtLeft | Condition.PhaseQuarterAtLeft);

			CreateEntry(4436).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(4437).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);
			CreateEntry(4438).AddCondition(Condition.Hardmode, Condition.PhaseEmpty | Condition.PhaseQuarterAtRight);

			CreateEntry(4439).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(4440).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
			CreateEntry(4441).AddCondition(Condition.Hardmode, Condition.PhaseHalfAtRight | Condition.PhaseThreeQuartersAtRight);
		}
	}

	public class ArmsDealerShop : NPCShop
	{
		public override int NPCType => NPCID.ArmsDealer;

		public override void SetDefaults() {
			CreateEntry(97);

			CreateEntry(4915).AddCondition(Condition.Hardmode | (Condition.BloodMoon & new SimpleCondition(NetworkText.FromKey("WorldSilverOre"), () => WorldGen.SavedOreTiers.Silver == 168)));
			CreateEntry(278).AddCondition(Condition.Hardmode | (Condition.BloodMoon & new SimpleCondition(NetworkText.FromKey("WorldTungstenOre"), () => WorldGen.SavedOreTiers.Silver != 168)));
			CreateEntry(47).AddCondition((Condition.DownedEvilBoss & Condition.TimeNight) | Condition.Hardmode);

			CreateEntry(95);
			CreateEntry(98);

			CreateEntry(4703).AddCondition(Condition.InGraveyardBiome, Condition.DownedSkeletron);
			CreateEntry(324).AddCondition(Condition.TimeNight);
			CreateEntry(534).AddCondition(Condition.Hardmode);
			CreateEntry(1432).AddCondition(Condition.Hardmode);

			CreateEntry(1261).AddCondition(Condition.HasItem(1258));
			CreateEntry(1836).AddCondition(Condition.HasItem(1835));
			CreateEntry(3108).AddCondition(Condition.HasItem(3107));
			CreateEntry(1783).AddCondition(Condition.HasItem(1782));
			CreateEntry(1785).AddCondition(Condition.HasItem(1784));

			CreateEntry(1736).AddCondition(Condition.Halloween);
			CreateEntry(1737).AddCondition(Condition.Halloween);
			CreateEntry(1738).AddCondition(Condition.Halloween);
		}
	}

	public class MerchantShop : NPCShop
	{
		public override int NPCType => NPCID.Merchant;

		public override void SetDefaults() {
			CreateEntry(88);
			CreateEntry(87);
			CreateEntry(35);
			CreateEntry(1991);
			CreateEntry(3509);
			CreateEntry(3506);
			CreateEntry(8);
			CreateEntry(28);
			CreateEntry(110);
			CreateEntry(40);
			CreateEntry(42);
			CreateEntry(965);

			CreateEntry(967).AddCondition(Condition.InSnow);
			CreateEntry(33).AddCondition(Condition.InJungle);
			CreateEntry(4074).AddCondition(Condition.TimeDay, new SimpleCondition(NetworkText.FromLiteral("HappyWind"), () => Main.IsItAHappyWindyDay));
			CreateEntry(279).AddCondition(Condition.BloodMoon);
			CreateEntry(282).AddCondition(Condition.TimeNight);
			CreateEntry(346).AddCondition(Condition.DownedSkeletron);
			CreateEntry(488).AddCondition(Condition.Hardmode);

			CreateEntry(931).AddCondition(Condition.HasItem(930));
			CreateEntry(1614).AddCondition(Condition.HasItem(930));

			CreateEntry(1786);
			CreateEntry(1348).AddCondition(Condition.Hardmode);
			CreateEntry(3198).AddCondition(Condition.Hardmode);

			CreateEntry(3108).AddCondition(Condition.HasItem(3107));
			CreateEntry(4063).AddCondition(Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.Hardmode);
			CreateEntry(4673).AddCondition(Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.Hardmode);

			for (int i = 0; i < 50; i++)
			{
				CreateEntry(ItemID.DirtBlock)
					.SetPrice((i + 1) * 100);
			}
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

			list.CreateEntry(3309).AddCondition(new LuckCondition(array[4]));
			list.CreateEntry(3314).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(1987).AddCondition(new LuckCondition(array[5]));
			list.CreateEntry(2270).AddCondition(new LuckCondition(array[4]), Condition.Hardmode);
			list.CreateEntry(2278).AddCondition(new LuckCondition(array[4]));
			list.CreateEntry(2271).AddCondition(new LuckCondition(array[4]));

			list.CreateEntry(4347).AddCondition(new LuckCondition(array[4]), Condition.DownedEoC | Condition.DownedEvilBoss | Condition.DownedSkeletron | Condition.DownedQueenBee);
			list.CreateEntry(4348).AddCondition(new LuckCondition(array[4]), Condition.Hardmode);

			list.CreateEntry(2223).AddCondition(new LuckCondition(array[3]), Condition.Hardmode, Condition.DownedPlantera);
			list.CreateEntry(2272).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2219).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2276).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2284).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2285).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2286).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2287).AddCondition(new LuckCondition(array[3]));

			list.CreateEntry(4744).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(2296).AddCondition(new LuckCondition(array[3]), Condition.DownedSkeletron);
			list.CreateEntry(3628).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(4091).AddCondition(new LuckCondition(array[3]), Condition.Hardmode);
			list.CreateEntry(4603).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(4604).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(4605).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(4550).AddCondition(new LuckCondition(array[3]));
			list.CreateEntry(4550).AddCondition(new LuckCondition(array[3]));

			list.CreateEntry(2269).AddCondition(new LuckCondition(array[2]), Condition.OrbSmashed);
			list.CreateEntry(2177).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(1988).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(2275).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(2279).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(2277).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4555).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(4555),
				new EntryItem(4556),
				new EntryItem(4557)
			}).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(4321),
				new EntryItem(4322)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(4323).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(4323),
				new EntryItem(4324),
				new EntryItem(4365)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(4549).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4561).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4774).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4562).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4558).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4559).AddCondition(new LuckCondition(array[2]));
			list.CreateEntry(4563).AddCondition(new LuckCondition(array[2]));

			list.AddEntry(new EntryList
			{
				new EntryItem(4666),
				new EntryItem(4665),
				new EntryItem(4664)
			}).AddCondition(new LuckCondition(array[2]));

			list.CreateEntry(3262).AddCondition(new LuckCondition(array[2]), Condition.DownedEoC);
			list.CreateEntry(3284).AddCondition(new LuckCondition(array[2]), Condition.DownedAnyMechBoss);
			list.CreateEntry(3596).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMoonLord);
			list.CreateEntry(2865).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(2866).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(2867).AddCondition(new LuckCondition(array[2]), Condition.Hardmode, Condition.DownedMartians);
			list.CreateEntry(3055).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(3056).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(3057).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(3058).AddCondition(new LuckCondition(array[2]), Condition.Christmas);
			list.CreateEntry(3059).AddCondition(new LuckCondition(array[2]), Condition.Christmas);

			list.CreateEntry(2214).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(2215).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(2216).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(2217).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(3624).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(2273).AddCondition(new LuckCondition(array[1]));
			list.CreateEntry(2274).AddCondition(new LuckCondition(array[1]));

			list.CreateEntry(2266).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(2267).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(2268).AddCondition(new LuckCondition(array[0]));

			list.AddEntry(new EntryListRandom
			{
				new EntryItem(2281),
				new EntryItem(2282),
				new EntryItem(2283)
			}).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(2274).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(2258).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(2242).AddCondition(new LuckCondition(array[0]));

			list.AddEntry(new EntryList
			{
				new EntryItem(2260),
				new EntryItem(2261),
				new EntryItem(2262)
			}).AddCondition(new LuckCondition(array[0]));

			EntryListRandom teamBlocks = new EntryListRandom
			{
				new EntryList
				{
					new EntryItem(3637),
					new EntryItem(3642)
				},
				new EntryList
				{
					new EntryItem(3621),
					new EntryItem(3622)
				},
				new EntryList
				{
					new EntryItem(3634),
					new EntryItem(3639)
				},
				new EntryList
				{
					new EntryItem(3633),
					new EntryItem(3638)
				},
				new EntryList
				{
					new EntryItem(3635),
					new EntryItem(3640)
				},
				new EntryList
				{
					new EntryItem(3636),
					new EntryItem(3641)
				}
			};
			list.AddEntry(teamBlocks).AddCondition(new LuckCondition(array[0]));

			list.CreateEntry(4420).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(3119).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(3118).AddCondition(new LuckCondition(array[0]));
			list.CreateEntry(3099).AddCondition(new LuckCondition(array[0]));
		}
	}
}