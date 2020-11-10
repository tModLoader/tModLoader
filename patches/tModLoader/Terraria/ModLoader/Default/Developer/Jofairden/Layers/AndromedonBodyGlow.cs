using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class AndromedonBodyGlow : AndromedonGlow
	{
		private static Asset<Texture2D> _glowTexture;

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_glowTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Body_Body_Glow");

			return GetBodyDrawDataInfo(info, _glowTexture.Value);
		}

		public override IEnumerable<LayerConstraint> GetConstraints() {
			yield return LayerConstraint.After(Torso);
			yield return LayerConstraint.Before(OffhandAcc);
		}
	}
}
