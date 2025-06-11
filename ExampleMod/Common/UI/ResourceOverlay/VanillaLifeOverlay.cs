using ExampleMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ResourceOverlay
{
	public class VanillaLifeOverlay : ModResourceOverlay
	{
		// This field is used to cache vanilla assets used in the CompareAssets helper method further down in this file
		private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

		// These fields are used to cache the result of ModContent.Request<Texture2D>()
		private Asset<Texture2D> heartTexture, fancyPanelTexture, barsFillingTexture, barsPanelTexture;

		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			Asset<Texture2D> asset = context.texture;

			string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
			string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

			bool drawingBarsPanels = CompareAssets(asset, barsFolder + "HP_Panel_Middle");

			int exampleFruits = Main.LocalPlayer.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits;

			// Life resources are drawn over in groups of two
			if (context.resourceNumber >= 2 * exampleFruits)
				return;

			// NOTE: CompareAssets is defined below this method's body
			if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2) {
				// Draw over the Classic hearts
				DrawClassicFancyOverlay(context);
			}
			else if (CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B")) {
				// Draw over the Fancy hearts
				DrawClassicFancyOverlay(context);
			}
			else if (CompareAssets(asset, barsFolder + "HP_Fill") || CompareAssets(asset, barsFolder + "HP_Fill_Honey")) {
				// Draw over the Bars life bars
				DrawBarsOverlay(context);
			}
			else if (CompareAssets(asset, fancyFolder + "Heart_Left") || CompareAssets(asset, fancyFolder + "Heart_Middle") || CompareAssets(asset, fancyFolder + "Heart_Right") || CompareAssets(asset, fancyFolder + "Heart_Right_Fancy") || CompareAssets(asset, fancyFolder + "Heart_Single_Fancy")) {
				// Draw over the Fancy heart panels
				DrawFancyPanelOverlay(context);
			}
			else if (drawingBarsPanels) {
				// Draw over the Bars middle life panels
				DrawBarsPanelOverlay(context);
			}
		}

		private bool CompareAssets(Asset<Texture2D> existingAsset, string compareAssetPath) {
			// This is a helper method for checking if a certain vanilla asset was drawn
			if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
				asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);

			return existingAsset == asset;
		}

		private void DrawClassicFancyOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Classic / Fancy hearts
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
			context.texture = heartTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/ClassicLifeOverlay");
			context.Draw();
		}

		// Drawing over the panel backgrounds is not required.
		// This example just showcases changing the "inner" part of the heart panels to more closely resemble the example life fruit.
		private void DrawFancyPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Fancy heart panels
			string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";

			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the heart),
			// the position should be modified to compensate for the sprite size difference
			Vector2 positionOffset;

			if (context.resourceNumber == context.snapshot.AmountOfLifeHearts - 1) {
				// Final panel to draw has a special "Fancy" variant.  Determine whether it has panels to the left of it
				if (CompareAssets(context.texture, fancyFolder + "Heart_Single_Fancy")) {
					// First and only panel in this panel's row
					positionOffset = new Vector2(8, 8);
				}
				else {
					// Other panels existed in this panel's row
					// Vanilla texture is "Heart_Right_Fancy"
					positionOffset = new Vector2(8, 8);
				}
			}
			else if (CompareAssets(context.texture, fancyFolder + "Heart_Left")) {
				// First panel in this row
				positionOffset = new Vector2(4, 4);
			}
			else if (CompareAssets(context.texture, fancyFolder + "Heart_Middle")) {
				// Any panel that has a panel to its left AND right
				positionOffset = new Vector2(0, 4);
			}
			else {
				// Final panel in the first row
				// Vanilla texture is "Heart_Right"
				positionOffset = new Vector2(0, 4);
			}

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
			context.texture = fancyPanelTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/FancyLifeOverlay_Panel");
			// Due to the replacement texture and the vanilla texture having different dimensions, the source needs to also be modified
			context.source = context.texture.Frame();
			context.position += positionOffset;
			context.Draw();
		}

		private void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars life bars
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla bars, just replace the texture and have the context draw the new texture
			context.texture = barsFillingTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/BarsLifeOverlay_Fill");
			context.Draw();
		}

		// Drawing over the panel backgrounds is not required.
		// This example just showcases changing the "inner" part of the bar panels to more closely resemble the example life fruit.
		private void DrawBarsPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars middle life panels
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla bar panels, just replace the texture and have the context draw the new texture
			context.texture = barsPanelTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/BarsLifeOverlay_Panel");
			// Due to the replacement texture and the vanilla texture having different heights, the source needs to also be modified
			context.source = context.texture.Frame();
			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the bar filling),
			// the position should be modified to compensate for the sprite size difference
			context.position.Y += 6;
			context.Draw();
		}
	}
}
