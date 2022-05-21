using System;
using Terraria.Audio;

namespace Terraria.ID
{
	public partial class SoundID {
		// Start of IDs that used to be 'int' typed.
		public static readonly LegacySoundStyle Dig = new(0, stackalloc int[] { 0, 1, 2 }) { PitchVariance = 0.2f };
		public static readonly LegacySoundStyle PlayerHit = new(1, stackalloc int[] { 0, 1, 2 }) { PitchVariance = 0.2f };
		public static readonly LegacySoundStyle Item = new(2, 0);
		//public static readonly LegacySoundStyle NPCHit = new(3, 0);
		//public static readonly LegacySoundStyle NPCKilled = new(4, 0);
		public static readonly LegacySoundStyle PlayerKilled = new(5, 0);
		public static readonly LegacySoundStyle Grass = new(6, 0) { PitchVariance = 0.6f };
		public static readonly LegacySoundStyle Grab = new(7, 0) { PitchVariance = 0.2f };
		public static readonly LegacySoundStyle DoorOpen = new(8, 0) { PitchVariance = 0.4f };
		public static readonly LegacySoundStyle DoorClosed = new(9, 0) { PitchVariance = 0.4f };
		public static readonly LegacySoundStyle MenuOpen = new(10, 0);
		public static readonly LegacySoundStyle MenuClose = new(11, 0);
		public static readonly LegacySoundStyle MenuTick = new(12, 0) { PlayOnlyIfFocused = true };
		public static readonly LegacySoundStyle Shatter = new(13, 0);
		public static readonly LegacySoundStyle ZombieMoan = new(14, stackalloc int[] { 0, 1, 2 }) { Volume = 0.4f };
		public static readonly LegacySoundStyle ZombieMoan_SandShark = new(14, 7) { Volume = 0.4f };
		public static readonly LegacySoundStyle ZombieMoan_BloodZombie = new(14, stackalloc int[] { 21, 22, 23 }) { Volume = 0.4f };
		public static readonly LegacySoundStyle Roar = new(15, 0) { RestartIfPlaying = false };
		public static readonly LegacySoundStyle Roar_WormDig = new(15, 1) { RestartIfPlaying = false };
		//public static readonly LegacySoundStyle Roar_Something = new(15, 2) { RestartIfPlaying = false };
		public static readonly LegacySoundStyle DoubleJump = new(16, 0) { PitchVariance = 0.2f };
		public static readonly LegacySoundStyle Run = new(17, 0) { PitchVariance = 0.2f };
		public static readonly LegacySoundStyle Coins = new(18, 0);
		public static readonly LegacySoundStyle Splash = new(19, 0) { PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly LegacySoundStyle FemaleHit = new(20, stackalloc int[] { 0, 1, 2 });
		public static readonly LegacySoundStyle Tink = new(21, stackalloc int[] { 0, 1, 2 });
		public static readonly LegacySoundStyle Unlock = new(22, 0);
		public static readonly LegacySoundStyle Drown = new(23, 0);
		public static readonly LegacySoundStyle Chat = new(24, 0);
		public static readonly LegacySoundStyle MaxMana = new(25, 0);
		public static readonly LegacySoundStyle Mummy = new(LegacySoundIDs.Zombie, stackalloc int[] { 3, 4 }) { Volume = 0.9f, PitchVariance = 0.2f }; //26
		public static readonly LegacySoundStyle Pixie = new(27, 0) { PitchVariance = 0.2f, RestartIfPlaying = false };
		public static readonly LegacySoundStyle Mech = new(28, 0) { PitchVariance = 0.2f, RestartIfPlaying = false };
		//public static readonly LegacySoundStyle Zombie = new(29, 0);
		public static readonly LegacySoundStyle Duck = new(30, 0);
		public static readonly LegacySoundStyle Frog = new(31, 0);
		public static readonly LegacySoundStyle Bird = new(32, 0);
		public static readonly LegacySoundStyle Critter = new(33, 0);
		public static readonly LegacySoundStyle Waterfall = new(34, 0);
		public static readonly LegacySoundStyle Lavafall = new(35, 0);
		public static readonly LegacySoundStyle ForceRoar = new(36, 0);
		public static readonly LegacySoundStyle Meowmere = new(37, 0);
		public static readonly LegacySoundStyle CoinPickup = new(38, 0);
		public static readonly LegacySoundStyle Drip = new(39, 0);
		public static readonly LegacySoundStyle Camera = new(40, 0);
		public static readonly LegacySoundStyle MoonLord = new(41, 0);
		public static readonly LegacySoundStyle Trackable = new(42, 0);
		public static readonly LegacySoundStyle Thunder = new(43, 0) {
			MaxInstances = SoundEngine.LegacySoundPlayer.SoundThunder.Length,
			PitchVariance = 0.2f,
		};
		public static readonly LegacySoundStyle Seagull = new(LegacySoundIDs.Zombie, stackalloc int[] { 106, 107, 108 }) { Volume = 0.2f, PitchRange = (-0.7f, 0f) }; //44
		public static readonly LegacySoundStyle Dolphin = new(LegacySoundIDs.Zombie, 109) { Volume = 0.3f, PitchVariance = 0.2f }; //45
		public static readonly LegacySoundStyle Owl = new(LegacySoundIDs.Zombie, stackalloc int[] { 110, 111 }); //46
		public static readonly LegacySoundStyle OwlFunny = new(LegacySoundIDs.Zombie, stackalloc int[] { 110, 111 }); //46
		/*
		public static readonly LegacySoundStyle GuitarC = new(47, 0);
		public static readonly LegacySoundStyle GuitarD = new(48, 0);
		public static readonly LegacySoundStyle GuitarEm = new(49, 0);
		public static readonly LegacySoundStyle GuitarG = new(50, 0);
		public static readonly LegacySoundStyle GuitarAm = new(51, 0);
		public static readonly LegacySoundStyle GuitarF = new(52, 0);
		public static readonly LegacySoundStyle DrumHiHat = new(53, 0);
		public static readonly LegacySoundStyle DrumTomHigh = new(54, 0);
		public static readonly LegacySoundStyle DrumTomLow = new(55, 0);
		public static readonly LegacySoundStyle DrumTomMid = new(56, 0);
		public static readonly LegacySoundStyle DrumClosedHiHat = new(57, 0);
		public static readonly LegacySoundStyle DrumCymbal1 = new(58, 0);
		public static readonly LegacySoundStyle DrumCymbal2 = new(59, 0);
		public static readonly LegacySoundStyle DrumKick = new(60, 0);
		public static readonly LegacySoundStyle DrumTamaSnare = new(61, 0);
		public static readonly LegacySoundStyle DrumFloorTom = new(62, 0);
		*/
		public static readonly LegacySoundStyle Research = new(63, stackalloc int[] { 1, 2, 3 });
		public static readonly LegacySoundStyle ResearchComplete = new(64, 0);
		public static readonly LegacySoundStyle QueenSlime = new(65, 0);
		public static readonly LegacySoundStyle Count = new(66, 0);
		// End of replaced IDs.

		public static readonly LegacySoundStyle NPCHit1 = NPCHitSound(3, 1);
		public static readonly LegacySoundStyle NPCHit2 = NPCHitSound(3, 2);
		public static readonly LegacySoundStyle NPCHit3 = NPCHitSound(3, 3);
		public static readonly LegacySoundStyle NPCHit4 = NPCHitSound(3, 4);
		public static readonly LegacySoundStyle NPCHit5 = NPCHitSound(3, 5);
		public static readonly LegacySoundStyle NPCHit6 = NPCHitSound(3, 6);
		public static readonly LegacySoundStyle NPCHit7 = NPCHitSound(3, 7);
		public static readonly LegacySoundStyle NPCHit8 = NPCHitSound(3, 8);
		public static readonly LegacySoundStyle NPCHit9 = NPCHitSound(3, 9);
		public static readonly LegacySoundStyle NPCHit10 = NPCHitSound(3, 10);
		public static readonly LegacySoundStyle NPCHit11 = NPCHitSound(3, 11);
		public static readonly LegacySoundStyle NPCHit12 = NPCHitSound(3, 12);
		public static readonly LegacySoundStyle NPCHit13 = NPCHitSound(3, 13);
		public static readonly LegacySoundStyle NPCHit14 = NPCHitSound(3, 14);
		public static readonly LegacySoundStyle NPCHit15 = NPCHitSound(3, 15);
		public static readonly LegacySoundStyle NPCHit16 = NPCHitSound(3, 16);
		public static readonly LegacySoundStyle NPCHit17 = NPCHitSound(3, 17);
		public static readonly LegacySoundStyle NPCHit18 = NPCHitSound(3, 18);
		public static readonly LegacySoundStyle NPCHit19 = NPCHitSound(3, 19);
		public static readonly LegacySoundStyle NPCHit20 = NPCHitSound(3, 20) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit21 = NPCHitSound(3, 21) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit22 = NPCHitSound(3, 22) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit23 = NPCHitSound(3, 23) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit24 = NPCHitSound(3, 24) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit25 = NPCHitSound(3, 25) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit26 = NPCHitSound(3, 26) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit27 = NPCHitSound(3, 27) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit28 = NPCHitSound(3, 28) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit29 = NPCHitSound(3, 29) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit30 = NPCHitSound(3, 30) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit31 = NPCHitSound(3, 31) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit32 = NPCHitSound(3, 32) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit33 = NPCHitSound(3, 33) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit34 = NPCHitSound(3, 34) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit35 = NPCHitSound(3, 35) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit36 = NPCHitSound(3, 36) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit37 = NPCHitSound(3, 37) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit38 = NPCHitSound(3, 38) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit39 = NPCHitSound(3, 39) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit40 = NPCHitSound(3, 40) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit41 = NPCHitSound(3, 41) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit42 = NPCHitSound(3, 42) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit43 = NPCHitSound(3, 43) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit44 = NPCHitSound(3, 44) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit45 = NPCHitSound(3, 45) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit46 = NPCHitSound(3, 46) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit47 = NPCHitSound(3, 47) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit48 = NPCHitSound(3, 48) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit49 = NPCHitSound(3, 49) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit50 = NPCHitSound(3, 50) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit51 = NPCHitSound(3, 51) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit52 = NPCHitSound(3, 52) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit53 = NPCHitSound(3, 53) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit54 = NPCHitSound(3, 54) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit55 = NPCHitSound(3, 55) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit56 = NPCHitSound(3, 56) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCHit57 = NPCHitSound(3, 57) with { Volume = 0.6f, RestartIfPlaying = false };
		public static int NPCHitCount = 58; // Added by tML
		public static readonly LegacySoundStyle NPCDeath1 = NPCDeathSound(4, 1);
		public static readonly LegacySoundStyle NPCDeath2 = NPCDeathSound(4, 2);
		public static readonly LegacySoundStyle NPCDeath3 = NPCDeathSound(4, 3);
		public static readonly LegacySoundStyle NPCDeath4 = NPCDeathSound(4, 4);
		public static readonly LegacySoundStyle NPCDeath5 = NPCDeathSound(4, 5);
		public static readonly LegacySoundStyle NPCDeath6 = NPCDeathSound(4, 6);
		public static readonly LegacySoundStyle NPCDeath7 = NPCDeathSound(4, 7);
		public static readonly LegacySoundStyle NPCDeath8 = NPCDeathSound(4, 8);
		public static readonly LegacySoundStyle NPCDeath9 = NPCDeathSound(4, 9);
		public static readonly LegacySoundStyle NPCDeath10 = NPCDeathSound(4, 10) with { RestartIfPlaying = false };
		public static readonly LegacySoundStyle NPCDeath11 = NPCDeathSound(4, 11);
		public static readonly LegacySoundStyle NPCDeath12 = NPCDeathSound(4, 12);
		public static readonly LegacySoundStyle NPCDeath13 = NPCDeathSound(4, 13);
		public static readonly LegacySoundStyle NPCDeath14 = NPCDeathSound(4, 14);
		public static readonly LegacySoundStyle NPCDeath15 = NPCDeathSound(4, 15);
		public static readonly LegacySoundStyle NPCDeath16 = NPCDeathSound(4, 16);
		public static readonly LegacySoundStyle NPCDeath17 = NPCDeathSound(4, 17);
		public static readonly LegacySoundStyle NPCDeath18 = NPCDeathSound(4, 18);
		public static readonly LegacySoundStyle NPCDeath19 = NPCDeathSound(4, 19);
		public static readonly LegacySoundStyle NPCDeath20 = NPCDeathSound(4, 20);
		public static readonly LegacySoundStyle NPCDeath21 = NPCDeathSound(4, 21);
		public static readonly LegacySoundStyle NPCDeath22 = NPCDeathSound(4, 22);
		public static readonly LegacySoundStyle NPCDeath23 = NPCDeathSound(4, 23) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath24 = NPCDeathSound(4, 24) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath25 = NPCDeathSound(4, 25) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath26 = NPCDeathSound(4, 26) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath27 = NPCDeathSound(4, 27) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath28 = NPCDeathSound(4, 28) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath29 = NPCDeathSound(4, 29) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath30 = NPCDeathSound(4, 30) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath31 = NPCDeathSound(4, 31) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath32 = NPCDeathSound(4, 32) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath33 = NPCDeathSound(4, 33) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath34 = NPCDeathSound(4, 34) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath35 = NPCDeathSound(4, 35) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath36 = NPCDeathSound(4, 36) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath37 = NPCDeathSound(4, 37) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath38 = NPCDeathSound(4, 38) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath39 = NPCDeathSound(4, 39) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath40 = NPCDeathSound(4, 40) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath41 = NPCDeathSound(4, 41) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath42 = NPCDeathSound(4, 42) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath43 = NPCDeathSound(4, 43) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath44 = NPCDeathSound(4, 44) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath45 = NPCDeathSound(4, 45) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath46 = NPCDeathSound(4, 46) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath47 = NPCDeathSound(4, 47) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath48 = NPCDeathSound(4, 48) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath49 = NPCDeathSound(4, 49) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath50 = NPCDeathSound(4, 50) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath51 = NPCDeathSound(4, 51) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath52 = NPCDeathSound(4, 52) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath53 = NPCDeathSound(4, 53) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath54 = NPCDeathSound(4, 54) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath55 = NPCDeathSound(4, 55) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath56 = NPCDeathSound(4, 56) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath57 = NPCDeathSound(4, 57) with { Volume = 0.5f };
		public static readonly LegacySoundStyle NPCDeath58 = NPCDeathSound(4, 58);
		public static readonly LegacySoundStyle NPCDeath59 = NPCDeathSound(4, 59);
		public static readonly LegacySoundStyle NPCDeath60 = NPCDeathSound(4, 60);
		public static readonly LegacySoundStyle NPCDeath61 = NPCDeathSound(4, 61) with { Volume = 0.6f };
		public static readonly LegacySoundStyle NPCDeath62 = NPCDeathSound(4, 62) with { Volume = 0.6f };
		public static readonly LegacySoundStyle NPCDeath63 = NPCDeathSound(4, 63);
		public static readonly LegacySoundStyle NPCDeath64 = NPCDeathSound(4, 64);
		public static readonly LegacySoundStyle NPCDeath65 = NPCDeathSound(4, 65);
		public static int NPCDeathCount = 66; // TML: Changed from short to int.
		public static readonly LegacySoundStyle Item1 = ItemSound(2, stackalloc int[] { 1, 18, 19 });
		public static readonly LegacySoundStyle Item2 = ItemSound(2, 2);
		public static readonly LegacySoundStyle Item3 = ItemSound(2, 3);
		public static readonly LegacySoundStyle Item4 = ItemSound(2, 4);
		public static readonly LegacySoundStyle Item5 = ItemSound(2, 5);
		public static readonly LegacySoundStyle Item6 = ItemSound(2, 6);
		public static readonly LegacySoundStyle Item7 = ItemSound(2, 7);
		public static readonly LegacySoundStyle Item8 = ItemSound(2, 8);
		public static readonly LegacySoundStyle Item9 = ItemSound(2, 9);
		public static readonly LegacySoundStyle Item10 = ItemSound(2, 10);
		public static readonly LegacySoundStyle Item11 = ItemSound(2, 11);
		public static readonly LegacySoundStyle Item12 = ItemSound(2, 12);
		public static readonly LegacySoundStyle Item13 = ItemSound(2, 13);
		public static readonly LegacySoundStyle Item14 = ItemSound(2, 14);
		public static readonly LegacySoundStyle Item15 = ItemSound(2, 15);
		public static readonly LegacySoundStyle Item16 = ItemSound(2, 16);
		public static readonly LegacySoundStyle Item17 = ItemSound(2, 17);
		public static readonly LegacySoundStyle Item18 = ItemSound(2, 18);
		public static readonly LegacySoundStyle Item19 = ItemSound(2, 19);
		public static readonly LegacySoundStyle Item20 = ItemSound(2, 20);
		public static readonly LegacySoundStyle Item21 = ItemSound(2, 21);
		public static readonly LegacySoundStyle Item22 = ItemSound(2, 22);
		public static readonly LegacySoundStyle Item23 = ItemSound(2, 23);
		public static readonly LegacySoundStyle Item24 = ItemSound(2, 24);
		public static readonly LegacySoundStyle Item25 = ItemSound(2, 25);
		public static readonly LegacySoundStyle Item26 = ItemSound(2, 26) with { UsesMusicPitch = true };
		public static readonly LegacySoundStyle Item27 = ItemSound(2, 27);
		public static readonly LegacySoundStyle Item28 = ItemSound(2, 28);
		public static readonly LegacySoundStyle Item29 = ItemSound(2, 29);
		public static readonly LegacySoundStyle Item30 = ItemSound(2, 30);
		public static readonly LegacySoundStyle Item31 = ItemSound(2, 31);
		public static readonly LegacySoundStyle Item32 = ItemSound(2, 32);
		public static readonly LegacySoundStyle Item33 = ItemSound(2, 33);
		public static readonly LegacySoundStyle Item34 = ItemSound(2, 34);
		public static readonly LegacySoundStyle Item35 = ItemSound(2, 35) with { UsesMusicPitch = true };
		public static readonly LegacySoundStyle Item36 = ItemSound(2, 36);
		public static readonly LegacySoundStyle Item37 = ItemSound(2, 37) with { Volume = 0.5f };
		public static readonly LegacySoundStyle Item38 = ItemSound(2, 38);
		public static readonly LegacySoundStyle Item39 = ItemSound(2, 39);
		public static readonly LegacySoundStyle Item40 = ItemSound(2, 40);
		public static readonly LegacySoundStyle Item41 = ItemSound(2, 41);
		public static readonly LegacySoundStyle Item42 = ItemSound(2, 42);
		public static readonly LegacySoundStyle Item43 = ItemSound(2, 43);
		public static readonly LegacySoundStyle Item44 = ItemSound(2, 44);
		public static readonly LegacySoundStyle Item45 = ItemSound(2, 45);
		public static readonly LegacySoundStyle Item46 = ItemSound(2, 46);
		public static readonly LegacySoundStyle Item47 = ItemSound(2, 47) with { UsesMusicPitch = true };
		public static readonly LegacySoundStyle Item48 = ItemSound(2, 48);
		public static readonly LegacySoundStyle Item49 = ItemSound(2, 49);
		public static readonly LegacySoundStyle Item50 = ItemSound(2, 50);
		public static readonly LegacySoundStyle Item51 = ItemSound(2, 51);
		public static readonly LegacySoundStyle Item52 = ItemSound(2, 52) with { Volume = 0.35f };
		public static readonly LegacySoundStyle Item53 = ItemSound(2, 53) with { Volume = 0.75f, PitchRange = (-0.4f, -0.2f), RestartIfPlaying = false };
		public static readonly LegacySoundStyle Item54 = ItemSound(2, 54);
		public static readonly LegacySoundStyle Item55 = ItemSound(2, 55) with { Volume = 0.75f * 0.75f, PitchRange = (-0.4f, -0.2f), RestartIfPlaying = false };
		public static readonly LegacySoundStyle Item56 = ItemSound(2, 56);
		public static readonly LegacySoundStyle Item57 = ItemSound(2, 57);
		public static readonly LegacySoundStyle Item58 = ItemSound(2, 58);
		public static readonly LegacySoundStyle Item59 = ItemSound(2, 59);
		public static readonly LegacySoundStyle Item60 = ItemSound(2, 60);
		public static readonly LegacySoundStyle Item61 = ItemSound(2, 61);
		public static readonly LegacySoundStyle Item62 = ItemSound(2, 62);
		public static readonly LegacySoundStyle Item63 = ItemSound(2, 63);
		public static readonly LegacySoundStyle Item64 = ItemSound(2, 64);
		public static readonly LegacySoundStyle Item65 = ItemSound(2, 65);
		public static readonly LegacySoundStyle Item66 = ItemSound(2, 66);
		public static readonly LegacySoundStyle Item67 = ItemSound(2, 67);
		public static readonly LegacySoundStyle Item68 = ItemSound(2, 68);
		public static readonly LegacySoundStyle Item69 = ItemSound(2, 69);
		public static readonly LegacySoundStyle Item70 = ItemSound(2, 70);
		public static readonly LegacySoundStyle Item71 = ItemSound(2, 71);
		public static readonly LegacySoundStyle Item72 = ItemSound(2, 72);
		public static readonly LegacySoundStyle Item73 = ItemSound(2, 73);
		public static readonly LegacySoundStyle Item74 = ItemSound(2, 74);
		public static readonly LegacySoundStyle Item75 = ItemSound(2, 75);
		public static readonly LegacySoundStyle Item76 = ItemSound(2, 76);
		public static readonly LegacySoundStyle Item77 = ItemSound(2, 77);
		public static readonly LegacySoundStyle Item78 = ItemSound(2, 78);
		public static readonly LegacySoundStyle Item79 = ItemSound(2, 79);
		public static readonly LegacySoundStyle Item80 = ItemSound(2, 80);
		public static readonly LegacySoundStyle Item81 = ItemSound(2, 81);
		public static readonly LegacySoundStyle Item82 = ItemSound(2, 82);
		public static readonly LegacySoundStyle Item83 = ItemSound(2, 83);
		public static readonly LegacySoundStyle Item84 = ItemSound(2, 84);
		public static readonly LegacySoundStyle Item85 = ItemSound(2, 85);
		public static readonly LegacySoundStyle Item86 = ItemSound(2, 86);
		public static readonly LegacySoundStyle Item87 = ItemSound(2, 87);
		public static readonly LegacySoundStyle Item88 = ItemSound(2, 88);
		public static readonly LegacySoundStyle Item89 = ItemSound(2, 89);
		public static readonly LegacySoundStyle Item90 = ItemSound(2, 90);
		public static readonly LegacySoundStyle Item91 = ItemSound(2, 91);
		public static readonly LegacySoundStyle Item92 = ItemSound(2, 92);
		public static readonly LegacySoundStyle Item93 = ItemSound(2, 93);
		public static readonly LegacySoundStyle Item94 = ItemSound(2, 94);
		public static readonly LegacySoundStyle Item95 = ItemSound(2, 95);
		public static readonly LegacySoundStyle Item96 = ItemSound(2, 96);
		public static readonly LegacySoundStyle Item97 = ItemSound(2, 97);
		public static readonly LegacySoundStyle Item98 = ItemSound(2, 98);
		public static readonly LegacySoundStyle Item99 = ItemSound(2, 99);
		public static readonly LegacySoundStyle Item100 = ItemSound(2, 100);
		public static readonly LegacySoundStyle Item101 = ItemSound(2, 101);
		public static readonly LegacySoundStyle Item102 = ItemSound(2, 102);
		public static readonly LegacySoundStyle Item103 = ItemSound(2, 103);
		public static readonly LegacySoundStyle Item104 = ItemSound(2, 104);
		public static readonly LegacySoundStyle Item105 = ItemSound(2, 105);
		public static readonly LegacySoundStyle Item106 = ItemSound(2, 106);
		public static readonly LegacySoundStyle Item107 = ItemSound(2, 107);
		public static readonly LegacySoundStyle Item108 = ItemSound(2, 108);
		public static readonly LegacySoundStyle Item109 = ItemSound(2, 109);
		public static readonly LegacySoundStyle Item110 = ItemSound(2, 110);
		public static readonly LegacySoundStyle Item111 = ItemSound(2, 111);
		public static readonly LegacySoundStyle Item112 = ItemSound(2, 112);
		public static readonly LegacySoundStyle Item113 = ItemSound(2, 113);
		public static readonly LegacySoundStyle Item114 = ItemSound(2, 114);
		public static readonly LegacySoundStyle Item115 = ItemSound(2, 115);
		public static readonly LegacySoundStyle Item116 = ItemSound(2, 116) with { Volume = 0.5f };
		public static readonly LegacySoundStyle Item117 = ItemSound(2, 117);
		public static readonly LegacySoundStyle Item118 = ItemSound(2, 118);
		public static readonly LegacySoundStyle Item119 = ItemSound(2, 119);
		public static readonly LegacySoundStyle Item120 = ItemSound(2, 120);
		public static readonly LegacySoundStyle Item121 = ItemSound(2, 121);
		public static readonly LegacySoundStyle Item122 = ItemSound(2, 122);
		public static readonly LegacySoundStyle Item123 = ItemSound(2, 123) with { Volume = 0.5f };
		public static readonly LegacySoundStyle Item124 = ItemSound(2, 124) with { Volume = 0.65f };
		public static readonly LegacySoundStyle Item125 = ItemSound(2, 125) with { Volume = 0.65f };
		public static readonly LegacySoundStyle Item126 = ItemSound(2, 126);
		public static readonly LegacySoundStyle Item127 = ItemSound(2, 127);
		public static readonly LegacySoundStyle Item128 = ItemSound(2, 128);
		public static readonly LegacySoundStyle Item129 = ItemSound(2, 129) with { Volume = 0.6f };
		public static readonly LegacySoundStyle Item130 = ItemSound(2, 130);
		public static readonly LegacySoundStyle Item131 = ItemSound(2, 131);
		public static readonly LegacySoundStyle Item132 = ItemSound(2, 132) with { PitchVariance = 0.04f };
		public static readonly LegacySoundStyle Item133 = ItemSound(2, 133);
		public static readonly LegacySoundStyle Item134 = ItemSound(2, 134);
		public static readonly LegacySoundStyle Item135 = ItemSound(2, 135);
		public static readonly LegacySoundStyle Item136 = ItemSound(2, 136);
		public static readonly LegacySoundStyle Item137 = ItemSound(2, 137);
		public static readonly LegacySoundStyle Item138 = ItemSound(2, 138);
		public static readonly LegacySoundStyle Item139 = ItemSound(2, 139);
		public static readonly LegacySoundStyle Item140 = ItemSound(2, 140);
		public static readonly LegacySoundStyle Item141 = ItemSound(2, 141);
		public static readonly LegacySoundStyle Item142 = ItemSound(2, 142);
		public static readonly LegacySoundStyle Item143 = ItemSound(2, 143);
		public static readonly LegacySoundStyle Item144 = ItemSound(2, 144);
		public static readonly LegacySoundStyle Item145 = ItemSound(2, 145);
		public static readonly LegacySoundStyle Item146 = ItemSound(2, 146);
		public static readonly LegacySoundStyle Item147 = ItemSound(2, 147);
		public static readonly LegacySoundStyle Item148 = ItemSound(2, 148);
		public static readonly LegacySoundStyle Item149 = ItemSound(2, 149);
		public static readonly LegacySoundStyle Item150 = ItemSound(2, 150);
		public static readonly LegacySoundStyle Item151 = ItemSound(2, 151);
		public static readonly LegacySoundStyle Item152 = ItemSound(2, 152);
		public static readonly LegacySoundStyle Item153 = ItemSound(2, 153) with { PitchVariance = 0.3f };
		public static readonly LegacySoundStyle Item154 = ItemSound(2, 154);
		public static readonly LegacySoundStyle Item155 = ItemSound(2, 155);
		public static readonly LegacySoundStyle Item156 = ItemSound(2, 156) with { Volume = 0.6f, PitchVariance = 0.2f };
		public static readonly LegacySoundStyle Item157 = ItemSound(2, 157) with { Volume = 0.7f };
		public static readonly LegacySoundStyle Item158 = ItemSound(2, 158) with { Volume = 0.8f };
		public static readonly LegacySoundStyle Item159 = ItemSound(2, 159) with { Volume = 0.75f, RestartIfPlaying = false, };
		public static readonly LegacySoundStyle Item160 = ItemSound(2, 160);
		public static readonly LegacySoundStyle Item161 = ItemSound(2, 161);
		public static readonly LegacySoundStyle Item162 = ItemSound(2, 162);
		public static readonly LegacySoundStyle Item163 = ItemSound(2, 163);
		public static readonly LegacySoundStyle Item164 = ItemSound(2, 164);
		public static readonly LegacySoundStyle Item165 = ItemSound(2, 165);
		public static readonly LegacySoundStyle Item166 = ItemSound(2, 166);
		public static readonly LegacySoundStyle Item167 = ItemSound(2, 167);
		public static readonly LegacySoundStyle Item168 = ItemSound(2, 168);
		public static readonly LegacySoundStyle Item169 = ItemSound(2, 169) with { Pitch = -0.8f };
		public static readonly LegacySoundStyle Item170 = ItemSound(2, 170);
		public static readonly LegacySoundStyle Item171 = ItemSound(2, 171);
		public static readonly LegacySoundStyle Item172 = ItemSound(2, 172);

		private static readonly SoundStyleDefaults NPCHitDefaults = new(1f, 0.2f);
		private static readonly SoundStyleDefaults NPCDeathDefaults = new(1f, 0.2f);

		// Util methods below

		private static LegacySoundStyle SoundWithDefaults(SoundStyleDefaults defaults, LegacySoundStyle style)
		{
			defaults.Apply(ref style);

			return style;
		}

		private static LegacySoundStyle NPCHitSound(int soundId, int soundStyle)
			=> SoundWithDefaults(NPCHitDefaults, new(soundId, soundStyle));

		private static LegacySoundStyle NPCHitSound(int soundId, ReadOnlySpan<int> soundStyles)
			=> SoundWithDefaults(NPCHitDefaults, new(soundId, soundStyles));

		private static LegacySoundStyle NPCDeathSound(int soundId, int soundStyle)
			=> SoundWithDefaults(NPCDeathDefaults, new(soundId, soundStyle));

		private static LegacySoundStyle NPCDeathSound(int soundId, ReadOnlySpan<int> soundStyles)
			=> SoundWithDefaults(NPCDeathDefaults, new(soundId, soundStyles));
		
		private static LegacySoundStyle ItemSound(int soundId, int soundStyle)
			=> SoundWithDefaults(ItemDefaults, new(soundId, soundStyle));

		private static LegacySoundStyle ItemSound(int soundId, ReadOnlySpan<int> soundStyles)
			=> SoundWithDefaults(ItemDefaults, new(soundId, soundStyles));
	}
}
