using Terraria.Map;

namespace Terraria.ModLoader;

/// <summary>
/// This class is used to facilitate easily drawing icons and other things over the map. Pylons and spawn/bed icons are examples of vanilla map layers. Use <see cref="ModSystem.PreDrawMapIconOverlay(System.Collections.Generic.IReadOnlyList{IMapLayer}, MapOverlayDrawContext)"/> to selectively hide vanilla layers if needed.
/// </summary>
public abstract class ModMapLayer : ModType, IMapLayer
{
	public bool Visible { get; set; } = true;

	/// <summary>
	/// This method is called when this MapLayer is to be drawn. Map layers are drawn after the map itself is drawn. Use <see cref="MapOverlayDrawContext.Draw(Microsoft.Xna.Framework.Graphics.Texture2D, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Color, DataStructures.SpriteFrame, float, float, Terraria.UI.Alignment)"/> as described in ExampleMod and in vanilla examples for full compatibility and simplicity of code.
	/// </summary>
	/// <param name="context">Contains the scaling and positional data of the map being drawn. You should use the MapOverlayDrawContext.Draw method for all drawing</param>
	/// <param name="text">The mouse hover text. Assign a value typically if the user is hovering over something you draw</param>
	public abstract void Draw(ref MapOverlayDrawContext context, ref string text);

	protected sealed override void Register()
	{
		ModTypeLookup<ModMapLayer>.Register(this);
		Main.MapIcons.AddLayer(this);
	}
}
