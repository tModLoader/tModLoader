using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class is used to keep track of and support the existence of custom sounds that have been added to the game.
	/// </summary>
	public static class SoundLoader
	{
		/// <summary>
		/// This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound that is not an item, npcHit, or npcKilled sound.
		/// </summary>
		public static readonly int CustomSoundType = 500;

		private static readonly Dictionary<string, int> sounds = new();
		private static readonly Dictionary<int, ModSound> modSoundsBySoundId = new();
		private static readonly Dictionary<Mod, List<ModSound>> modSoundsByMod = new();

		internal static Asset<SoundEffect>[] customSounds = Array.Empty<Asset<SoundEffect>>();
		internal static SoundEffectInstance[] customSoundInstances = Array.Empty<SoundEffectInstance>();

		public static int SoundCount { get; set; }

		internal static int ReserveSoundID() => SoundCount++;

		/// <summary>
		/// Adds the given sound file to the game as the given type of sound and with the given custom sound playing. If no ModSound instance is provided, the custom sound will play in a similar manner as the default vanilla ones.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="type">The type.</param>
		/// <param name="soundPath">The sound path.</param>
		/// <param name="modSound">The mod sound.</param>
		public static void AddSound(Mod mod, string soundPath, ModSound modSound = null) {
			if (!mod.loading)
				throw new Exception("AddSound can only be called from Mod.Load or Mod.Autoload");

			soundPath = $"{mod.Name}/{soundPath}";

			int id = ReserveSoundID();

			sounds[soundPath] = id;

			if (modSound != null) {
				modSoundsBySoundId[id] = modSound;
				modSound.Sound = ModContent.Request<SoundEffect>(soundPath);
			}
		}

		/// <summary>
		/// Returns the style (last parameter passed to Main.PlaySound) of the sound corresponding to the given sound file path. Returns 0 if there is no corresponding style.
		/// </summary>
		public static int GetSoundSlot(Mod mod, string soundPath)
			=> GetSoundSlot($"{mod.Name}/{soundPath}");

		/// <summary>
		/// Returns the style (last parameter passed to Main.PlaySound) of the sound corresponding to the given full sound file path. Returns 0 if there is no corresponding style.
		/// </summary>
		public static int GetSoundSlot(string fullSoundPath) {
			if (sounds.TryGetValue(fullSoundPath, out int slot)) {
				return slot;
			}

			return -1;
		}

		/// <summary>
		/// Returns a LegacySoundStyle object which encapsulates both a sound type and a sound style (This is the new way to do sounds in 1.3.4) Returns null if there is no corresponding style.
		/// </summary>
		public static LegacySoundStyle GetLegacySoundSlot(Mod mod, string soundPath)
			=> GetLegacySoundSlot($"{mod.Name}/{soundPath}");

		/// <summary>
		/// Returns a LegacySoundStyle object which encapsulates both a sound type and a sound style (This is the new way to do sounds in 1.3.4) Returns null if there is no corresponding style.
		/// </summary>
		public static LegacySoundStyle GetLegacySoundSlot(string sound) {
			if (sounds.TryGetValue(sound, out int slot)) {
				return new LegacySoundStyle(CustomSoundType, slot);
			}

			return null;
		}

		internal static void RegisterModSound(ModSound modSound) {
			if (!modSoundsByMod.TryGetValue(modSound.Mod, out var list)) {
				modSoundsByMod[modSound.Mod] = list = new();
			}

			list.Add(modSound);
		}

		internal static void AutoloadSounds(Mod mod) {
			var modSoundNames = modSoundsByMod.TryGetValue(mod, out var list) ? list.ToDictionary(t => t.FullName) : null;

			foreach (string fullSoundPath in mod.RootContentSource.EnumerateAssets().Where(t => t.StartsWith("Sounds/") || t.Contains("/Sounds/"))) {
				string soundPath = Path.ChangeExtension(fullSoundPath, null);
				ModSound modSound = null;

				modSoundNames?.TryGetValue($"{mod.Name}/{soundPath}".Replace('/', '.'), out modSound);

				AddSound(mod, soundPath, modSound);
			}
		}

		internal static void ResizeAndFillArrays() {
			if (!SoundEngine.IsAudioSupported)
				return;

			customSounds = new Asset<SoundEffect>[SoundCount];
			customSoundInstances = new SoundEffectInstance[SoundCount];

			foreach (string soundName in sounds.Keys) {
				int slot = GetSoundSlot(soundName);
				var sound = ModContent.Request<SoundEffect>(soundName, AssetRequestMode.ImmediateLoad);

				customSounds[slot] = sound;
				customSoundInstances[slot] = sound.Value.CreateInstance();
			}
		}

		internal static void Unload() {
			SoundCount = 0;

			sounds.Clear();
			modSoundsBySoundId.Clear();
		}

		internal static bool PlayModSound(int type, int style, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (!SoundEngine.IsAudioSupported)
				return false;

			if (!modSoundsBySoundId.TryGetValue(style, out var modSound))
				return false;

			soundEffectInstance = modSound.PlaySound(ref customSoundInstances[style], volume, pan);

			return true;
		}
	}
}
