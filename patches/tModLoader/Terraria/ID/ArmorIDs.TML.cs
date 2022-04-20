using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ID
{
	partial class ArmorIDs
	{
		partial class Head
		{
			partial class Sets
			{
				// Created based on 'fullHair' definition in 'Player.GetHairSettings'.
				public static bool[] DrawFullHair = Factory.CreateBoolSet(Goggles, Sunglasses, MimeMask, HallowedHeadgear, Tiara, EyePatch, CenxsTiara, NurseHat, SteampunkGoggles, CatMask, CatEars, GiantBow, ReindeerAntlers, SeashellHairpin, HiTekSunglasses, LazuresValkyrieCirclet, Yoraiz0rsRecoloredGoggles, _0x33sAviators, Maid, MaidAlt, GolfVisor, StarPrincessCrown, BunnyEars, DevilHorns, StarHairpin, HeartHairpin, SuperHeroMask, PrettyPinkRibbon, GhostarSkullPin, SafemanSunHair, DogEars, FoxEars, LizardEars, PandaEars, RoyalTiara);
				// Created based on 'hatHair' definition in 'Player.GetHairSettings'.
				public static bool[] DrawHatHair = Factory.CreateBoolSet(EmptyBucket, WizardHat, TopHat, SummerHat, PlumbersHat, ArchaeologistsHat, RedHat, RobotHat, GoldCrown, CobaltHat, ClownHat, SantaHat, PlatinumCrown, RuneHat, SteampunkHat, BeeHat, GreenCap, MushroomCap, TamOShanter, CowboyHat, PirateHat, VikingHelmet, RainHat, UmbrellaHat, BallaHat, GangstaHat, Beanie, WizardsHat, PumpkinMask, LeprechaunHat, ScarecrowHat, WolfMask, MrsClausHat, SnowHat, Fez, PeddlersHat, MagicHat, AnglerHat, TaxCollectorsHat, BuccaneerBandana, WeddingVeil, PartyHat, LeinforsHat, UltraBrightHelmet, GolfHat, DemonHorns, Fedora, ChefHat, UndertakerHat, FuneralHat, VictorianGothHat, GraduationCapBlue, GraduationCapMaroon, GraduationCapBlack, BadgersHat, RoninHat);
				// Created based on 'backHairDraw' definition in 'Player.GetHairSettings'.
				/// <summary>
				/// Index using Player.hair
				/// </summary>
				public static bool[] DrawBackHair = Factory.CreateBoolSet(PlatinumCrown, WoodHelmet, EbonwoodHelmet, RichMahoganyHelmet, PearlwoodHelmet, MushroomCap, TamOShanter, MummyMask, CowboyHat, PirateHat, VikingHelmet, CactusHelmet, ShadewoodHelmet, AncientIronHelmet, AncientGoldHelmet, ChlorophyteMask, ChlorophyteHelmet, ChlorophyteHeadgear, RainHat, TikiMask, PalladiumMask, PalladiumHelmet, PalladiumHeadgear, OrichalcumMask, OrichalcumHelmet, TitaniumHelmet, TitaniumHeadgear, UmbrellaHat, Skull, BallaHat, GangstaHat, SailorHat, EyePatch, SkeletronMask, TurtleHelmet, SpectreHood, SWATHelmet, ShroomiteHeadgear, ShroomiteHelmet, CenxsTiara, CrownosMask, WillsHelmet, JimsHelmet, AaronsHelmet, DTownsHelmet, NurseHat, WizardsHat, GuyFawkesMask);
				// Created based on 'drawsBackHairWithoutHeadgear' definition in 'Player.GetHairSettings'.
				public static bool[] DrawsBackHairWithoutHeadgear = Factory.CreateBoolSet(FamiliarWig, JungleRose, RabbitOrder);
				// Created based on 'PlayerDrawLayers.DrawPlayer_21_Head_TheFace'.
				public static bool[] DrawHead = Factory.CreateBoolSet(true, Werewolf, SpaceCreatureMask, FloretProtectorHelmet);
			}
		}

		partial class Body
		{
			partial class Sets
			{
				// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesTopSkin = Factory.CreateBoolSet(Werewolf, Merfolk, PumpkinShirt, RobotShirt, ReaperRobe);
				// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesBottomSkin = Factory.CreateBoolSet(ReaperRobe);
				// Created based on 'missingHand' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesHands = Factory.CreateBoolSet(true, TuxedoShirt, PlumbersShirt, HerosShirt, ArchaeologistsJacket, NinjaShirt, Robe, TheDoctorsShirt, MiningShirt, RuneRobe, EskimoCoat, SteampunkShirt, BeeShirt, PrincessCostume, PharaohsRobe, MummyShirt, CowboyJacket, PirateShirt, PinkEskimoCoat, RainCoat, TikiShirt, SailorShirt, AmethystRobe, TopazRobe, SapphireRobe, EmeraldRobe, RubyRobe, DiamondRobe, WhiteTuxedoShirt, CenxsBreastplate, CenxsDress, NurseShirt, DyeTraderRobe, CyborgShirt, GhostShirt, VampireShirt, LeprechaunShirt, PixieShirt, PrincessDress, TreasureHunterShirt, DryadCoverings, MrsClausShirt, UglySweater, ElfShirt, Gi, Kimono, GypsyRobe, BeeBreastplate, AnglerVest, MermaidAdornment, SolarCultistRobe, LunarCultistRobe, GladiatorBreastplate, LazuresValkyrieCloak, TaxCollectorsSuit, ClothiersJacket, BuccaneerTunic, ObsidianLongcoat, FallenTuxedoShirt, FossilPlate, WeddingDress, Yoraiz0rsUniform, PedguinsJacket, AncientArmor, AncientBattleArmor, Lamia, HuntressJerkin, MonkShirt, LeinforsShirt, Maid, MaidAlt, AmberRobe);
				// Created based on 'missingArm' definition in 'PlayerDrawSet.BoringSetup'.
				/// <summary>
				/// Hiding arms also hides hands
				/// </summary>
				public static bool[] HidesArms = Factory.CreateBoolSet(RobotShirt);
			}
		}

		partial class Legs
		{
			partial class Sets
			{
				// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckPants'.
				public static bool[] OverridesLegs = Factory.CreateBoolSet(CreeperPants, MermaidTail, SillySunflowerBottoms, DjinnsCurse, Lamia, MoonLordLegs, TimelessTravelerBottom, CapricornTail, RoyalDressBottom);
				// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesTopSkin = Factory.CreateBoolSet();
				// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesBottomSkin = Factory.CreateBoolSet(20, 21);
			}
		}

		partial class Shoe
		{
			partial class Sets
			{
				// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckShoes'.
				public static bool[] OverridesLegs = Factory.CreateBoolSet(FrogLeg);
			}
		}
	}
}
