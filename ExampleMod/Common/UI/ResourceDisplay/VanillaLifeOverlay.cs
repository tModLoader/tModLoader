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
	public class VanillaLifeOverlay : ModResourceOverlay
	{
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			// Life resources are drawn over in groups of two
			// Bars panel drawing has two additional elements, so the "resource index" needs to be offset
			int fillResourceNumber = context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifePanels) ? context.resourceNumber - 1 : context.resourceNumber;
			int exampleFruits = Main.LocalPlayer.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits;

			bool canReplaceResource;

			if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifePanels)) {
				// Vanilla "replaces" the bars from right to left, so it would be a good idea to mimic that behaviour
				canReplaceResource = fillResourceNumber > context.snapshot.AmountOfLifeHearts - 2 * exampleFruits;
			} else {
				canReplaceResource = fillResourceNumber <= 2 * exampleFruits;
			}

			if (!canReplaceResource)
				return;

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture
			if (context.IsSlot(ClassicPlayerResourcesDisplaySet.Hearts) || context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartsFirstRow) || context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartsSecondRow)) {
				// Draw over the Classic / Fancy hearts
				DrawClassicFancyOverlay(context);
			} else if (context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartPanelsFirstRow) || context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartPanelsSecondRow)) {
				// Draw over the Fancy heart panels
				DrawFancyPanelOverlay(context);
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifePanels) && fillResourceNumber >= 1 && fillResourceNumber <= context.snapshot.AmountOfLifeHearts) {
				// Draw over the Bars middle life panels
				DrawBarsPanelOverlay(context);
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifeBars)) {
				// Draw over the Bars life bars
				DrawBarsOverlay(context);
			} else {
				// None of the above cases applied, but this could be a modded resource that uses vanilla textures.
				// So, it would be a good idea to check if certain vanilla resources are being drawn and then draw over them like usual
				string name = context.texture.Name;

				string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
				string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

				// NOTE: CompareAssetNames is defined below this method's body
				if (name == TextureAssets.Heart.Name || name == TextureAssets.Heart2.Name) {
					// Draw over the Classic hearts
					DrawClassicFancyOverlay(context);
				} else if (CompareAssetNames(name, fancyFolder + "Heart_Fill") || CompareAssetNames(name, fancyFolder + "Heart_Fill_B")) {
					// Draw over the Fancy hearts
					DrawClassicFancyOverlay(context);
				} else if (CompareAssetNames(name, barsFolder + "HP_Fill") || CompareAssetNames(name, barsFolder + "HP_Fill_Honey")) {
					// Draw over the Bars life bars
					DrawBarsOverlay(context);
				} else if (CompareAssetNames(name, fancyFolder + "Heart_Left") || CompareAssetNames(name, fancyFolder + "Heart_Middle") || CompareAssetNames(name, fancyFolder + "Heart_Right") || CompareAssetNames(name, fancyFolder + "Heart_Right_Fancy") || CompareAssetNames(name, fancyFolder + "Heart_Single_Fancy")) {
					// Draw over the Fancy heart panels
					DrawFancyPanelOverlay(context);
				} else if (CompareAssetNames(name, barsFolder + "HP_Panel_Middle")) {
					// Draw over the Bars middle life panels
					DrawBarsPanelOverlay(context);
				}
			}
		}

		private static bool CompareAssetNames(string existingName, string compareAssetPath) {
			// This is a helper method for checking if a certain vanilla asset was drawn
			return existingName == Main.Assets.Request<Texture2D>(compareAssetPath).Name;
		}

		private static void DrawClassicFancyOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Classic / Fancy hearts
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicLifeOverlay", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawFancyPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Fancy heart panels
			string texture = "ExampleMod/Common/UI/ResourceDisplay/FancyLifeOverlay_Panel";
			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the heart),
			// the position should be modified to compensate for the sprite size difference
			Vector2 positionOffset;

			if (context.resourceNumber == context.snapshot.AmountOfLifeHearts) {
				// Final panel to draw has a special "Fancy" variant.  Determine whether it has panels to the left of it
				if (context.resourceNumber % 10 == 1) {
					// First and only panel in this panel's row
					// Vanilla texture is "Heart_Single_Fancy"
					positionOffset = new Vector2(8, 8);
				} else {
					// Other panels existed in this panel's row
					// Vanilla texture is "Heart_Right_Fancy"
					positionOffset = new Vector2(8, 8);
				}
			} else if (context.resourceNumber % 10 == 1) {
				// First panel in this row
				// Vanilla texture is "Heart_Left"
				positionOffset = new Vector2(4, 4);
			} else if (context.resourceNumber % 10 <= 9) {
				// Any panel that has a panel to its left AND right
				// Vanilla texture is "Heart_Middle"
				positionOffset = new Vector2(0, 4);
			} else {
				// Final panel in the first row
				// Vanilla texture is "Heart_Right"
				positionOffset = new Vector2(0, 4);
			}

			context.texture = ModContent.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);
			context.position += positionOffset;
			context.Draw();
		}

		private static void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars life bars
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Fill", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawBarsPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars middle life panels
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Panel", AssetRequestMode.ImmediateLoad);
			// The original position refers to the entire panel slice.
			// However, since this overlay only modifies the "inner" portion of the slice (aka the part behind the bar filling),
			// the position should be modified to compensate for the sprite size difference
			context.position.Y += 6;
			context.Draw();
		}
	}
}
