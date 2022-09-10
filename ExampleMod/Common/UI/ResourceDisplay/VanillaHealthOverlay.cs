using ExampleMod.Content.Items.Consumables;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ResourceDisplay
{
	public class VanillaHealthOverlay : ModResourceOverlay
	{
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			// Only hearts are replaced
			if (context.DrawSource is not ResourceDrawSource_Life)
				return;

			// Hearts are replaced in groups of two
			// Bars panel drawing has two additional elements, so the "resource index" needs to be offset
			int fillResourceNumber = context.DrawSource is ResourceDrawSource_BarsLifePanel ? context.resourceNumber - 1 : context.resourceNumber;
			if (fillResourceNumber > 2 * Main.LocalPlayer.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits)
				return;

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
			if (context.DrawSource is ResourceDrawSource_ClassicLife || context.DrawSource is ResourceDrawSource_FancyLife) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicHeartOverlay", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.DrawSource is ResourceDrawSource_FancyLifePanel) {
				string texture = "ExampleMod/Common/UI/ResourceDisplay/FancyHeartOverlay_";

				if (context.resourceNumber == context.snapshot.AmountOfLifeHearts) {
					// Final panel to draw has a special "Fancy" variant.  Determine whether it has panels to the left of it
					if (context.resourceNumber % 10 == 1) {
						// First and only panel in this panel's row
						texture += "Single_Fancy";
					} else {
						// Other panels existed in this panel's row
						texture += "Right_Fancy";
					}
				} else if (context.resourceNumber % 10 == 1) {
					// First panel in this row
					texture += "Left";
				} else if (context.resourceNumber % 10 <= 9) {
					// Any panel that has a panel to its left AND right
					texture += "Middle";
				} else {
					// Final panel in the first row
					texture += "Right";
				}

				context.texture = ModContent.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.DrawSource is ResourceDrawSource_BarsLifePanel && fillResourceNumber >= 1 && fillResourceNumber <= 20) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Panel_Middle", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.DrawSource is ResourceDrawSource_BarsLife) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Fill", AssetRequestMode.ImmediateLoad);
				context.Draw();
			}
		}
	}
}
