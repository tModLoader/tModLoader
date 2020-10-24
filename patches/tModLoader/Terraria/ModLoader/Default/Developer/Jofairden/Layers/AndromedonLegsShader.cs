using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class AndromedonLegsShader : AndromedonShader
	{
		private static Asset<Texture2D> _shaderTexture;

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_shaderTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Legs_Legs_Shader");

			return GetLegDrawDataInfo(info, _shaderTexture.Value);
		}

		public override void GetDefaults(PlayerDrawSet drawInfo, out bool visible, out LayerConstraint constraint) {
			base.GetDefaults(drawInfo, out visible, out _);

			constraint = new LayerConstraint(Leggings, true);
		}
	}
}
