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
	public class VanillaManaOverlay : ModResourceOverlay
	{
		// This field is used to cache vanilla assets used in the CompareAssets helper method further down in this file
		private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

		// These fields are used to cache the result of ModContent.Request<Texture2D>()
		private Asset<Texture2D> starTexture, fancyPanelTexture, barsFillingTexture, barsPanelTexture;

		// Unlike VanillaLifeOverlay, every star is drawn over by this hook.
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			Asset<Texture2D> asset = context.texture;

			string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
			string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

			if (Main.LocalPlayer.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals <= 0)
				return;

			// NOTE: CompareAssets is defined below this method's body
			if (asset == TextureAssets.Mana) {
				// Draw over the Classic stars
				DrawClassicFancyOverlay(context);
			}
			else if (CompareAssets(asset, fancyFolder + "Star_Fill")) {
				// Draw over the Fancy stars
				DrawClassicFancyOverlay(context);
			}
			else if (CompareAssets(asset, barsFolder + "MP_Fill")) {
				// Draw over the Bars mana bars
				DrawBarsOverlay(context);
			}
			else if (CompareAssets(asset, fancyFolder + "Star_A") || CompareAssets(asset, fancyFolder + "Star_B") || CompareAssets(asset, fancyFolder + "Star_C") || CompareAssets(asset, fancyFolder + "Star_Single")) {
				// Draw over the Fancy star panels
				DrawFancyPanelOverlay(context);
			}
			else if (CompareAssets(asset, barsFolder + "MP_Panel_Middle")) {
				// Draw over the Bars middle mana panels
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
			// Draw over the Classic / Mana stars
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla stars, just replace the texture and have the context draw the new texture
			context.texture = starTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/ClassicManaOverlay");
			context.Draw();
		}

		// Drawing over the panel backgrounds is not required.
		// This example just showcases changing the "inner" part of the star panels to more closely resemble the example life fruit.
		private void DrawFancyPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Fancy star panels
			string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";

			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the star),
			// the position should be modified to compensate for the sprite size difference
			Vector2 positionOffset;

			if (context.resourceNumber == context.snapshot.AmountOfManaStars - 1) {
				// Final panel in the column.  Determine whether it has panels above it
				if (CompareAssets(context.texture, fancyFolder + "Star_Single")) {
					// First and only panel
					positionOffset = new Vector2(4, 4);
				}
				else {
					// Other panels existed above this panel
					// Vanilla texture is "Star_C"
					positionOffset = new Vector2(4, 0);
				}
			}
			else if (CompareAssets(context.texture, fancyFolder + "Star_A")) {
				// First panel in the column
				positionOffset = new Vector2(4, 4);
			}
			else {
				// Any panel that has a panel above AND below it
				// Vanilla texture is "Star_B"
				positionOffset = new Vector2(4, 0);
			}

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla stars, just replace the texture and have the context draw the new texture
			context.texture = fancyPanelTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/FancyManaOverlay_Panel");
			// Due to the replacement texture and the vanilla texture having different dimensions, the source needs to also be modified
			context.source = context.texture.Frame();
			context.position += positionOffset;
			context.Draw();
		}

		private void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars mana bars
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla bars, just replace the texture and have the context draw the new texture
			context.texture = barsFillingTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/BarsManaOverlay_Fill");
			context.Draw();
		}

		// Drawing over the panel backgrounds is not required.
		// This example just showcases changing the "inner" part of the bar panels to more closely resemble the example life fruit.
		private void DrawBarsPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars middle life panels
			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla bar panels, just replace the texture and have the context draw the new texture
			context.texture = barsPanelTexture ??= ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceOverlay/BarsManaOverlay_Panel");
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
