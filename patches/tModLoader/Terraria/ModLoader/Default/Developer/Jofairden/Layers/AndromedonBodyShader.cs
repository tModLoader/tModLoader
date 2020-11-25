using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class AndromedonBodyShader : AndromedonShader
	{
		private static Asset<Texture2D> _shaderTexture;

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_shaderTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Body_Body_Shader");

			return GetBodyDrawDataInfo(info, _shaderTexture.Value);
		}

		public override IEnumerable<LayerConstraint> GetConstraints() {
			yield return LayerConstraint.After(ArmorLongCoat);
			yield return LayerConstraint.Before(Torso);
		}
	}
}
