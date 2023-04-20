using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader;

public static class ResourceOverlayLoader
{
	public static int OverlayCount => overlays.Count;

	internal static readonly IList<ModResourceOverlay> overlays = new List<ModResourceOverlay>();

	internal static int Add(ModResourceOverlay overlay)
	{
		overlays.Add(overlay);
		return OverlayCount - 1;
	}

	public static ModResourceOverlay GetOverlay(int type)
	{
		return type >= 0 && type < OverlayCount ? overlays[type] : null;
	}

	internal static void Unload()
	{
		overlays.Clear();
	}

	public static bool PreDrawResource(ResourceOverlayDrawContext context)
	{
		bool result = true;

		foreach (ModResourceOverlay overlay in overlays) {
			result &= overlay.PreDrawResource(context);
		}

		return result;
	}

	public static void PostDrawResource(ResourceOverlayDrawContext context)
	{
		foreach (ModResourceOverlay overlay in overlays) {
			overlay.PostDrawResource(context);
		}
	}

	/// <summary>
	/// Draws a resource, typically life or mana
	/// </summary>
	/// <param name="drawContext">The drawing context</param>
	public static void DrawResource(ResourceOverlayDrawContext drawContext)
	{
		if (PreDrawResource(drawContext))
			drawContext.Draw();
		PostDrawResource(drawContext);
	}

	public static bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText)
	{
		bool result = true;
		drawText = true;

		foreach (ModResourceOverlay overlay in overlays) {
			result &= overlay.PreDrawResourceDisplay(snapshot, displaySet, drawingLife, ref textColor, out bool draw);
			drawText &= draw;
		}

		return result;
	}

	public static void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText)
	{
		foreach (ModResourceOverlay overlay in overlays) {
			overlay.PostDrawResourceDisplay(snapshot, displaySet, drawingLife, textColor, drawText);
		}
	}

	public static bool DisplayHoverText(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife)
	{
		bool result = true;

		foreach (ModResourceOverlay overlay in overlays) {
			result &= overlay.DisplayHoverText(snapshot, displaySet, drawingLife);
		}

		return result;
	}
}
