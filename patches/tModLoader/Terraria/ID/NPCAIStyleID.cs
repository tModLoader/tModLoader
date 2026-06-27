using ReLogic.Reflection;

namespace Terraria.ID;

public class NPCAIStyleID
{
	public static readonly IdDictionary Search = IdDictionary.Create<NPCAIStyleID, short>();
	/// <summary>
	/// Used by: <see cref="NPCID.BoundGoblin"/>, <see cref="NPCID.BoundWizard"/>, <see cref="NPCID.BoundMechanic"/>, <see cref="NPCID.WebbedStylist"/>, <see cref="NPCID.SleepingAngler"/>, <see cref="NPCID.StardustWormBody"/>, <see cref="NPCID.StardustWormTail"/>, <see cref="NPCID.StardustJellyfishSmall"/>, <see cref="NPCID.BartenderUnconscious"/>, <see cref="NPCID.GolferRescue"/>, <see cref="NPCID.TorchGod"/>
	/// </summary>
	public const short FaceClosestPlayer = 0;
	/// <summary>
	/// Used by: <see cref="NPCID.BigCrimslime"/>, <see cref="NPCID.LittleCrimslime"/>, <see cref="NPCID.JungleSlime"/>, <see cref="NPCID.YellowSlime"/>, <see cref="NPCID.RedSlime"/>, <see cref="NPCID.PurpleSlime"/>, <see cref="NPCID.BlackSlime"/>, <see cref="NPCID.BabySlime"/>, <see cref="NPCID.Pinky"/>, <see cref="NPCID.GreenSlime"/>, <see cref="NPCID.Slimer2"/>, <see cref="NPCID.Slimeling"/>, <see cref="NPCID.BlueSlime"/>, <see cref="NPCID.MotherSlime"/>, <see cref="NPCID.LavaSlime"/>, <see cref="NPCID.DungeonSlime"/>, <see cref="NPCID.CorruptSlime"/>, <see cref="NPCID.IlluminantSlime"/>, <see cref="NPCID.ToxicSludge"/>, <see cref="NPCID.IceSlime"/>, <see cref="NPCID.Crimslime"/>, <see cref="NPCID.SpikedIceSlime"/>, <see cref="NPCID.SpikedJungleSlime"/>, <see cref="NPCID.UmbrellaSlime"/>, <see cref="NPCID.RainbowSlime"/>, <see cref="NPCID.SlimeMasked"/>, <see cref="NPCID.HoppinJack"/>, <see cref="NPCID.SlimeRibbonWhite"/>, <see cref="NPCID.SlimeRibbonYellow"/>, <see cref="NPCID.SlimeRibbonGreen"/>, <see cref="NPCID.SlimeRibbonRed"/>, <see cref="NPCID.Grasshopper"/>, <see cref="NPCID.GoldGrasshopper"/>, <see cref="NPCID.SlimeSpiked"/>, <see cref="NPCID.SandSlime"/>, <see cref="NPCID.QueenSlimeMinionBlue"/>, <see cref="NPCID.QueenSlimeMinionPink"/>, <see cref="NPCID.GoldenSlime"/>, <see cref="NPCID.ShimmerSlime"/>, <see cref="NPCID.BoundTownSlimeOld"/>
	/// </summary>
	public const short Slime = 1;
	/// <summary>
	/// Used by: <see cref="NPCID.DemonEye2"/>, <see cref="NPCID.PurpleEye2"/>, <see cref="NPCID.GreenEye2"/>, <see cref="NPCID.DialatedEye2"/>, <see cref="NPCID.SleepyEye2"/>, <see cref="NPCID.CataractEye2"/>, <see cref="NPCID.DemonEye"/>, <see cref="NPCID.TheHungryII"/>, <see cref="NPCID.WanderingEye"/>, <see cref="NPCID.PigronCorruption"/>, <see cref="NPCID.PigronHallow"/>, <see cref="NPCID.PigronCrimson"/>, <see cref="NPCID.CataractEye"/>, <see cref="NPCID.SleepyEye"/>, <see cref="NPCID.DialatedEye"/>, <see cref="NPCID.GreenEye"/>, <see cref="NPCID.PurpleEye"/>, <see cref="NPCID.DemonEyeOwl"/>, <see cref="NPCID.DemonEyeSpaceship"/>
	/// </summary>
	public const short DemonEye = 2;
	/// <summary>
	/// Used by: <see cref="NPCID.BigRainZombie"/>, <see cref="NPCID.SmallRainZombie"/>, <see cref="NPCID.BigPantlessSkeleton"/>, <see cref="NPCID.SmallPantlessSkeleton"/>, <see cref="NPCID.BigMisassembledSkeleton"/>, <see cref="NPCID.SmallMisassembledSkeleton"/>, <see cref="NPCID.BigHeadacheSkeleton"/>, <see cref="NPCID.SmallHeadacheSkeleton"/>, <see cref="NPCID.BigSkeleton"/>, <see cref="NPCID.SmallSkeleton"/>, <see cref="NPCID.BigFemaleZombie"/>, <see cref="NPCID.SmallFemaleZombie"/>, <see cref="NPCID.BigTwiggyZombie"/>, <see cref="NPCID.SmallTwiggyZombie"/>, <see cref="NPCID.BigSwampZombie"/>, <see cref="NPCID.SmallSwampZombie"/>, <see cref="NPCID.BigSlimedZombie"/>, <see cref="NPCID.SmallSlimedZombie"/>, <see cref="NPCID.BigPincushionZombie"/>, <see cref="NPCID.SmallPincushionZombie"/>, <see cref="NPCID.BigBaldZombie"/>, <see cref="NPCID.SmallBaldZombie"/>, <see cref="NPCID.BigZombie"/>, <see cref="NPCID.SmallZombie"/>, <see cref="NPCID.HeavySkeleton"/>, <see cref="NPCID.BigBoned"/>, <see cref="NPCID.ShortBones"/>, <see cref="NPCID.Zombie"/>, <see cref="NPCID.Skeleton"/>, <see cref="NPCID.GoblinPeon"/>, <see cref="NPCID.GoblinThief"/>, <see cref="NPCID.GoblinWarrior"/>, <see cref="NPCID.AngryBones"/>, <see cref="NPCID.UndeadMiner"/>, <see cref="NPCID.CorruptBunny"/>, <see cref="NPCID.DoctorBones"/>, <see cref="NPCID.TheGroom"/>, <see cref="NPCID.Crab"/>, <see cref="NPCID.GoblinScout"/>, <see cref="NPCID.ArmoredSkeleton"/>, <see cref="NPCID.Mummy"/>, <see cref="NPCID.DarkMummy"/>, <see cref="NPCID.LightMummy"/>, <see cref="NPCID.Werewolf"/>, <see cref="NPCID.Clown"/>, <see cref="NPCID.SkeletonArcher"/>, <see cref="NPCID.GoblinArcher"/>, <see cref="NPCID.ChaosElemental"/>, <see cref="NPCID.BaldZombie"/>, <see cref="NPCID.PossessedArmor"/>, <see cref="NPCID.Vampire"/>, <see cref="NPCID.ZombieEskimo"/>, <see cref="NPCID.Frankenstein"/>, <see cref="NPCID.BlackRecluse"/>, <see cref="NPCID.WallCreeper"/>, <see cref="NPCID.SwampThing"/>, <see cref="NPCID.UndeadViking"/>, <see cref="NPCID.CorruptPenguin"/>, <see cref="NPCID.FaceMonster"/>, <see cref="NPCID.SnowFlinx"/>, <see cref="NPCID.PincushionZombie"/>, <see cref="NPCID.SlimedZombie"/>, <see cref="NPCID.SwampZombie"/>, <see cref="NPCID.TwiggyZombie"/>, <see cref="NPCID.Nymph"/>, <see cref="NPCID.ArmoredViking"/>, <see cref="NPCID.Lihzahrd"/>, <see cref="NPCID.LihzahrdCrawler"/>, <see cref="NPCID.FemaleZombie"/>, <see cref="NPCID.HeadacheSkeleton"/>, <see cref="NPCID.MisassembledSkeleton"/>, <see cref="NPCID.PantlessSkeleton"/>, <see cref="NPCID.IcyMerman"/>, <see cref="NPCID.PirateDeckhand"/>, <see cref="NPCID.PirateCorsair"/>, <see cref="NPCID.PirateDeadeye"/>, <see cref="NPCID.PirateCrossbower"/>, <see cref="NPCID.PirateCaptain"/>, <see cref="NPCID.CochinealBeetle"/>, <see cref="NPCID.CyanBeetle"/>, <see cref="NPCID.LacBeetle"/>, <see cref="NPCID.SeaSnail"/>, <see cref="NPCID.ZombieRaincoat"/>, <see cref="NPCID.JungleCreeper"/>, <see cref="NPCID.BloodCrawler"/>, <see cref="NPCID.IceGolem"/>, <see cref="NPCID.Eyezor"/>, <see cref="NPCID.ZombieMushroom"/>, <see cref="NPCID.ZombieMushroomHat"/>, <see cref="NPCID.AnomuraFungus"/>, <see cref="NPCID.MushiLadybug"/>, <see cref="NPCID.RustyArmoredBonesAxe"/>, <see cref="NPCID.RustyArmoredBonesFlail"/>, <see cref="NPCID.RustyArmoredBonesSword"/>, <see cref="NPCID.RustyArmoredBonesSwordNoArmor"/>, <see cref="NPCID.BlueArmoredBones"/>, <see cref="NPCID.BlueArmoredBonesMace"/>, <see cref="NPCID.BlueArmoredBonesNoPants"/>, <see cref="NPCID.BlueArmoredBonesSword"/>, <see cref="NPCID.HellArmoredBones"/>, <see cref="NPCID.HellArmoredBonesSpikeShield"/>, <see cref="NPCID.HellArmoredBonesMace"/>, <see cref="NPCID.HellArmoredBonesSword"/>, <see cref="NPCID.BoneLee"/>, <see cref="NPCID.Paladin"/>, <see cref="NPCID.SkeletonSniper"/>, <see cref="NPCID.TacticalSkeleton"/>, <see cref="NPCID.SkeletonCommando"/>, <see cref="NPCID.AngryBonesBig"/>, <see cref="NPCID.AngryBonesBigMuscle"/>, <see cref="NPCID.AngryBonesBigHelmet"/>, <see cref="NPCID.Scarecrow1"/>, <see cref="NPCID.Scarecrow2"/>, <see cref="NPCID.Scarecrow3"/>, <see cref="NPCID.Scarecrow4"/>, <see cref="NPCID.Scarecrow5"/>, <see cref="NPCID.Scarecrow6"/>, <see cref="NPCID.Scarecrow7"/>, <see cref="NPCID.Scarecrow8"/>, <see cref="NPCID.Scarecrow9"/>, <see cref="NPCID.Scarecrow10"/>, <see cref="NPCID.ZombieDoctor"/>, <see cref="NPCID.ZombieSuperman"/>, <see cref="NPCID.ZombiePixie"/>, <see cref="NPCID.SkeletonTopHat"/>, <see cref="NPCID.SkeletonAstonaut"/>, <see cref="NPCID.SkeletonAlien"/>, <see cref="NPCID.Splinterling"/>, <see cref="NPCID.ZombieXmas"/>, <see cref="NPCID.ZombieSweater"/>, <see cref="NPCID.ZombieElf"/>, <see cref="NPCID.ZombieElfBeard"/>, <see cref="NPCID.ZombieElfGirl"/>, <see cref="NPCID.GingerbreadMan"/>, <see cref="NPCID.Yeti"/>, <see cref="NPCID.Nutcracker"/>, <see cref="NPCID.NutcrackerSpinning"/>, <see cref="NPCID.ElfArcher"/>, <see cref="NPCID.Krampus"/>, <see cref="NPCID.CultistArcherBlue"/>, <see cref="NPCID.CultistArcherWhite"/>, <see cref="NPCID.BrainScrambler"/>, <see cref="NPCID.RayGunner"/>, <see cref="NPCID.MartianOfficer"/>, <see cref="NPCID.GrayGrunt"/>, <see cref="NPCID.MartianEngineer"/>, <see cref="NPCID.GigaZapper"/>, <see cref="NPCID.Scutlix"/>, <see cref="NPCID.StardustSpiderBig"/>, <see cref="NPCID.StardustSoldier"/>, <see cref="NPCID.SolarDrakomire"/>, <see cref="NPCID.SolarSolenian"/>, <see cref="NPCID.NebulaSoldier"/>, <see cref="NPCID.VortexRifleman"/>, <see cref="NPCID.VortexHornetQueen"/>, <see cref="NPCID.VortexHornet"/>, <see cref="NPCID.VortexLarva"/>, <see cref="NPCID.VortexSoldier"/>, <see cref="NPCID.ArmedZombie"/>, <see cref="NPCID.ArmedZombieEskimo"/>, <see cref="NPCID.ArmedZombiePincussion"/>, <see cref="NPCID.ArmedZombieSlimed"/>, <see cref="NPCID.ArmedZombieSwamp"/>, <see cref="NPCID.ArmedZombieTwiggy"/>, <see cref="NPCID.ArmedZombieCenx"/>, <see cref="NPCID.BoneThrowingSkeleton"/>, <see cref="NPCID.BoneThrowingSkeleton2"/>, <see cref="NPCID.BoneThrowingSkeleton3"/>, <see cref="NPCID.BoneThrowingSkeleton4"/>, <see cref="NPCID.Butcher"/>, <see cref="NPCID.CreatureFromTheDeep"/>, <see cref="NPCID.Fritz"/>, <see cref="NPCID.Nailhead"/>, <see cref="NPCID.CrimsonBunny"/>, <see cref="NPCID.Psycho"/>, <see cref="NPCID.DrManFly"/>, <see cref="NPCID.ThePossessed"/>, <see cref="NPCID.CrimsonPenguin"/>, <see cref="NPCID.GoblinSummoner"/>, <see cref="NPCID.Medusa"/>, <see cref="NPCID.GreekSkeleton"/>, <see cref="NPCID.GraniteGolem"/>, <see cref="NPCID.BloodZombie"/>, <see cref="NPCID.Crawdad"/>, <see cref="NPCID.Crawdad2"/>, <see cref="NPCID.Salamander"/>, <see cref="NPCID.Salamander2"/>, <see cref="NPCID.Salamander3"/>, <see cref="NPCID.Salamander4"/>, <see cref="NPCID.Salamander5"/>, <see cref="NPCID.Salamander6"/>, <see cref="NPCID.Salamander7"/>, <see cref="NPCID.Salamander8"/>, <see cref="NPCID.Salamander9"/>, <see cref="NPCID.GiantWalkingAntlion"/>, <see cref="NPCID.SolarSpearman"/>, <see cref="NPCID.MartianWalker"/>, <see cref="NPCID.DesertGhoul"/>, <see cref="NPCID.DesertGhoulCorruption"/>, <see cref="NPCID.DesertGhoulCrimson"/>, <see cref="NPCID.DesertGhoulHallow"/>, <see cref="NPCID.DesertLamiaLight"/>, <see cref="NPCID.DesertLamiaDark"/>, <see cref="NPCID.DesertScorpionWalk"/>, <see cref="NPCID.DesertBeast"/>, <see cref="NPCID.DemonTaxCollector"/>, <see cref="NPCID.TheBride"/>, <see cref="NPCID.WalkingAntlion"/>, <see cref="NPCID.LarvaeAntlion"/>, <see cref="NPCID.ZombieMerman"/>, <see cref="NPCID.TorchZombie"/>, <see cref="NPCID.ArmedTorchZombie"/>, <see cref="NPCID.Gnome"/>, <see cref="NPCID.BloodMummy"/>, <see cref="NPCID.RockGolem"/>, <see cref="NPCID.MaggotZombie"/>, <see cref="NPCID.SporeSkeleton"/>
	/// </summary>
	public const short Fighter = 3;
	/// <summary>
	/// Used by: <see cref="NPCID.EyeofCthulhu"/>
	/// </summary>
	public const short EyeOfCthulhu = 4;
	/// <summary>
	/// Behavior: Includes things such as Eaters of Souls<br/><br/>
	/// Used by: <see cref="NPCID.BigHornetStingy"/>, <see cref="NPCID.LittleHornetStingy"/>, <see cref="NPCID.BigHornetSpikey"/>, <see cref="NPCID.LittleHornetSpikey"/>, <see cref="NPCID.BigHornetLeafy"/>, <see cref="NPCID.LittleHornetLeafy"/>, <see cref="NPCID.BigHornetHoney"/>, <see cref="NPCID.LittleHornetHoney"/>, <see cref="NPCID.BigHornetFatty"/>, <see cref="NPCID.LittleHornetFatty"/>, <see cref="NPCID.BigCrimera"/>, <see cref="NPCID.LittleCrimera"/>, <see cref="NPCID.GiantMossHornet"/>, <see cref="NPCID.BigMossHornet"/>, <see cref="NPCID.LittleMossHornet"/>, <see cref="NPCID.TinyMossHornet"/>, <see cref="NPCID.BigStinger"/>, <see cref="NPCID.LittleStinger"/>, <see cref="NPCID.BigEater"/>, <see cref="NPCID.LittleEater"/>, <see cref="NPCID.ServantofCthulhu"/>, <see cref="NPCID.EaterofSouls"/>, <see cref="NPCID.MeteorHead"/>, <see cref="NPCID.Hornet"/>, <see cref="NPCID.Corruptor"/>, <see cref="NPCID.Probe"/>, <see cref="NPCID.Crimera"/>, <see cref="NPCID.MossHornet"/>, <see cref="NPCID.Moth"/>, <see cref="NPCID.Bee"/>, <see cref="NPCID.BeeSmall"/>, <see cref="NPCID.HornetFatty"/>, <see cref="NPCID.HornetHoney"/>, <see cref="NPCID.HornetLeafy"/>, <see cref="NPCID.HornetSpikey"/>, <see cref="NPCID.HornetStingy"/>, <see cref="NPCID.Parrot"/>, <see cref="NPCID.BloodSquid"/>
	/// </summary>
	public const short Flying = 5;
	/// <summary>
	/// Used by: <see cref="NPCID.DevourerHead"/>, <see cref="NPCID.DevourerBody"/>, <see cref="NPCID.DevourerTail"/>, <see cref="NPCID.GiantWormHead"/>, <see cref="NPCID.GiantWormBody"/>, <see cref="NPCID.GiantWormTail"/>, <see cref="NPCID.EaterofWorldsHead"/>, <see cref="NPCID.EaterofWorldsBody"/>, <see cref="NPCID.EaterofWorldsTail"/>, <see cref="NPCID.BoneSerpentHead"/>, <see cref="NPCID.BoneSerpentBody"/>, <see cref="NPCID.BoneSerpentTail"/>, <see cref="NPCID.WyvernHead"/>, <see cref="NPCID.WyvernLegs"/>, <see cref="NPCID.WyvernBody"/>, <see cref="NPCID.WyvernBody2"/>, <see cref="NPCID.WyvernBody3"/>, <see cref="NPCID.WyvernTail"/>, <see cref="NPCID.DiggerHead"/>, <see cref="NPCID.DiggerBody"/>, <see cref="NPCID.DiggerTail"/>, <see cref="NPCID.SeekerHead"/>, <see cref="NPCID.SeekerBody"/>, <see cref="NPCID.SeekerTail"/>, <see cref="NPCID.LeechHead"/>, <see cref="NPCID.LeechBody"/>, <see cref="NPCID.LeechTail"/>, <see cref="NPCID.TruffleWormDigger"/>, <see cref="NPCID.StardustWormHead"/>, <see cref="NPCID.SolarCrawltipedeHead"/>, <see cref="NPCID.SolarCrawltipedeBody"/>, <see cref="NPCID.SolarCrawltipedeTail"/>, <see cref="NPCID.CultistDragonHead"/>, <see cref="NPCID.CultistDragonBody1"/>, <see cref="NPCID.CultistDragonBody2"/>, <see cref="NPCID.CultistDragonBody3"/>, <see cref="NPCID.CultistDragonBody4"/>, <see cref="NPCID.CultistDragonTail"/>, <see cref="NPCID.DuneSplicerHead"/>, <see cref="NPCID.DuneSplicerBody"/>, <see cref="NPCID.DuneSplicerTail"/>, <see cref="NPCID.TombCrawlerHead"/>, <see cref="NPCID.TombCrawlerBody"/>, <see cref="NPCID.TombCrawlerTail"/>, <see cref="NPCID.BloodEelHead"/>, <see cref="NPCID.BloodEelBody"/>, <see cref="NPCID.BloodEelTail"/>
	/// </summary>
	public const short Worm = 6;
	/// <summary>
	/// Behavior: Includes Town NPCs and some ambient creatures, only Town NPCs will have defense with this AI, due to type-based hardcode<br/><br/>
	/// Used by: <see cref="NPCID.Merchant"/>, <see cref="NPCID.Nurse"/>, <see cref="NPCID.ArmsDealer"/>, <see cref="NPCID.Dryad"/>, <see cref="NPCID.Guide"/>, <see cref="NPCID.OldMan"/>, <see cref="NPCID.Demolitionist"/>, <see cref="NPCID.Bunny"/>, <see cref="NPCID.Clothier"/>, <see cref="NPCID.GoblinTinkerer"/>, <see cref="NPCID.Wizard"/>, <see cref="NPCID.Mechanic"/>, <see cref="NPCID.SantaClaus"/>, <see cref="NPCID.Penguin"/>, <see cref="NPCID.PenguinBlack"/>, <see cref="NPCID.Truffle"/>, <see cref="NPCID.Steampunker"/>, <see cref="NPCID.DyeTrader"/>, <see cref="NPCID.PartyGirl"/>, <see cref="NPCID.Cyborg"/>, <see cref="NPCID.Painter"/>, <see cref="NPCID.WitchDoctor"/>, <see cref="NPCID.Pirate"/>, <see cref="NPCID.GoldfishWalker"/>, <see cref="NPCID.Squirrel"/>, <see cref="NPCID.Mouse"/>, <see cref="NPCID.BunnySlimed"/>, <see cref="NPCID.BunnyXmas"/>, <see cref="NPCID.Stylist"/>, <see cref="NPCID.Frog"/>, <see cref="NPCID.Duck"/>, <see cref="NPCID.DuckWhite"/>, <see cref="NPCID.ScorpionBlack"/>, <see cref="NPCID.Scorpion"/>, <see cref="NPCID.TravellingMerchant"/>, <see cref="NPCID.Angler"/>, <see cref="NPCID.TaxCollector"/>, <see cref="NPCID.GoldBunny"/>, <see cref="NPCID.GoldFrog"/>, <see cref="NPCID.GoldMouse"/>, <see cref="NPCID.SkeletonMerchant"/>, <see cref="NPCID.SquirrelRed"/>, <see cref="NPCID.SquirrelGold"/>, <see cref="NPCID.PartyBunny"/>, <see cref="NPCID.DD2Bartender"/>, <see cref="NPCID.Golfer"/>, <see cref="NPCID.GoldGoldfishWalker"/>, <see cref="NPCID.Seagull"/>, <see cref="NPCID.Grebe"/>, <see cref="NPCID.Rat"/>, <see cref="NPCID.ExplosiveBunny"/>, <see cref="NPCID.Turtle"/>, <see cref="NPCID.TurtleJungle"/>, <see cref="NPCID.SeaTurtle"/>, <see cref="NPCID.BestiaryGirl"/>, <see cref="NPCID.TownCat"/>, <see cref="NPCID.TownDog"/>, <see cref="NPCID.GemSquirrelAmethyst"/>, <see cref="NPCID.GemSquirrelTopaz"/>, <see cref="NPCID.GemSquirrelSapphire"/>, <see cref="NPCID.GemSquirrelEmerald"/>, <see cref="NPCID.GemSquirrelRuby"/>, <see cref="NPCID.GemSquirrelDiamond"/>, <see cref="NPCID.GemSquirrelAmber"/>, <see cref="NPCID.GemBunnyAmethyst"/>, <see cref="NPCID.GemBunnyTopaz"/>, <see cref="NPCID.GemBunnySapphire"/>, <see cref="NPCID.GemBunnyEmerald"/>, <see cref="NPCID.GemBunnyRuby"/>, <see cref="NPCID.GemBunnyDiamond"/>, <see cref="NPCID.GemBunnyAmber"/>, <see cref="NPCID.TownBunny"/>, <see cref="NPCID.Princess"/>, <see cref="NPCID.TownSlimeBlue"/>, <see cref="NPCID.TownSlimeGreen"/>, <see cref="NPCID.TownSlimeOld"/>, <see cref="NPCID.TownSlimePurple"/>, <see cref="NPCID.TownSlimeRainbow"/>, <see cref="NPCID.TownSlimeRed"/>, <see cref="NPCID.TownSlimeYellow"/>, <see cref="NPCID.TownSlimeCopper"/>, <see cref="NPCID.BoundTownSlimeYellow"/>
	/// </summary>
	public const short Passive = 7;
	/// <summary>
	/// Used by: <see cref="NPCID.FireImp"/>, <see cref="NPCID.GoblinSorcerer"/>, <see cref="NPCID.DarkCaster"/>, <see cref="NPCID.Tim"/>, <see cref="NPCID.RuneWizard"/>, <see cref="NPCID.RaggedCaster"/>, <see cref="NPCID.RaggedCasterOpenCoat"/>, <see cref="NPCID.Necromancer"/>, <see cref="NPCID.NecromancerArmored"/>, <see cref="NPCID.DiabolistRed"/>, <see cref="NPCID.DiabolistWhite"/>, <see cref="NPCID.DesertDjinn"/>
	/// </summary>
	public const short Caster = 8;
	/// <summary>
	/// Used by: <see cref="NPCID.BurningSphere"/>, <see cref="NPCID.ChaosBall"/>, <see cref="NPCID.WaterSphere"/>, <see cref="NPCID.VileSpit"/>, <see cref="NPCID.SolarFlare"/>, <see cref="NPCID.ChaosBallTim"/>, <see cref="NPCID.VileSpitEaterOfWorlds"/>
	/// </summary>
	public const short Spell = 9;
	/// <summary>
	/// Used by: <see cref="NPCID.CursedSkull"/>, <see cref="NPCID.GiantCursedSkull"/>
	/// </summary>
	public const short CursedSkull = 10;
	/// <summary>
	/// Used by: <see cref="NPCID.SkeletronHead"/>, <see cref="NPCID.DungeonGuardian"/>
	/// </summary>
	public const short SkeletronHead = 11;
	/// <summary>
	/// Used by: <see cref="NPCID.SkeletronHand"/>
	/// </summary>
	public const short SkeletronHand = 12;
	/// <summary>
	/// Used by: <see cref="NPCID.ManEater"/>, <see cref="NPCID.Snatcher"/>, <see cref="NPCID.Clinger"/>, <see cref="NPCID.AngryTrapper"/>, <see cref="NPCID.FungiBulb"/>, <see cref="NPCID.GiantFungiBulb"/>
	/// </summary>
	public const short ManEater = 13;
	/// <summary>
	/// Used by: <see cref="NPCID.Harpy"/>, <see cref="NPCID.CaveBat"/>, <see cref="NPCID.JungleBat"/>, <see cref="NPCID.Hellbat"/>, <see cref="NPCID.Demon"/>, <see cref="NPCID.VoodooDemon"/>, <see cref="NPCID.GiantBat"/>, <see cref="NPCID.Slimer"/>, <see cref="NPCID.IlluminantBat"/>, <see cref="NPCID.IceBat"/>, <see cref="NPCID.Lavabat"/>, <see cref="NPCID.GiantFlyingFox"/>, <see cref="NPCID.RedDevil"/>, <see cref="NPCID.VampireBat"/>, <see cref="NPCID.FlyingSnake"/>, <see cref="NPCID.SporeBat"/>, <see cref="NPCID.QueenSlimeMinionPurple"/>
	/// </summary>
	public const short Bat = 14;
	/// <summary>
	/// Used by: <see cref="NPCID.KingSlime"/>
	/// </summary>
	public const short KingSlime = 15;
	/// <summary>
	/// Used by: <see cref="NPCID.Goldfish"/>, <see cref="NPCID.CorruptGoldfish"/>, <see cref="NPCID.Piranha"/>, <see cref="NPCID.Shark"/>, <see cref="NPCID.AnglerFish"/>, <see cref="NPCID.Arapaima"/>, <see cref="NPCID.BloodFeeder"/>, <see cref="NPCID.CrimsonGoldfish"/>, <see cref="NPCID.GoldGoldfish"/>, <see cref="NPCID.Pupfish"/>, <see cref="NPCID.Dolphin"/>
	/// </summary>
	public const short Piranha = 16;
	/// <summary>
	/// Used by: <see cref="NPCID.Vulture"/>, <see cref="NPCID.Raven"/>
	/// </summary>
	public const short Vulture = 17;
	/// <summary>
	/// Used by: <see cref="NPCID.BlueJellyfish"/>, <see cref="NPCID.PinkJellyfish"/>, <see cref="NPCID.GreenJellyfish"/>, <see cref="NPCID.Squid"/>, <see cref="NPCID.BloodJelly"/>, <see cref="NPCID.FungoFish"/>
	/// </summary>
	public const short Jellyfish = 18;
	/// <summary>
	/// Used by: <see cref="NPCID.Antlion"/>
	/// </summary>
	public const short Antlion = 19;
	/// <summary>
	/// Behavior: For the spike balls in the dungoen, not the projectile<br/><br/>
	/// Used by: <see cref="NPCID.SpikeBall"/>
	/// </summary>
	public const short SpikeBall = 20;
	/// <summary>
	/// Used by: <see cref="NPCID.BlazingWheel"/>
	/// </summary>
	public const short BlazingWheel = 21;
	/// <summary>
	/// Behavior: Includes enemies such as Wraiths or Ghosts<br/><br/>
	/// Used by: <see cref="NPCID.Pixie"/>, <see cref="NPCID.Wraith"/>, <see cref="NPCID.Gastropod"/>, <see cref="NPCID.IceElemental"/>, <see cref="NPCID.FloatyGross"/>, <see cref="NPCID.Reaper"/>, <see cref="NPCID.IchorSticker"/>, <see cref="NPCID.Ghost"/>, <see cref="NPCID.Poltergeist"/>, <see cref="NPCID.Drippler"/>
	/// </summary>
	public const short HoveringFighter = 22;
	/// <summary>
	/// Behavior: Includes Shadow Hammer and Crimson Axe<br/><br/>
	/// Used by: <see cref="NPCID.CursedHammer"/>, <see cref="NPCID.EnchantedSword"/>, <see cref="NPCID.CrimsonAxe"/>
	/// </summary>
	public const short EnchantedSword = 23;
	/// <summary>
	/// Used by: <see cref="NPCID.Bird"/>, <see cref="NPCID.BirdBlue"/>, <see cref="NPCID.BirdRed"/>, <see cref="NPCID.GoldBird"/>, <see cref="NPCID.Owl"/>, <see cref="NPCID.ScarletMacaw"/>, <see cref="NPCID.BlueMacaw"/>, <see cref="NPCID.Toucan"/>, <see cref="NPCID.YellowCockatiel"/>, <see cref="NPCID.GrayCockatiel"/>
	/// </summary>
	public const short Bird = 24;
	/// <summary>
	/// Used by: <see cref="NPCID.Mimic"/>, <see cref="NPCID.PresentMimic"/>, <see cref="NPCID.IceMimic"/>
	/// </summary>
	public const short Mimic = 25;
	/// <summary>
	/// Used by: <see cref="NPCID.Unicorn"/>, <see cref="NPCID.Wolf"/>, <see cref="NPCID.HeadlessHorseman"/>, <see cref="NPCID.Hellhound"/>, <see cref="NPCID.StardustSpiderSmall"/>, <see cref="NPCID.NebulaBeast"/>, <see cref="NPCID.Tumbleweed"/>
	/// </summary>
	public const short Unicorn = 26;
	/// <summary>
	/// Used by: <see cref="NPCID.WallofFlesh"/>
	/// </summary>
	public const short WallOfFleshMouth = 27;
	/// <summary>
	/// Used by: <see cref="NPCID.WallofFleshEye"/>
	/// </summary>
	public const short WallOfFleshEye = 28;
	/// <summary>
	/// Used by: <see cref="NPCID.TheHungry"/>
	/// </summary>
	public const short TheHungry = 29;
	/// <summary>
	/// Used by: <see cref="NPCID.Retinazer"/>
	/// </summary>
	public const short Retinazer = 30;
	/// <summary>
	/// Used by: <see cref="NPCID.Spazmatism"/>
	/// </summary>
	public const short Spaazmatism = 31;
	/// <summary>
	/// Used by: <see cref="NPCID.SkeletronPrime"/>
	/// </summary>
	public const short SkeletronPrimeHead = 32;
	/// <summary>
	/// Used by: <see cref="NPCID.PrimeSaw"/>
	/// </summary>
	public const short PrimeSaw = 33;
	/// <summary>
	/// Used by: <see cref="NPCID.PrimeVice"/>
	/// </summary>
	public const short PrimeVice = 34;
	/// <summary>
	/// Used by: <see cref="NPCID.PrimeCannon"/>
	/// </summary>
	public const short PrimeCannon = 35;
	/// <summary>
	/// Used by: <see cref="NPCID.PrimeLaser"/>
	/// </summary>
	public const short PrimeLaser = 36;
	/// <summary>
	/// Used by: <see cref="NPCID.TheDestroyer"/>, <see cref="NPCID.TheDestroyerBody"/>, <see cref="NPCID.TheDestroyerTail"/>
	/// </summary>
	public const short TheDestroyer = 37;
	/// <summary>
	/// Used by: <see cref="NPCID.SnowmanGangsta"/>, <see cref="NPCID.MisterStabby"/>, <see cref="NPCID.SnowBalla"/>
	/// </summary>
	public const short Snowman = 38;
	/// <summary>
	/// Behavior: Also includes Srollers and Giant Shellies<br/><br/>
	/// Used by: <see cref="NPCID.GiantTortoise"/>, <see cref="NPCID.IceTortoise"/>, <see cref="NPCID.SolarSroller"/>, <see cref="NPCID.GiantShelly"/>, <see cref="NPCID.GiantShelly2"/>
	/// </summary>
	public const short GiantTortoise = 39;
	/// <summary>
	/// Behavior: Used for the wall climbing variants of spiders, the ground variant is<br/><br/>
	/// Used by: <see cref="NPCID.WallCreeperWall"/>, <see cref="NPCID.JungleCreeperWall"/>, <see cref="NPCID.BlackRecluseWall"/>, <see cref="NPCID.BloodCrawlerWall"/>, <see cref="NPCID.DesertScorpionWall"/>
	/// </summary>
	public const short Spider = 40;
	/// <summary>
	/// Used by: <see cref="NPCID.Herpling"/>, <see cref="NPCID.Derpling"/>, <see cref="NPCID.ChatteringTeethBomb"/>
	/// </summary>
	public const short Herpling = 41;
	/// <summary>
	/// Behavior: Only used for the Lost Girl, nymphs use<br/><br/>
	/// Used by: <see cref="NPCID.LostGirl"/>
	/// </summary>
	public const short LostGirl = 42;
	/// <summary>
	/// Used by: <see cref="NPCID.QueenBee"/>
	/// </summary>
	public const short QueenBee = 43;
	/// <summary>
	/// Behavior: Also used for Antlion Swarmers<br/><br/>
	/// Used by: <see cref="NPCID.FlyingFish"/>, <see cref="NPCID.GiantFlyingAntlion"/>, <see cref="NPCID.FlyingAntlion"/>, <see cref="NPCID.EyeballFlyingFish"/>
	/// </summary>
	public const short FlyingFish = 44;
	/// <summary>
	/// Used by: <see cref="NPCID.Golem"/>
	/// </summary>
	public const short GolemBody = 45;
	/// <summary>
	/// Behavior: Only used for the unmoving golem head, the moving one is<br/><br/>
	/// Used by: <see cref="NPCID.GolemHead"/>
	/// </summary>
	public const short GolemHead = 46;
	/// <summary>
	/// Used by: <see cref="NPCID.GolemFistLeft"/>, <see cref="NPCID.GolemFistRight"/>
	/// </summary>
	public const short GolemFist = 47;
	/// <summary>
	/// Used by: <see cref="NPCID.GolemHeadFree"/>
	/// </summary>
	public const short FreeGolemHead = 48;
	/// <summary>
	/// Used by: <see cref="NPCID.AngryNimbus"/>
	/// </summary>
	public const short AngryNimbus = 49;
	/// <summary>
	/// Used by: <see cref="NPCID.FungiSpore"/>, <see cref="NPCID.Spore"/>
	/// </summary>
	public const short Spore = 50;
	/// <summary>
	/// Used by: <see cref="NPCID.Plantera"/>
	/// </summary>
	public const short Plantera = 51;
	/// <summary>
	/// Used by: <see cref="NPCID.PlanterasHook"/>
	/// </summary>
	public const short PlanteraHook = 52;
	/// <summary>
	/// Used by: <see cref="NPCID.PlanterasTentacle"/>
	/// </summary>
	public const short PlanteraTentacle = 53;
	/// <summary>
	/// Used by: <see cref="NPCID.BrainofCthulhu"/>
	/// </summary>
	public const short BrainOfCthulhu = 54;
	/// <summary>
	/// Behavior: For the Brain of Cthulhu's minions<br/><br/>
	/// Used by: <see cref="NPCID.Creeper"/>
	/// </summary>
	public const short Creeper = 55;
	/// <summary>
	/// Used by: <see cref="NPCID.DungeonSpirit"/>
	/// </summary>
	public const short DungeonSpirit = 56;
	/// <summary>
	/// Behavior: Includes Everscream<br/><br/>
	/// Used by: <see cref="NPCID.MourningWood"/>, <see cref="NPCID.Everscream"/>
	/// </summary>
	public const short MourningWood = 57;
	/// <summary>
	/// Used by: <see cref="NPCID.Pumpking"/>
	/// </summary>
	public const short Pumpking = 58;
	/// <summary>
	/// Used by: <see cref="NPCID.PumpkingBlade"/>
	/// </summary>
	public const short PumpkingScythe = 59;
	/// <summary>
	/// Used by: <see cref="NPCID.IceQueen"/>
	/// </summary>
	public const short IceQueen = 60;
	/// <summary>
	/// Used by: <see cref="NPCID.SantaNK1"/>
	/// </summary>
	public const short SantaNK1 = 61;
	/// <summary>
	/// Used by: <see cref="NPCID.ElfCopter"/>
	/// </summary>
	public const short ElfCopter = 62;
	/// <summary>
	/// Used by: <see cref="NPCID.Flocko"/>
	/// </summary>
	public const short Flocko = 63;
	/// <summary>
	/// Used by: <see cref="NPCID.Firefly"/>, <see cref="NPCID.LightningBug"/>, <see cref="NPCID.Lavafly"/>, <see cref="NPCID.Shimmerfly"/>
	/// </summary>
	public const short Firefly = 64;
	/// <summary>
	/// Used by: <see cref="NPCID.Butterfly"/>, <see cref="NPCID.GoldButterfly"/>, <see cref="NPCID.HellButterfly"/>, <see cref="NPCID.EmpressButterfly"/>
	/// </summary>
	public const short Butterfly = 65;
	/// <summary>
	/// Used by: <see cref="NPCID.Worm"/>, <see cref="NPCID.TruffleWorm"/>, <see cref="NPCID.GoldWorm"/>, <see cref="NPCID.EnchantedNightcrawler"/>, <see cref="NPCID.Grubby"/>, <see cref="NPCID.Sluggy"/>, <see cref="NPCID.Buggy"/>, <see cref="NPCID.Maggot"/>
	/// </summary>
	public const short CritterWorm = 66;
	/// <summary>
	/// Used by: <see cref="NPCID.Snail"/>, <see cref="NPCID.GlowingSnail"/>, <see cref="NPCID.MagmaSnail"/>
	/// </summary>
	public const short Snail = 67;
	/// <summary>
	/// Used by: <see cref="NPCID.Duck2"/>, <see cref="NPCID.DuckWhite2"/>, <see cref="NPCID.Seagull2"/>, <see cref="NPCID.Grebe2"/>
	/// </summary>
	public const short Duck = 68;
	/// <summary>
	/// Used by: <see cref="NPCID.DukeFishron"/>
	/// </summary>
	public const short DukeFishron = 69;
	/// <summary>
	/// Used by: <see cref="NPCID.DetonatingBubble"/>
	/// </summary>
	public const short DukeFishronBubble = 70;
	/// <summary>
	/// Used by: <see cref="NPCID.Sharkron"/>, <see cref="NPCID.Sharkron2"/>
	/// </summary>
	public const short Sharkron = 71;
	/// <summary>
	/// Used by: <see cref="NPCID.ForceBubble"/>
	/// </summary>
	public const short BubbleShield = 72;
	/// <summary>
	/// Used by: <see cref="NPCID.MartianTurret"/>
	/// </summary>
	public const short TeslaTurret = 73;
	/// <summary>
	/// Used by: <see cref="NPCID.MartianDrone"/>, <see cref="NPCID.SolarCorite"/>
	/// </summary>
	public const short Corite = 74;
	/// <summary>
	/// Behavior: Includes Drakomire Rider, Dutchman Cannon, Martian Saucer,Martian Saucer Cannon, Martian Saucer Turret, and Scutlix Gunner<br/><br/>
	/// Used by: <see cref="NPCID.ScutlixRider"/>, <see cref="NPCID.MartianSaucer"/>, <see cref="NPCID.MartianSaucerTurret"/>, <see cref="NPCID.MartianSaucerCannon"/>, <see cref="NPCID.SolarDrakomireRider"/>, <see cref="NPCID.PirateShipCannon"/>
	/// </summary>
	public const short Rider = 75;
	/// <summary>
	/// Used by: <see cref="NPCID.MartianSaucerCore"/>
	/// </summary>
	public const short MartianSaucer = 76;
	/// <summary>
	/// Used by: <see cref="NPCID.MoonLordCore"/>
	/// </summary>
	public const short MoonLordCore = 77;
	/// <summary>
	/// Used by: <see cref="NPCID.MoonLordHand"/>
	/// </summary>
	public const short MoonLordHand = 78;
	/// <summary>
	/// Used by: <see cref="NPCID.MoonLordHead"/>
	/// </summary>
	public const short MoonLordHead = 79;
	/// <summary>
	/// Used by: <see cref="NPCID.MartianProbe"/>
	/// </summary>
	public const short MartianProbe = 80;
	/// <summary>
	/// Used by: <see cref="NPCID.MoonLordFreeEye"/>
	/// </summary>
	public const short TrueEyeOfCthulhu = 81;
	/// <summary>
	/// Used by: <see cref="NPCID.MoonLordLeechBlob"/>
	/// </summary>
	public const short MoonLeachClot = 82;
	/// <summary>
	/// Used by: <see cref="NPCID.CultistTablet"/>, <see cref="NPCID.CultistDevote"/>
	/// </summary>
	public const short LunaticDevote = 83;
	/// <summary>
	/// Used by: <see cref="NPCID.CultistBoss"/>, <see cref="NPCID.CultistBossClone"/>
	/// </summary>
	public const short LunaticCultist = 84;
	/// <summary>
	/// Behavior: Includes Brain Sucklers and Deadly Spheres<br/><br/>
	/// Used by: <see cref="NPCID.StardustCellBig"/>, <see cref="NPCID.NebulaHeadcrab"/>, <see cref="NPCID.DeadlySphere"/>
	/// </summary>
	public const short StarCell = 85;
	/// <summary>
	/// Used by: <see cref="NPCID.ShadowFlameApparition"/>, <see cref="NPCID.AncientCultistSquidhead"/>
	/// </summary>
	public const short AncientVision = 86;
	/// <summary>
	/// Used by: <see cref="NPCID.BigMimicCorruption"/>, <see cref="NPCID.BigMimicCrimson"/>, <see cref="NPCID.BigMimicHallow"/>, <see cref="NPCID.BigMimicJungle"/>
	/// </summary>
	public const short BiomeMimic = 87;
	/// <summary>
	/// Used by: <see cref="NPCID.Mothron"/>
	/// </summary>
	public const short Mothron = 88;
	/// <summary>
	/// Used by: <see cref="NPCID.MothronEgg"/>
	/// </summary>
	public const short MothronEgg = 89;
	/// <summary>
	/// Used by: <see cref="NPCID.MothronSpawn"/>
	/// </summary>
	public const short BabyMothron = 90;
	/// <summary>
	/// Used by: <see cref="NPCID.GraniteFlyer"/>
	/// </summary>
	public const short GraniteElemental = 91;
	/// <summary>
	/// Used by: <see cref="NPCID.TargetDummy"/>
	/// </summary>
	public const short TargetDummy = 92;
	/// <summary>
	/// Used by: <see cref="NPCID.PirateShip"/>
	/// </summary>
	public const short FlyingDutchman = 93;
	/// <summary>
	/// Used by: <see cref="NPCID.LunarTowerVortex"/>, <see cref="NPCID.LunarTowerStardust"/>, <see cref="NPCID.LunarTowerNebula"/>, <see cref="NPCID.LunarTowerSolar"/>
	/// </summary>
	public const short CelestialPillar = 94;
	/// <summary>
	/// Used by: <see cref="NPCID.StardustCellSmall"/>
	/// </summary>
	public const short SmallStarCell = 95;
	/// <summary>
	/// Used by: <see cref="NPCID.StardustJellyfishBig"/>
	/// </summary>
	public const short FlowInvader = 96;
	/// <summary>
	/// Used by: <see cref="NPCID.NebulaBrain"/>
	/// </summary>
	public const short NebulaFloater = 97;
	/// <summary>
	/// Behavior: Stays in place and shoots<br/><br/>
	/// Used by: None
	/// </summary>
	public const short Unused0 = 98;
	/// <summary>
	/// Behavior: The fireball-like "projectiles" shot by the solar pillar<br/><br/>
	/// Used by: <see cref="NPCID.SolarGoop"/>
	/// </summary>
	public const short SolarFragment = 99;
	/// <summary>
	/// Used by: <see cref="NPCID.AncientLight"/>
	/// </summary>
	public const short AncientLight = 100;
	/// <summary>
	/// Used by: <see cref="NPCID.AncientDoom"/>
	/// </summary>
	public const short AncientDoom = 101;
	/// <summary>
	/// Used by: <see cref="NPCID.SandElemental"/>
	/// </summary>
	public const short SandElemental = 102;
	/// <summary>
	/// Used by: <see cref="NPCID.SandShark"/>, <see cref="NPCID.SandsharkCorrupt"/>, <see cref="NPCID.SandsharkCrimson"/>, <see cref="NPCID.SandsharkHallow"/>
	/// </summary>
	public const short SandShark = 103;
	/// <summary>
	/// Behavior: Instantly despawns<br/><br/>
	/// Used by: <see cref="NPCID.DD2AttackerTest"/>
	/// </summary>
	public const short Unknown1 = 104;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2EterniaCrystal"/>
	/// </summary>
	public const short DD2EterniaCrystal = 105;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2LanePortal"/>
	/// </summary>
	public const short DD2MysteriousPortal = 106;
	/// <summary>
	/// Behavior: Used for things such as Etherian Goblins<br/><br/>
	/// Used by: <see cref="NPCID.DD2GoblinT1"/>, <see cref="NPCID.DD2GoblinT2"/>, <see cref="NPCID.DD2GoblinT3"/>, <see cref="NPCID.DD2GoblinBomberT1"/>, <see cref="NPCID.DD2GoblinBomberT2"/>, <see cref="NPCID.DD2GoblinBomberT3"/>, <see cref="NPCID.DD2JavelinstT1"/>, <see cref="NPCID.DD2JavelinstT2"/>, <see cref="NPCID.DD2JavelinstT3"/>, <see cref="NPCID.DD2SkeletonT1"/>, <see cref="NPCID.DD2SkeletonT3"/>, <see cref="NPCID.DD2WitherBeastT2"/>, <see cref="NPCID.DD2WitherBeastT3"/>, <see cref="NPCID.DD2DrakinT2"/>, <see cref="NPCID.DD2DrakinT3"/>, <see cref="NPCID.DD2KoboldWalkerT2"/>, <see cref="NPCID.DD2KoboldWalkerT3"/>, <see cref="NPCID.DD2OgreT2"/>, <see cref="NPCID.DD2OgreT3"/>, <see cref="NPCID.GoblinShark"/>
	/// </summary>
	public const short DD2Fighter = 107;
	/// <summary>
	/// Behavior: Used for things such as Etherian Wyverns<br/><br/>
	/// Used by: <see cref="NPCID.DD2WyvernT1"/>, <see cref="NPCID.DD2WyvernT2"/>, <see cref="NPCID.DD2WyvernT3"/>, <see cref="NPCID.DD2KoboldFlyerT2"/>, <see cref="NPCID.DD2KoboldFlyerT3"/>
	/// </summary>
	public const short DD2Flying = 108;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2DarkMageT1"/>, <see cref="NPCID.DD2DarkMageT3"/>
	/// </summary>
	public const short DD2DarkMage = 109;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2Betsy"/>
	/// </summary>
	public const short DD2Betsy = 110;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2LightningBugT3"/>
	/// </summary>
	public const short DD2LightningBug = 111;
	/// <summary>
	/// Used by: <see cref="NPCID.FairyCritterPink"/>, <see cref="NPCID.FairyCritterGreen"/>, <see cref="NPCID.FairyCritterBlue"/>
	/// </summary>
	public const short Fairy = 112;
	/// <summary>
	/// Used by: <see cref="NPCID.WindyBalloon"/>
	/// </summary>
	public const short Balloon = 113;
	/// <summary>
	/// Used by: <see cref="NPCID.BlackDragonfly"/>, <see cref="NPCID.BlueDragonfly"/>, <see cref="NPCID.GreenDragonfly"/>, <see cref="NPCID.OrangeDragonfly"/>, <see cref="NPCID.RedDragonfly"/>, <see cref="NPCID.YellowDragonfly"/>, <see cref="NPCID.GoldDragonfly"/>
	/// </summary>
	public const short Dragonfly = 114;
	/// <summary>
	/// Used by: <see cref="NPCID.LadyBug"/>, <see cref="NPCID.GoldLadyBug"/>, <see cref="NPCID.Stinkbug"/>
	/// </summary>
	public const short Ladybug = 115;
	/// <summary>
	/// Used by: <see cref="NPCID.WaterStrider"/>, <see cref="NPCID.GoldWaterStrider"/>
	/// </summary>
	public const short WaterStrider = 116;
	/// <summary>
	/// Used by: <see cref="NPCID.BloodNautilus"/>
	/// </summary>
	public const short Dreadnautilus = 117;
	/// <summary>
	/// Used by: <see cref="NPCID.Seahorse"/>, <see cref="NPCID.GoldSeahorse"/>
	/// </summary>
	public const short Seahorse = 118;
	/// <summary>
	/// Used by: <see cref="NPCID.Dandelion"/>
	/// </summary>
	public const short AngryDandelion = 119;
	/// <summary>
	/// Used by: <see cref="NPCID.HallowBoss"/>
	/// </summary>
	public const short EmpressOfLight = 120;
	/// <summary>
	/// Used by: <see cref="NPCID.QueenSlimeBoss"/>
	/// </summary>
	public const short QueenSlime = 121;
	/// <summary>
	/// Used by: <see cref="NPCID.PirateGhost"/>
	/// </summary>
	public const short PiratesCurse = 122;
}
