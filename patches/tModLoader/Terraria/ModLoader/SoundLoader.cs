using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary> This class is used to keep track of and support the existence of custom sounds that have been added to the game. </summary>
	//TODO: Load asynchronously and on demand.
	public static class SoundLoader
	{
		private class SoundData
		{
			public SoundEffect soundEffect;
			public SoundEffectInstance soundEffectInstance;
		}

		/// <summary> This value should be passed as the first parameter to Main.PlaySound whenever you want to play a custom sound. </summary>
		public const int CustomSoundType = 66;

		//Music boxes
		//TODO: Move to MusicLoader?
		internal static readonly IDictionary<int, int> musicToItem = new Dictionary<int, int>();
		internal static readonly IDictionary<int, int> itemToMusic = new Dictionary<int, int>();
		internal static readonly IDictionary<int, IDictionary<int, int>> tileToMusic = new Dictionary<int, IDictionary<int, int>>();

		private static readonly Dictionary<Asset<SoundEffect>, SoundData> soundEffectInstances = new();

		internal static void AutoloadSounds(Mod mod) {
			// do some preloading here to avoid stuttering when playing a sound ingame
			foreach (string path in mod.RootContentSource.EnumerateAssets().Where(s => s.Contains("Sounds"+Path.DirectorySeparatorChar))) {
				mod.Assets.Request<SoundEffect>(path, AssetRequestMode.AsyncLoad);
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

			foreach (var soundData in soundEffectInstances.Values)
				soundData.soundEffectInstance?.Dispose();

			soundEffectInstances.Clear();

			musicToItem.Clear();
			itemToMusic.Clear();
			tileToMusic.Clear();
		}

		internal static bool PlayModSound(Asset<SoundEffect> sound, float volume, float pan, ref SoundEffectInstance soundEffectInstance) {
			if (!soundEffectInstances.TryGetValue(sound, out var soundData))
				soundEffectInstances[sound] = soundData = new();

			soundData.soundEffectInstance?.Stop();

			var soundEffect = sound.Value;
			if (soundData.soundEffect != soundEffect) { // if the asset's value has been changed, recreate the underlying instance
				soundData.soundEffectInstance?.Dispose();
				soundData.soundEffectInstance = soundEffect.CreateInstance();
			}

			soundEffectInstance = soundData.soundEffectInstance;
			soundEffectInstance.Volume = volume;
			soundEffectInstance.Pan = pan;
			soundEffectInstance.Pitch = 0f;
			return true;
		}
	}
}