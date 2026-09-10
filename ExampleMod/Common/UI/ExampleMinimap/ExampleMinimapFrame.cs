using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ExampleMinimap;

/// <summary>
/// Demonstrates how to add a minimap border to the vanilla minimap border setting.
/// Override the four asset path properties to use custom textures from your mod.
/// </summary>
public sealed class ExampleMinimapFrame : ModMinimapFrame
{
	public override string Texture => "ExampleMod/Common/UI/ExampleMinimap/MinimapFrame";
	public override string ZoomInTexture => "ExampleMod/Common/UI/ExampleMinimap/MinimapButton_ZoomIn";
	public override string ZoomOutTexture => "ExampleMod/Common/UI/ExampleMinimap/MinimapButton_ZoomOut";
	public override string ResetTexture => "ExampleMod/Common/UI/ExampleMinimap/MinimapButton_Reset";

	public override Vector2 FrameOffset => new(-10f, -10f);
	public override Vector2 ResetButtonPosition => new(150f, 236f);
	public override Vector2 ZoomInButtonPosition => new(202f, 236f);
	public override Vector2 ZoomOutButtonPosition => new(176f, 236f);
}
