using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

//todo: further documentation
/// <summary>
/// This serves as the central class from which wall-related functions are supported and carried out.
/// </summary>
public static class WallLoader
{
	private static int nextWall = WallID.Count;
	internal static readonly IList<ModWall> walls = new List<ModWall>();
	internal static readonly IList<GlobalWall> globalWalls = new List<GlobalWall>();
	/// <summary> Maps Wall type to the Item type that places the wall. </summary>
	internal static readonly Dictionary<int, int> wallTypeToItemType = new();
	public delegate bool ConvertWall(int i, int j, int type, int conversionType);
	internal static List<ConvertWall>[][] wallConversionDelegates = null;
	internal static int[][] wallConversionFallbacks = null;
	private static bool loaded = false;

	private static Func<int, int, int, bool, bool>[] HookKillSound;
	private delegate void DelegateNumDust(int i, int j, int type, bool fail, ref int num);
	private static DelegateNumDust[] HookNumDust;
	private delegate bool DelegateCreateDust(int i, int j, int type, ref int dustType);
	private static DelegateCreateDust[] HookCreateDust;
	private delegate bool DelegateDrop(int i, int j, int type, ref int dropType);
	private static DelegateDrop[] HookDrop;
	private delegate void DelegateKillWall(int i, int j, int type, ref bool fail);
	private static DelegateKillWall[] HookKillWall;
	private static Func<int, int, int, bool>[] HookCanPlace;
	private static Func<int, int, int, bool>[] HookCanExplode;
	private static Func<int, int, int, Player, string, bool>[] HookCanBeTeleportedTo;
	private delegate void DelegateModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
	private static DelegateModifyLight[] HookModifyLight;
	private static Action<int, int, int>[] HookRandomUpdate;
	private delegate bool DelegateWallFrame(int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber);
	private static DelegateWallFrame[] HookWallFrame;
	private static Func<int, int, int, SpriteBatch, bool>[] HookPreDraw;
	private static Action<int, int, int, SpriteBatch>[] HookPostDraw;
	private static Action<int, int, int, Item>[] HookPlaceInWorld;
	private static Action<int, int, int, int, int>[] HookOnWallConverted;

	internal static int ReserveWallID()
	{
		int reserveID = nextWall;
		nextWall++;
		return reserveID;
	}

	public static int WallCount => nextWall;

	/// <summary>
	/// Gets the ModWall instance with the given type. If no ModWall with the given type exists, returns null.
	/// </summary>
	public static ModWall GetWall(int type)
	{
		return type >= WallID.Count && type < WallCount ? walls[type - WallID.Count] : null;
	}

	private static void Resize2DArray<T>(ref T[,] array, int newSize)
	{
		int dim1 = array.GetLength(0);
		int dim2 = array.GetLength(1);
		T[,] newArray = new T[newSize, dim2];
		for (int j = 0; j < newSize && j < dim1; j++) {
			for (int k = 0; k < dim2; k++) {
				newArray[j, k] = array[j, k];
			}
		}
		array = newArray;
	}

	internal static void ResizeArrays(bool unloading = false)
	{
		//Textures
		Array.Resize(ref TextureAssets.Wall, nextWall);

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(WallID));

		//Etc
		Array.Resize(ref Main.wallHouse, nextWall);
		Array.Resize(ref Main.wallDungeon, nextWall);
		Array.Resize(ref Main.wallLight, nextWall);
		Array.Resize(ref Main.wallBlend, nextWall);
		for (int k = WallID.Count; k < nextWall; k++) {
			Main.wallBlend[k] = k;
		}
		Array.Resize(ref Main.wallLargeFrames, nextWall);
		Array.Resize(ref Main.wallFrame, nextWall);
		Array.Resize(ref Main.wallFrameCounter, nextWall);

		wallConversionDelegates = new List<ConvertWall>[nextWall][];
		wallConversionFallbacks = new int[nextWall][];
		InitializeConversionFallbacks();

