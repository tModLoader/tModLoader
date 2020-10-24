using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class AndromedonHeadGlow : AndromedonGlow
	{
		private static Asset<Texture2D> _glowTexture;

		public override bool IsHeadLayer => true;

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_glowTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Head_Head_Glow");

			return GetHeadDrawDataInfo(info, _glowTexture.Value);
		}

		public override void GetDefaults(PlayerDrawSet drawInfo, out bool visible, out LayerConstraint constraint) {
			base.GetDefaults(drawInfo, out visible, out _);

			constraint = new LayerConstraint(Head, false);
		}
	}
}
