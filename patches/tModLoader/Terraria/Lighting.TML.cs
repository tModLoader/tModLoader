using Terraria.Graphics.Light;

namespace Terraria;

partial class Lighting
{
	public static LightMapBuffer GetBufferTexture()
	{
		return _activeEngine.GetBufferTexture();
	}
}
