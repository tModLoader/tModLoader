using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Legs : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}

		private static Asset<Texture2D> _glowTexture;
		private static Asset<Texture2D> _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonLegsGlow", false, PlayerLayer.Head, drawInfo => {
			_glowTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Legs_Legs_Glow");

			return GetLegDrawDataInfo(drawInfo, _glowTexture.Value);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonLegsShader", false, PlayerLayer.Torso, drawInfo => {
			_shaderTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Legs_Legs_Shader");

			return GetLegDrawDataInfo(drawInfo, _shaderTexture.Value);
		});
	}
}
