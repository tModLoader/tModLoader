using Terraria.ModLoader;

namespace Terraria.ID;

public partial class DustID
{
#if !TMLANALYZER
	public static class Sets
	{
		public static SetFactory Factory = new SetFactory(DustLoader.DustCount, nameof(DustID), Search);
	}
#endif

	// Naming is based on the best approximation of the earliest or most popular implementation.
	// There are a few duplicates, unlike other ID classes, since some dust are used for different purposes.
	public const short WoodFurniture = 7;
	public const short HeartCrystal = 12;
	public const short Glass = 13;
	public const short Demonite = 14;
	public const short Corruption = 14;
	public const short MagicMirror = 15;
	public const short Sunflower = 19;
	public const short PurificationPowder = 20;
	public const short VilePowder = 21; // Assumption
	public const short Pot = 22;
	public const short PiggyBank = 23;
	public const short Meteorite = 23;
	public const short CorruptionThorns = 24;
	public const short MeteorHead = 25;
	public const short GoblinSorcerer = 27;
	public const short Clay = 28;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water = 33;
	/// <summary>
	/// Only survives while in liquid
	/// </summary>
	public const short BreatheBubble = 34;
	public const short Obsidian = 37;
	public const short Mud = 38;
	public const short JungleGrass = 39;
	/// <summary>
	/// Wanders and lingers with gravity enabled.
	/// </summary>
	public const short GlowingMushroom = 41;
	public const short Harpy = 42;
	/// <summary>
	/// Wanders and lingers with gravity enabled.
	/// </summary>
	public const short JungleSpore = 44;
	public const short ManaRegeneration = 45;
	public const short Poisoned = 46;
	public const short Cobalt = 48;
	public const short Mythril = 49;
	public const short Adamantite = 50;
	public const short SnowBlock = 51;
	public const short Pearlsand = 51;
	public const short UnholyWater = 52; //??? might need verification
	public const short Silt = 53;
	public const short Wraith = 54;
	public const short Pixie = 55;
	public const short BlueFairy = 56;
	/// <summary>
	/// Stuff like Enchanted Boomerang, Enchanted Sword, and Jester Arrows
	/// </summary>
	public const short Enchanted_Gold = 57;
	public const short HallowedWeapons = 57;
	/// <summary>
	/// Stuff like Enchanted Boomerang, Enchanted Sword, and Jester Arrows
	/// </summary>
	public const short Enchanted_Pink = 58;
	/// <summary>
	/// Lingers and scales up with gravity enabled
	/// </summary>
	public const short RainbowRod = 66;
	public const short IceRod = 67;
	public const short UndergroundHallowedEnemies = 71;
	public const short Gastropod = 72;
	public const short PinkFairy = 73;
	public const short GreenFairy = 74;
	/// <summary>
	/// Wanders and lingers with gravity enabled
	/// </summary>
	public const short Snow = 76;
	public const short Ebonwood = 77;
	public const short RichMahogany = 78;
	public const short Pearlwood = 79;
	public const short UnusedBrown = 85; // ?????, need to dig deeper
	public const short Frost = 92;
	public const short FrostStaff = 92;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Corruption = 98;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Jungle = 99;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Hallowed = 100;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Snow = 101;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Desert = 102;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Space = 103;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_GlowingMushroom = 103;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Cavern = 104;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_BloodMoon = 105;
	public const short RuneWizard = 106;
	public const short TerraBlade = 107;
	public const short RainCloud = 108;
	public const short Asphalt = 109;
	public const short Clentaminator_Green = 110;
	public const short Clentaminator_Cyan = 111;
	public const short Clentaminator_Purple = 112;
	public const short Clentaminator_Blue = 113;
	public const short Clentaminator_Red = 114;
	public const short CrimtaneWeapons = 115;
	public const short Skyware = 116; //Better name may be available
	public const short Crimstone = 117;
	public const short Ice_Purple = 118;
	public const short Ice_Pink = 119;
	public const short Ice_Red = 120;
	public const short Shadewood_Tree = 121;
	public const short BorealWood = 122;
	/// <summary>
	/// Use Dust.dustWater() if you want to calculate water dust based on water style
	/// </summary>
	public const short Water_Crimson = 123;
	public const short SandstormInABottle = 124;
	public const short Shadewood = 126;
	public const short Chlorophyte = 128;
	public const short Rope = 129;
	public const short Firework_Red = 130;
	public const short Firework_Green = 131;
	public const short Firework_Blue = 132;
	public const short Firework_Yellow = 133;
	public const short Firework_Pink = 134;
	public const short Crimslime = 136; // Better name may be available
	public const short IcyMerman = 137; // Better name may be available
	public const short Confetti_Blue = 139;
	public const short Confetti_Green = 140;
	public const short Confetti_Pink = 141;
	public const short Confetti_Yellow = 142;
	/// <summary>
	/// When you paint a tile
	/// </summary>
	public const short Paint = 143;
	public const short Palladium = 144;
	public const short Orichalcum = 145;
	public const short Titanium = 146;
	public const short Hive = 147;
	public const short Lihzahrd = 148;
	public const short Slush = 149;
	public const short Bee = 150;
	public const short SeaSnail = 151;
	public const short Squid = 151;
	public const short Honey2 = 153;
	public const short Rain = 154;
	public const short Ambient_DarkBrown = 155; // Better name may be available
	public const short ChlorophyteWeapon = 157;
	public const short Teleporter = 159;
	public const short MagnetSphere = 160;
	public const short IceGolem = 161;
	public const short HeatRay = 162;
	public const short PoisonStaff = 163; // Better name may be available, at a glance it's implementation was only used for Poison Staff projectile's death dust
	public const short DryadsWard = 163;
	public const short TeleportationPotion = 164;
	public const short FungiHit = 165;
	public const short Plantera_Pink = 166;
	public const short Plantera_Green = 167;
	/// <summary>
	/// Lingers and scales with gravity enabled
	/// </summary>
	public const short PlanteraBulb = 168;
	public const short Ichor = 170;
	public const short Venom = 171;
	public const short DungeonWater = 172;
	public const short ShadowbeamStaff = 173;
	public const short InfernoFork = 174;
	public const short SpectreStaff = 175;
	public const short BubbleBurst_Blue = 176;
	public const short BubbleBurst_Pink = 177;
	public const short BubbleBurst_Green = 178;
	public const short BubbleBurst_Purple = 179;
	public const short DungeonSpirit = 180;
	public const short GiantCursedSkullBolt = 181;
	public const short TheDestroyer = 182;
	public const short VampireHeal = 183;
	public const short ScourgeOfTheCorruptor = 184;
	public const short FrostHydra = 185;
	public const short RedsWingsRun = 186;
	public const short Flare_Blue = 187;
	public const short FartInAJar = 188;
	public const short Pumpkin = 189;
	public const short Hay = 190;
	public const short SpookyWood = 191;
	public const short Ghost = 192;
	public const short BunnySlime = 192;
	public const short SlimeBunny = 193; // Better name may be available
	public const short Scarecrow = 194;
	public const short BatScepter = 195;
	public const short Everscream = 196;
	public const short NorthPole = 197;
	public const short FireflyHit = 198;
	public const short Butterfly = 199;
	public const short Worm = 200;
	public const short Snail = 201;
	public const short GlowingSnail = 202;
	public const short Scorpion = 203;
	public const short TreasureSparkle = 204;
	public const short SpelunkerGlowstickSparkle = 204;
	public const short VenomStaff = 205;
	public const short UnusedWhiteBluePurple = 206; // Need to dig deeper
	public const short DynastyWood = 207;
	public const short DynastyShingle_Red = 208;
	public const short DynastyShingle_Blue = 209;
	public const short DynastyWall = 210;
	public const short Wet = 211;
	public const short BubbleBurst_White = 212;
	public const short MinecartSpark = 213;
	public const short BorealWood_Small = 214;
	public const short PalmWood = 215;
	public const short PirateStaff = 216;
	public const short FishronWings = 217;
	public const short RazorbladeTyphoon = 217;
	public const short Rain_BloodMoon = 218;
	public const short FireworkFountain_Red = 219;
	public const short FireworkFountain_Green = 220;
	public const short FireworkFountain_Blue = 221;
	public const short FireworkFountain_Yellow = 222;
	public const short FireworkFountain_Pink = 223;
	public const short Shiverthorn = 224;
	public const short Coralstone = 225;
	public const short MartianHit = 227;
	public const short MartianSaucerSpark = 228;
	public const short InfluxWaver = 229;
	public const short Phantasmal = 229;
	/// <summary>
	/// Scales with gravity enabled
	/// </summary>
	public const short DrillContainmentUnit = 230;
	/// <summary>
	/// Scales with gravity enabled
	/// </summary>
	public const short CosmicCarKeys = 230;
	public const short ViciousPowder = 231;
	public const short GoldCritter = 232;
	public const short GoldCritter_LessOutline = 233;
	public const short LifeDrain = 235;
	public const short FrostDaggerfish = 252;
	public const short TsunamiInABottle = 253;
	public const short SailfishBoots = 253;
	public const short CrystalSerpent = 254;
	public const short CrystalSerpent_Pink = 255; // Needs verification, ID 255 is cumbersome to easily check
	public const short Toxikarp = 256;
	public const short MechanicalCart = 260;
	public const short BloodWater = 266;
	/// <summary>
	/// Lingers and scales with gravity enabled
	/// </summary>
	public const short LastPrism = 267;
}
