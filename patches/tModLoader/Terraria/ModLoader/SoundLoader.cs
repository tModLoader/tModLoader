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
		/// <summary> This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound. </summary>
		public const int CustomSoundType = 50;

		internal static readonly IDictionary<string, int> sounds = new Dictionary<string, int>();
		internal static readonly IDictionary<int, ModSound> modSounds = new Dictionary<int, ModSound>();
		//Music boxes
		internal static readonly IDictionary<int, int> musicToItem = new Dictionary<int, int>();
		internal static readonly IDictionary<int, int> itemToMusic = new Dictionary<int, int>();
		internal static readonly IDictionary<int, IDictionary<int, int>> tileToMusic = new Dictionary<int, IDictionary<int, int>>();

		internal static Asset<SoundEffect>[] customSounds = new Asset<SoundEffect>[0];
		internal static SoundEffectInstance[] customSoundInstances = new SoundEffectInstance[0];

		public static int SoundCount { get; private set; }

		internal static int ReserveSoundID() => SoundCount++;

		//Get

		/// <summary> Gets the style (last parameter passed to Main.PlaySound) of the sound corresponding to the given sound file path. This throws exceptions on failure. </summary>
		public static int GetSoundSlot(string soundPath) => sounds[soundPath];

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. This throws exceptions on failure. </summary>
		internal static LegacySoundStyle GetLegacySoundSlot(string soundPath) => new LegacySoundStyle(CustomSoundType, sounds[soundPath]);

		//TryGet

		/// <summary> Safely attempts to get the style (last parameter passed to Main.PlaySound) of the sound corresponding to the given sound file path. This throws exceptions on failure. </summary>
		public static bool TryGetSoundSlot(string soundPath, out int result) => sounds.TryGetValue(soundPath, out result);

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. This throws exceptions on failure. </summary>
		internal static bool TryGetLegacySoundSlot(string soundPath, out LegacySoundStyle result) {
			if(sounds.TryGetValue(soundPath, out int id)) {
				result = new LegacySoundStyle(CustomSoundType, id);

				return true;
			}

			result = default;

			return false;
		}

		internal static void ResizeAndFillArrays() {
			customSounds = new Asset<SoundEffect>[SoundCount];
			customSoundInstances = new SoundEffectInstance[SoundCount];
			
			//Array.Resize(ref Main.music, SoundCount[SoundType.Music]);
			//Array.Resize(ref Main.musicFade, SoundCount[SoundType.Music]);

			foreach (var pair in sounds) {
				string soundPath = pair.Key;
				int slot = pair.Value;

				customSounds[slot] = ModContent.GetSound(soundPath);
				customSoundInstances[slot] = customSounds[slot]?.Value.CreateInstance() ?? null;
			}
		}

		internal static void Unload() {
			/*for (int i = Main.maxMusic; i < Main.music.Length; i++) {
				Main.music[i].Stop(AudioStopOptions.Immediate);
			}*/

			SoundCount = 0;
			sounds.Clear();
			modSounds.Clear();

			musicToItem.Clear();
			itemToMusic.Clear();
			tileToMusic.Clear();
		}
		internal static bool PlayModSound(int type, int style, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (type != CustomSoundType) {
				return false;
			}

			if (!modSounds.TryGetValue(style, out var modSound)) {
				return false;
			}

			soundEffectInstance = modSound.PlaySound(ref customSoundInstances[style], volume, pan);
			
			return true;
		}
	}
}
