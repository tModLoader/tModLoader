using ExampleMod.Content.Items.Consumables;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ResourceDisplay
{
	public class VanillaHealthOverlay : ModResourceOverlay
	{
		public override void PostDrawClassicLifeHeart(ResourceOverlayDrawContext context) {
			// Hearts are replaced in groups of two
			if (context.resourceNumber > 2 * Main.LocalPlayer.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits)
				return;

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and origin, then have the context draw the new texture
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicHeartOverlay", AssetRequestMode.ImmediateLoad);
			context.origin = context.texture.Size() / 2f;
			context.Draw();
		}
	}
}
