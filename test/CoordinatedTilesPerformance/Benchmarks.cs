using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;

namespace CoordinatedTilesPerformance;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark")]
[SimpleJob, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class Benchmarks
{
	[GlobalSetup]
	public void Setup()
	{
		Terraria.Program.SavePath = Path.GetTempPath();

		Main.maxTilesX = 8400;
		Main.maxTilesY = 2400;
		Main.tile = Tilemap__ctor((ushort)Main.maxTilesX, (ushort)Main.maxTilesY);

		// Ugh.
		[UnsafeAccessor(UnsafeAccessorKind.Constructor)]
		static extern Tilemap Tilemap__ctor(ushort width, ushort height);

		var random = new Random(12345);
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				if (random.NextDouble() < 0.7)
					continue;

				var tile = Main.tile[x, y];
				tile.HasTile = random.Next(2) != 0;
				tile.TileType = (ushort)random.Next(TileID.Count);
			}
		}

		int count = 0;
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				var tile = Main.tile[x, y];
				if (x > y && tile.HasTile)
					count++;
			}
		}
		Debug.WriteLine($"Dorito Tiles {count}");

		// Dorito Tiles 2592494
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		Main.tile.ClearEverything();
	}

#pragma warning disable IDE0060 // Remove unused parameter
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(Tile tile) { }

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(Tile tile, int humanIRemember, int youreGenocides) { }
#pragma warning restore IDE0060

	[Benchmark(Baseline = true), BenchmarkCategory("Normal")]
	public void Normal()
	{
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				var tile = Main.tile[x, y];
				if (x > y && tile.HasTile) {
					NoInline(tile);
				}
			}
		}
	}

	[Benchmark, BenchmarkCategory("Normal")]
	public void Normal_XYLive()
	{
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				var tile = Main.tile[x, y];
				if (x > y && tile.HasTile) {
					NoInline(tile, x, y);
				}
			}
		}
	}

	[Benchmark(Baseline = true), BenchmarkCategory("LastResort")]
	public void LastResort()
	{
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				var tile = Main.tile[x, y];
				if (tile.X > tile.Y && tile.HasTile) {
					NoInline(tile);
				}
			}
		}
	}

	[Benchmark, BenchmarkCategory("LastResort")]
	public void LastResort_XYLive()
	{
		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				var tile = Main.tile[x, y];
				int x2 = tile.X, y2 = tile.Y;
				if (x2 > y2 && tile.HasTile) {
					NoInline(tile, x2, y2);
				}
			}
		}
	}
}
