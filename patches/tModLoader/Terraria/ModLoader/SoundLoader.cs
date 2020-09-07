using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary> This class is used to keep track of and support the existence of custom sounds that have been added to the game. </summary>
	//TODO: Load asynchronously and on demand.
	public static class SoundLoader
	{
		private struct SoundData
		{
			public Asset<SoundEffect> soundEffect;
			public SoundEffectInstance soundEffectInstance;
		}

		/// <summary> This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound. </summary>
		public const int CustomSoundType = 66;

		//Music boxes
		//TODO: Move to MusicLoader?
		internal static readonly IDictionary<int, int> MusicToItem = new Dictionary<int, int>();
		internal static readonly IDictionary<int, int> ItemToMusic = new Dictionary<int, int>();
		internal static readonly IDictionary<int, IDictionary<int, int>> TileToMusic = new Dictionary<int, IDictionary<int, int>>();

		private static readonly List<SoundData> Sounds = new List<SoundData>();
		private static readonly Dictionary<string, LegacySoundStyle> SoundSlotByFullPath = new Dictionary<string, LegacySoundStyle>();
		private static readonly Dictionary<string, Dictionary<string, LegacySoundStyle>> SoundSlotByModAndPath = new Dictionary<string, Dictionary<string, LegacySoundStyle>>();

		public static int SoundCount => Sounds.Count;

		//Get

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. This throws exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public static LegacySoundStyle GetSoundSlot(string soundPath) => SoundSlotByFullPath[soundPath];

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. This throws exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public static LegacySoundStyle GetSoundSlot(string modName, string soundPath) => SoundSlotByModAndPath[modName][soundPath];

		//TryGet

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. </summary>
		internal static bool TryGetSoundSlot(string soundPath, out LegacySoundStyle result) => SoundSlotByFullPath.TryGetValue(soundPath, out result);

		/// <summary> Gets a LegacySoundStyle object which encapsulates both the custom sound type and this sound's id as style. </summary>
		internal static bool TryGetSoundSlot(string modName, string soundPath, out LegacySoundStyle result) {
			if (!SoundSlotByModAndPath.TryGetValue(modName, out var subDict)) {
				result = default;

				return false;
			}

			return subDict.TryGetValue(soundPath, out result);
		}

		private static void Add(string path,SoundData soundData) {
			SoundSlotByFullPath[path] = new LegacySoundStyle(CustomSoundType, SoundCount);

			Sounds.Add(soundData);
		}

		internal static void Autoload(Mod mod) {
			foreach (string soundPath in mod.Assets.EnumeratePaths<SoundEffect>()) {
				Add(soundPath,new SoundData {
					soundEffect = mod.Assets.Request<SoundEffect>(soundPath, AssetRequestMode.ImmediateLoad)
				});
			}

			/*foreach (string music in musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music.Substring("Sounds/".Length);

				if (substring.StartsWith("Music/")) {
					AddSound(SoundType.Music, Name + '/' + music);
				}
			}*/
		}

		internal static void ResizeAndFillArrays() {
			//Array.Resize(ref Main.music, SoundCount[SoundType.Music]);
			//Array.Resize(ref Main.musicFade, SoundCount[SoundType.Music]);
		}

		internal static void Unload() {
			/*for (int i = Main.maxMusic; i < Main.music.Length; i++) {
				Main.music[i].Stop(AudioStopOptions.Immediate);
			}*/

			Sounds.Clear();
			SoundSlotByFullPath.Clear();

			MusicToItem.Clear();
			ItemToMusic.Clear();
			TileToMusic.Clear();
		}

		internal static bool PlayModSound(int type, int style, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (type != CustomSoundType || style < 0 || style >= SoundCount) {
				return false;
			}

			var soundData = Sounds[style];
			ref var soundInstance = ref soundData.soundEffectInstance;

			if (soundInstance == null) {
				soundInstance = soundData.soundEffect.Value.CreateInstance();
			}

			soundEffectInstance.Volume = volume;
			soundEffectInstance.Pan = pan;
			soundEffectInstance.Pitch = 0f;
			soundEffectInstance.IsLooped = false;

			return true;
		}
	}
}
