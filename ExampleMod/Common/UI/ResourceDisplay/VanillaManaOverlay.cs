using ExampleMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ResourceDisplay
{
	// The code reponsible for tracking and modifying how many extra mana stars/bars are displayed can be found in Common/Systems/ExampleStatIncreaseSystem.cs
	public class VanillaManaOverlay : ModResourceOverlay
	{
		// Unlike VanillaLifeOverlay, every star is drawn over by this hook.
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			// Bars panel drawing has two additional elements, so the "resource index" needs to be offset
			int fillResourceNumber = context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.ManaPanels) ? context.resourceNumber - 1 : context.resourceNumber;
			if (Main.LocalPlayer.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals <= 0)
				return;

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
			if (context.IsSlot(ClassicPlayerResourcesDisplaySet.Stars) || context.IsSlot(FancyClassicPlayerResourcesDisplaySet.Stars)) {
				// Draw over the Classic / Fancy stars
				DrawClassicFancyOverlay(context);
			} else if (context.IsSlot(FancyClassicPlayerResourcesDisplaySet.StarPanels)) {
				// Draw over the Fancy star panels
				DrawFancyPanelOverlay(context);
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.ManaPanels) && fillResourceNumber >= 1 && fillResourceNumber <= context.snapshot.AmountOfManaStars) {
				// Draw over the Bars middle mana panels
				DrawBarsPanelOverlay(context);
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.ManaBars)) {
				// Draw over the Bars mana bars
				DrawBarsOverlay(context);
			} else {
				// None of the above cases applied, but this could be a modded resource that uses vanilla textures.
				// So, it would be a good idea to check if certain vanilla resources are being drawn and then draw over them like usual
				Asset<Texture2D> asset = context.texture;

				string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
				string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

				// NOTE: CompareAssets is defined below this method's body
				if (asset == TextureAssets.Mana) {
					// Draw over the Classic stars
					DrawClassicFancyOverlay(context);
				} else if (CompareAssets(asset, fancyFolder + "Star_Fill")) {
					// Draw over the Fancy stars
					DrawClassicFancyOverlay(context);
				} else if (CompareAssets(asset, barsFolder + "MP_Fill")) {
					// Draw over the Bars mana bars
					DrawBarsOverlay(context);
				} else if (CompareAssets(asset, fancyFolder + "Star_A") || CompareAssets(asset, fancyFolder + "Star_B") || CompareAssets(asset, fancyFolder + "Star_C") || CompareAssets(asset, fancyFolder + "Star_Single")) {
					// Draw over the Fancy star panels
					DrawFancyPanelOverlay(context);
				} else if (CompareAssets(asset, barsFolder + "MP_Panel_Middle")) {
					// Draw over the Bars middle mana panels
					DrawBarsPanelOverlay(context);
				}
			}
		}

		private static bool CompareAssets(Asset<Texture2D> existingAsset, string compareAssetPath) {
			// This is a helper method for checking if a certain vanilla asset was drawn
			return existingAsset == Main.Assets.Request<Texture2D>(compareAssetPath);
		}

		private static void DrawClassicFancyOverlay(ResourceOverlayDrawContext context) {
			//Draw over the Classic / Mana stars
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicManaOverlay", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawFancyPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Fancy star panels
			string texture = "ExampleMod/Common/UI/ResourceDisplay/FancyManaOverlay_Panel";
			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the heart),
			// the position should be modified to compensate for the sprite size difference
			Vector2 positionOffset;

			if (context.resourceNumber == context.snapshot.AmountOfManaStars) {
				//Final panel in the column.  Determine whether it has panels above it
				if (context.resourceNumber == 1) {
					// First and only panel
					// Vanilla texture is "Star_Single"
					positionOffset = new Vector2(4, 4);
				} else {
					// Other panels existed above this panel
					// Vanilla texture is "Star_C"
					positionOffset = new Vector2(4, 0);
				}
			} else if (context.resourceNumber == 1) {
				// First panel in the column
				// Vanilla texture is "Star_A"
				positionOffset = new Vector2(4, 4);
			} else {
				// Any panel that has a panel above AND below it
				// Vanilla texture is "Star_B"
				positionOffset = new Vector2(4, 0);
			}

			context.texture = ModContent.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);
			context.position += positionOffset;
			context.Draw();
		}

		private static void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars mana bars
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Fill", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawBarsPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars middle life panels
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Panel", AssetRequestMode.ImmediateLoad);
			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the bar filling),
			// the position should be modified to compensate for the sprite size difference
			context.position.Y += 6;
			context.Draw();
		}
	}
}
