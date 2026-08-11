using ReLogic.Reflection;
using static Terraria.ID.NPCID;

namespace Terraria.ID;

public class NPCAIStyleID
{
	public static readonly IdDictionary Search = IdDictionary.Create<NPCAIStyleID, short>();
	/// <summary>
	/// Used by: <see cref="BoundGoblin"/>, <see cref="BoundWizard"/>, <see cref="BoundMechanic"/>, <see cref="WebbedStylist"/>, <see cref="SleepingAngler"/>, <see cref="StardustWormBody"/>, <see cref="StardustWormTail"/>, <see cref="StardustJellyfishSmall"/>, <see cref="BartenderUnconscious"/>, <see cref="GolferRescue"/>, <see cref="TorchGod"/>
	/// </summary>
	public const short FaceClosestPlayer = 0;
	/// <summary>
	/// Used by: <see cref="BigCrimslime"/>, <see cref="LittleCrimslime"/>, <see cref="JungleSlime"/>, <see cref="YellowSlime"/>, <see cref="RedSlime"/>, <see cref="PurpleSlime"/>, <see cref="BlackSlime"/>, <see cref="BabySlime"/>, <see cref="Pinky"/>, <see cref="GreenSlime"/>, <see cref="Slimer2"/>, <see cref="Slimeling"/>, <see cref="BlueSlime"/>, <see cref="MotherSlime"/>, <see cref="LavaSlime"/>, <see cref="DungeonSlime"/>, <see cref="CorruptSlime"/>, <see cref="IlluminantSlime"/>, <see cref="ToxicSludge"/>, <see cref="IceSlime"/>, <see cref="Crimslime"/>, <see cref="SpikedIceSlime"/>, <see cref="SpikedJungleSlime"/>, <see cref="UmbrellaSlime"/>, <see cref="RainbowSlime"/>, <see cref="SlimeMasked"/>, <see cref="HoppinJack"/>, <see cref="SlimeRibbonWhite"/>, <see cref="SlimeRibbonYellow"/>, <see cref="SlimeRibbonGreen"/>, <see cref="SlimeRibbonRed"/>, <see cref="Grasshopper"/>, <see cref="GoldGrasshopper"/>, <see cref="SlimeSpiked"/>, <see cref="SandSlime"/>, <see cref="QueenSlimeMinionBlue"/>, <see cref="QueenSlimeMinionPink"/>, <see cref="GoldenSlime"/>, <see cref="ShimmerSlime"/>, <see cref="BoundTownSlimeOld"/>
	/// </summary>
	public const short Slime = 1;
	/// <summary>
	/// Used by: <see cref="DemonEye2"/>, <see cref="PurpleEye2"/>, <see cref="GreenEye2"/>, <see cref="DialatedEye2"/>, <see cref="SleepyEye2"/>, <see cref="CataractEye2"/>, <see cref="NPCID.DemonEye"/>, <see cref="TheHungryII"/>, <see cref="WanderingEye"/>, <see cref="PigronCorruption"/>, <see cref="PigronHallow"/>, <see cref="PigronCrimson"/>, <see cref="CataractEye"/>, <see cref="SleepyEye"/>, <see cref="DialatedEye"/>, <see cref="GreenEye"/>, <see cref="PurpleEye"/>, <see cref="DemonEyeOwl"/>, <see cref="DemonEyeSpaceship"/>
	/// </summary>
	public const short DemonEye = 2;
	/// <summary>
	/// Used by: <see cref="BigRainZombie"/>, <see cref="SmallRainZombie"/>, <see cref="BigPantlessSkeleton"/>, <see cref="SmallPantlessSkeleton"/>, <see cref="BigMisassembledSkeleton"/>, <see cref="SmallMisassembledSkeleton"/>, <see cref="BigHeadacheSkeleton"/>, <see cref="SmallHeadacheSkeleton"/>, <see cref="BigSkeleton"/>, <see cref="SmallSkeleton"/>, <see cref="BigFemaleZombie"/>, <see cref="SmallFemaleZombie"/>, <see cref="BigTwiggyZombie"/>, <see cref="SmallTwiggyZombie"/>, <see cref="BigSwampZombie"/>, <see cref="SmallSwampZombie"/>, <see cref="BigSlimedZombie"/>, <see cref="SmallSlimedZombie"/>, <see cref="BigPincushionZombie"/>, <see cref="SmallPincushionZombie"/>, <see cref="BigBaldZombie"/>, <see cref="SmallBaldZombie"/>, <see cref="BigZombie"/>, <see cref="SmallZombie"/>, <see cref="HeavySkeleton"/>, <see cref="BigBoned"/>, <see cref="ShortBones"/>, <see cref="Zombie"/>, <see cref="Skeleton"/>, <see cref="GoblinPeon"/>, <see cref="GoblinThief"/>, <see cref="GoblinWarrior"/>, <see cref="AngryBones"/>, <see cref="UndeadMiner"/>, <see cref="CorruptBunny"/>, <see cref="DoctorBones"/>, <see cref="TheGroom"/>, <see cref="Crab"/>, <see cref="GoblinScout"/>, <see cref="ArmoredSkeleton"/>, <see cref="Mummy"/>, <see cref="DarkMummy"/>, <see cref="LightMummy"/>, <see cref="Werewolf"/>, <see cref="Clown"/>, <see cref="SkeletonArcher"/>, <see cref="GoblinArcher"/>, <see cref="ChaosElemental"/>, <see cref="BaldZombie"/>, <see cref="PossessedArmor"/>, <see cref="Vampire"/>, <see cref="ZombieEskimo"/>, <see cref="Frankenstein"/>, <see cref="BlackRecluse"/>, <see cref="WallCreeper"/>, <see cref="SwampThing"/>, <see cref="UndeadViking"/>, <see cref="CorruptPenguin"/>, <see cref="FaceMonster"/>, <see cref="SnowFlinx"/>, <see cref="PincushionZombie"/>, <see cref="SlimedZombie"/>, <see cref="SwampZombie"/>, <see cref="TwiggyZombie"/>, <see cref="Nymph"/>, <see cref="ArmoredViking"/>, <see cref="Lihzahrd"/>, <see cref="LihzahrdCrawler"/>, <see cref="FemaleZombie"/>, <see cref="HeadacheSkeleton"/>, <see cref="MisassembledSkeleton"/>, <see cref="PantlessSkeleton"/>, <see cref="IcyMerman"/>, <see cref="PirateDeckhand"/>, <see cref="PirateCorsair"/>, <see cref="PirateDeadeye"/>, <see cref="PirateCrossbower"/>, <see cref="PirateCaptain"/>, <see cref="CochinealBeetle"/>, <see cref="CyanBeetle"/>, <see cref="LacBeetle"/>, <see cref="SeaSnail"/>, <see cref="ZombieRaincoat"/>, <see cref="JungleCreeper"/>, <see cref="BloodCrawler"/>, <see cref="IceGolem"/>, <see cref="Eyezor"/>, <see cref="ZombieMushroom"/>, <see cref="ZombieMushroomHat"/>, <see cref="AnomuraFungus"/>, <see cref="MushiLadybug"/>, <see cref="RustyArmoredBonesAxe"/>, <see cref="RustyArmoredBonesFlail"/>, <see cref="RustyArmoredBonesSword"/>, <see cref="RustyArmoredBonesSwordNoArmor"/>, <see cref="BlueArmoredBones"/>, <see cref="BlueArmoredBonesMace"/>, <see cref="BlueArmoredBonesNoPants"/>, <see cref="BlueArmoredBonesSword"/>, <see cref="HellArmoredBones"/>, <see cref="HellArmoredBonesSpikeShield"/>, <see cref="HellArmoredBonesMace"/>, <see cref="HellArmoredBonesSword"/>, <see cref="BoneLee"/>, <see cref="Paladin"/>, <see cref="SkeletonSniper"/>, <see cref="TacticalSkeleton"/>, <see cref="SkeletonCommando"/>, <see cref="AngryBonesBig"/>, <see cref="AngryBonesBigMuscle"/>, <see cref="AngryBonesBigHelmet"/>, <see cref="Scarecrow1"/>, <see cref="Scarecrow2"/>, <see cref="Scarecrow3"/>, <see cref="Scarecrow4"/>, <see cref="Scarecrow5"/>, <see cref="Scarecrow6"/>, <see cref="Scarecrow7"/>, <see cref="Scarecrow8"/>, <see cref="Scarecrow9"/>, <see cref="Scarecrow10"/>, <see cref="ZombieDoctor"/>, <see cref="ZombieSuperman"/>, <see cref="ZombiePixie"/>, <see cref="SkeletonTopHat"/>, <see cref="SkeletonAstonaut"/>, <see cref="SkeletonAlien"/>, <see cref="Splinterling"/>, <see cref="ZombieXmas"/>, <see cref="ZombieSweater"/>, <see cref="ZombieElf"/>, <see cref="ZombieElfBeard"/>, <see cref="ZombieElfGirl"/>, <see cref="GingerbreadMan"/>, <see cref="Yeti"/>, <see cref="Nutcracker"/>, <see cref="NutcrackerSpinning"/>, <see cref="ElfArcher"/>, <see cref="Krampus"/>, <see cref="CultistArcherBlue"/>, <see cref="CultistArcherWhite"/>, <see cref="BrainScrambler"/>, <see cref="RayGunner"/>, <see cref="MartianOfficer"/>, <see cref="GrayGrunt"/>, <see cref="MartianEngineer"/>, <see cref="GigaZapper"/>, <see cref="Scutlix"/>, <see cref="StardustSpiderBig"/>, <see cref="StardustSoldier"/>, <see cref="SolarDrakomire"/>, <see cref="SolarSolenian"/>, <see cref="NebulaSoldier"/>, <see cref="VortexRifleman"/>, <see cref="VortexHornetQueen"/>, <see cref="VortexHornet"/>, <see cref="VortexLarva"/>, <see cref="VortexSoldier"/>, <see cref="ArmedZombie"/>, <see cref="ArmedZombieEskimo"/>, <see cref="ArmedZombiePincussion"/>, <see cref="ArmedZombieSlimed"/>, <see cref="ArmedZombieSwamp"/>, <see cref="ArmedZombieTwiggy"/>, <see cref="ArmedZombieCenx"/>, <see cref="BoneThrowingSkeleton"/>, <see cref="BoneThrowingSkeleton2"/>, <see cref="BoneThrowingSkeleton3"/>, <see cref="BoneThrowingSkeleton4"/>, <see cref="Butcher"/>, <see cref="CreatureFromTheDeep"/>, <see cref="Fritz"/>, <see cref="Nailhead"/>, <see cref="CrimsonBunny"/>, <see cref="Psycho"/>, <see cref="DrManFly"/>, <see cref="ThePossessed"/>, <see cref="CrimsonPenguin"/>, <see cref="GoblinSummoner"/>, <see cref="Medusa"/>, <see cref="GreekSkeleton"/>, <see cref="GraniteGolem"/>, <see cref="BloodZombie"/>, <see cref="Crawdad"/>, <see cref="Crawdad2"/>, <see cref="Salamander"/>, <see cref="Salamander2"/>, <see cref="Salamander3"/>, <see cref="Salamander4"/>, <see cref="Salamander5"/>, <see cref="Salamander6"/>, <see cref="Salamander7"/>, <see cref="Salamander8"/>, <see cref="Salamander9"/>, <see cref="GiantWalkingAntlion"/>, <see cref="SolarSpearman"/>, <see cref="MartianWalker"/>, <see cref="DesertGhoul"/>, <see cref="DesertGhoulCorruption"/>, <see cref="DesertGhoulCrimson"/>, <see cref="DesertGhoulHallow"/>, <see cref="DesertLamiaLight"/>, <see cref="DesertLamiaDark"/>, <see cref="DesertScorpionWalk"/>, <see cref="DesertBeast"/>, <see cref="DemonTaxCollector"/>, <see cref="TheBride"/>, <see cref="WalkingAntlion"/>, <see cref="LarvaeAntlion"/>, <see cref="ZombieMerman"/>, <see cref="TorchZombie"/>, <see cref="ArmedTorchZombie"/>, <see cref="Gnome"/>, <see cref="BloodMummy"/>, <see cref="RockGolem"/>, <see cref="MaggotZombie"/>, <see cref="SporeSkeleton"/>, <see cref="MossZombie"/>
	/// </summary>
	public const short Fighter = 3;
	/// <summary>
	/// Used by: <see cref="EyeofCthulhu"/>
	/// </summary>
	public const short EyeOfCthulhu = 4;
	/// <summary>
	/// Behavior: Includes things such as Eaters of Souls<br/><br/>
	/// Used by: <see cref="BigHornetStingy"/>, <see cref="LittleHornetStingy"/>, <see cref="BigHornetSpikey"/>, <see cref="LittleHornetSpikey"/>, <see cref="BigHornetLeafy"/>, <see cref="LittleHornetLeafy"/>, <see cref="BigHornetHoney"/>, <see cref="LittleHornetHoney"/>, <see cref="BigHornetFatty"/>, <see cref="LittleHornetFatty"/>, <see cref="BigCrimera"/>, <see cref="LittleCrimera"/>, <see cref="GiantMossHornet"/>, <see cref="BigMossHornet"/>, <see cref="LittleMossHornet"/>, <see cref="TinyMossHornet"/>, <see cref="BigStinger"/>, <see cref="LittleStinger"/>, <see cref="BigEater"/>, <see cref="LittleEater"/>, <see cref="ServantofCthulhu"/>, <see cref="EaterofSouls"/>, <see cref="MeteorHead"/>, <see cref="Hornet"/>, <see cref="Corruptor"/>, <see cref="Probe"/>, <see cref="Crimera"/>, <see cref="MossHornet"/>, <see cref="Moth"/>, <see cref="Bee"/>, <see cref="BeeSmall"/>, <see cref="HornetFatty"/>, <see cref="HornetHoney"/>, <see cref="HornetLeafy"/>, <see cref="HornetSpikey"/>, <see cref="HornetStingy"/>, <see cref="Parrot"/>, <see cref="BloodSquid"/>
	/// </summary>
	public const short Flying = 5;
	/// <summary>
	/// Used by: <see cref="DevourerHead"/>, <see cref="DevourerBody"/>, <see cref="DevourerTail"/>, <see cref="GiantWormHead"/>, <see cref="GiantWormBody"/>, <see cref="GiantWormTail"/>, <see cref="EaterofWorldsHead"/>, <see cref="EaterofWorldsBody"/>, <see cref="EaterofWorldsTail"/>, <see cref="BoneSerpentHead"/>, <see cref="BoneSerpentBody"/>, <see cref="BoneSerpentTail"/>, <see cref="WyvernHead"/>, <see cref="WyvernLegs"/>, <see cref="WyvernBody"/>, <see cref="WyvernBody2"/>, <see cref="WyvernBody3"/>, <see cref="WyvernTail"/>, <see cref="DiggerHead"/>, <see cref="DiggerBody"/>, <see cref="DiggerTail"/>, <see cref="SeekerHead"/>, <see cref="SeekerBody"/>, <see cref="SeekerTail"/>, <see cref="LeechHead"/>, <see cref="LeechBody"/>, <see cref="LeechTail"/>, <see cref="TruffleWormDigger"/>, <see cref="StardustWormHead"/>, <see cref="SolarCrawltipedeHead"/>, <see cref="SolarCrawltipedeBody"/>, <see cref="SolarCrawltipedeTail"/>, <see cref="CultistDragonHead"/>, <see cref="CultistDragonBody1"/>, <see cref="CultistDragonBody2"/>, <see cref="CultistDragonBody3"/>, <see cref="CultistDragonBody4"/>, <see cref="CultistDragonTail"/>, <see cref="DuneSplicerHead"/>, <see cref="DuneSplicerBody"/>, <see cref="DuneSplicerTail"/>, <see cref="TombCrawlerHead"/>, <see cref="TombCrawlerBody"/>, <see cref="TombCrawlerTail"/>, <see cref="BloodEelHead"/>, <see cref="BloodEelBody"/>, <see cref="BloodEelTail"/>
	/// </summary>
	public const short Worm = 6;
	/// <summary>
	/// Behavior: Includes Town NPCs and some ambient creatures, only Town NPCs will have defense with this AI, due to type-based hardcode<br/><br/>
	/// Used by: <see cref="Merchant"/>, <see cref="Nurse"/>, <see cref="ArmsDealer"/>, <see cref="Dryad"/>, <see cref="Guide"/>, <see cref="OldMan"/>, <see cref="Demolitionist"/>, <see cref="Bunny"/>, <see cref="Clothier"/>, <see cref="GoblinTinkerer"/>, <see cref="Wizard"/>, <see cref="Mechanic"/>, <see cref="SantaClaus"/>, <see cref="Penguin"/>, <see cref="PenguinBlack"/>, <see cref="Truffle"/>, <see cref="Steampunker"/>, <see cref="DyeTrader"/>, <see cref="PartyGirl"/>, <see cref="Cyborg"/>, <see cref="Painter"/>, <see cref="WitchDoctor"/>, <see cref="Pirate"/>, <see cref="GoldfishWalker"/>, <see cref="Squirrel"/>, <see cref="Mouse"/>, <see cref="BunnySlimed"/>, <see cref="BunnyXmas"/>, <see cref="Stylist"/>, <see cref="Frog"/>, <see cref="NPCID.Duck"/>, <see cref="DuckWhite"/>, <see cref="ScorpionBlack"/>, <see cref="Scorpion"/>, <see cref="TravellingMerchant"/>, <see cref="Angler"/>, <see cref="TaxCollector"/>, <see cref="GoldBunny"/>, <see cref="GoldFrog"/>, <see cref="GoldMouse"/>, <see cref="SkeletonMerchant"/>, <see cref="SquirrelRed"/>, <see cref="SquirrelGold"/>, <see cref="PartyBunny"/>, <see cref="DD2Bartender"/>, <see cref="Golfer"/>, <see cref="GoldGoldfishWalker"/>, <see cref="Seagull"/>, <see cref="Grebe"/>, <see cref="Rat"/>, <see cref="ExplosiveBunny"/>, <see cref="Turtle"/>, <see cref="TurtleJungle"/>, <see cref="SeaTurtle"/>, <see cref="BestiaryGirl"/>, <see cref="TownCat"/>, <see cref="TownDog"/>, <see cref="GemSquirrelAmethyst"/>, <see cref="GemSquirrelTopaz"/>, <see cref="GemSquirrelSapphire"/>, <see cref="GemSquirrelEmerald"/>, <see cref="GemSquirrelRuby"/>, <see cref="GemSquirrelDiamond"/>, <see cref="GemSquirrelAmber"/>, <see cref="GemBunnyAmethyst"/>, <see cref="GemBunnyTopaz"/>, <see cref="GemBunnySapphire"/>, <see cref="GemBunnyEmerald"/>, <see cref="GemBunnyRuby"/>, <see cref="GemBunnyDiamond"/>, <see cref="GemBunnyAmber"/>, <see cref="TownBunny"/>, <see cref="Princess"/>, <see cref="TownSlimeBlue"/>, <see cref="TownSlimeGreen"/>, <see cref="TownSlimeOld"/>, <see cref="TownSlimePurple"/>, <see cref="TownSlimeRainbow"/>, <see cref="TownSlimeRed"/>, <see cref="TownSlimeYellow"/>, <see cref="TownSlimeCopper"/>, <see cref="BoundTownSlimeYellow"/>
	/// </summary>
	public const short Passive = 7;
	/// <summary>
	/// Used by: <see cref="FireImp"/>, <see cref="GoblinSorcerer"/>, <see cref="DarkCaster"/>, <see cref="Tim"/>, <see cref="RuneWizard"/>, <see cref="RaggedCaster"/>, <see cref="RaggedCasterOpenCoat"/>, <see cref="Necromancer"/>, <see cref="NecromancerArmored"/>, <see cref="DiabolistRed"/>, <see cref="DiabolistWhite"/>, <see cref="DesertDjinn"/>, <see cref="LibrarianSkeleton"/>
	/// </summary>
	public const short Caster = 8;
	/// <summary>
	/// Used by: <see cref="BurningSphere"/>, <see cref="ChaosBall"/>, <see cref="WaterSphere"/>, <see cref="VileSpit"/>, <see cref="SolarFlare"/>, <see cref="ChaosBallTim"/>, <see cref="VileSpitEaterOfWorlds"/>
	/// </summary>
	public const short Spell = 9;
	/// <summary>
	/// Used by: <see cref="NPCID.CursedSkull"/>, <see cref="GiantCursedSkull"/>, <see cref="WaterBoltMimic"/>
	/// </summary>
	public const short CursedSkull = 10;
	/// <summary>
	/// Used by: <see cref="NPCID.SkeletronHead"/>, <see cref="DungeonGuardian"/>
	/// </summary>
	public const short SkeletronHead = 11;
	/// <summary>
	/// Used by: <see cref="NPCID.SkeletronHand"/>
	/// </summary>
	public const short SkeletronHand = 12;
	/// <summary>
	/// Used by: <see cref="NPCID.ManEater"/>, <see cref="Snatcher"/>, <see cref="Clinger"/>, <see cref="AngryTrapper"/>, <see cref="FungiBulb"/>, <see cref="GiantFungiBulb"/>
	/// </summary>
	public const short ManEater = 13;
	/// <summary>
	/// Used by: <see cref="Harpy"/>, <see cref="CaveBat"/>, <see cref="JungleBat"/>, <see cref="Hellbat"/>, <see cref="Demon"/>, <see cref="VoodooDemon"/>, <see cref="GiantBat"/>, <see cref="Slimer"/>, <see cref="IlluminantBat"/>, <see cref="IceBat"/>, <see cref="Lavabat"/>, <see cref="GiantFlyingFox"/>, <see cref="RedDevil"/>, <see cref="VampireBat"/>, <see cref="FlyingSnake"/>, <see cref="SporeBat"/>, <see cref="QueenSlimeMinionPurple"/>
	/// </summary>
	public const short Bat = 14;
	/// <summary>
	/// Used by: <see cref="NPCID.KingSlime"/>
	/// </summary>
	public const short KingSlime = 15;
	/// <summary>
	/// Used by: <see cref="Goldfish"/>, <see cref="CorruptGoldfish"/>, <see cref="NPCID.Piranha"/>, <see cref="Shark"/>, <see cref="AnglerFish"/>, <see cref="Arapaima"/>, <see cref="BloodFeeder"/>, <see cref="CrimsonGoldfish"/>, <see cref="GoldGoldfish"/>, <see cref="Pupfish"/>, <see cref="Dolphin"/>, <see cref="Pufferfish"/>, <see cref="Orca"/>
	/// </summary>
	public const short Piranha = 16;
	/// <summary>
	/// Used by: <see cref="NPCID.Vulture"/>, <see cref="Raven"/>
	/// </summary>
	public const short Vulture = 17;
	/// <summary>
	/// Used by: <see cref="BlueJellyfish"/>, <see cref="PinkJellyfish"/>, <see cref="GreenJellyfish"/>, <see cref="Squid"/>, <see cref="BloodJelly"/>, <see cref="FungoFish"/>
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
	/// Used by: <see cref="Pixie"/>, <see cref="Wraith"/>, <see cref="Gastropod"/>, <see cref="IceElemental"/>, <see cref="FloatyGross"/>, <see cref="Reaper"/>, <see cref="IchorSticker"/>, <see cref="Ghost"/>, <see cref="Poltergeist"/>, <see cref="Drippler"/>
	/// </summary>
	public const short HoveringFighter = 22;
	/// <summary>
	/// Behavior: Includes Shadow Hammer and Crimson Axe<br/><br/>
	/// Used by: <see cref="CursedHammer"/>, <see cref="NPCID.EnchantedSword"/>, <see cref="CrimsonAxe"/>
	/// </summary>
	public const short EnchantedSword = 23;
	/// <summary>
	/// Used by: <see cref="NPCID.Bird"/>, <see cref="BirdBlue"/>, <see cref="BirdRed"/>, <see cref="GoldBird"/>, <see cref="Owl"/>, <see cref="ScarletMacaw"/>, <see cref="BlueMacaw"/>, <see cref="Toucan"/>, <see cref="YellowCockatiel"/>, <see cref="GrayCockatiel"/>, <see cref="OwlMimic"/>
	/// </summary>
	public const short Bird = 24;
	/// <summary>
	/// Used by: <see cref="NPCID.Mimic"/>, <see cref="PresentMimic"/>, <see cref="IceMimic"/>
	/// </summary>
	public const short Mimic = 25;
	/// <summary>
	/// Used by: <see cref="NPCID.Unicorn"/>, <see cref="Wolf"/>, <see cref="HeadlessHorseman"/>, <see cref="Hellhound"/>, <see cref="StardustSpiderSmall"/>, <see cref="NebulaBeast"/>, <see cref="Tumbleweed"/>
	/// </summary>
	public const short Unicorn = 26;
	/// <summary>
	/// Used by: <see cref="WallofFlesh"/>
	/// </summary>
	public const short WallOfFleshMouth = 27;
	/// <summary>
	/// Used by: <see cref="WallofFleshEye"/>
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
	/// Used by: <see cref="Spazmatism"/>
	/// </summary>
	public const short Spaazmatism = 31;
	/// <summary>
	/// Used by: <see cref="SkeletronPrime"/>
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
	/// Used by: <see cref="NPCID.TheDestroyer"/>, <see cref="TheDestroyerBody"/>, <see cref="TheDestroyerTail"/>
	/// </summary>
	public const short TheDestroyer = 37;
	/// <summary>
	/// Used by: <see cref="SnowmanGangsta"/>, <see cref="MisterStabby"/>, <see cref="SnowBalla"/>
	/// </summary>
	public const short Snowman = 38;
	/// <summary>
	/// Behavior: Also includes Srollers and Giant Shellies<br/><br/>
	/// Used by: <see cref="NPCID.GiantTortoise"/>, <see cref="IceTortoise"/>, <see cref="SolarSroller"/>, <see cref="GiantShelly"/>, <see cref="GiantShelly2"/>
	/// </summary>
	public const short GiantTortoise = 39;
	/// <summary>
	/// Behavior: Used for the wall climbing variants of spiders, the ground variant is<br/><br/>
	/// Used by: <see cref="WallCreeperWall"/>, <see cref="JungleCreeperWall"/>, <see cref="BlackRecluseWall"/>, <see cref="BloodCrawlerWall"/>, <see cref="DesertScorpionWall"/>
	/// </summary>
	public const short Spider = 40;
	/// <summary>
	/// Used by: <see cref="NPCID.Herpling"/>, <see cref="Derpling"/>, <see cref="ChatteringTeethBomb"/>
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
	/// Used by: <see cref="NPCID.FlyingFish"/>, <see cref="GiantFlyingAntlion"/>, <see cref="FlyingAntlion"/>, <see cref="EyeballFlyingFish"/>
	/// </summary>
	public const short FlyingFish = 44;
	/// <summary>
	/// Used by: <see cref="Golem"/>
	/// </summary>
	public const short GolemBody = 45;
	/// <summary>
	/// Behavior: Only used for the unmoving golem head, the moving one is<br/><br/>
	/// Used by: <see cref="NPCID.GolemHead"/>
	/// </summary>
	public const short GolemHead = 46;
	/// <summary>
	/// Used by: <see cref="GolemFistLeft"/>, <see cref="GolemFistRight"/>
	/// </summary>
	public const short GolemFist = 47;
	/// <summary>
	/// Used by: <see cref="GolemHeadFree"/>
	/// </summary>
	public const short FreeGolemHead = 48;
	/// <summary>
	/// Used by: <see cref="NPCID.AngryNimbus"/>
	/// </summary>
	public const short AngryNimbus = 49;
	/// <summary>
	/// Used by: <see cref="FungiSpore"/>, <see cref="NPCID.Spore"/>
	/// </summary>
	public const short Spore = 50;
	/// <summary>
	/// Used by: <see cref="NPCID.Plantera"/>
	/// </summary>
	public const short Plantera = 51;
	/// <summary>
	/// Used by: <see cref="PlanterasHook"/>
	/// </summary>
	public const short PlanteraHook = 52;
	/// <summary>
	/// Used by: <see cref="PlanterasTentacle"/>
	/// </summary>
	public const short PlanteraTentacle = 53;
	/// <summary>
	/// Used by: <see cref="BrainofCthulhu"/>
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
	/// Used by: <see cref="NPCID.MourningWood"/>, <see cref="Everscream"/>
	/// </summary>
	public const short MourningWood = 57;
	/// <summary>
	/// Used by: <see cref="NPCID.Pumpking"/>
	/// </summary>
	public const short Pumpking = 58;
	/// <summary>
	/// Used by: <see cref="PumpkingBlade"/>
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
	/// Used by: <see cref="NPCID.Firefly"/>, <see cref="LightningBug"/>, <see cref="Lavafly"/>, <see cref="Shimmerfly"/>
	/// </summary>
	public const short Firefly = 64;
	/// <summary>
	/// Used by: <see cref="NPCID.Butterfly"/>, <see cref="GoldButterfly"/>, <see cref="HellButterfly"/>, <see cref="EmpressButterfly"/>
	/// </summary>
	public const short Butterfly = 65;
	/// <summary>
	/// Used by: <see cref="NPCID.Worm"/>, <see cref="TruffleWorm"/>, <see cref="GoldWorm"/>, <see cref="EnchantedNightcrawler"/>, <see cref="Grubby"/>, <see cref="Sluggy"/>, <see cref="Buggy"/>, <see cref="Maggot"/>
	/// </summary>
	public const short CritterWorm = 66;
	/// <summary>
	/// Used by: <see cref="NPCID.Snail"/>, <see cref="GlowingSnail"/>, <see cref="MagmaSnail"/>
	/// </summary>
	public const short Snail = 67;
	/// <summary>
	/// Used by: <see cref="Duck2"/>, <see cref="DuckWhite2"/>, <see cref="Seagull2"/>, <see cref="Grebe2"/>
	/// </summary>
	public const short Duck = 68;
	/// <summary>
	/// Used by: <see cref="NPCID.DukeFishron"/>
	/// </summary>
	public const short DukeFishron = 69;
	/// <summary>
	/// Used by: <see cref="DetonatingBubble"/>
	/// </summary>
	public const short DukeFishronBubble = 70;
	/// <summary>
	/// Used by: <see cref="NPCID.Sharkron"/>, <see cref="Sharkron2"/>
	/// </summary>
	public const short Sharkron = 71;
	/// <summary>
	/// Used by: <see cref="ForceBubble"/>
	/// </summary>
	public const short BubbleShield = 72;
	/// <summary>
	/// Used by: <see cref="MartianTurret"/>
	/// </summary>
	public const short TeslaTurret = 73;
	/// <summary>
	/// Used by: <see cref="MartianDrone"/>, <see cref="SolarCorite"/>
	/// </summary>
	public const short Corite = 74;
	/// <summary>
	/// Behavior: Includes Drakomire Rider, Dutchman Cannon, Martian Saucer,Martian Saucer Cannon, Martian Saucer Turret, and Scutlix Gunner<br/><br/>
	/// Used by: <see cref="ScutlixRider"/>, <see cref="NPCID.MartianSaucer"/>, <see cref="MartianSaucerTurret"/>, <see cref="MartianSaucerCannon"/>, <see cref="SolarDrakomireRider"/>, <see cref="PirateShipCannon"/>
	/// </summary>
	public const short Rider = 75;
	/// <summary>
	/// Used by: <see cref="MartianSaucerCore"/>
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
	/// Used by: <see cref="MoonLordFreeEye"/>
	/// </summary>
	public const short TrueEyeOfCthulhu = 81;
	/// <summary>
	/// Used by: <see cref="MoonLordLeechBlob"/>
	/// </summary>
	public const short MoonLeachClot = 82;
	/// <summary>
	/// Used by: <see cref="CultistTablet"/>, <see cref="CultistDevote"/>
	/// </summary>
	public const short LunaticDevote = 83;
	/// <summary>
	/// Used by: <see cref="CultistBoss"/>, <see cref="CultistBossClone"/>
	/// </summary>
	public const short LunaticCultist = 84;
	/// <summary>
	/// Behavior: Includes Brain Sucklers and Deadly Spheres<br/><br/>
	/// Used by: <see cref="StardustCellBig"/>, <see cref="NebulaHeadcrab"/>, <see cref="DeadlySphere"/>
	/// </summary>
	public const short StarCell = 85;
	/// <summary>
	/// Used by: <see cref="ShadowFlameApparition"/>, <see cref="AncientCultistSquidhead"/>
	/// </summary>
	public const short AncientVision = 86;
	/// <summary>
	/// Used by: <see cref="BigMimicCorruption"/>, <see cref="BigMimicCrimson"/>, <see cref="BigMimicHallow"/>, <see cref="BigMimicJungle"/>
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
	/// Used by: <see cref="MothronSpawn"/>
	/// </summary>
	public const short BabyMothron = 90;
	/// <summary>
	/// Used by: <see cref="GraniteFlyer"/>
	/// </summary>
	public const short GraniteElemental = 91;
	/// <summary>
	/// Used by: <see cref="NPCID.TargetDummy"/>
	/// </summary>
	public const short TargetDummy = 92;
	/// <summary>
	/// Used by: <see cref="PirateShip"/>
	/// </summary>
	public const short FlyingDutchman = 93;
	/// <summary>
	/// Used by: <see cref="LunarTowerVortex"/>, <see cref="LunarTowerStardust"/>, <see cref="LunarTowerNebula"/>, <see cref="LunarTowerSolar"/>
	/// </summary>
	public const short CelestialPillar = 94;
	/// <summary>
	/// Used by: <see cref="StardustCellSmall"/>
	/// </summary>
	public const short SmallStarCell = 95;
	/// <summary>
	/// Used by: <see cref="StardustJellyfishBig"/>
	/// </summary>
	public const short FlowInvader = 96;
	/// <summary>
	/// Used by: <see cref="NebulaBrain"/>
	/// </summary>
	public const short NebulaFloater = 97;
	/// <summary>
	/// Behavior: Stays in place and shoots<br/><br/>
	/// Used by: None
	/// </summary>
	public const short Unused0 = 98;
	/// <summary>
	/// Behavior: The fireball-like "projectiles" shot by the solar pillar<br/><br/>
	/// Used by: <see cref="SolarGoop"/>
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
	/// Used by: <see cref="NPCID.SandShark"/>, <see cref="SandsharkCorrupt"/>, <see cref="SandsharkCrimson"/>, <see cref="SandsharkHallow"/>
	/// </summary>
	public const short SandShark = 103;
	/// <summary>
	/// Behavior: Instantly despawns<br/><br/>
	/// Used by: <see cref="DD2AttackerTest"/>
	/// </summary>
	public const short Unknown1 = 104;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2EterniaCrystal"/>
	/// </summary>
	public const short DD2EterniaCrystal = 105;
	/// <summary>
	/// Used by: <see cref="DD2LanePortal"/>
	/// </summary>
	public const short DD2MysteriousPortal = 106;
	/// <summary>
	/// Behavior: Used for things such as Etherian Goblins<br/><br/>
	/// Used by: <see cref="DD2GoblinT1"/>, <see cref="DD2GoblinT2"/>, <see cref="DD2GoblinT3"/>, <see cref="DD2GoblinBomberT1"/>, <see cref="DD2GoblinBomberT2"/>, <see cref="DD2GoblinBomberT3"/>, <see cref="DD2JavelinstT1"/>, <see cref="DD2JavelinstT2"/>, <see cref="DD2JavelinstT3"/>, <see cref="DD2SkeletonT1"/>, <see cref="DD2SkeletonT3"/>, <see cref="DD2WitherBeastT2"/>, <see cref="DD2WitherBeastT3"/>, <see cref="DD2DrakinT2"/>, <see cref="DD2DrakinT3"/>, <see cref="DD2KoboldWalkerT2"/>, <see cref="DD2KoboldWalkerT3"/>, <see cref="DD2OgreT2"/>, <see cref="DD2OgreT3"/>, <see cref="GoblinShark"/>
	/// </summary>
	public const short DD2Fighter = 107;
	/// <summary>
	/// Behavior: Used for things such as Etherian Wyverns<br/><br/>
	/// Used by: <see cref="DD2WyvernT1"/>, <see cref="DD2WyvernT2"/>, <see cref="DD2WyvernT3"/>, <see cref="DD2KoboldFlyerT2"/>, <see cref="DD2KoboldFlyerT3"/>
	/// </summary>
	public const short DD2Flying = 108;
	/// <summary>
	/// Used by: <see cref="DD2DarkMageT1"/>, <see cref="DD2DarkMageT3"/>
	/// </summary>
	public const short DD2DarkMage = 109;
	/// <summary>
	/// Used by: <see cref="NPCID.DD2Betsy"/>
	/// </summary>
	public const short DD2Betsy = 110;
	/// <summary>
	/// Used by: <see cref="DD2LightningBugT3"/>
	/// </summary>
	public const short DD2LightningBug = 111;
	/// <summary>
	/// Used by: <see cref="FairyCritterPink"/>, <see cref="FairyCritterGreen"/>, <see cref="FairyCritterBlue"/>
	/// </summary>
	public const short Fairy = 112;
	/// <summary>
	/// Used by: <see cref="WindyBalloon"/>
	/// </summary>
	public const short Balloon = 113;
	/// <summary>
	/// Used by: <see cref="BlackDragonfly"/>, <see cref="BlueDragonfly"/>, <see cref="GreenDragonfly"/>, <see cref="OrangeDragonfly"/>, <see cref="RedDragonfly"/>, <see cref="YellowDragonfly"/>, <see cref="GoldDragonfly"/>
	/// </summary>
	public const short Dragonfly = 114;
	/// <summary>
	/// Used by: <see cref="LadyBug"/>, <see cref="GoldLadyBug"/>, <see cref="Stinkbug"/>
	/// </summary>
	public const short Ladybug = 115;
	/// <summary>
	/// Used by: <see cref="NPCID.WaterStrider"/>, <see cref="GoldWaterStrider"/>
	/// </summary>
	public const short WaterStrider = 116;
	/// <summary>
	/// Used by: <see cref="BloodNautilus"/>
	/// </summary>
	public const short Dreadnautilus = 117;
	/// <summary>
	/// Used by: <see cref="NPCID.Seahorse"/>, <see cref="GoldSeahorse"/>
	/// </summary>
	public const short Seahorse = 118;
	/// <summary>
	/// Used by: <see cref="Dandelion"/>
	/// </summary>
	public const short AngryDandelion = 119;
	/// <summary>
	/// Used by: <see cref="HallowBoss"/>
	/// </summary>
	public const short EmpressOfLight = 120;
	/// <summary>
	/// Used by: <see cref="QueenSlimeBoss"/>
	/// </summary>
	public const short QueenSlime = 121;
	/// <summary>
	/// Used by: <see cref="PirateGhost"/>
	/// </summary>
	public const short PiratesCurse = 122;
}
