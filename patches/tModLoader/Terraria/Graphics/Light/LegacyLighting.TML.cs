namespace Terraria.Graphics.Light;

partial class LegacyLighting
{
	public LightMapBuffer GetBufferTexture()
	{
		return LightMapBuffer.FromLightMap(_lightMap);
	}
}
