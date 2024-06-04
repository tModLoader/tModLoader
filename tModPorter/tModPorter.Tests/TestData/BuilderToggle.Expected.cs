using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class BuilderToggleTest : BuilderToggle
{
	public override string DisplayValue() => "Test";

#if COMPILE_ERROR
	public override Color DisplayColorTexture()/* tModPorter Note: Removed. Use BuilderToggle.Draw */ => Color.Blue;
#endif
}
