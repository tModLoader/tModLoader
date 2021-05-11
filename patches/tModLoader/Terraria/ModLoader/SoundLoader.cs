using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary> This class is used to keep track of and support the existence of custom sounds that have been added to the game. </summary>
	//TODO: Load asynchronously and on demand.
	public static class SoundLoader
	{
		private class SoundData
		{
			public Asset<SoundEffect> soundEffect;
			public SoundEffectInstance soundEffectInstance;
		}

		/// <summary> This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound. </summary>
		public const int CustomSoundType = 66;

		//Music boxes
		//TODO: Move to MusicLoader?
		internal static readonly IDictionary<int, int> musicToItem = new Dictionary<int, int>();
		internal static readonly IDictionary<int, int> itemToMusic = new Dictionary<int, int>();
		internal static readonly IDictionary<int, IDictionary<int, int>> tileToMusic = new Dictionary<int, IDictionary<int, int>>();

		private static readonly List<SoundData> Sounds = new List<SoundData>();
		private static readonly Dictionary<string, SoundData> SoundsByFullPath = new Dictionary<string, SoundData>();
		private static readonly Dictionary<string, Dictionary<string, SoundData>> SoundsByModAndPath = new Dictionary<string, Dictionary<string, SoundData>>();

		public static int SoundCount => Sounds.Count;

		/// <summary> Gets a SoundEffect Asset object. This throws exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public static Asset<SoundEffect> GetSound(string soundPath) => SoundsByFullPath[soundPath].soundEffect;

		/// <summary> Gets a SoundEffect Asset object. This throws exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public static Asset<SoundEffect> GetSound(string modName, string soundPath) => SoundsByModAndPath[modName][soundPath].soundEffect;

		internal static void AutoloadSounds(Mod mod) {
			string modName = mod.Name;

			foreach (string soundPathWithExtension in mod.Assets.EnumeratePaths<SoundEffect>()) {
				string soundPath = Path.ChangeExtension(soundPathWithExtension, null);
				var data = new SoundData {
					soundEffect = mod.Assets.Request<SoundEffect>(soundPathWithExtension, AssetRequestMode.ImmediateLoad)
				};

				if (!SoundsByModAndPath.TryGetValue(modName, out var modSoundSlots)) {
					SoundsByModAndPath[modName] = modSoundSlots = new Dictionary<string, SoundData>();
				}

				modSoundSlots[soundPath] = data;
				modSoundSlots[soundPathWithExtension] = data;
				SoundsByFullPath[modName + '/' + soundPath] = data;
				SoundsByFullPath[modName + '/' + soundPathWithExtension] = data;

				Sounds.Add(data);
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
			SoundsByFullPath.Clear();

			musicToItem.Clear();
			itemToMusic.Clear();
			tileToMusic.Clear();
		}

		internal static bool PlayModSound(int type, int style, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (type != CustomSoundType || style < 0 || style >= SoundCount) {
				return false;
			}

			var soundData = Sounds[style];

			soundEffectInstance = soundData.soundEffectInstance ??= soundData.soundEffect.Value.CreateInstance();

			soundEffectInstance.Stop();

			soundEffectInstance.Volume = volume;
			soundEffectInstance.Pan = pan;
			soundEffectInstance.Pitch = 0f;

			return true;
		}
	}
}