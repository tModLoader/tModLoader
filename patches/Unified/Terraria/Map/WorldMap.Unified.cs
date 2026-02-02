using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria.IO;
using Terraria.Social;
using Terraria.Testing;
using Terraria.Utilities;

namespace Terraria.Map;

public readonly struct MapChunk(int chunkSize)
{
	public MapTile[] Tiles { get; } = new MapTile[chunkSize * chunkSize];
}

public sealed class WorldMap(int maxWidth, int maxHeight)
{
	public const int CHUNK_SIZE = 2 << (CHUNK_SHIFT - 1);
	public const int CHUNK_SHIFT = 5;
	public const int CHUNK_MASK = CHUNK_SIZE - 1;

	private const int black_edge_width = 40;

	public int MaxWidth { get; } = maxWidth;

	public int MaxHeight { get; } = maxHeight;

	private readonly Dictionary<long, MapChunk> chunks = [];

	public MapTile this[int x, int y] {
		get {
			int cx = x >> CHUNK_SHIFT;
			int cy = y >> CHUNK_SHIFT;

			if (!chunks.TryGetValue(ChunkKey(cx, cy), out var chunk)) {
				return default;
			}

			return chunk.Tiles[TileIndex(x & CHUNK_MASK, y & CHUNK_MASK)];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ref MapTile GetOrCreateTile(int x, int y)
	{
		int cx = x >> CHUNK_SHIFT;
		int cy = y >> CHUNK_SHIFT;
		long key = ChunkKey(cx, cy);

		if (!chunks.TryGetValue(key, out var chunk)) {
			chunk = new MapChunk(CHUNK_SIZE);
			chunks.Add(key, chunk);
		}

		return ref chunk.Tiles[TileIndex(x & CHUNK_MASK, y & CHUNK_MASK)];
	}

	public void ConsumeUpdate(int x, int y)
	{
		ref MapTile tile = ref GetOrCreateTile(x, y);
		tile.IsChanged = false;
	}

	public void Update(int x, int y, byte light)
	{
		ref MapTile tile = ref GetOrCreateTile(x, y);
		tile = MapHelper.CreateMapTile(x, y, light);
	}

	public void SetTile(int x, int y, ref MapTile tile)
	{
		ref MapTile dst = ref GetOrCreateTile(x, y);
		dst = tile;
	}

	public bool IsRevealed(int x, int y)
	{
		int cx = x >> CHUNK_SHIFT;
		int cy = y >> CHUNK_SHIFT;

		if (!chunks.TryGetValue(ChunkKey(cx, cy), out var chunk)) {
			return false;
		}

		return chunk.Tiles[TileIndex(x & CHUNK_MASK, y & CHUNK_MASK)].Light > 0;
	}

	public bool UpdateLighting(int x, int y, byte light)
	{
		int cx = x >> CHUNK_SHIFT;
		int cy = y >> CHUNK_SHIFT;
		long key = ChunkKey(cx, cy);

		if (!chunks.TryGetValue(key, out var chunk)) {
			if (light == 0)
				return false;

			chunk = new MapChunk(CHUNK_SIZE);
			chunks.Add(key, chunk);
		}

		int index = TileIndex(x & CHUNK_MASK, y & CHUNK_MASK);
		MapTile other = chunk.Tiles[index];

		if (light == 0 && other.Light == 0) {
			return false;
		}

		MapTile updated = MapHelper.CreateMapTile(x, y, Math.Max(other.Light, light));
		if (updated.Equals(other)) {
			return false;
		}

		chunk.Tiles[index] = updated;
		return true;
	}

	public bool UpdateType(int x, int y)
	{
		ref MapTile tile = ref GetOrCreateTile(x, y);
		return UpdateType(x, y, ref tile);
	}

	private bool UpdateType(int x, int y, ref MapTile mapTile)
	{
		if (!mapTile.UpdateQueued) {
			return false;
		}

		mapTile.UpdateQueued = false;

		if (mapTile.Light == 0) {
			return false;
		}

		if (!Main.sectionManager.TileLoaded(x, y)) {
			return false;
		}

		bool isBackground = MapHelper.IsBackground(mapTile.Type);
		MapTile updated = MapHelper.CreateMapTile(
			x,
			y,
			mapTile.Light,
			isBackground ? mapTile.Type : 0
		);

		if (updated.Equals(mapTile)) {
			return false;
		}

		mapTile = updated;
		return true;
	}

	internal bool QueueUpdate(int x, int y)
	{
		ref MapTile tile = ref GetOrCreateTile(x, y);
		if (tile.Light == 0 || tile.UpdateQueued) {
			return false;
		}

		tile.UpdateQueued = true;
		return true;
	}

	public void UnlockMapSection(int sectionX, int sectionY)
	{
		int x0 = Utils.Clamp(sectionX * 200, black_edge_width, Main.maxTilesX - black_edge_width);
		int x1 = Utils.Clamp(x0 + 200, black_edge_width, Main.maxTilesX - black_edge_width);
		int y0 = Utils.Clamp(sectionY * 150, black_edge_width, Main.maxTilesY - black_edge_width);
		int y1 = Utils.Clamp(y0 + 150, black_edge_width, Main.maxTilesY - black_edge_width);

		if (DebugOptions.unlockMap == 2) {
			for (int x = x0; x < x1; x++) {
				for (int y = y0; y < y1; y++) {
					UnlockMapTilePretty(x, y);
				}
			}

			return;
		}

		for (int x = x0; x < x1; x++) {
			for (int y = y0; y < y1; y++) {
				UpdateLighting(x, y, byte.MaxValue);
			}
		}
	}

	public void UnlockMapTilePretty(int x, int y)
	{
		if (!WorldGen.InWorld(x, y, 12) || WorldGen.SolidTile(x, y)) {
			return;
		}

		const int radius = 5;
		float light = 255f;

		Tile tile = Framing.GetTileSafely(x, y);
		if (tile.liquid > 0 && !tile.lava()) {
			return;
		}

		if (tile.wall > 0) {
			light *= 0.8f;
		}

		if (y >= Main.worldSurface) {
			light *= 0.7f;
		}

		for (int dx = -radius; dx <= radius; dx++) {
			for (int dy = -radius; dy <= radius; dy++) {
				float strength = radius - Math.Abs(dx) - Math.Abs(dy);

				if (strength >= 0f) {
					UpdateLighting(x + dx, y + dy, (byte)(light * (strength / radius)));
				}
			}
		}
	}

	public void Load()
	{
		Lighting.Clear();
		bool isCloudSave = Main.ActivePlayerFileData.IsCloudSave;
		if ((isCloudSave && SocialAPI.Cloud == null) || !Main.mapEnabled) {
			return;
		}

		if (!TryGetMapPath(Main.ActivePlayerFileData, Main.ActiveWorldFileData, out var mapPath)) {
			Main.MapFileMetadata = FileMetadata.FromCurrentSettings(FileType.Map);
			return;
		}

		using var input = new MemoryStream(FileUtilities.ReadAllBytes(mapPath, isCloudSave));
		using var binaryReader = new BinaryReader(input);
		try {
			int version = binaryReader.ReadInt32();
			bool compressed = (version & 0x8000) == 32768;
			version &= -32769;

			if (version <= 316) {
				if (compressed) {
					MapHelper.LoadMapVersionCompressed(binaryReader, version);
				}
				else if (version <= 91) {
					MapHelper.LoadMapVersion1(binaryReader, version);
				}
				else {
					MapHelper.LoadMapVersion2(binaryReader, version);
				}

				ClearEdges();
				Main.clearMap = true;
				Main.loadMap = true;
				Main.loadMapLock = true;
				Main.refreshMap = false;
			}
		}
		catch (Exception value) {
			using (var streamWriter = new StreamWriter("client-crashlog.txt", append: true)) {
				streamWriter.WriteLine(DateTime.Now);
				streamWriter.WriteLine(value);
				streamWriter.WriteLine("");
			}

			if (!isCloudSave) {
				File.Copy(mapPath, mapPath + ".bad", overwrite: true);
			}

			Clear();
		}
	}

	public static bool TryGetMapPath(PlayerFileData playerFileData, WorldFileData worldFileData, out string mapPath)
	{
		string text = playerFileData.Path.Substring(0, playerFileData.Path.Length - 4);
		mapPath = text + Path.DirectorySeparatorChar + worldFileData.MapFileName + ".map";
		if (worldFileData.UseGuidAsMapName && !FileUtilities.Exists(mapPath, playerFileData.IsCloudSave)) {
			mapPath = text + Path.DirectorySeparatorChar.ToString() + worldFileData.WorldId + ".map";
		}

		return FileUtilities.Exists(mapPath, playerFileData.IsCloudSave);
	}

	public void Save()
	{
		MapHelper.SaveMap();
	}

	public void Clear()
	{
		chunks.Clear();
	}

	public void ClearEdges()
	{
		int minCx = black_edge_width >> CHUNK_SHIFT;
		int maxCx = (MaxWidth - black_edge_width) >> CHUNK_SHIFT;
		int minCy = black_edge_width >> CHUNK_SHIFT;
		int maxCy = (MaxHeight - black_edge_width) >> CHUNK_SHIFT;

		var toRemove = new List<long>();

		foreach (var pair in chunks) {
			long key = pair.Key;
			int cx = (int)(key >> 32);
			int cy = (int)key;

			if (cx < minCx || cx > maxCx || cy < minCy || cy > maxCy)
				toRemove.Add(key);
		}

		foreach (long key in toRemove)
			chunks.Remove(key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long ChunkKey(int cx, int cy)
	{
		return ((long)cx << 32) | (uint)cy;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int TileIndex(int lx, int ly)
	{
		return lx + ly * CHUNK_SIZE;
	}
}