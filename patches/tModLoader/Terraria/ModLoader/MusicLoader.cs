using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader;

public sealed class MusicLoader : ILoader
{
	internal static readonly string[] supportedExtensions = { ".mp3", ".ogg", ".wav" };

	/// <summary>Unloaded server side </summary>
	internal static readonly Dictionary<int, int> musicToItem = new();
	/// <summary>Unloaded server side </summary>
	internal static readonly Dictionary<int, int> itemToMusic = new();
	/// <summary>Only Loads the two keys, Tile type and Tile Y frame server side, the value is set to 0</summary>
	internal static readonly Dictionary<int, Dictionary<int, int>> tileToMusic = new();
	internal static readonly Dictionary<string, int> musicByPath = new();
	internal static readonly Dictionary<string, string> musicExtensions = new();
	internal static readonly Dictionary<int, bool> musicSkipsVolumeRemap = new();

	public static int MusicCount { get; private set; } = MusicID.Count;

	/// <summary> Gets the music id of the track with the specified mod path. The path must not have a file extension.
	/// <para/> <include file = 'CommonDocs.xml' path='Common/MusicAutoloadReminder' /> </summary>
	public static int GetMusicSlot(Mod mod, string musicPath)
		=> GetMusicSlot($"{mod.Name}/{musicPath}");

	/// <summary> Gets the music id of the track with the specified full path. The path must be prefixed with a mod name and must not have a file extension.
	/// <para/> <include file = 'CommonDocs.xml' path='Common/MusicAutoloadReminder' /> </summary>
	public static int GetMusicSlot(string musicPath)
	{
		if (musicByPath.ContainsKey(musicPath)) {
			return musicByPath[musicPath];
		}

		return 0;
	}

	/// <summary> Returns whether or not a music track with the specified mod path exists. The path must not have a file extension. </summary>
	public static bool MusicExists(Mod mod, string musicPath)
		=> MusicExists($"{mod.Name}/{musicPath}");

	/// <summary> Returns whether or not a music track with the specified path exists. The path must be prefixed with a mod name and must not have a file extension.</summary>
	public static bool MusicExists(string musicPath)
		=> GetMusicSlot(musicPath) > 0;

	/// <summary> Gets the music track with the specified mod path. The path must not have a file extension. </summary>
	public static IAudioTrack GetMusic(Mod mod, string musicPath)
		=> GetMusic($"{mod.Name}/{musicPath}");

	/// <summary> Gets the music track with the specified full path. The path must be prefixed with a mod name and must not have a file extension. </summary>
	public static IAudioTrack GetMusic(string musicPath)
	{
		if (Main.dedServ)
			return null;

		int slot = GetMusicSlot(musicPath);

		if (slot == 0 || Main.audioSystem is not LegacyAudioSystem audioSystem)
			return null;

		audioSystem.AudioTracks[slot] ??= LoadMusic(musicPath, musicExtensions[musicPath]);

		return ((LegacyAudioSystem)Main.audioSystem).AudioTracks[slot];
	}

	/// <summary>
	/// Registers a new music track with the provided mod and its local path to the sound file.
	/// <para/> Use this for any music not autoloaded by the <see cref="Mod.MusicAutoloadingEnabled"/> logic.
	/// </summary>
	/// <param name="mod"> The mod that owns the music track. </param>
	/// <param name="musicPath"> The provided mod's local path to the music track file, case-sensitive and without extensions. </param>
	public static void AddMusic(Mod mod, string musicPath)
	{
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
		musicSkipsVolumeRemap[id] = mod.MusicSkipsVolumeRemap;

		MusicID.Search.Add(musicPath, id);
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
	public static void AddMusicBox(Mod mod, int musicSlot, int itemType, int tileType, int tileFrameY = 0)
	{
		//if (!mod.loading)
		//	throw new Exception($"{nameof(AddMusicBox)} can only be called during mod loading.");

		if (musicSlot < Main.maxMusic && !(Main.dedServ && musicSlot == 0)) {
			if (musicSlot == 0)
				throw new ArgumentOutOfRangeException($"An invalid music audio file was provided. Note that when using GetMusicSlot the file extension should not be included and that by default only .mp3, .wav, and .ogg are supported audio file formats. Double check the GetMusicSlot documentation to ensure that the path you are providing matches the expected input.");
			throw new ArgumentOutOfRangeException($"Cannot assign music box to vanilla music ID {musicSlot}");
		}

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

		if (!Main.dedServ) {
			musicToItem[musicSlot] = itemType;
			itemToMusic[itemType] = musicSlot;
		}
		tileToMusic[tileType][tileFrameY] = musicSlot;
	}

	internal static void AutoloadMusic(Mod mod)
	{
		if (mod.File is null)
			return;

		foreach (string musicPath in mod.RootContentSource.EnumerateAssets().Where(path => supportedExtensions.Contains(Path.GetExtension(path)) && (path.StartsWith("Music/") || path.Contains("/Music/")))) {
			AddMusic(mod, Path.ChangeExtension(musicPath, null));
		}
	}

	internal static int ReserveMusicID() => MusicCount++;

	internal static IAudioTrack LoadMusic(string path, string extension)
	{
		path = $"tmod:{path}{extension}";

		Stream stream = ModContent.OpenRead(path, true);

		return extension switch {
			".wav" => new WAVAudioTrack(stream),
			".mp3" => new MP3AudioTrack(stream),
			".ogg" => new OGGAudioTrack(stream),
			_ => throw new ResourceLoadException($"Unknown music extension {extension}"),
		};
	}

	internal static void CloseModStreams(Mod mod)
	{
		if (!Program.IsMainThread) {
			Main.RunOnMainThread(() => CloseModStreams(mod)).GetAwaiter().GetResult();
			return;
		}

		string prefix = $"{mod.Name}/";

		foreach (string musicPath in musicByPath.Keys.Where(x => x.StartsWith(prefix)))
			CloseStream(musicPath);
	}

	internal static void CloseStream(string musicPath)
	{
		if (Main.audioSystem is not LegacyAudioSystem legacyAudioSystem)
			return;

		int slot = musicByPath[musicPath];
		if (slot < legacyAudioSystem.AudioTracks.Length)
			legacyAudioSystem.AudioTracks[slot]?.Dispose();
	}

	void ILoader.ResizeArrays()
	{
		if (Main.audioSystem is not LegacyAudioSystem legacyAudioSystem)
			return;

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(MusicID));

		//Etc
		Array.Resize(ref legacyAudioSystem.AudioTracks, MusicCount);
		Array.Resize(ref Main.musicFade, MusicCount);
		Array.Resize(ref Main.musicNoCrossFade, MusicCount);

		foreach (string sound in musicByPath.Keys) {
			int slot = GetMusicSlot(sound);

			if (Main.audioSystem is DisabledAudioSystem)
				return;

			legacyAudioSystem.AudioTracks[slot] = GetMusic(sound);
			MusicID.Sets.SkipsVolumeRemap[slot] = musicSkipsVolumeRemap[slot];
		}

		Main.audioSystem = legacyAudioSystem;
	}

	void ILoader.Unload()
	{
		musicToItem.Clear();
		itemToMusic.Clear();
		tileToMusic.Clear();
		musicByPath.Clear();
		musicExtensions.Clear();
		musicSkipsVolumeRemap.Clear();
		MusicCount = MusicID.Count;
	}
}
