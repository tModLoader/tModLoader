using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Body : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(34, 22);
		}

		private static Asset<Texture2D> _glowTexture;
		private static Asset<Texture2D> _shaderTexture;

		public static PlayerDrawLayer GlowLayer = CreateGlowLayer("AndromedonBodyGlow", false, PlayerDrawLayer.Torso, drawInfo => {
			_glowTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Body_Body_Glow");

			return GetBodyDrawDataInfo(drawInfo, _glowTexture.Value);
		});

		public static PlayerDrawLayer ShaderLayer = CreateShaderLayer("AndromedonBodyShader", false, PlayerDrawLayer.Torso, drawInfo => {
			_shaderTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Body_Body_Shader");

			return GetBodyDrawDataInfo(drawInfo, _shaderTexture.Value);
		});
	}
}
