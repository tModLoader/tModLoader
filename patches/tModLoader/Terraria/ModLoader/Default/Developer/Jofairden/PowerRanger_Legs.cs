using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Legs : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonLegsGlow", PlayerLayer.Head, drawInfo => {
			_glowTexture ??= ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Glow");
			return GetLegDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonLegsShader", PlayerLayer.Body, drawInfo => {
			_shaderTexture ??= ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Shader");
			return GetLegDrawDataInfo(drawInfo, _shaderTexture);
		});
	}
}
