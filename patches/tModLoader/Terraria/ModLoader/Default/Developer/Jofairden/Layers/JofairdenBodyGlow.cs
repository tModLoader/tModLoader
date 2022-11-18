using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

internal class JofairdenBodyGlow : JofairdenArmorGlowLayer
{
	private Asset<Texture2D> _glowTexture;

	public override DrawDataInfo GetData(PlayerDrawSet info)
	{
		_glowTexture ??= ModContent.Request<Texture2D>("ModLoader/Developer.Jofairden.Jofairden_Body_Body_Glow");

		return GetBodyDrawDataInfo(info, _glowTexture.Value);
	}

	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);
}
