using System;
using Terraria.Audio;
using static Terraria.ID.SoundID.SoundStyleDefaults;

namespace Terraria.ID
{
	partial class SoundID
	{
		partial struct SoundStyleDefaults
		{
			// Defaults had to be moved out of SoundID to ensure that their fields are initialized before SoundID's.
			public static readonly SoundStyleDefaults ItemDefaults = new(1f, 0.12f); // Pitch variance is now 'max - min', not a half.
			public static readonly SoundStyleDefaults NPCHitDefaults = new(1f, 0.2f);
			public static readonly SoundStyleDefaults NPCDeathDefaults = new(1f, 0.2f);
		}

		private const string Prefix = "Terraria/Sounds/";
		private const string PrefixCustom = "Terraria/Sounds/Custom/";

		// Start of IDs that used to be 'int' typed.
		public static readonly SoundStyle Dig = new($"{Prefix}Dig_", 0, 3) { PitchVariance = 0.2f };
		public static readonly SoundStyle PlayerHit = new($"{Prefix}Player_Hit_", 0, 3) { PitchVariance = 0.2f };
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
		public static readonly SoundStyle ZombieMoan = new($"{Prefix}Zombie_", 0, 3) { Volume = 0.4f };
		public static readonly SoundStyle ZombieMoan_SandShark = new($"{Prefix}Zombie_7") { Volume = 0.4f };
		public static readonly SoundStyle ZombieMoan_BloodZombie = new($"{Prefix}Zombie_", 21, 3) { Volume = 0.4f };
		public static readonly SoundStyle Roar = new($"{Prefix}Roar_0") { RestartIfPlaying = false };
		public static readonly SoundStyle Roar_WormDig = new($"{Prefix}Roar_1") { RestartIfPlaying = false };
		//public static readonly SoundStyle Roar_Something = new($"{Prefix}Roar_2") { RestartIfPlaying = false };
		public static readonly SoundStyle DoubleJump = new($"{Prefix}Double_Jump") { PitchVariance = 0.2f };
		public static readonly SoundStyle Run = new($"{Prefix}Run") { PitchVariance = 0.2f };
		public static readonly SoundStyle Coins = new($"{Prefix}Coins");
		public static readonly SoundStyle Splash = new($"{Prefix}Splash_0") { PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly SoundStyle SplashWeak = new($"{Prefix}Splash_1") { PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly SoundStyle FemaleHit = new($"{Prefix}Female_Hit_", 0, 3);
		public static readonly SoundStyle Tink = new($"{Prefix}Tink_", 0, 3);
		public static readonly SoundStyle Unlock = new($"{Prefix}Unlock");
		public static readonly SoundStyle Drown = new($"{Prefix}Drown");
		public static readonly SoundStyle Chat = new($"{Prefix}Chat");
		public static readonly SoundStyle MaxMana = new($"{Prefix}MaxMana");
		public static readonly SoundStyle Mummy = new($"{Prefix}Zombie_", 3, 2) { Volume = 0.9f, PitchVariance = 0.2f };
		public static readonly SoundStyle Pixie = new($"{Prefix}Pixie") { PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly SoundStyle Mech = new($"{Prefix}Mech_0") { PitchVariance = 0.2f, RestartIfPlaying = false };
		//public static readonly SoundStyle Zombie = new($"{Prefix}Zombie_", 3, 2);
		public static readonly SoundStyle Duck = new($"{Prefix}Zombie_", 10, 2) { Volume = 0.75f, PitchRange = (-0.7f, 0.0f) };
		//TODO: This should play with 1 in 300 chance in Duck.
		public static readonly SoundStyle DuckFunny = new($"{Prefix}Zombie_12") { Volume = 0.75f, PitchRange = (-0.4f, 0.2f), RestartIfPlaying = false };
		public static readonly SoundStyle Frog = new($"{Prefix}Zombie_13") { Volume = 0.35f, PitchRange = (-0.4f, 0.2f) };
		//TODO: Inaccurate variants, search & analyze "PlaySound(32," in vanilla src.
		public static readonly SoundStyle Bird = new($"{Prefix}Zombie_", 14, 5) { Volume = 0.15f, PitchRange = (-0.7f, 0.26f), RestartIfPlaying = false };
		public static readonly SoundStyle Critter = new($"{Prefix}Zombie_15") { Volume = 0.2f, PitchRange = (-0.1f, 0.3f), RestartIfPlaying = false };
		public static readonly SoundStyle Waterfall = new($"{Prefix}Liquid_0") { Volume = 0.2f };
		public static readonly SoundStyle Lavafall = new($"{Prefix}Liquid_1") { Volume = 0.65f };
		public static readonly SoundStyle ForceRoar = new($"{Prefix}Roar_0");
		public static readonly SoundStyle ForceRoarPitched = new($"{Prefix}Roar_0") { Pitch = 0.6f };
		public static readonly SoundStyle Meowmere = new($"{Prefix}Item_", 57, 2) { PitchVariance = 0.8f };
		public static readonly SoundStyle CoinPickup = new($"{Prefix}Coin_", 0, 5) { PitchVariance = 0.16f };
		public static readonly SoundStyle Drip = new($"{Prefix}Drip_", 0, 3) { Volume = 0.5f, PitchVariance = 0.6f };
		public static readonly SoundStyle Camera = new($"{Prefix}Camera");
		//TODO: Might need special distance falloff rules.
		public static readonly SoundStyle MoonLord = new($"{Prefix}NPC_Killed_10") { PitchVariance = 0.2f };
		public static readonly SoundStyle Thunder = new($"{Prefix}Thunder_", 0, 7) { MaxInstances = 7, PitchVariance = 0.2f, };
		public static readonly SoundStyle Seagull = new($"{Prefix}Zombie_", 106, 3) { Volume = 0.2f, PitchRange = (-0.7f, 0f) };
		public static readonly SoundStyle Dolphin = new($"{Prefix}Zombie_109") { Volume = 0.3f, PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly SoundStyle Owl = new($"{Prefix}Zombie_", 110, 2) { PitchVariance = 0.2f };
		//TODO: This should play with 1 in 300 chance in Owl.
		public static readonly SoundStyle OwlFunny = new($"{Prefix}Zombie_", 112, 3) { PitchVariance = 0.2f };
		public static readonly SoundStyle GuitarC = new($"{Prefix}Item_47") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle GuitarD = new($"{Prefix}Item_48") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle GuitarEm = new($"{Prefix}Item_49") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle GuitarG = new($"{Prefix}Item_50") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle GuitarAm = new($"{Prefix}Item_51") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle GuitarF = new($"{Prefix}Item_52") { Volume = 0.45f, Group = "Terraria/Guitar" };
		public static readonly SoundStyle DrumHiHat = new($"{Prefix}Item_53") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumTomHigh = new($"{Prefix}Item_54") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumTomLow = new($"{Prefix}Item_55") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumTomMid = new($"{Prefix}Item_56") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumClosedHiHat = new($"{Prefix}Item_57") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumCymbal1 = new($"{Prefix}Item_58") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumCymbal2 = new($"{Prefix}Item_59") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumKick = new($"{Prefix}Item_60") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumTamaSnare = new($"{Prefix}Item_61") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle DrumFloorTom = new($"{Prefix}Item_62") { Volume = 0.7f, Group = "Terraria/Drums" };
		public static readonly SoundStyle Research = new($"{Prefix}Research_", 1, 3);
		public static readonly SoundStyle ResearchComplete = new($"{Prefix}Research_0");
		public static readonly SoundStyle QueenSlime = new($"{Prefix}Zombie_", 115, 3) { Volume = 0.5f, RestartIfPlaying = false };
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
		public static readonly SoundStyle NPCHit57 = NPCHitSound(57) with { Volume = 0.6f, RestartIfPlaying = false };
		public static int NPCHitCount = 58; // Added by tML
		public static readonly SoundStyle NPCDeath1 = NPCDeathSound(1);
		public static readonly SoundStyle NPCDeath2 = NPCDeathSound(2);
		public static readonly SoundStyle NPCDeath3 = NPCDeathSound(3);
		public static readonly SoundStyle NPCDeath4 = NPCDeathSound(4);
		public static readonly SoundStyle NPCDeath5 = NPCDeathSound(5);
		public static readonly SoundStyle NPCDeath6 = NPCDeathSound(6);
		public static readonly SoundStyle NPCDeath7 = NPCDeathSound(7);
		public static readonly SoundStyle NPCDeath8 = NPCDeathSound(8);
		public static readonly SoundStyle NPCDeath9 = NPCDeathSound(9);
		public static readonly SoundStyle NPCDeath10 = NPCDeathSound(10) with { RestartIfPlaying = false };
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
		public static int NPCDeathCount = 66; // TML: Changed from short to int.
		public static readonly SoundStyle Item1 = ItemSound(stackalloc int[] { 1, 18, 19 });
		public static readonly SoundStyle Item2 = ItemSound(2);
		public static readonly SoundStyle Item3 = ItemSound(3);
		public static readonly SoundStyle Item4 = ItemSound(4);
		public static readonly SoundStyle Item5 = ItemSound(5);
		public static readonly SoundStyle Item6 = ItemSound(6);
		public static readonly SoundStyle Item7 = ItemSound(7);
		public static readonly SoundStyle Item8 = ItemSound(8);
		public static readonly SoundStyle Item9 = ItemSound(9);
		public static readonly SoundStyle Item10 = ItemSound(10);
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
		public static readonly SoundStyle Item24 = ItemSound(24);
		public static readonly SoundStyle Item25 = ItemSound(25);
		public static readonly SoundStyle Item26 = ItemSound(26) with { UsesMusicPitch = true };
		public static readonly SoundStyle Item27 = ItemSound(27);
		public static readonly SoundStyle Item28 = ItemSound(28);
		public static readonly SoundStyle Item29 = ItemSound(29);
		public static readonly SoundStyle Item30 = ItemSound(30);
		public static readonly SoundStyle Item31 = ItemSound(31);
		public static readonly SoundStyle Item32 = ItemSound(32);
		public static readonly SoundStyle Item33 = ItemSound(33);
		public static readonly SoundStyle Item34 = ItemSound(34);
		public static readonly SoundStyle Item35 = ItemSound(35) with { UsesMusicPitch = true };
		public static readonly SoundStyle Item36 = ItemSound(36);
		public static readonly SoundStyle Item37 = ItemSound(37) with { Volume = 0.5f };
		public static readonly SoundStyle Item38 = ItemSound(38);
		public static readonly SoundStyle Item39 = ItemSound(39);
		public static readonly SoundStyle Item40 = ItemSound(40);
		public static readonly SoundStyle Item41 = ItemSound(41);
		public static readonly SoundStyle Item42 = ItemSound(42);
		public static readonly SoundStyle Item43 = ItemSound(43);
		public static readonly SoundStyle Item44 = ItemSound(44);
		public static readonly SoundStyle Item45 = ItemSound(45);
		public static readonly SoundStyle Item46 = ItemSound(46);
		public static readonly SoundStyle Item47 = ItemSound(47) with { UsesMusicPitch = true };
		public static readonly SoundStyle Item48 = ItemSound(48);
		public static readonly SoundStyle Item49 = ItemSound(49);
		public static readonly SoundStyle Item50 = ItemSound(50);
		public static readonly SoundStyle Item51 = ItemSound(51);
		public static readonly SoundStyle Item52 = ItemSound(52) with { Volume = 0.35f };
		public static readonly SoundStyle Item53 = ItemSound(53) with { Volume = 0.75f, PitchRange = (-0.4f, -0.2f), RestartIfPlaying = false };
		public static readonly SoundStyle Item54 = ItemSound(54);
		public static readonly SoundStyle Item55 = ItemSound(55) with { Volume = 0.75f * 0.75f, PitchRange = (-0.4f, -0.2f), RestartIfPlaying = false };
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
		public static readonly SoundStyle Item103 = ItemSound(103);
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
		public static readonly SoundStyle Item156 = ItemSound(156) with { Volume = 0.6f, PitchVariance = 0.2f };
		public static readonly SoundStyle Item157 = ItemSound(157) with { Volume = 0.7f };
		public static readonly SoundStyle Item158 = ItemSound(158) with { Volume = 0.8f };
		public static readonly SoundStyle Item159 = ItemSound(159) with { Volume = 0.75f, RestartIfPlaying = false, };
		public static readonly SoundStyle Item160 = ItemSound(160);
		public static readonly SoundStyle Item161 = ItemSound(161);
		public static readonly SoundStyle Item162 = ItemSound(162);
		public static readonly SoundStyle Item163 = ItemSound(163);
		public static readonly SoundStyle Item164 = ItemSound(164);
		public static readonly SoundStyle Item165 = ItemSound(165);
		public static readonly SoundStyle Item166 = ItemSound(166);
		public static readonly SoundStyle Item167 = ItemSound(167);
		public static readonly SoundStyle Item168 = ItemSound(168);
		public static readonly SoundStyle Item169 = ItemSound(169) with { Pitch = -0.8f };
		public static readonly SoundStyle Item170 = ItemSound(170);
		public static readonly SoundStyle Item171 = ItemSound(171);
		public static readonly SoundStyle Item172 = ItemSound(172);

		// Util methods below

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

		// Mapping:

		internal static SoundStyle GetLegacyStyle(int type, int style) => type switch {
			LegacySoundIDs.Dig => Dig,
			LegacySoundIDs.PlayerHit => PlayerHit,
			LegacySoundIDs.Item => Item,
			LegacySoundIDs.NPCHit => style switch {
				1 => NPCHit1,
				2 => NPCHit2,
				3 => NPCHit3,
				4 => NPCHit4,
				5 => NPCHit5,
				6 => NPCHit6,
				7 => NPCHit7,
				8 => NPCHit8,
				9 => NPCHit9,
				10 => NPCHit10,
				11 => NPCHit11,
				12 => NPCHit12,
				13 => NPCHit13,
				14 => NPCHit14,
				15 => NPCHit15,
				16 => NPCHit16,
				17 => NPCHit17,
				18 => NPCHit18,
				19 => NPCHit19,
				20 => NPCHit20,
				21 => NPCHit21,
				22 => NPCHit22,
				23 => NPCHit23,
				24 => NPCHit24,
				25 => NPCHit25,
				26 => NPCHit26,
				27 => NPCHit27,
				28 => NPCHit28,
				29 => NPCHit29,
				30 => NPCHit30,
				31 => NPCHit31,
				32 => NPCHit32,
				33 => NPCHit33,
				34 => NPCHit34,
				35 => NPCHit35,
				36 => NPCHit36,
				37 => NPCHit37,
				38 => NPCHit38,
				39 => NPCHit39,
				40 => NPCHit40,
				41 => NPCHit41,
				42 => NPCHit42,
				43 => NPCHit43,
				44 => NPCHit44,
				45 => NPCHit45,
				46 => NPCHit46,
				47 => NPCHit47,
				48 => NPCHit48,
				49 => NPCHit49,
				50 => NPCHit50,
				51 => NPCHit51,
				52 => NPCHit52,
				53 => NPCHit53,
				54 => NPCHit54,
				55 => NPCHit55,
				56 => NPCHit56,
				57 => NPCHit57,
				_ => default,
			},
			LegacySoundIDs.NPCKilled => style switch {
				1 => NPCDeath1,
				2 => NPCDeath2,
				3 => NPCDeath3,
				4 => NPCDeath4,
				5 => NPCDeath5,
				6 => NPCDeath6,
				7 => NPCDeath7,
				8 => NPCDeath8,
				9 => NPCDeath9,
				10 => NPCDeath10,
				11 => NPCDeath11,
				12 => NPCDeath12,
				13 => NPCDeath13,
				14 => NPCDeath14,
				15 => NPCDeath15,
				16 => NPCDeath16,
				17 => NPCDeath17,
				18 => NPCDeath18,
				19 => NPCDeath19,
				20 => NPCDeath20,
				21 => NPCDeath21,
				22 => NPCDeath22,
				23 => NPCDeath23,
				24 => NPCDeath24,
				25 => NPCDeath25,
				26 => NPCDeath26,
				27 => NPCDeath27,
				28 => NPCDeath28,
				29 => NPCDeath29,
				30 => NPCDeath30,
				31 => NPCDeath31,
				32 => NPCDeath32,
				33 => NPCDeath33,
				34 => NPCDeath34,
				35 => NPCDeath35,
				36 => NPCDeath36,
				37 => NPCDeath37,
				38 => NPCDeath38,
				39 => NPCDeath39,
				40 => NPCDeath40,
				41 => NPCDeath41,
				42 => NPCDeath42,
				43 => NPCDeath43,
				44 => NPCDeath44,
				45 => NPCDeath45,
				46 => NPCDeath46,
				47 => NPCDeath47,
				48 => NPCDeath48,
				49 => NPCDeath49,
				50 => NPCDeath50,
				51 => NPCDeath51,
				52 => NPCDeath52,
				53 => NPCDeath53,
				54 => NPCDeath54,
				55 => NPCDeath55,
				56 => NPCDeath56,
				57 => NPCDeath57,
				58 => NPCDeath58,
				59 => NPCDeath59,
				60 => NPCDeath60,
				61 => NPCDeath61,
				62 => NPCDeath62,
				63 => NPCDeath63,
				64 => NPCDeath64,
				65 => NPCDeath65,
				_ => default,
			},
			LegacySoundIDs.PlayerKilled => PlayerKilled,
			LegacySoundIDs.Grass => Grass,
			LegacySoundIDs.Grab => Grab,
			LegacySoundIDs.DoorOpen => DoorOpen,
			LegacySoundIDs.DoorClosed => DoorClosed,
			LegacySoundIDs.MenuOpen => MenuOpen,
			LegacySoundIDs.MenuClose => MenuClose,
			LegacySoundIDs.MenuTick => MenuTick,
			LegacySoundIDs.Shatter => Shatter,
			LegacySoundIDs.ZombieMoan => ZombieMoan,
			LegacySoundIDs.Roar => Roar,
			LegacySoundIDs.DoubleJump => DoubleJump,
			LegacySoundIDs.Run => Run,
			LegacySoundIDs.Coins => Coins,
			LegacySoundIDs.Splash => style switch { 1 => SplashWeak, _ => Splash },
			LegacySoundIDs.FemaleHit => FemaleHit,
			LegacySoundIDs.Tink => Tink,
			LegacySoundIDs.Unlock => Unlock,
			LegacySoundIDs.Drown => Drown,
			LegacySoundIDs.Chat => Chat,
			LegacySoundIDs.MaxMana => MaxMana,
			LegacySoundIDs.Mummy => Mummy,
			LegacySoundIDs.Pixie => Pixie,
			LegacySoundIDs.Mech => Mech,
			LegacySoundIDs.Zombie => new SoundStyle($"{Prefix}Zombie_{style}"),
			LegacySoundIDs.Duck => Duck,
			LegacySoundIDs.Frog => Frog,
			LegacySoundIDs.Bird => Bird,
			LegacySoundIDs.Critter => Critter,
			LegacySoundIDs.Waterfall => Waterfall,
			LegacySoundIDs.Lavafall => Lavafall,
			LegacySoundIDs.ForceRoar => ForceRoar,
			LegacySoundIDs.Meowmere => Meowmere,
			LegacySoundIDs.CoinPickup => CoinPickup,
			LegacySoundIDs.Drip => Drip,
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
			LegacySoundIDs.GuitarAm => GuitarAm,
			LegacySoundIDs.GuitarF => GuitarF,
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
			_ => default,
		};
	}
}
