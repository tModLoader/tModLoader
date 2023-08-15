using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class LiquidLoader
{
	private static int nextLiquid = LiquidID.Count;
	private static bool loaded = false;

	internal static readonly IList<ModLiquid> liquids = new List<ModLiquid>();

	public static int LiquidCount => nextLiquid;

	public static ModLiquid GetLiquid(int type)
	{
		return type >= LiquidID.Count && type < LiquidCount ? liquids[type - LiquidID.Count] : null;
	}

	internal static int ReserveLiquidID()
	{
		if (ModNet.AllowVanillaClients) 
			throw new Exception("Adding liquid breaks vanilla client compatibility");
		int reserveId = nextLiquid;
		nextLiquid++;
		return reserveId;
	}

	internal static void ResizeArrays(bool unloading = false)
	{
		Array.Resize(ref LiquidRenderer.WATERFALL_LENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.DEFAULT_OPACITY, nextLiquid);
		Array.Resize(ref LiquidRenderer.WAVE_MASK_STRENGTH, nextLiquid + 1);
		Array.Resize(ref LiquidRenderer.VISCOSITY_MASK, nextLiquid + 1);

		Array.Resize(ref TextureAssets.Liquid, 15 + nextLiquid - LiquidID.Count);

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		nextLiquid = LiquidID.Count;

		liquids.Clear();
	}
}