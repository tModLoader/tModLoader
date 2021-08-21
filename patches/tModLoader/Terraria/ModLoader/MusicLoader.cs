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
		internal static readonly string[] supportedExtensions = { ".mp3", ".ogg", ".wav" };
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
					AddMusic(mod, mod.Name + '/' + music.Name);
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

		internal static void CloseModStreams(Mod mod) {
			string prefix = $"{mod.Name}/";

			foreach (string musicPath in musicByPath.Keys.Where(x => x.StartsWith(prefix)))
				CloseStream(musicPath);
		}

		internal static void CloseStream(string musicPath) {
			if (Main.audioSystem is not LegacyAudioSystem legacyAudioSystem)
				return;

			int slot = musicByPath[musicPath];

			if (legacyAudioSystem.AudioTracks[slot] is not null)
				legacyAudioSystem.AudioTracks[slot]?.Dispose();
		}

		public static int GetMusicSlot(string musicPath) {
			if (musicByPath.ContainsKey(musicPath)) {
				return musicByPath[musicPath];
			}
			
			return 0;
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool MusicExists(string musicPath) {
			if (!musicPath.Contains('/'))
				return false;

			ModContent.SplitName(musicPath, out string modName, out string _);

			return ModLoader.TryGetMod(modName, out _) && GetMusicSlot(musicPath) != 0;
		}

		/// <summary>
		/// Gets the music with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the music does not exist. Note: SoundMP3 is in the Terraria.ModLoader namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static IAudioTrack GetMusic(string musicPath) {
			if (Main.dedServ)
				return null;

			ModContent.SplitName(musicPath, out string modName, out string subName);

			if (!ModLoader.TryGetMod(modName, out var mod))
				throw new MissingResourceException("Missing mod: " + musicPath);

			int slot = GetMusicSlot(musicPath);

			if (slot == 0 || Main.audioSystem is not LegacyAudioSystem audioSystem)
				return null;

			audioSystem.AudioTracks[slot] ??= LoadMusic(musicPath, musicExtensions[musicPath]);

			return ((LegacyAudioSystem)Main.audioSystem).AudioTracks[slot];
		}

		/// <summary>
		/// Registers a new music track with the provided mod and its local path to the sound file.
		/// </summary>
		/// <param name="mod"> The mod that owns the music track. </param>
		/// <param name="musicPath"> The provided mod's local path to the music track file, case-sensitive and without extensions. </param>
		public static void AddMusic(Mod mod, string musicPath) {
			if (!mod.loading)
				throw new Exception($"{nameof(AddMusic)} can only be called during mod loading.");

			int id = ReserveMusicID();

			string chosenExtension = "";

			// Manually check if a file exists according to the path appended with any of the three supported extensions.
			foreach (string extension in supportedExtensions.Where(extension => mod.FileExists(musicPath + extension)))
				chosenExtension = extension;

			if (string.IsNullOrEmpty(chosenExtension))
				throw new ArgumentException($"Given path found no files matching the extensions [ {string.Join(", ", supportedExtensions)} ]");

			musicPath = $"{mod.Name}/{musicPath}";
			
			musicByPath[musicPath] = id;
			musicExtensions[musicPath] = chosenExtension;
		}

		/// <summary>
		/// Allows you to tie a music ID, and item ID, and a tile ID together to form a music box.
		/// <br/> When music with the given ID is playing, equipped music boxes have a chance to change their ID to the given item type.
		/// <br/> When an item with the given item type is equipped, it will play the music that has musicSlot as its ID.
		/// <br/> When a tile with the given type and Y-frame is nearby, if its X-frame is >= 36, it will play the music that has musicSlot as its ID.
		/// </summary>
		/// <param name="mod"> The music slot. </param>
		/// <param name="musicSlot"> The music slot. </param>
		/// <param name="itemType"> Type of the item. </param>
		/// <param name="tileType"> Type of the tile. </param>
		/// <param name="tileFrameY"> The tile frame y. </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Cannot assign music box to vanilla music id.
		/// or
		/// The provided music id does not exist.
		/// or
		/// Cannot assign music box to a vanilla item id.
		/// or
		/// The provided item id does not exist.
		/// or
		/// Cannot assign music box to a vanilla tile id.
		/// or
		/// The provided tile id does not exist
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The provided music id has already been assigned a music box.
		/// or
		/// The provided item id has already been assigned a music.
		/// or
		/// Y-frame must be divisible by 36
		/// </exception>
		public static void AddMusicBox(Mod mod, int musicSlot, int itemType, int tileType, int tileFrameY = 0) {
			if (!mod.loading)
				throw new Exception($"{nameof(AddMusicBox)} can only be called during mod loading.");

			if (Main.audioSystem == null)
				return;

			if (musicSlot < Main.maxMusic)
				throw new ArgumentOutOfRangeException($"Cannot assign music box to vanilla music ID {musicSlot}");

			if (musicSlot >= MusicCount)
				throw new ArgumentOutOfRangeException($"Music ID {musicSlot} does not exist");

			if (itemType < ItemID.Count)
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla item ID " + itemType);

			if (ItemLoader.GetItem(itemType) == null)
				throw new ArgumentOutOfRangeException($"Item ID {itemType} does not exist");

			if (tileType < TileID.Count)
				throw new ArgumentOutOfRangeException($"Cannot assign music box to vanilla tile ID {tileType}");

			if (TileLoader.GetTile(tileType) == null)
				throw new ArgumentOutOfRangeException($"Tile ID {tileType} does not exist");

			if (musicToItem.ContainsKey(musicSlot))
				throw new ArgumentException($"Music ID {musicSlot} has already been assigned a music box");

			if (itemToMusic.ContainsKey(itemType))
				throw new ArgumentException($"Item ID {itemType} has already been assigned a music");

			if (!tileToMusic.TryGetValue(tileType, out var tileToMusicDictionary))
				tileToMusic[tileType] = tileToMusicDictionary = new Dictionary<int, int>();

			if (tileToMusicDictionary.ContainsKey(tileFrameY))
				throw new ArgumentException($"Y-frame {tileFrameY} of tile type {tileType} has already been assigned a music");

			if (tileFrameY % 36 != 0)
				throw new ArgumentException("Y-frame must be divisible by 36");

			musicToItem[musicSlot] = itemType;
			itemToMusic[itemType] = musicSlot;
			tileToMusic[tileType][tileFrameY] = musicSlot;
		}

		void ILoader.ResizeArrays() {
			if (Main.audioSystem is not LegacyAudioSystem legacyAudioSystem)
				return;

			Array.Resize(ref legacyAudioSystem.AudioTracks, MusicCount);
			Array.Resize(ref Main.musicFade, MusicCount);
			Array.Resize(ref Main.musicNoCrossFade, MusicCount);

			foreach (string sound in musicByPath.Keys) {
				int slot = GetMusicSlot(sound);

				if (Main.audioSystem is DisabledAudioSystem)
					return;

				legacyAudioSystem.AudioTracks[slot] = GetMusic(sound);
			}

			Main.audioSystem = legacyAudioSystem;
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
