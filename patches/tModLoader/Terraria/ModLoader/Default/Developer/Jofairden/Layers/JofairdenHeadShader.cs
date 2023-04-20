using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

internal class JofairdenHeadShader : JofairdenArmorShaderLayer
{
	private Asset<Texture2D> _shaderTexture;

	public override bool IsHeadLayer => true;

	public override DrawDataInfo GetData(PlayerDrawSet info)
	{
		_shaderTexture ??= ModContent.Request<Texture2D>("ModLoader/Developer.Jofairden.Jofairden_Head_Head_Shader");

		return GetHeadDrawDataInfo(info, _shaderTexture.Value);
	}

	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);
}
