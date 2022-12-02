using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a place for you to customize how the vanilla resource displays (Classic, Fancy and Bars) are drawn.<br/>
/// For implementing your own resource displays, use <see cref="ModResourceDisplaySet"/>.
/// </summary>
[Autoload(true, Side = ModSide.Client)]
public abstract class ModResourceOverlay : ModType
{
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		ModTypeLookup<ModResourceOverlay>.Register(this);
		Type = ResourceOverlayLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to draw below any resource (hearts, stars, bars or panels) in a display set
	/// </summary>
	/// <param name="context">Contains the drawing data for the resource being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
	/// <returns><see langword="true"/> if the intended resource sprite should draw, <see langword="false"/> otherwise.</returns>
	public virtual bool PreDrawResource(ResourceOverlayDrawContext context)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw on top of any resource (hearts, stars, bars or panels) in a display set
	/// </summary>
	/// <param name="context">Contains the drawing data for the resource being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
	public virtual void PostDrawResource(ResourceOverlayDrawContext context)
	{
	}

	/// <summary>
	/// Allows you to draw before the resources (hearts, stars, bars and/or panels) in a display set are drawn.<br/>
	/// If you want to implement your own display set, it is recommended to use <see cref="ModResourceDisplaySet"/> instead of this hook.
	/// </summary>
	/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
	/// <param name="displaySet">The display set being drawn</param>
	/// <param name="drawingLife">
	/// Whether the life or mana display is going to be drawn.
	/// <see langword="true"/> if the life display is going to be drawn, <see langword="false"/> if the mana display is going to be drawn.
	/// </param>
	/// <param name="textColor">The color to draw the text above the resources with.  Only applies to the Classic display set.</param>
	/// <param name="drawText">Whether the text above the resources should draw.  Only applies to the Classic display set.</param>
	/// <returns>Whether the resources in the display set are drawn</returns>
	public virtual bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText)
	{
		drawText = true;
		return true;
	}

	/// <summary>
	/// Allows you to draw after the resources (hearts, stars, bars and/or panels) in a display set are drawn
	/// </summary>
	/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
	/// <param name="displaySet">The display set that was drawn</param>
	/// <param name="drawingLife">
	/// Whether the life or mana display was drawn.
	/// <see langword="true"/> if the life display was drawn, <see langword="false"/> if the mana display was drawn.
	/// </param>
	/// <param name="textColor">The color the text above the resources was drawn with.  Only applies to the Class display set.</param>
	/// <param name="drawText">Whether the text above the resources was drawn.  Only applies to the Classic display set.</param>
	public virtual void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText)
	{
	}

	/// <summary>
	/// Allows you to specify if the hover text for a resource (life or mana) should be displayed
	/// </summary>
	/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
	/// <param name="displaySet">The display set that was drawn</param>
	/// <param name="drawingLife">
	/// Whether the life or mana display was drawn.
	/// <see langword="true"/> if the life display was drawn, <see langword="false"/> if the mana display was drawn.
	/// </param>
	/// <returns>Whether the hover text should be displayed</returns>
	public virtual bool DisplayHoverText(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife)
	{
		return true;
	}
}
