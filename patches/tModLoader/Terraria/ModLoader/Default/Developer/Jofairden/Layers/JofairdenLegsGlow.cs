﻿using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class JofairdenLegsGlow : JofairdenArmorGlowLayer
	{
		private static Asset<Texture2D> _glowTexture;

		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo)
			=> drawInfo.drawPlayer.legs == ModContent.GetInstance<Jofairden_Legs>().Item.legSlot && base.GetDefaultVisiblity(drawInfo);

		public override DrawDataInfo GetData(PlayerDrawSet info) {
			_glowTexture ??= ModContent.Request<Texture2D>("ModLoader/Developer.Jofairden.Jofairden_Legs_Legs_Glow");

			return GetLegDrawDataInfo(info, _glowTexture.Value);
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Leggings);
	}
}