		// .NET 6 SDK bug: https://github.com/dotnet/roslyn/issues/57517
		// Remove generic arguments once fixed.
		ModLoader.BuildGlobalHook(ref HookKillSound, globalWalls, g => g.KillSound);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateNumDust>(ref HookNumDust, globalWalls, g => g.NumDust);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateCreateDust>(ref HookCreateDust, globalWalls, g => g.CreateDust);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateDrop>(ref HookDrop, globalWalls, g => g.Drop);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateKillWall>(ref HookKillWall, globalWalls, g => g.KillWall);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateWallFrame>(ref HookWallFrame, globalWalls, g => g.WallFrame);
		ModLoader.BuildGlobalHook(ref HookCanPlace, globalWalls, g => g.CanPlace);
		ModLoader.BuildGlobalHook(ref HookCanExplode, globalWalls, g => g.CanExplode);
		ModLoader.BuildGlobalHook(ref HookCanBeTeleportedTo, globalWalls, g => g.CanBeTeleportedTo);
		ModLoader.BuildGlobalHook<GlobalWall, DelegateModifyLight>(ref HookModifyLight, globalWalls, g => g.ModifyLight);
		ModLoader.BuildGlobalHook(ref HookRandomUpdate, globalWalls, g => g.RandomUpdate);
		ModLoader.BuildGlobalHook(ref HookPreDraw, globalWalls, g => g.PreDraw);
		ModLoader.BuildGlobalHook(ref HookPostDraw, globalWalls, g => g.PostDraw);
		ModLoader.BuildGlobalHook(ref HookPlaceInWorld, globalWalls, g => g.PlaceInWorld);
		ModLoader.BuildGlobalHook(ref HookOnWallConverted, globalWalls, g => g.OnWallConverted);

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		walls.Clear();
		nextWall = WallID.Count;
		globalWalls.Clear();
		wallTypeToItemType.Clear();
		wallConversionDelegates = null;
	}

	//change type of Terraria.Tile.wall to ushort and fix associated compile errors
	//in Terraria.IO.WorldFile.SaveWorldTiles increase length of array by 1 from 13 to 14
	//in Terraria.IO.WorldFile.SaveWorldTiles inside block if (tile.wall != 0) after incrementing num2
	//  call WallLoader.WriteType(tile.wall, array, ref num2, ref b3);
	internal static void WriteType(ushort wall, byte[] data, ref int index, ref byte flags)
	{
		if (wall > 255) {
			data[index] = (byte)(wall >> 8);
			index++;
			flags |= 32;
		}
	}
	//in Terraria.IO.WorldFile.LoadWorldTiles after setting tile.wall call
	//  WallLoader.ReadType(ref tile.wall, reader, b, modWalls);
	//in Terraria.IO.WorldFile.ValidateWorld before if ((b2 & 16) == 16)
	//  replace fileIO.ReadByte(); with ushort wall = fileIO.ReadByte();
	//  ushort _ = 0; WallLoader.ReadType(ref wall, fileIO, b2, new Dictionary<int, int>());
	internal static void ReadType(ref ushort wall, BinaryReader reader, byte flags, IDictionary<int, int> wallTable)
	{
		if ((flags & 32) == 32) {
			wall |= (ushort)(reader.ReadByte() << 8);
		}
		if (wallTable.ContainsKey(wall)) {
			wall = (ushort)wallTable[wall];
		}
	}

	public static bool KillSound(int i, int j, int type, bool fail)
	{
		foreach (var hook in HookKillSound) {
			if (!hook(i, j, type, fail))
				return false;
		}

		var modWall = GetWall(type);

		if (modWall != null) {
			if (!modWall.KillSound(i, j, fail))
				return false;

			SoundEngine.PlaySound(modWall.HitSound, new Vector2(i * 16, j * 16));

			return false;
		}

		return true;
	}
	//in Terraria.WorldGen.KillWall after if statement setting num to 3 add
	//  WallLoader.NumDust(i, j, tile.wall, fail, ref num);
	public static void NumDust(int i, int j, int type, bool fail, ref int numDust)
	{
		GetWall(type)?.NumDust(i, j, fail, ref numDust);

		foreach (var hook in HookNumDust) {
			hook(i, j, type, fail, ref numDust);
		}
	}

	public static bool CreateDust(int i, int j, int type, ref int dustType)
	{
		foreach (var hook in HookCreateDust) {
			if (!hook(i, j, type, ref dustType)) {
				return false;
			}
		}
		return GetWall(type)?.CreateDust(i, j, ref dustType) ?? true;
	}

	//in Terraria.WorldGen.KillWall replace if (num4 > 0) with
	//  if (WallLoader.Drop(i, j, tile.wall, ref num4) && num4 > 0)
	public static bool Drop(int i, int j, int type, ref int dropType)
	{
		foreach (var hook in HookDrop) {
			if (!hook(i, j, type, ref dropType)) {
				return false;
			}
		}
		ModWall modWall = GetWall(type);
		if (modWall != null) {
			if (wallTypeToItemType.TryGetValue(type, out int value)) {
				dropType = value;
			}
			return modWall.Drop(i, j, ref dropType);
		}
		return true;
	}
	//in Terraria.WorldGen.KillWall after if statements setting fail to true call
	//  WallLoader.KillWall(i, j, tile.wall, ref fail);
	public static void KillWall(int i, int j, int type, ref bool fail)
	{
		GetWall(type)?.KillWall(i, j, ref fail);

		foreach (var hook in HookKillWall) {
			hook(i, j, type, ref fail);
		}
	}

	//in Terraria.Player.PlaceThing_Walls after bool flag = true;, before PlaceThing_TryReplacingWalls
	//  flag &= WallLoader.CanPlace(tileTargetX, tileTargetY, inventory[selectedItem].createWall);
	public static bool CanPlace(int i, int j, int type)
	{
		foreach (var hook in HookCanPlace) {
			if (!hook(i, j, type)) {
				return false;
			}
		}
		return GetWall(type)?.CanPlace(i, j) ?? true;
	}

	public static bool CanExplode(int i, int j, int type)
	{
		foreach (var hook in HookCanExplode) {
			if (!hook(i, j, type)) {
				return false;
			}
		}
		return GetWall(type)?.CanExplode(i, j) ?? true;
	}

	public static bool CanBeTeleportedTo(int i, int j, int type, Player player, string context)
	{
		foreach (var hook in HookCanBeTeleportedTo) {
			if (!hook(i, j, type, player, context)) {
				return false;
			}
		}
		return GetWall(type)?.CanBeTeleportedTo(i, j, player, context) ?? true;
	}

	//in Terraria.Lighting.PreRenderPhase after wall modifies light call
	//  WallLoader.ModifyLight(n, num17, wall, ref num18, ref num19, ref num20);
	public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
	{
		GetWall(type)?.ModifyLight(i, j, ref r, ref g, ref b);

		foreach (var hook in HookModifyLight) {
			hook(i, j, type, ref r, ref g, ref b);
		}
	}

	/// <summary>
	/// Registers a wall type as having custom biome conversion code for this specific <see cref="BiomeConversionID"/>. For modded walls, you can directly use <see cref="Convert"/> <br/>
	/// If you need to register conversions that rely on <see cref="WallID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="wallType">The wall type that has is affected by this custom conversion.</param>
	/// <param name="conversionType">The conversion type for which the wall should use custom conversion code.</param>
	/// <param name="conversionDelegate">Code to run when the wall attempts to get converted. Return false to signal that your custom conversion took place and that vanilla code shouldn't be ran.</param>
	public static void RegisterConversion(int wallType, int conversionType, ConvertWall conversionDelegate)
	{
		if (wallConversionDelegates == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "WallLoader.RegisterConversion"));

		var conversions = wallConversionDelegates[wallType] ??= new List<ConvertWall>[BiomeConversionLoader.BiomeConversionCount];
		var list = conversions[conversionType] ??= new();
		list.Add(conversionDelegate);
	}

	/// <summary>
	/// Registers a wall type as having custom biome conversion code for this specific <see cref="BiomeConversionID"/>. For modded walls, you can directly use <see cref="Convert"/> <br/>
	/// If you need to register conversions that rely on <see cref="WallID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="wallType">The wall type that has is affected by this custom conversion.</param>
	/// <param name="conversionType">The conversion type for which the wall should use custom conversion code.</param>
	/// <param name="toType">What <paramref name="wallType"/> is converted into when it's hit with the <paramref name="conversionType"/>.</param>
	public static void RegisterConversion(int wallType, int conversionType, int toType)
	{
		RegisterConversion(wallType, conversionType, (int i, int j, int type, int conversionType) => {
			WorldGen.ConvertWall(i, j, toType);
			return false;
		});
	}

	/// <summary>
	/// Registers a conversion that replaces <paramref name="wallType"/> with <paramref name="toType"/> when touched by <paramref name="conversionType"/> <br/>
	/// Also registers <paramref name="wallType"/> as a fallback for <paramref name="toType"/> so that other conversions can convert <paramref name="toType"/> as if it was <paramref name="wallType"/>. <br/>
	/// If you need to register conversions that rely on <see cref="WallID.Sets.Conversion"/> being fully populated, consider doing it in <see cref="ModBiomeConversion.PostSetupContent"/>
	/// </summary>
	/// <param name="wallType">The wall type that has is affected by this conversion.</param>
	/// <param name="conversionType">The conversion type for which the wall should use this conversion.</param>
	/// <param name="toType">The wall type that this conversion should convert the wall to.</param>
	/// <param name="purification">If true, automatically registers purification conversions from toType to wallType as well.</param>
	public static void RegisterSimpleConversion(int wallType, int conversionType, int toType, bool purification = true)
	{
		RegisterConversion(wallType, conversionType, (int i, int j, int type, int conversionType) => {
			WorldGen.ConvertWall(i, j, toType);
			return false;
		});
		RegisterConversionFallback(toType, wallType, conversionType);

		if (purification) {
			bool Purify(int i, int j, int type, int conversionType)
			{
				WorldGen.ConvertWall(i, j, wallType);
				return false;
			}
			RegisterConversion(toType, BiomeConversionID.Purity, Purify);
			RegisterConversion(toType, BiomeConversionID.PurificationPowder, Purify);
		}
	}

	private static void InitializeConversionFallbacks()
	{
		RegisterConversionFallback(WallID.JungleUnsafe, WallID.GrassUnsafe, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.FlowerUnsafe, WallID.GrassUnsafe, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Grass, WallID.GrassUnsafe, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Jungle, WallID.GrassUnsafe, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Flower, WallID.GrassUnsafe, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.CorruptGrassUnsafe, WallID.GrassUnsafe, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowedGrassUnsafe, WallID.GrassUnsafe, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonGrassUnsafe, WallID.GrassUnsafe, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.CorruptGrassEcho, WallID.GrassUnsafe, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowedGrassEcho, WallID.GrassUnsafe, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonGrassEcho, WallID.GrassUnsafe, BiomeConversionID.Crimson);

		RegisterConversionFallback(WallID.Cave7Unsafe, WallID.Stone, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Cave8Unsafe, WallID.Stone, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.EbonstoneUnsafe, WallID.Stone, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.PearlstoneBrickUnsafe, WallID.Stone, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimstoneUnsafe, WallID.Stone, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.Cave7Echo, WallID.Stone, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Cave8Echo, WallID.Stone, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.EbonstoneEcho, WallID.Stone, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.PearlstoneEcho, WallID.Stone, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimstoneEcho, WallID.Stone, BiomeConversionID.Crimson);

		RegisterConversionFallback(WallID.Dirt, WallID.DirtUnsafe, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.SnowWallEcho, WallID.SnowWallUnsafe, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.IceEcho, WallID.IceUnsafe, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.CorruptSandstone, WallID.Sandstone, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowSandstone, WallID.Sandstone, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonSandstone, WallID.Sandstone, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.SandstoneEcho, WallID.Sandstone, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.CorruptSandstoneEcho, WallID.Sandstone, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowSandstoneEcho, WallID.Sandstone, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonSandstoneEcho, WallID.Sandstone, BiomeConversionID.Crimson);

		RegisterConversionFallback(WallID.CorruptHardenedSand, WallID.HardenedSand, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowHardenedSand, WallID.HardenedSand, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonHardenedSand, WallID.HardenedSand, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.HardenedSandEcho, WallID.HardenedSand, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.CorruptHardenedSandEcho, WallID.HardenedSand, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.HallowHardenedSandEcho, WallID.HardenedSand, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.CrimsonHardenedSandEcho, WallID.HardenedSand, BiomeConversionID.Crimson);

		RegisterConversionFallback(WallID.CorruptionUnsafe1, WallID.RocksUnsafe1, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.CrimsonUnsafe1, WallID.RocksUnsafe1, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.HallowUnsafe1, WallID.RocksUnsafe1, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.JungleUnsafe1, WallID.RocksUnsafe1, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Corruption1Echo, WallID.RocksUnsafe1, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.Crimson1Echo, WallID.RocksUnsafe1, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.Hallow1Echo, WallID.RocksUnsafe1, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.Jungle1Echo, WallID.RocksUnsafe1, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Rocks1Echo, WallID.RocksUnsafe1, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.CorruptionUnsafe2, WallID.RocksUnsafe2, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.CrimsonUnsafe2, WallID.RocksUnsafe2, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.HallowUnsafe2, WallID.RocksUnsafe2, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.JungleUnsafe2, WallID.RocksUnsafe2, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Corruption2Echo, WallID.RocksUnsafe2, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.Crimson2Echo, WallID.RocksUnsafe2, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.Hallow2Echo, WallID.RocksUnsafe2, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.Jungle2Echo, WallID.RocksUnsafe2, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Rocks2Echo, WallID.RocksUnsafe2, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.CorruptionUnsafe3, WallID.RocksUnsafe3, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.CrimsonUnsafe3, WallID.RocksUnsafe3, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.HallowUnsafe3, WallID.RocksUnsafe3, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.JungleUnsafe3, WallID.RocksUnsafe3, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Corruption3Echo, WallID.RocksUnsafe3, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.Crimson3Echo, WallID.RocksUnsafe3, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.Hallow3Echo, WallID.RocksUnsafe3, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.Jungle3Echo, WallID.RocksUnsafe3, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Rocks3Echo, WallID.RocksUnsafe3, BiomeConversionID.Purity);

		RegisterConversionFallback(WallID.CorruptionUnsafe4, WallID.RocksUnsafe4, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.CrimsonUnsafe4, WallID.RocksUnsafe4, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.HallowUnsafe4, WallID.RocksUnsafe4, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.JungleUnsafe4, WallID.RocksUnsafe4, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Corruption4Echo, WallID.RocksUnsafe4, BiomeConversionID.Corruption);
		RegisterConversionFallback(WallID.Crimson4Echo, WallID.RocksUnsafe4, BiomeConversionID.Crimson);
		RegisterConversionFallback(WallID.Hallow4Echo, WallID.RocksUnsafe4, BiomeConversionID.Hallow);
		RegisterConversionFallback(WallID.Jungle4Echo, WallID.RocksUnsafe4, BiomeConversionID.Purity);
		RegisterConversionFallback(WallID.Rocks4Echo, WallID.RocksUnsafe4, BiomeConversionID.Purity);
	}

	private static int[] GetOrInitConversionFallbacks(int wallType)
	{
		if (wallConversionFallbacks == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "WallLoader.RegisterConversionFallback"));

		ref var fallbacks = ref wallConversionFallbacks[wallType];
		if (fallbacks is null) {
			fallbacks = new int[BiomeConversionLoader.BiomeConversionCount];
			Array.Fill(fallbacks, -1);
		}

		return fallbacks;
	}

	/// <summary>
	/// Sets a fallback wall type for all conversion types except those in <paramref name="exceptForConversionTypes"/> <br/>
	/// When <see cref="WorldGen.Convert(int, int, int, int, bool, bool)"/> is called on the <paramref name="wallType"/> but there is no registsred conversion, the tile will be temporarily replaced with <paramref name="fallbackType"/> and conversion will be reattempted.<br/>
	/// If the <paramref name="fallbackType"/> also has no conversion, the tile remains unchanged. <br/>
	/// <br/>
	/// For example <see cref="WallID.EbonstoneUnsafe"/> falls back to <see cref="TileID.Stone"/> so a modded conversion that affects Stone can convert Ebonstone without needing to register a conversion for Ebonstone directly.
	/// </summary>
	public static void RegisterConversionFallback(int wallType, int fallbackType, params int[] exceptForConversionTypes)
	{
		var fallbacks = GetOrInitConversionFallbacks(wallType);
		var backup = (int[])fallbacks.Clone();
		Array.Fill(fallbacks, fallbackType);
		foreach (var i in exceptForConversionTypes)
			fallbacks[i] = backup[i];
	}

	/// <summary>
	/// Sets an individual conversion fallback. For advanced uses only.
	/// </summary>
	public static void SetConversionFallback(int wallType, int conversionType, int fallbackType)
	{
		GetOrInitConversionFallbacks(wallType)[conversionType] = fallbackType;
	}

	/// <summary>
	/// Tries to retrieve the <paramref name="fallbackType"/> corresponding to the provided <paramref name="wallType"/> and <paramref name="conversionType"/> <br/>
	/// See also: <seealso cref="RegisterConversionFallback"/>
	/// </summary>
	/// <returns>True if the wall has a registered fallback for the given conversion type</returns>
	public static bool TryGetConversionFallback(int wallType, int conversionType, out int fallbackType)
	{
		if (wallConversionFallbacks == null)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorCallDuringLoad", "WallLoader.TryGetConversionFallback"));

		fallbackType = wallConversionFallbacks[wallType]?[conversionType] ?? -1;
		return fallbackType >= 0;
	}

	public static bool Convert(int i, int j, int conversionType)
	{
		using var recursionCounter = new WorldGen.ConversionRecursion();
		var tile = Main.tile[i, j];
		int type = tile.wall;
		var list = wallConversionDelegates[type]?[conversionType];
		if (list != null) {
			foreach (var hook in CollectionsMarshal.AsSpan(list)) {
				if (!hook(i, j, type, conversionType)) {
					return false;
				}
			}
		}

		ModWall modWall = GetWall(type);
		modWall?.Convert(i, j, conversionType);

		if (tile.wall == type && TryGetConversionFallback(type, conversionType, out var fallback)) {
			tile.wall = (ushort)fallback;
			WorldGen.Convert(i, j, conversionType, size: 0, tiles: false);

			if (tile.wall == fallback)
				tile.wall = (ushort)type;
		}
		return true;
	}

	//in Terraria.WorldGen.UpdateWorld after each call to TileLoader.RandomUpdate call
	//  WallLoader.RandomUpdate(num7, num8, Main.tile[num7, num8].wall);
	//  WallLoader.RandomUpdate(num64, num65, Main.tile[num64, num65].wall);
	public static void RandomUpdate(int i, int j, int type)
	{
		GetWall(type)?.RandomUpdate(i, j);

		foreach (var hook in HookRandomUpdate) {
			hook(i, j, type);
		}
	}

	//in Terraria.Framing.WallFrame after the 'if (num == 15)' block
	//	if (!WallLoader.WallFrame(i, j, tile.wall, resetFrame, ref num, ref num2))
	//		return;
	public static bool WallFrame(int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber)
	{
		ModWall modWall = GetWall(type);

		if (modWall != null) {
			if (!modWall.WallFrame(i, j, randomizeFrame, ref style, ref frameNumber))
				return false;
		}

		foreach (var hook in HookWallFrame) {
			if (!hook(i, j, type, randomizeFrame, ref style, ref frameNumber))
				return false;
		}

		return true;
	}

	//in Terraria.Main.Update after vanilla wall animations call WallLoader.AnimateWalls();
	public static void AnimateWalls()
	{
		if (loaded) {
			for (int i = 0; i < walls.Count; i++) {
				ModWall modWall = walls[i];
				modWall.AnimateWall(ref Main.wallFrame[modWall.Type], ref Main.wallFrameCounter[modWall.Type]);
			}
		}
	}
	//in Terraria.Main.DrawWalls before if statements that do the drawing add
	//  if(!WallLoader.PreDraw(j, i, wall, Main.spriteBatch))
	//  { WallLoader.PostDraw(j, i, wall, Main.spriteBatch); continue; }
	public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		foreach (var hook in HookPreDraw) {
			if (!hook(i, j, type, spriteBatch)) {
				return false;
			}
		}
		return GetWall(type)?.PreDraw(i, j, spriteBatch) ?? true;
	}
	//in Terraria.Main.DrawWalls after wall outlines are drawn call
	//  WallLoader.PostDraw(j, i, wall, Main.spriteBatch);
	public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		GetWall(type)?.PostDraw(i, j, spriteBatch);

		foreach (var hook in HookPostDraw) {
			hook(i, j, type, spriteBatch);
		}
	}

	public static void PlaceInWorld(int i, int j, Item item)
	{
		Tile tile = Main.tile[i, j];
		int type = tile.WallType;
		if (type == 0)
			return;

		foreach (var hook in HookPlaceInWorld) {
			hook(i, j, type, item);
		}

		GetWall(type)?.PlaceInWorld(i, j, item);
	}

	public static void OnWallConverted(int i, int j, int fromType, int toType, int conversionType)
	{
		foreach (var hook in HookOnWallConverted) {
			hook(i, j, fromType, toType, conversionType);
		}

		GetWall(fromType)?.OnWallConverted(i, j, fromType, toType, conversionType);
		GetWall(toType)?.OnWallConverted(i, j, fromType, toType, conversionType);
	}

	internal static void FinishSetup()
	{
		for (int k = 0; k < ItemLoader.ItemCount; k++) {
			Item item = ContentSamples.ItemsByType[k];
			if (!ItemID.Sets.DisableAutomaticPlaceableDrop[k]) {
				if (item.createWall > -1) {
					// TryAdd won't override existing value if present. Existing ModWall.RegisterItemDrop entries take precedence
					WallLoader.wallTypeToItemType.TryAdd(item.createWall, item.type);
				}
			}
		}
	}
}
