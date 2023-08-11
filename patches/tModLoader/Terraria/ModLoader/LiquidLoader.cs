using System;
using System.Collections.Generic;
using Terraria.GameContent.Liquid;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class LiquidLoader
{
	private static int nextLiquid = LiquidID.Count;
	public static int LiquidCount => nextLiquid;

	internal static readonly IList<ModLiquid> liquids = new List<ModLiquid>();


	internal static int ReserveLiquidID()
	{
		if (ModNet.AllowVanillaClients) 
			throw new Exception("Adding liquid breaks vanilla client compatibility");
		int reserveId = nextLiquid;
		nextLiquid++;
		return reserveId;
	}

	internal static void ResizeArray(bool unloading = false)
	{
		Array.Resize(ref LiquidRenderer.WATERFALL_LENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.DEFAULT_OPACITY, nextLiquid);
		Array.Resize(ref LiquidRenderer.WAVE_MASK_STRENGTH, nextLiquid + 1);
		Array.Resize(ref LiquidRenderer.VISCOSITY_MASK, nextLiquid + 1);
	}
}