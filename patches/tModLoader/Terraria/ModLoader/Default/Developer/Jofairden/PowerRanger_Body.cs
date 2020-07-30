using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Body : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(34, 22);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonBodyGlow", PlayerLayer.Body, drawInfo => {
			_glowTexture ??= ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Glow");
			return GetBodyDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonBodyShader", PlayerLayer.Body, drawInfo => {
			_shaderTexture ??= ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Shader");
			return GetBodyDrawDataInfo(drawInfo, _shaderTexture);
		});
	}
}
