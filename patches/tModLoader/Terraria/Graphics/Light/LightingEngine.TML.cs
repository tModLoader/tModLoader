namespace Terraria.Graphics.Light;

partial class LightingEngine
{
	public LightMapBuffer GetBufferTexture()
	{
		return LightMapBuffer.FromLightMap(_activeLightMap);
	}
}
