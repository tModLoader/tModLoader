using System;
using System.Reflection;
using Terraria.Audio;
using static Terraria.Audio.SoundLimitBehavior;
using static Terraria.ID.SoundID.SoundStyleDefaults;

#pragma warning disable IDE0047 // Parentheses can be removed.

namespace Terraria.ID;

partial class SoundID
{
	partial struct SoundStyleDefaults
	{
		// Defaults had to be moved out of SoundID to ensure that their fields are initialized before SoundID's.
		public static readonly SoundStyleDefaults ItemDefaults = new(1f, 0.12f); // Pitch variance is now 'max - min', not a half.
		public static readonly SoundStyleDefaults NPCHitDefaults = new(1f, 0.2f);
		public static readonly SoundStyleDefaults NPCDeathDefaults = new(1f, 0.2f);
		public static readonly SoundStyleDefaults ZombieDefaults = new(1f, 0.2f);
	}

	private const string Prefix = "Terraria/Sounds/";
	private const string PrefixCustom = "Terraria/Sounds/Custom/";

	// Start of IDs that used to be 'int' typed.
	public static readonly SoundStyle Dig = new($"{Prefix}Dig_", 0, 3) { PitchVariance = 0.2f, LimitsArePerVariant = true };
	public static readonly SoundStyle PlayerHit = new($"{Prefix}Player_Hit_", 0, 3) { LimitsArePerVariant = true };
	public static readonly SoundStyle Item = new($"{Prefix}Item_");
	//public static readonly SoundStyle NPCHit = new($"{Prefix}NPC_Hit");
	//public static readonly SoundStyle NPCKilled = new($"{Prefix}NPC_Killed");
	public static readonly SoundStyle PlayerKilled = new($"{Prefix}Player_Killed");
	public static readonly SoundStyle Grass = new($"{Prefix}Grass") { PitchVariance = 0.6f };
	public static readonly SoundStyle Grab = new($"{Prefix}Grab") { PitchVariance = 0.2f };
	public static readonly SoundStyle DoorOpen = new($"{Prefix}Door_Opened") { PitchVariance = 0.4f };
	public static readonly SoundStyle DoorClosed = new($"{Prefix}Door_Closed") { PitchVariance = 0.4f };
	public static readonly SoundStyle MenuOpen = new($"{Prefix}Menu_Open");
	public static readonly SoundStyle MenuClose = new($"{Prefix}Menu_Close");
	public static readonly SoundStyle MenuTick = new($"{Prefix}Menu_Tick") { PlayOnlyIfFocused = true };
	public static readonly SoundStyle Shatter = new($"{Prefix}Shatter");
	public static readonly SoundStyle ZombieMoan = new($"{Prefix}Zombie_", 0, 3) { Identifier = "Terraria/ZombieMoan", Volume = 0.4f, MaxInstances = 0 };
	public static readonly SoundStyle SandShark = new($"{Prefix}Zombie_7") { Volume = 0.4f, MaxInstances = 0 }; // New field
	public static readonly SoundStyle BloodZombie = new($"{Prefix}Zombie_", 21, 3) { Identifier = "Terraria/BloodZombie", Volume = 0.4f, MaxInstances = 0 }; // New field
	public static readonly SoundStyle Roar = new($"{Prefix}Roar_0") { Identifier = "Terraria/Roar", SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle WormDig = new($"{Prefix}Roar_1") { SoundLimitBehavior = IgnoreNew }; // New field
	public static readonly SoundStyle WormDigQuiet = WormDig with { Volume = 0.25f }; // New field
	public static readonly SoundStyle ScaryScream = new($"{Prefix}Roar_2") { SoundLimitBehavior = IgnoreNew }; // New field
	public static readonly SoundStyle DoubleJump = new($"{Prefix}Double_Jump") { PitchVariance = 0.2f };
	public static readonly SoundStyle Run = new($"{Prefix}Run") { PitchVariance = 0.2f };
	public static readonly SoundStyle Coins = new($"{Prefix}Coins") { MaxInstances = 0 };
	public static readonly SoundStyle Splash = new($"{Prefix}Splash_0") { PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle SplashWeak = new($"{Prefix}Splash_1") { PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	// Adjust these names if you have better ideas
	public static readonly SoundStyle Shimmer1 = new($"{Prefix}Splash_2") { Volume = 0.75f, PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Shimmer2 = new($"{Prefix}Splash_3") { Volume = 0.75f, PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle ShimmerWeak1 = new($"{Prefix}Splash_4") { Identifier = "Terraria/ShimmerWeak", Volume = 0.75f, Pitch = -0.1f, PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle ShimmerWeak2 = new($"{Prefix}Splash_5") { Identifier = "Terraria/ShimmerWeak", Volume = 0.75f, Pitch = -0.1f, PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle FemaleHit = new($"{Prefix}Female_Hit_", 0, 3) { LimitsArePerVariant = true };
	public static readonly SoundStyle Tink = new($"{Prefix}Tink_", 0, 3) { LimitsArePerVariant = true };
	public static readonly SoundStyle Unlock = new($"{Prefix}Unlock");
	public static readonly SoundStyle Drown = new($"{Prefix}Drown");
	public static readonly SoundStyle Chat = new($"{Prefix}Chat") { MaxInstances = 0 };
	public static readonly SoundStyle MaxMana = new($"{Prefix}MaxMana") { MaxInstances = 0 };
	public static readonly SoundStyle Mummy = new($"{Prefix}Zombie_", 3, 2) { Identifier = "Terraria/Mummy", Volume = 0.9f, PitchVariance = 0.2f, MaxInstances = 0 };
	public static readonly SoundStyle Pixie = new($"{Prefix}Pixie") { PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew }; // Not yet handled: should adjust pitch/pan/volume of playing instance rather than just IgnoreNew, but even that will sound bad with multiple sources playing the sound so no need to fix yet.
	public static readonly SoundStyle Mech = new($"{Prefix}Mech_0") { PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };

	//public static readonly SoundStyle Zombie = new($"{Prefix}Zombie_", 3, 2);
	
	// There is a 1 in 300 chance for a duck to play an easter egg sound variant.
	public static readonly SoundStyle Duck = new($"{Prefix}Zombie_", stackalloc (int, float)[] {
		(10, (299f / 300f) * (1f / 2f)),
		(11, (299f / 300f) * (1f / 2f)),
		(12, (1.0f / 300f)),
	}, SoundType.Ambient) {
		Identifier = "Terraria/Duck",
		Volume = 0.75f,
		PitchRange = (-0.7f, 0.0f),
		MaxInstances = 0
	}; // Not yet handled: PitchRange should be (-0.4, 0.2) for variant 12 and should also be IgnoreNew.
	public static readonly SoundStyle Frog = new($"{Prefix}Zombie_13", SoundType.Ambient) { Volume = 0.35f, PitchRange = (-0.4f, 0.2f), MaxInstances = 0 };
	public static readonly SoundStyle Bird = new($"{Prefix}Zombie_", [14, 16, 17, 18, 19], SoundType.Ambient) { Identifier = "Terraria/Bird", Volume = 0.15f, PitchRange = (-0.7f, 0.26f), SoundLimitBehavior = IgnoreNew, LimitsArePerVariant = true };
	public static readonly SoundStyle Bird14 = Bird with { Variants = [14] };
	public static readonly SoundStyle Bird16 = Bird with { Variants = [16] };
	public static readonly SoundStyle Bird17 = Bird with { Variants = [17] };
	public static readonly SoundStyle Bird18 = Bird with { Variants = [18] };
	public static readonly SoundStyle Bird19 = Bird with { Variants = [19] };
	public static readonly SoundStyle Critter = new($"{Prefix}Zombie_15", SoundType.Ambient) { Volume = 0.2f, PitchRange = (-0.1f, 0.3f), SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Waterfall = new($"{Prefix}Liquid_0", SoundType.Ambient) { Volume = 0.2f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Lavafall = new($"{Prefix}Liquid_1", SoundType.Ambient) { Volume = 0.65f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle ForceRoar = new($"{Prefix}Roar_0") { Identifier = "Terraria/Roar", MaxInstances = 0 };
	public static readonly SoundStyle ForceRoarPitched = ForceRoar with { Pitch = 0.6f };
	public static readonly SoundStyle Meowmere = new($"{Prefix}Item_", 57, 2) { Identifier = "Terraria/Meowmere", PitchVariance = 0.8f, MaxInstances = 0 };
	public static readonly SoundStyle CoinPickup = new($"{Prefix}Coin_", 0, 5) { PitchVariance = 0.16f, MaxInstances = 0 };
	public static readonly SoundStyle Drip = new($"{Prefix}Drip_", 0, 2, SoundType.Ambient) { Volume = 0.5f, PitchVariance = 0.6f, MaxInstances = 0 };
	public static readonly SoundStyle DripSplash = new($"{Prefix}Drip_2", SoundType.Ambient) { Volume = 0.5f, PitchVariance = 0.6f, MaxInstances = 0 };
	public static readonly SoundStyle Camera = new($"{Prefix}Camera");
	// Note: Vanilla logic has special distance falloff rules, but this sound is unused.
	public static readonly SoundStyle MoonLord = new($"{Prefix}NPC_Killed_10") { PitchVariance = 0.2f, MaxInstances = 0 };
	public static readonly SoundStyle Thunder = new($"{Prefix}Thunder_", 0, 7, SoundType.Ambient) { PitchVariance = 0.2f, RerollAttempts = 6, LimitsArePerVariant = true };
	public static readonly SoundStyle Seagull = new($"{Prefix}Zombie_", 106, 3, SoundType.Ambient) { Identifier = "Terraria/Seagull", Volume = 0.2f, PitchRange = (-0.7f, 0f), MaxInstances = 0 };
	public static readonly SoundStyle Dolphin = new($"{Prefix}Zombie_109", SoundType.Ambient) { Volume = 0.3f, PitchVariance = 0.2f, SoundLimitBehavior = IgnoreNew };
	// There is a 1 in 300 chance for an owl to play one of 3 easter egg sound variants.
	public static readonly SoundStyle Owl = new($"{Prefix}Zombie_", stackalloc (int, float)[] {
		(110, (299f / 300f) * (1f / 2f)),
		(111, (299f / 300f) * (1f / 2f)),
		(112, (1.0f / 300f) * (1f / 3f)),
		(113, (1.0f / 300f) * (1f / 3f)),
		(114, (1.0f / 300f) * (1f / 3f)),
	}, SoundType.Ambient) {
		Identifier = "Terraria/Owl",
		Volume = 0.9f,
		PitchVariance = 0.2f,
		SoundLimitBehavior = IgnoreNew,
	};
	public static readonly SoundStyle GuitarC = new($"{Prefix}Item_133") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle GuitarD = new($"{Prefix}Item_134") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle GuitarEm = new($"{Prefix}Item_135") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle GuitarG = new($"{Prefix}Item_136") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle GuitarBm = new($"{Prefix}Item_137") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle GuitarAm = new($"{Prefix}Item_138") { Volume = 0.45f, Identifier = "Terraria/Guitar" };
	public static readonly SoundStyle DrumHiHat = new($"{Prefix}Item_139") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumTomHigh = new($"{Prefix}Item_140") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumTomLow = new($"{Prefix}Item_141") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumTomMid = new($"{Prefix}Item_142") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumClosedHiHat = new($"{Prefix}Item_143") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumCymbal1 = new($"{Prefix}Item_144") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumCymbal2 = new($"{Prefix}Item_145") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumKick = new($"{Prefix}Item_146") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumTamaSnare = new($"{Prefix}Item_147") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle DrumFloorTom = new($"{Prefix}Item_148") { Volume = 0.7f, Identifier = "Terraria/Drums" };
	public static readonly SoundStyle Research = new($"{Prefix}Research_", 1, 3) { LimitsArePerVariant = true };
	public static readonly SoundStyle ResearchComplete = new($"{Prefix}Research_0");
	public static readonly SoundStyle QueenSlime = new($"{Prefix}Zombie_", 115, 3) { Identifier = "Terraria/QueenSlime", Volume = 0.5f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Clown = new($"{Prefix}Zombie_", 121, 3) { Identifier = "Terraria/Clown", Volume = 0.45f, PitchVariance = 0.3f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Cockatiel = new($"{Prefix}Zombie_", 118, 3, SoundType.Ambient) { Identifier = "Terraria/Cockatiel", Volume = 0.3f, PitchVariance = 0.1f, SoundLimitBehavior = IgnoreNew, LimitsArePerVariant = true };
	public static readonly SoundStyle Macaw = new($"{Prefix}Zombie_", 126, 3, SoundType.Ambient) { Identifier = "Terraria/Macaw", Volume = 0.22f, PitchVariance = 0.1f, SoundLimitBehavior = IgnoreNew, LimitsArePerVariant = true };
	public static readonly SoundStyle Toucan = new($"{Prefix}Zombie_", 129, 2, SoundType.Ambient) { Identifier = "Terraria/Toucan", Volume = 0.2f, PitchVariance = 0.1f, SoundLimitBehavior = IgnoreNew, LimitsArePerVariant = true };
	// End of replaced IDs.

	public static readonly SoundStyle NPCHit1 = NPCHitSound(1);
	public static readonly SoundStyle NPCHit2 = NPCHitSound(2);
	public static readonly SoundStyle NPCHit3 = NPCHitSound(3);
	public static readonly SoundStyle NPCHit4 = NPCHitSound(4);
	public static readonly SoundStyle NPCHit5 = NPCHitSound(5);
	public static readonly SoundStyle NPCHit6 = NPCHitSound(6);
	public static readonly SoundStyle NPCHit7 = NPCHitSound(7);
	public static readonly SoundStyle NPCHit8 = NPCHitSound(8);
	public static readonly SoundStyle NPCHit9 = NPCHitSound(9);
	public static readonly SoundStyle NPCHit10 = NPCHitSound(10);
	public static readonly SoundStyle NPCHit11 = NPCHitSound(11);
	public static readonly SoundStyle NPCHit12 = NPCHitSound(12);
	public static readonly SoundStyle NPCHit13 = NPCHitSound(13);
	public static readonly SoundStyle NPCHit14 = NPCHitSound(14);
	public static readonly SoundStyle NPCHit15 = NPCHitSound(15);
	public static readonly SoundStyle NPCHit16 = NPCHitSound(16);
	public static readonly SoundStyle NPCHit17 = NPCHitSound(17);
	public static readonly SoundStyle NPCHit18 = NPCHitSound(18);
	public static readonly SoundStyle NPCHit19 = NPCHitSound(19);
	public static readonly SoundStyle NPCHit20 = NPCHitSound(20) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit21 = NPCHitSound(21) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit22 = NPCHitSound(22) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit23 = NPCHitSound(23) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit24 = NPCHitSound(24) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit25 = NPCHitSound(25) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit26 = NPCHitSound(26) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit27 = NPCHitSound(27) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit28 = NPCHitSound(28) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit29 = NPCHitSound(29) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit30 = NPCHitSound(30) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit31 = NPCHitSound(31) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit32 = NPCHitSound(32) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit33 = NPCHitSound(33) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit34 = NPCHitSound(34) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit35 = NPCHitSound(35) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit36 = NPCHitSound(36) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit37 = NPCHitSound(37) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit38 = NPCHitSound(38) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit39 = NPCHitSound(39) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit40 = NPCHitSound(40) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit41 = NPCHitSound(41) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit42 = NPCHitSound(42) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit43 = NPCHitSound(43) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit44 = NPCHitSound(44) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit45 = NPCHitSound(45) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit46 = NPCHitSound(46) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit47 = NPCHitSound(47) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit48 = NPCHitSound(48) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit49 = NPCHitSound(49) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit50 = NPCHitSound(50) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit51 = NPCHitSound(51) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit52 = NPCHitSound(52) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit53 = NPCHitSound(53) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit54 = NPCHitSound(54) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit55 = NPCHitSound(55) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit56 = NPCHitSound(56) with { Volume = 0.5f };
	public static readonly SoundStyle NPCHit57 = NPCHitSound(57) with { Volume = 0.6f, SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle NPCDeath1 = NPCDeathSound(1);
	public static readonly SoundStyle NPCDeath2 = NPCDeathSound(2);
	public static readonly SoundStyle NPCDeath3 = NPCDeathSound(3);
	public static readonly SoundStyle NPCDeath4 = NPCDeathSound(4);
	public static readonly SoundStyle NPCDeath5 = NPCDeathSound(5);
	public static readonly SoundStyle NPCDeath6 = NPCDeathSound(6);
	public static readonly SoundStyle NPCDeath7 = NPCDeathSound(7);
	public static readonly SoundStyle NPCDeath8 = NPCDeathSound(8);
	public static readonly SoundStyle NPCDeath9 = NPCDeathSound(9);
	public static readonly SoundStyle NPCDeath10 = NPCDeathSound(10) with { SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle NPCDeath11 = NPCDeathSound(11);
	public static readonly SoundStyle NPCDeath12 = NPCDeathSound(12);
	public static readonly SoundStyle NPCDeath13 = NPCDeathSound(13);
	public static readonly SoundStyle NPCDeath14 = NPCDeathSound(14);
	public static readonly SoundStyle NPCDeath15 = NPCDeathSound(15);
	public static readonly SoundStyle NPCDeath16 = NPCDeathSound(16);
	public static readonly SoundStyle NPCDeath17 = NPCDeathSound(17);
	public static readonly SoundStyle NPCDeath18 = NPCDeathSound(18);
	public static readonly SoundStyle NPCDeath19 = NPCDeathSound(19);
	public static readonly SoundStyle NPCDeath20 = NPCDeathSound(20);
	public static readonly SoundStyle NPCDeath21 = NPCDeathSound(21);
	public static readonly SoundStyle NPCDeath22 = NPCDeathSound(22);
	public static readonly SoundStyle NPCDeath23 = NPCDeathSound(23) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath24 = NPCDeathSound(24) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath25 = NPCDeathSound(25) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath26 = NPCDeathSound(26) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath27 = NPCDeathSound(27) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath28 = NPCDeathSound(28) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath29 = NPCDeathSound(29) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath30 = NPCDeathSound(30) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath31 = NPCDeathSound(31) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath32 = NPCDeathSound(32) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath33 = NPCDeathSound(33) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath34 = NPCDeathSound(34) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath35 = NPCDeathSound(35) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath36 = NPCDeathSound(36) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath37 = NPCDeathSound(37) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath38 = NPCDeathSound(38) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath39 = NPCDeathSound(39) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath40 = NPCDeathSound(40) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath41 = NPCDeathSound(41) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath42 = NPCDeathSound(42) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath43 = NPCDeathSound(43) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath44 = NPCDeathSound(44) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath45 = NPCDeathSound(45) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath46 = NPCDeathSound(46) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath47 = NPCDeathSound(47) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath48 = NPCDeathSound(48) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath49 = NPCDeathSound(49) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath50 = NPCDeathSound(50) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath51 = NPCDeathSound(51) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath52 = NPCDeathSound(52) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath53 = NPCDeathSound(53) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath54 = NPCDeathSound(54) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath55 = NPCDeathSound(55) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath56 = NPCDeathSound(56) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath57 = NPCDeathSound(57) with { Volume = 0.5f };
	public static readonly SoundStyle NPCDeath58 = NPCDeathSound(58);
	public static readonly SoundStyle NPCDeath59 = NPCDeathSound(59);
	public static readonly SoundStyle NPCDeath60 = NPCDeathSound(60);
	public static readonly SoundStyle NPCDeath61 = NPCDeathSound(61) with { Volume = 0.6f };
	public static readonly SoundStyle NPCDeath62 = NPCDeathSound(62) with { Volume = 0.6f };
	public static readonly SoundStyle NPCDeath63 = NPCDeathSound(63);
	public static readonly SoundStyle NPCDeath64 = NPCDeathSound(64);
	public static readonly SoundStyle NPCDeath65 = NPCDeathSound(65);
	public static readonly SoundStyle NPCDeath66 = NPCDeathSound(66);
	public static readonly SoundStyle Item1 = ItemSound(stackalloc int[] { 1, 18, 19 });
	public static readonly SoundStyle Item2 = ItemSound(2);
	public static readonly SoundStyle Item3 = ItemSound(3);
	public static readonly SoundStyle Item4 = ItemSound(4);
	public static readonly SoundStyle Item5 = ItemSound(5);
	public static readonly SoundStyle Item6 = ItemSound(6);
	public static readonly SoundStyle Item7 = ItemSound(7);
	public static readonly SoundStyle Item8 = ItemSound(8);
	public static readonly SoundStyle Item9 = ItemSound(9) with { MaxInstances = 0 };
	public static readonly SoundStyle Item10 = ItemSound(10) with { MaxInstances = 0 };
	public static readonly SoundStyle Item11 = ItemSound(11);
	public static readonly SoundStyle Item12 = ItemSound(12);
	public static readonly SoundStyle Item13 = ItemSound(13);
	public static readonly SoundStyle Item14 = ItemSound(14);
	public static readonly SoundStyle Item15 = ItemSound(15);
	public static readonly SoundStyle Item16 = ItemSound(16);
	public static readonly SoundStyle Item17 = ItemSound(17);
	public static readonly SoundStyle Item18 = ItemSound(18);
	public static readonly SoundStyle Item19 = ItemSound(19);
	public static readonly SoundStyle Item20 = ItemSound(20);
	public static readonly SoundStyle Item21 = ItemSound(21);
	public static readonly SoundStyle Item22 = ItemSound(22);
	public static readonly SoundStyle Item23 = ItemSound(23);
	public static readonly SoundStyle Item24 = ItemSound(24) with { MaxInstances = 0 };
	public static readonly SoundStyle Item25 = ItemSound(25);
	public static readonly SoundStyle Item26 = ItemSound(26) with { Volume = 0.75f, PitchVariance = 0f, UsesMusicPitch = true, MaxInstances = 0 };
	public static readonly SoundStyle Item27 = ItemSound(27);
	public static readonly SoundStyle Item28 = ItemSound(28);
	public static readonly SoundStyle Item29 = ItemSound(29);
	public static readonly SoundStyle Item30 = ItemSound(30);
	public static readonly SoundStyle Item31 = ItemSound(31);
	public static readonly SoundStyle Item32 = ItemSound(32);
	public static readonly SoundStyle Item33 = ItemSound(33);
	public static readonly SoundStyle Item34 = ItemSound(34) with { MaxInstances = 0 };
	public static readonly SoundStyle Item35 = ItemSound(35) with { Volume = 0.75f, PitchVariance = 0f, UsesMusicPitch = true };
	public static readonly SoundStyle Item36 = ItemSound(36);
	public static readonly SoundStyle Item37 = ItemSound(37) with { Volume = 0.5f };
	public static readonly SoundStyle Item38 = ItemSound(38);
	public static readonly SoundStyle Item39 = ItemSound(39);
	public static readonly SoundStyle Item40 = ItemSound(40);
	public static readonly SoundStyle Item41 = ItemSound(41);
	public static readonly SoundStyle Item42 = ItemSound(42);
	public static readonly SoundStyle Item43 = ItemSound(43) with { MaxInstances = 0 };
	public static readonly SoundStyle Item44 = ItemSound(44);
	public static readonly SoundStyle Item45 = ItemSound(45);
	public static readonly SoundStyle Item46 = ItemSound(46);
	public static readonly SoundStyle Item47 = ItemSound(47) with { Volume = 0.75f, PitchVariance = 0f, UsesMusicPitch = true };
	public static readonly SoundStyle Item48 = ItemSound(48);
	public static readonly SoundStyle Item49 = ItemSound(49);
	public static readonly SoundStyle Item50 = ItemSound(50);
	public static readonly SoundStyle Item51 = ItemSound(51);
	public static readonly SoundStyle Item52 = ItemSound(52) with { Volume = 0.35f };
	public static readonly SoundStyle Item53 = ItemSound(53) with { Volume = 0.75f, PitchRange = (-0.4f, -0.2f), SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Item54 = ItemSound(54);
	public static readonly SoundStyle Item55 = ItemSound(55) with { Volume = 0.75f * 0.75f, PitchRange = (0.2f, 0.4f), SoundLimitBehavior = IgnoreNew };
	public static readonly SoundStyle Item56 = ItemSound(56);
	public static readonly SoundStyle Item57 = ItemSound(57);
	public static readonly SoundStyle Item58 = ItemSound(58);
	public static readonly SoundStyle Item59 = ItemSound(59);
	public static readonly SoundStyle Item60 = ItemSound(60);
	public static readonly SoundStyle Item61 = ItemSound(61);
	public static readonly SoundStyle Item62 = ItemSound(62);
	public static readonly SoundStyle Item63 = ItemSound(63);
	public static readonly SoundStyle Item64 = ItemSound(64);
	public static readonly SoundStyle Item65 = ItemSound(65);
	public static readonly SoundStyle Item66 = ItemSound(66);
	public static readonly SoundStyle Item67 = ItemSound(67);
	public static readonly SoundStyle Item68 = ItemSound(68);
	public static readonly SoundStyle Item69 = ItemSound(69);
	public static readonly SoundStyle Item70 = ItemSound(70);
	public static readonly SoundStyle Item71 = ItemSound(71);
	public static readonly SoundStyle Item72 = ItemSound(72);
	public static readonly SoundStyle Item73 = ItemSound(73);
	public static readonly SoundStyle Item74 = ItemSound(74);
	public static readonly SoundStyle Item75 = ItemSound(75);
	public static readonly SoundStyle Item76 = ItemSound(76);
	public static readonly SoundStyle Item77 = ItemSound(77);
	public static readonly SoundStyle Item78 = ItemSound(78);
	public static readonly SoundStyle Item79 = ItemSound(79);
	public static readonly SoundStyle Item80 = ItemSound(80);
	public static readonly SoundStyle Item81 = ItemSound(81);
	public static readonly SoundStyle Item82 = ItemSound(82);
	public static readonly SoundStyle Item83 = ItemSound(83);
	public static readonly SoundStyle Item84 = ItemSound(84);
	public static readonly SoundStyle Item85 = ItemSound(85);
	public static readonly SoundStyle Item86 = ItemSound(86);
	public static readonly SoundStyle Item87 = ItemSound(87);
	public static readonly SoundStyle Item88 = ItemSound(88);
	public static readonly SoundStyle Item89 = ItemSound(89);
	public static readonly SoundStyle Item90 = ItemSound(90);
	public static readonly SoundStyle Item91 = ItemSound(91);
	public static readonly SoundStyle Item92 = ItemSound(92);
	public static readonly SoundStyle Item93 = ItemSound(93);
	public static readonly SoundStyle Item94 = ItemSound(94);
	public static readonly SoundStyle Item95 = ItemSound(95);
	public static readonly SoundStyle Item96 = ItemSound(96);
	public static readonly SoundStyle Item97 = ItemSound(97);
	public static readonly SoundStyle Item98 = ItemSound(98);
	public static readonly SoundStyle Item99 = ItemSound(99);
	public static readonly SoundStyle Item100 = ItemSound(100);
	public static readonly SoundStyle Item101 = ItemSound(101);
	public static readonly SoundStyle Item102 = ItemSound(102);
	public static readonly SoundStyle Item103 = ItemSound(103) with { MaxInstances = 0 };
	public static readonly SoundStyle Item104 = ItemSound(104);
	public static readonly SoundStyle Item105 = ItemSound(105);
	public static readonly SoundStyle Item106 = ItemSound(106);
	public static readonly SoundStyle Item107 = ItemSound(107);
	public static readonly SoundStyle Item108 = ItemSound(108);
	public static readonly SoundStyle Item109 = ItemSound(109);
	public static readonly SoundStyle Item110 = ItemSound(110);
	public static readonly SoundStyle Item111 = ItemSound(111);
	public static readonly SoundStyle Item112 = ItemSound(112);
	public static readonly SoundStyle Item113 = ItemSound(113);
	public static readonly SoundStyle Item114 = ItemSound(114);
	public static readonly SoundStyle Item115 = ItemSound(115);
	public static readonly SoundStyle Item116 = ItemSound(116) with { Volume = 0.5f };
	public static readonly SoundStyle Item117 = ItemSound(117);
	public static readonly SoundStyle Item118 = ItemSound(118);
	public static readonly SoundStyle Item119 = ItemSound(119);
	public static readonly SoundStyle Item120 = ItemSound(120);
	public static readonly SoundStyle Item121 = ItemSound(121);
	public static readonly SoundStyle Item122 = ItemSound(122);
	public static readonly SoundStyle Item123 = ItemSound(123) with { Volume = 0.5f };
	public static readonly SoundStyle Item124 = ItemSound(124) with { Volume = 0.65f };
	public static readonly SoundStyle Item125 = ItemSound(125) with { Volume = 0.65f };
	public static readonly SoundStyle Item126 = ItemSound(126);
	public static readonly SoundStyle Item127 = ItemSound(127);
	public static readonly SoundStyle Item128 = ItemSound(128);
	public static readonly SoundStyle Item129 = ItemSound(129) with { Volume = 0.6f };
	public static readonly SoundStyle Item130 = ItemSound(130);
	public static readonly SoundStyle Item131 = ItemSound(131);
	public static readonly SoundStyle Item132 = ItemSound(132) with { PitchVariance = 0.04f };
	public static readonly SoundStyle Item133 = ItemSound(133);
	public static readonly SoundStyle Item134 = ItemSound(134);
	public static readonly SoundStyle Item135 = ItemSound(135);
	public static readonly SoundStyle Item136 = ItemSound(136);
	public static readonly SoundStyle Item137 = ItemSound(137);
	public static readonly SoundStyle Item138 = ItemSound(138);
	public static readonly SoundStyle Item139 = ItemSound(139);
	public static readonly SoundStyle Item140 = ItemSound(140);
	public static readonly SoundStyle Item141 = ItemSound(141);
	public static readonly SoundStyle Item142 = ItemSound(142);
	public static readonly SoundStyle Item143 = ItemSound(143);
	public static readonly SoundStyle Item144 = ItemSound(144);
	public static readonly SoundStyle Item145 = ItemSound(145);
	public static readonly SoundStyle Item146 = ItemSound(146);
	public static readonly SoundStyle Item147 = ItemSound(147);
	public static readonly SoundStyle Item148 = ItemSound(148);
	public static readonly SoundStyle Item149 = ItemSound(149);
	public static readonly SoundStyle Item150 = ItemSound(150);
	public static readonly SoundStyle Item151 = ItemSound(151);
	public static readonly SoundStyle Item152 = ItemSound(152);
	public static readonly SoundStyle Item153 = ItemSound(153) with { PitchVariance = 0.3f };
	public static readonly SoundStyle Item154 = ItemSound(154);
	public static readonly SoundStyle Item155 = ItemSound(155);
	public static readonly SoundStyle Item156 = ItemSound(156) with { Volume = 0.6f, PitchVariance = 0.2f, MaxInstances = 0 };
	public static readonly SoundStyle Item157 = ItemSound(157) with { Volume = 0.7f };
	public static readonly SoundStyle Item158 = ItemSound(158) with { Volume = 0.8f };
	public static readonly SoundStyle Item159 = ItemSound(159) with { Volume = 0.75f, SoundLimitBehavior = IgnoreNew, };
	public static readonly SoundStyle Item160 = ItemSound(160);
	public static readonly SoundStyle Item161 = ItemSound(161);
	public static readonly SoundStyle Item162 = ItemSound(162) with { MaxInstances = 0 };
	public static readonly SoundStyle Item163 = ItemSound(163);
	public static readonly SoundStyle Item164 = ItemSound(164);
	public static readonly SoundStyle Item165 = ItemSound(165);
	public static readonly SoundStyle Item166 = ItemSound(166);
	public static readonly SoundStyle Item167 = ItemSound(167);
	public static readonly SoundStyle Item168 = ItemSound(168);
	public static readonly SoundStyle Item169 = ItemSound(169) with { Pitch = -0.8f };
	// 1.4.4 and onwards:
	public static readonly SoundStyle Item170 = ItemSound(170);
	public static readonly SoundStyle Item171 = ItemSound(171);
	public static readonly SoundStyle Item172 = ItemSound(172);
	public static readonly SoundStyle Item173 = ItemSound(173);
	public static readonly SoundStyle Item174 = ItemSound(174);
	public static readonly SoundStyle Item175 = ItemSound(175);
	public static readonly SoundStyle Item176 = ItemSound(176) with { Volume = 0.9f };
	public static readonly SoundStyle Item177 = ItemSound(177);
	public static readonly SoundStyle Item178 = ItemSound(178);
	// ZombieX sound styles are new, and weren't present in vanilla neither as int nor SoundStyle fields. 
	public static readonly SoundStyle Zombie1 = ZombieSound(1);
	public static readonly SoundStyle Zombie2 = ZombieSound(2);
	public static readonly SoundStyle Zombie3 = ZombieSound(3);
	public static readonly SoundStyle Zombie4 = ZombieSound(4);
	public static readonly SoundStyle Zombie5 = ZombieSound(5);
	public static readonly SoundStyle Zombie6 = ZombieSound(6);
	public static readonly SoundStyle Zombie7 = ZombieSound(7);
	public static readonly SoundStyle Zombie8 = ZombieSound(8);
	public static readonly SoundStyle Zombie9 = ZombieSound(9);
	public static readonly SoundStyle Zombie10 = ZombieSound(10);
	public static readonly SoundStyle Zombie11 = ZombieSound(11);
	public static readonly SoundStyle Zombie12 = ZombieSound(12);
	public static readonly SoundStyle Zombie13 = ZombieSound(13);
	public static readonly SoundStyle Zombie14 = ZombieSound(14);
	public static readonly SoundStyle Zombie15 = ZombieSound(15);
	public static readonly SoundStyle Zombie16 = ZombieSound(16);
	public static readonly SoundStyle Zombie17 = ZombieSound(17);
	public static readonly SoundStyle Zombie18 = ZombieSound(18);
	public static readonly SoundStyle Zombie19 = ZombieSound(19);
	public static readonly SoundStyle Zombie20 = ZombieSound(20);
	public static readonly SoundStyle Zombie21 = ZombieSound(21);
	public static readonly SoundStyle Zombie22 = ZombieSound(22);
	public static readonly SoundStyle Zombie23 = ZombieSound(23);
	public static readonly SoundStyle Zombie24 = ZombieSound(24) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie25 = ZombieSound(25) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie26 = ZombieSound(26) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie27 = ZombieSound(27) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie28 = ZombieSound(28) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie29 = ZombieSound(29) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie30 = ZombieSound(30) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie31 = ZombieSound(31) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie32 = ZombieSound(32) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie33 = ZombieSound(33) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie34 = ZombieSound(34) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie35 = ZombieSound(35) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie36 = ZombieSound(36) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie37 = ZombieSound(37) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie38 = ZombieSound(38) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie39 = ZombieSound(39) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie40 = ZombieSound(40) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie41 = ZombieSound(41) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie42 = ZombieSound(42) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie43 = ZombieSound(43) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie44 = ZombieSound(44) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie45 = ZombieSound(45) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie46 = ZombieSound(46) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie47 = ZombieSound(47) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie48 = ZombieSound(48) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie49 = ZombieSound(49) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie50 = ZombieSound(50) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie51 = ZombieSound(51) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie52 = ZombieSound(52) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie53 = ZombieSound(53) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie54 = ZombieSound(54) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie55 = ZombieSound(55) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie56 = ZombieSound(56) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie57 = ZombieSound(57) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie58 = ZombieSound(58) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie59 = ZombieSound(59) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie60 = ZombieSound(60) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie61 = ZombieSound(61) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie62 = ZombieSound(62) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie63 = ZombieSound(63) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie64 = ZombieSound(64) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie65 = ZombieSound(65) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie66 = ZombieSound(66) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie67 = ZombieSound(67) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie68 = ZombieSound(68) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie69 = ZombieSound(69) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie70 = ZombieSound(70) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie71 = ZombieSound(71) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie72 = ZombieSound(72) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie73 = ZombieSound(73) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie74 = ZombieSound(74) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie75 = ZombieSound(75) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie76 = ZombieSound(76) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie77 = ZombieSound(77) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie78 = ZombieSound(78) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie79 = ZombieSound(79) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie80 = ZombieSound(80) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie81 = ZombieSound(81) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie82 = ZombieSound(82) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie83 = ZombieSound(83) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie84 = ZombieSound(84) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie85 = ZombieSound(85) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie86 = ZombieSound(86) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie87 = ZombieSound(87) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie88 = ZombieSound(88) with { Volume = 0.7f };
	public static readonly SoundStyle Zombie89 = ZombieSound(89) with { Volume = 0.7f };
	public static readonly SoundStyle Zombie90 = ZombieSound(90) with { Volume = 0.7f };
	public static readonly SoundStyle Zombie91 = ZombieSound(91) with { Volume = 0.7f };
	public static readonly SoundStyle Zombie92 = ZombieSound(92) with { Volume = 0.5f };
	public static readonly SoundStyle Zombie93 = ZombieSound(93) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie94 = ZombieSound(94) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie95 = ZombieSound(95) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie96 = ZombieSound(96) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie97 = ZombieSound(97) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie98 = ZombieSound(98) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie99 = ZombieSound(99) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie100 = ZombieSound(100) with { Volume = 0.25f };
	public static readonly SoundStyle Zombie101 = ZombieSound(101) with { Volume = 0.25f };
	public static readonly SoundStyle Zombie102 = ZombieSound(102) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie103 = ZombieSound(103) with { Volume = 0.4f };
	public static readonly SoundStyle Zombie104 = ZombieSound(104) with { Volume = 0.55f };
	public static readonly SoundStyle Zombie105 = ZombieSound(105);
	public static readonly SoundStyle Zombie106 = ZombieSound(106);
	public static readonly SoundStyle Zombie107 = ZombieSound(107);
	public static readonly SoundStyle Zombie108 = ZombieSound(108);
	public static readonly SoundStyle Zombie109 = ZombieSound(109);
	public static readonly SoundStyle Zombie110 = ZombieSound(110);
	public static readonly SoundStyle Zombie111 = ZombieSound(111);
	public static readonly SoundStyle Zombie112 = ZombieSound(112);
	public static readonly SoundStyle Zombie113 = ZombieSound(113);
	public static readonly SoundStyle Zombie114 = ZombieSound(114);
	public static readonly SoundStyle Zombie115 = ZombieSound(115);
	public static readonly SoundStyle Zombie116 = ZombieSound(116);
	public static readonly SoundStyle Zombie117 = ZombieSound(117);
	// 1.4.4 and onwards:
	public static readonly SoundStyle Zombie118 = ZombieSound(118);
	public static readonly SoundStyle Zombie119 = ZombieSound(119);
	public static readonly SoundStyle Zombie120 = ZombieSound(120);
	public static readonly SoundStyle Zombie121 = ZombieSound(121);
	public static readonly SoundStyle Zombie122 = ZombieSound(122);
	public static readonly SoundStyle Zombie123 = ZombieSound(123);
	public static readonly SoundStyle Zombie124 = ZombieSound(124);
	public static readonly SoundStyle Zombie125 = ZombieSound(125);
	public static readonly SoundStyle Zombie126 = ZombieSound(126);
	public static readonly SoundStyle Zombie127 = ZombieSound(127);
	public static readonly SoundStyle Zombie128 = ZombieSound(128);
	public static readonly SoundStyle Zombie129 = ZombieSound(129);
	public static readonly SoundStyle Zombie130 = ZombieSound(130);
	public static readonly int ZombieSoundCount = LegacySoundPlayer.ZombieSoundCount; // Added by TML just like ZombieX.

	// Mapping

	private static SoundStyle[][] legacyArrayedStylesMapping = new SoundStyle[LegacySoundIDs.Count][];

	static SoundID()
	{
		FillLegacyArrayedStylesMap();
	}

	internal static bool TryGetLegacyStyle(int type, int style, out SoundStyle result)
	{
		var tempResult = GetLegacyStyle(type, style);

		if (tempResult.HasValue) {
			result = tempResult.Value;
			return true;
		}

		result = default;
		return false;
	}

	private static void FillLegacyArrayedStylesMap()
	{
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

		static void AddNumberedStyles(int type, string baseName, int start, int numStyles) {
			var array = legacyArrayedStylesMapping[type] = new SoundStyle[start + numStyles];

			for (int i = 0; i < numStyles; i++) {
				int ii = start + i;
				
				if (typeof(SoundID).GetField($"{baseName}{ii}", Flags)?.GetValue(null) is SoundStyle soundStyle) {
					array[ii] = soundStyle;
				}
			}
		}

		AddNumberedStyles(LegacySoundIDs.Item, nameof(LegacySoundIDs.Item), 0, ItemSoundCount);
		AddNumberedStyles(LegacySoundIDs.NPCHit, nameof(LegacySoundIDs.NPCHit), 0, NPCHitCount);
		AddNumberedStyles(LegacySoundIDs.NPCKilled, "NPCDeath", 0, NPCDeathCount);
		AddNumberedStyles(LegacySoundIDs.Zombie, nameof(LegacySoundIDs.Zombie), 0, ZombieSoundCount);
		AddNumberedStyles(LegacySoundIDs.Bird, nameof(LegacySoundIDs.Bird), 14, 6);
	}

	// Helper methods

	private static SoundStyle SoundWithDefaults(SoundStyleDefaults defaults, SoundStyle style)
	{
		defaults.Apply(ref style);

		return style;
	}

	private static SoundStyle NPCHitSound(int soundStyle)
		=> SoundWithDefaults(NPCHitDefaults, new($"{Prefix}NPC_Hit_{soundStyle}"));

	private static SoundStyle NPCHitSound(ReadOnlySpan<int> soundStyles)
		=> SoundWithDefaults(NPCHitDefaults, new($"{Prefix}NPC_Hit_", soundStyles));

	private static SoundStyle NPCDeathSound(int soundStyle)
		=> SoundWithDefaults(NPCDeathDefaults, new($"{Prefix}NPC_Killed_{soundStyle}"));

	private static SoundStyle NPCDeathSound(ReadOnlySpan<int> soundStyles)
		=> SoundWithDefaults(NPCDeathDefaults, new($"{Prefix}NPC_Killed_", soundStyles));
	
	private static SoundStyle ItemSound(int soundStyle)
		=> SoundWithDefaults(ItemDefaults, new($"{Prefix}Item_{soundStyle}"));

	private static SoundStyle ItemSound(ReadOnlySpan<int> soundStyles)
		=> SoundWithDefaults(ItemDefaults, new($"{Prefix}Item_", soundStyles));

	private static SoundStyle ZombieSound(int soundStyle)
		=> SoundWithDefaults(ZombieDefaults, new($"{Prefix}Zombie_{soundStyle}")) with { SoundLimitBehavior = IgnoreNew };

	private static SoundStyle ZombieSound(ReadOnlySpan<int> soundStyles)
		=> SoundWithDefaults(ZombieDefaults, new($"{Prefix}Zombie_", soundStyles)) with { SoundLimitBehavior = IgnoreNew };

	// Moved to bottom for its size

	internal static SoundStyle? GetLegacyStyle(int type, int style) => type switch {
		// Arrayed
		LegacySoundIDs.Zombie or
		LegacySoundIDs.Item or
		LegacySoundIDs.NPCHit or
		LegacySoundIDs.NPCKilled or
		LegacySoundIDs.Bird
			=> style >= 1 && style < legacyArrayedStylesMapping[type].Length ? legacyArrayedStylesMapping[type][style] : null,
		// Everything else
		LegacySoundIDs.Dig => Dig,
		LegacySoundIDs.PlayerHit => PlayerHit,
		LegacySoundIDs.PlayerKilled => PlayerKilled,
		LegacySoundIDs.Grass => Grass,
		LegacySoundIDs.Grab => Grab,
		LegacySoundIDs.DoorOpen => DoorOpen,
		LegacySoundIDs.DoorClosed => DoorClosed,
		LegacySoundIDs.MenuOpen => MenuOpen,
		LegacySoundIDs.MenuClose => MenuClose,
		LegacySoundIDs.MenuTick => MenuTick,
		LegacySoundIDs.Shatter => Shatter,
		LegacySoundIDs.ZombieMoan => style switch {
			NPCID.SandShark => SandShark,
			NPCID.BloodZombie or NPCID.ZombieMerman => BloodZombie,
			_ => ZombieMoan,
		},
		LegacySoundIDs.Roar => style switch {
			0 => Roar,
			1 => WormDig,
			2 => ScaryScream,
			4 => WormDigQuiet,
			_ => null,
		},
		LegacySoundIDs.DoubleJump => DoubleJump,
		LegacySoundIDs.Run => Run,
		LegacySoundIDs.Coins => Coins,
		LegacySoundIDs.Splash => style switch {
			1 => SplashWeak,
			2 => Shimmer1,
			3 => Shimmer2,
			4 => ShimmerWeak1,
			5 => ShimmerWeak2,
			_ => Splash
		},
		LegacySoundIDs.FemaleHit => FemaleHit,
		LegacySoundIDs.Tink => Tink,
		LegacySoundIDs.Unlock => Unlock,
		LegacySoundIDs.Drown => Drown,
		LegacySoundIDs.Chat => Chat,
		LegacySoundIDs.MaxMana => MaxMana,
		LegacySoundIDs.Mummy => Mummy,
		LegacySoundIDs.Pixie => Pixie,
		LegacySoundIDs.Mech => Mech,
		LegacySoundIDs.Duck => Duck,
		LegacySoundIDs.Frog => Frog,
		LegacySoundIDs.Critter => Critter,
		LegacySoundIDs.Waterfall => Waterfall,
		LegacySoundIDs.Lavafall => Lavafall,
		LegacySoundIDs.ForceRoar => style switch { -1 => ForceRoarPitched, _ => ForceRoar },
		LegacySoundIDs.Meowmere => Meowmere.WithVolumeScale(!Main.starGame ? style * 0.05f : 0.15f),
		LegacySoundIDs.CoinPickup => CoinPickup.WithVolumeScale(!Main.starGame ? 1.0f : 0.15f),
		LegacySoundIDs.Drip => style switch { 2 => DripSplash, _ => Drip },
		LegacySoundIDs.Camera => Camera,
		LegacySoundIDs.MoonLord => MoonLord,
		//LegacySoundIDs.Trackable => Trackable,
		LegacySoundIDs.Thunder => Thunder,
		LegacySoundIDs.Seagull => Seagull,
		LegacySoundIDs.Dolphin => Dolphin,
		LegacySoundIDs.Owl => Owl,
		LegacySoundIDs.GuitarC => GuitarC,
		LegacySoundIDs.GuitarD => GuitarD,
		LegacySoundIDs.GuitarEm => GuitarEm,
		LegacySoundIDs.GuitarG => GuitarG,
		LegacySoundIDs.GuitarBm => GuitarBm,
		LegacySoundIDs.GuitarAm => GuitarAm,
		LegacySoundIDs.DrumHiHat => DrumHiHat,
		LegacySoundIDs.DrumTomHigh => DrumTomHigh,
		LegacySoundIDs.DrumTomLow => DrumTomLow,
		LegacySoundIDs.DrumTomMid => DrumTomMid,
		LegacySoundIDs.DrumClosedHiHat => DrumClosedHiHat,
		LegacySoundIDs.DrumCymbal1 => DrumCymbal1,
		LegacySoundIDs.DrumCymbal2 => DrumCymbal2,
		LegacySoundIDs.DrumKick => DrumKick,
		LegacySoundIDs.DrumTamaSnare => DrumTamaSnare,
		LegacySoundIDs.DrumFloorTom => DrumFloorTom,
		LegacySoundIDs.Research => Research,
		LegacySoundIDs.ResearchComplete => ResearchComplete,
		LegacySoundIDs.QueenSlime => QueenSlime,
		LegacySoundIDs.Clown => Clown,
		LegacySoundIDs.Cockatiel => Cockatiel,
		LegacySoundIDs.Macaw => Macaw,
		LegacySoundIDs.Toucan => Toucan,
		_ => null,
	};
}
