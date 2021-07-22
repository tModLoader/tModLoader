using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class JofairdenBodyShader : JofairdenArmorShaderLayer
	{
		private static Asset<Texture2D> _shaderTexture;

		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo)
			=> drawInfo.drawPlayer.body == ModContent.GetInstance<Jofairden_Body>().Item.bodySlot && base.GetDefaultVisiblity(drawInfo);

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_shaderTexture ??= ModContent.Request<Texture2D>("ModLoader/Developer.Jofairden.Jofairden_Body_Body_Shader");

			return GetBodyDrawDataInfo(info, _shaderTexture.Value);
		}

		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);
	}
}
