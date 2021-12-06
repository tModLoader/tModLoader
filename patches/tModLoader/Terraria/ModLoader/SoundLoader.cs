using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class is used to keep track of and support the existence of custom sounds that have been added to the game.
	/// </summary>
	public static class SoundLoader
	{
		private static readonly IDictionary<SoundType, int> nextSound = new Dictionary<SoundType, int>();
		internal static readonly IDictionary<SoundType, IDictionary<string, int>> sounds = new Dictionary<SoundType, IDictionary<string, int>>();
		internal static readonly IDictionary<SoundType, IDictionary<int, ModSound>> modSounds = new Dictionary<SoundType, IDictionary<int, ModSound>>();
		internal static Asset<SoundEffect>[] customSounds = new Asset<SoundEffect>[0];
		internal static SoundEffectInstance[] customSoundInstances = new SoundEffectInstance[0];
		/// <summary>
		/// This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound that is not an item, npcHit, or npcKilled sound.
		/// </summary>
		public const int customSoundType = 50;

		static SoundLoader() {
			foreach (SoundType type in Enum.GetValues(typeof(SoundType))) {
				nextSound[type] = GetNumVanilla(type);
				sounds[type] = new Dictionary<string, int>();
				modSounds[type] = new Dictionary<int, ModSound>();
			}
		}

		internal static int ReserveSoundID(SoundType type) => nextSound[type]++;

		public static int SoundCount(SoundType type) {
			return nextSound[type];
		}

		/// <summary>
		/// Returns the style (last parameter passed to Main.PlaySound) of the sound corresponding to the given SoundType and the given sound file path. Returns 0 if there is no corresponding style.
		/// </summary>
		public static int GetSoundSlot(SoundType type, string sound) {
			if (sounds[type].ContainsKey(sound)) {
				return sounds[type][sound];
			}
			else {
				return 0;
			}
		}

		// TODO: Should we just get rid of the soundType Enum?

		/// <summary>
		/// Returns a LegacySoundStyle object which encapsulates both a sound type and a sound style (This is the new way to do sounds in 1.3.4) Returns null if there is no corresponding style.
		/// </summary>
		internal static LegacySoundStyle GetLegacySoundSlot(SoundType type, string sound) {
			if (sounds[type].ContainsKey(sound)) {
				return new LegacySoundStyle((int)type, sounds[type][sound]);
			}
			else {
				return null;
			}
		}

		internal static void ResizeAndFillArrays() {
			if (!SoundEngine.IsAudioSupported)
				return;

			customSounds = new Asset<SoundEffect>[nextSound[SoundType.Custom]];
			customSoundInstances = new SoundEffectInstance[nextSound[SoundType.Custom]];
			
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundItem,				nextSound[SoundType.Item]);
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundInstanceItem,		nextSound[SoundType.Item]);
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundNpcHit,				nextSound[SoundType.NPCHit]);
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundInstanceNpcHit,		nextSound[SoundType.NPCHit]);
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundNpcKilled,			nextSound[SoundType.NPCKilled]);
			Array.Resize(ref SoundEngine.LegacySoundPlayer.SoundInstanceNpcKilled,	nextSound[SoundType.NPCKilled]);
			//Array.Resize(ref Main.music, nextSound[SoundType.Music]);
			//Array.Resize(ref Main.musicFade, nextSound[SoundType.Music]);

			foreach (SoundType type in Enum.GetValues(typeof(SoundType))) {
				foreach (string soundName in sounds[type].Keys) {
					int slot = GetSoundSlot(type, soundName);
					var sound = ModContent.Request<SoundEffect>(soundName, AssetRequestMode.ImmediateLoad);

					GetSoundArray(type)[slot] = sound;
					GetSoundInstanceArray(type)[slot] = sound.Value.CreateInstance();
				}
			}
		}

		internal static void Unload() {
			//for (int i = Main.maxMusic; i < Main.music.Length; i++)
			//{
			//	Main.music[i].Stop(AudioStopOptions.Immediate);
			//}
			foreach (SoundType type in Enum.GetValues(typeof(SoundType))) {
				nextSound[type] = GetNumVanilla(type);
				sounds[type].Clear();
				modSounds[type].Clear();
			}
		}
		//in Terraria.Main.PlaySound before checking type to play sound add
		//  if (SoundLoader.PlayModSound(type, num, num2, num3)) { return; }
		internal static bool PlayModSound(int type, int style, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (!SoundEngine.IsAudioSupported)
				return false;

			SoundType soundType;
			switch (type) {
				case 2:
					soundType = SoundType.Item;
					break;
				case 3:
					soundType = SoundType.NPCHit;
					break;
				case 4:
					soundType = SoundType.NPCKilled;
					break;
				case customSoundType:
					soundType = SoundType.Custom;
					break;
				default:
					return false;
			}
			if (!modSounds[soundType].ContainsKey(style)) {
				return false;
			}
			soundEffectInstance = modSounds[soundType][style].PlaySound(ref GetSoundInstanceArray(soundType)[style], volume, pan, soundType);
			return true;
		}

		internal static int GetNumVanilla(SoundType type) {
			switch (type) {
				case SoundType.Custom:
					return 0;
				case SoundType.Item:
					return SoundID.ItemSoundCount;
				case SoundType.NPCHit:
					return SoundID.NPCHitCount;
				case SoundType.NPCKilled:
					return SoundID.NPCDeathCount;
			}

			return 0;
		}

		internal static Asset<SoundEffect>[] GetSoundArray(SoundType type) {
			if (!SoundEngine.IsAudioSupported)
				return null;

			switch (type) {
				case SoundType.Custom:
					return customSounds;
				case SoundType.Item:
					return SoundEngine.LegacySoundPlayer.SoundItem;
				case SoundType.NPCHit:
					return SoundEngine.LegacySoundPlayer.SoundNpcHit;
				case SoundType.NPCKilled:
					return SoundEngine.LegacySoundPlayer.SoundNpcKilled;
			}

			return null;
		}

		internal static SoundEffectInstance[] GetSoundInstanceArray(SoundType type) {
			if (!SoundEngine.IsAudioSupported)
				return null;

			switch (type) {
				case SoundType.Custom:
					return customSoundInstances;
				case SoundType.Item:
					return SoundEngine.LegacySoundPlayer.SoundInstanceItem;
				case SoundType.NPCHit:
					return SoundEngine.LegacySoundPlayer.SoundInstanceNpcHit;
				case SoundType.NPCKilled:
					return SoundEngine.LegacySoundPlayer.SoundInstanceNpcKilled;
			}

			return null;
		}
	}
}
