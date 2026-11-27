using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader;

public static class LiquidLoader
{
	private static int nextLiquid = LiquidID.Count;

	internal static readonly IList<ModLiquid> liquids = new List<ModLiquid>();

	internal static readonly IList<GlobalLiquid> globalLiquids = new List<GlobalLiquid>();

	private static bool loaded = false;

	public static int LiquidCount => nextLiquid;

	public static Asset<Texture2D>[] LiquidAssets = new Asset<Texture2D>[4];
	public static Asset<Texture2D>[] LiquidBlockAssets = new Asset<Texture2D>[4];
	public static Asset<Texture2D>[] LiquidSlopeAssets = new Asset<Texture2D>[4];

	internal static int ReserveLiquidID()
	{
		if (nextLiquid >= LiquidData.MaxLiquidTypes) {
			throw new Exception($"Cannot add more than {LiquidData.MaxLiquidTypes} liquid types");
		}

		int result = nextLiquid;
		nextLiquid++;
		return result;
	}

	/// <summary>
	/// Gets the ModLiquid instance with the given type. If no ModLiquid with the given type exists, returns null.
	/// </summary>
	public static ModLiquid GetLiquid(int type)
	{
		if (type < LiquidID.Count || type >= LiquidCount) {
			return null;
		}
		return liquids[type - LiquidID.Count];
	}

	internal static void ResizeArrays(bool unloading = false)
	{
		if (!Main.dedServ) {
			LiquidRenderer.Instance._cache = new LiquidRenderer.LiquidCache[1];
			LiquidRenderer.Instance._drawCache = new LiquidRenderer.LiquidDrawCache[1];
		}
		Array.Resize(ref LiquidAssets, nextLiquid);
		Array.Resize(ref LiquidBlockAssets, nextLiquid);
		Array.Resize(ref LiquidSlopeAssets, nextLiquid);
		Array.Resize(ref LiquidRenderer.WATERFALL_LENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.DEFAULT_OPACITY, nextLiquid);
		Array.Resize(ref LiquidRenderer.WAVE_MASK_STRENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.VISCOSITY_MASK, nextLiquid);

		LoaderUtils.ResetStaticMembers(typeof(LiquidID));

		TileID.Sets.ApplyResizeTileLiquidSourceForCraftingSet();

		ModLoader.BuildGlobalHook(ref HookModifyLight, globalLiquids, (GlobalLiquid g) => g.ModifyLight);
		//ModLoader.BuildGlobalHook(ref HookSettleLiquidMovement, globalLiquids, (GlobalLiquid g) => g.SettleLiquidMovement);
		//ModLoader.BuildGlobalHook(ref HookOnPump, globalLiquids, (GlobalLiquid g) => g.CanPump);

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		liquids.Clear();
		nextLiquid = LiquidID.Count;
		globalLiquids.Clear();
	}

	private delegate void DelegateModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
	private static DelegateModifyLight[] HookModifyLight;
	public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
	{
		GetLiquid(type)?.ModifyLight(i, j, ref r, ref g, ref b);
		foreach (var hook in HookModifyLight) {
			hook(i, j, type, ref r, ref g, ref b);
		}
	}

	private static Func<int, int, int, bool>[] HookSettleLiquidMovement;
	public static bool SettleLiquidMovement(int i, int j, int type)
	{
		/*
		foreach (var hook in HookSettleLiquidMovement) {
			bool result = hook(i, j, type);
			if (!result) {
				return false;
			}
		}
		return GetLiquid(type)?.SettleLiquidMovement(i, j) ?? true;
		*/
		var liquid = GetLiquid(type);
		if (liquid is UnloadedLiquid) {
			return (liquid as UnloadedLiquid).SettleLiquidMovement(i, j);
		}

		return true;
	}

	private static Func<int, int, int, int, int, bool>[] HookCanPump;
	public static bool CanPump(int type, int inletX, int inletY, int outletX, int outletY)
	{
		/*
		foreach (var hook in HookCanPump) {
			if (!hook(type, x, y, x2, y2)) {
				return false;
			}
		}
		return GetLiquid(type)?.CanPump(x, y, x2, y2) ?? true;
		*/

		var liquid = GetLiquid(type);
		if (liquid is UnloadedLiquid) {
			return (liquid as UnloadedLiquid).CanPump(inletX, inletX, outletX, outletY);
		}

		return true;
	}

	private static Func<int, int, int, Liquid, bool>[] HookPreLiquidMovement;
	public static bool PreLiquidMovement(int x, int y, int type, Liquid liquidCache)
	{
		/*
		foreach (var hook in HookPreLiquidMovement) {
			if (!hook(i, j, type, liquid)) {
				return false;
			}
		}
		return GetLiquid(type)?.PreLiquidMovment(i, j, liquid) ?? true;
		*/

		var liquid = GetLiquid(type);
		if (liquid is UnloadedLiquid) {
			return (liquid as UnloadedLiquid).PreLiquidMovement(x, y, liquidCache);
		}

		return true;
	}
}
