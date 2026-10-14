using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader.Core;

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
		Array.Resize(ref Main.liquidFrame, nextLiquid);
		Array.Resize(ref Main.liquidFrameState, nextLiquid);

		LoaderUtils.ResetStaticMembers(typeof(LiquidID));

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
}
