using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	public sealed class MusicLoader : ILoader
	{
		internal static readonly Dictionary<int, int> musicToItem = new();
		internal static readonly Dictionary<int, int> itemToMusic = new();
		internal static readonly Dictionary<int, Dictionary<int, int>> tileToMusic = new();
		internal static readonly Dictionary<string, int> musicByPath = new();
		internal static readonly Dictionary<string, string> musicExtensions = new();

		public static int MusicCount { get; private set; } = MusicID.Count;

		internal static void AutoloadMusic(Mod mod) {
			if (mod.File is null)
				return;

			List<string> extensions = new List<string> {".wav", ".mp3", ".ogg"};

			foreach (TmodFile.FileEntry music in mod.File.Where(x => extensions.Contains(Path.GetExtension(x.Name)) && x.Name.StartsWith("Sounds/"))) {
				string substring = music.Name["Sounds/".Length..];

				if (substring.StartsWith("Music/")) {
					mod.AddMusic(mod.Name + '/' + music.Name);
				}
			}
		}

		internal static int ReserveMusicID() => MusicCount++;

		internal static IAudioTrack LoadMusic(string path, string extension) {
			path = $"tmod:{path}{extension}";

			Stream stream = ModContent.OpenRead(path);

			switch (extension) {
				case ".wav":
					return new WAVAudioTrack(stream);
				case ".mp3":
					return new MP3AudioTrack(stream);
				case ".ogg":
					return new OGGAudioTrack(stream);
				default:
					throw new ResourceLoadException($"Unknown music extension {extension}");
			}
		}

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
			musicByPath.Clear();
			musicExtensions.Clear();
			MusicCount = MusicID.Count;
		}
	}
}
