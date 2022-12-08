using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

internal class JofairdenLegsGlow : JofairdenArmorGlowLayer
{
	private Asset<Texture2D> _glowTexture;

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> drawInfo.drawPlayer.legs == ModContent.GetInstance<Jofairden_Legs>().Item.legSlot && base.GetDefaultVisibility(drawInfo);

	public override DrawDataInfo GetData(PlayerDrawSet info)
	{
		_glowTexture ??= ModContent.Request<Texture2D>("ModLoader/Developer.Jofairden.Jofairden_Legs_Legs_Glow");

		return GetLegDrawDataInfo(info, _glowTexture.Value);
	}

	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Leggings);
}
