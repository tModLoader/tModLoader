using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public sealed class MusicLoader : ILoader
	{
		internal static readonly Dictionary<int, int> musicToItem = new();
		internal static readonly Dictionary<int, int> itemToMusic = new();
		internal static readonly Dictionary<int, Dictionary<int, int>> tileToMusic = new();
		internal static readonly Dictionary<string, int> musicByPath = new();

		public static int MusicCount { get; private set; } = MusicID.Count;

		internal static void AutoloadMusic(Mod mod) {
			foreach (string music in mod.musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music["Sounds/".Length..];

				if (substring.StartsWith("Music/")) {
					mod.AddMusic(mod.Name + '/' + music);
				}
			}
		}

		internal static int ReserveMusicID() => MusicCount++;

		public static int GetMusicSlot(string sound) {
			if (musicByPath.ContainsKey(sound)) {
				return musicByPath[sound];
			}
			
			return 0;
		}

		void ILoader.ResizeArrays() {
			if (Main.audioSystem is LegacyAudioSystem legacyAudioSystem) {
				Array.Resize(ref legacyAudioSystem.AudioTracks, MusicCount);
				Main.audioSystem = legacyAudioSystem;
			}

			foreach (string sound in musicByPath.Keys) {
				int slot = GetMusicSlot(sound);

				if (Main.audioSystem is DisabledAudioSystem)
					return;

				((LegacyAudioSystem) Main.audioSystem).AudioTracks[slot] = ModContent.GetMusic(sound);
			}
		}

		void ILoader.Unload() {
			musicToItem.Clear();
			itemToMusic.Clear();
			tileToMusic.Clear();
		}
	}
}
