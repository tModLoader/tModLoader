using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class BuilderToggleTest : BuilderToggle
{
	public override string DisplayValue() => "Test";

	public override Color DisplayColorTexture() => Color.Blue;
}