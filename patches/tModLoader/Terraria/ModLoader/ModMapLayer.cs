using System.Collections.Generic;
using Terraria.Map;

namespace Terraria.ModLoader;

/// <summary>
/// This class is used to facilitate easily drawing icons and other things over the map. Pylons and spawn/bed icons are examples of vanilla map layers. Use <see cref="ModSystem.PreDrawMapIconOverlay(System.Collections.Generic.IReadOnlyList{IMapLayer}, MapOverlayDrawContext)"/> to selectively hide vanilla layers if needed.
/// </summary>
public abstract class ModMapLayer : ModType, IMapLayer
{
	public bool Visible { get; set; } = true;

	/// <summary>
	/// Returns the map layer's default position in regard to vanilla's map layer ordering. Make use of e.g. <see cref="Before"/>/<see cref="After"/>, and provide a map layer. You can also use <see cref="BeforeFirstVanillaLayer"/> or <see cref="AfterLastVanillaLayer"/> to put your layer at the start/end of the vanilla layer order.<br/><br/>
	/// <b>NOTE:</b> The position must specify a vanilla <see cref="IMapLayer"/> otherwise an exception will be thrown. Use <see cref="GetModdedConstraints"/> to order modded layers.<br/><br/>
	/// By default, this hook positions this map layer after all vanilla layers.
	/// </summary>
	public virtual Position GetDefaultPosition() => AfterLastVanillaLayer;

	/// <summary>
	/// Modded layers are placed between vanilla layers via <see cref="GetDefaultPosition"/> and, by default, are sorted in load order.<br/>
	/// This hook allows you to sort this map layer before/after other modded layers that were placed between the same two vanilla layers.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;ExampleMapLayer&gt;());</c>
	/// </para>
	/// By default, this hook returns <see langword="null"/>, which indicates that this map layer has no modded ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position> GetModdedConstraints() => null;

	/// <summary>
	/// This method is called when this MapLayer is to be drawn. Map layers are drawn after the map itself is drawn. Use <see cref="MapOverlayDrawContext.Draw(Microsoft.Xna.Framework.Graphics.Texture2D, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Color, DataStructures.SpriteFrame, float, float, Terraria.UI.Alignment)"/> as described in ExampleMod and in vanilla examples for full compatibility and simplicity of code.
	/// <para/> <paramref name="context"/>: Contains the scaling and positional data of the map being drawn. You should use the MapOverlayDrawContext.Draw method for all drawing.
	/// <para/> <paramref name="text"/>: The mouse hover text. Assign a value typically if the user is hovering over something you draw.
	/// <para/> Note that MapLayers are drawn on the fullscreen map, overlay map, and minimap. Use <see cref="Main.mapFullscreen"/> and <see cref="Main.mapStyle"/> to limit drawing to specific map modes if appropriate.
	/// </summary>
	/// <param name="context">Contains the scaling and positional data of the map being drawn. You should use the MapOverlayDrawContext.Draw method for all drawing</param>
	/// <param name="text">The mouse hover text. Assign a value typically if the user is hovering over something you draw</param>
	public abstract void Draw(ref MapOverlayDrawContext context, ref string text);

	protected sealed override void Register()
	{
		ModTypeLookup<ModMapLayer>.Register(this);
		MapLayerLoader.Add(this);
	}

	#region Sort Positions

	public static Position BeforeFirstVanillaLayer => new Before(MapLayerLoader.FirstVanillaLayer);
	public static Position AfterLastVanillaLayer => new After(MapLayerLoader.LastVanillaLayer);

	public abstract class Position;

	public sealed class Before : Position
	{
		public IMapLayer Layer { get; }

		public Before(IMapLayer layer)
		{
			Layer = layer;
		}
	}

	public sealed class After : Position
	{
		public IMapLayer Layer { get; }

		public After(IMapLayer layer)
		{
			Layer = layer;
		}
	}

	#endregion
}
