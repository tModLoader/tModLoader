using ExampleMod.Common.Players;
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
				string name = context.texture.Name;

				string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
				string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

				// NOTE: CompareAssetNames is defined below this method's body
				if (name == TextureAssets.Mana.Name) {
					// Draw over the Classic stars
					DrawClassicFancyOverlay(context);
				} else if (CompareAssetNames(name, fancyFolder + "Star_Fill")) {
					// Draw over the Fancy stars
					DrawClassicFancyOverlay(context);
				} else if (CompareAssetNames(name, barsFolder + "MP_Fill")) {
					// Draw over the Bars mana bars
					DrawBarsOverlay(context);
				} else if (CompareAssetNames(name, fancyFolder + "Star_A") || CompareAssetNames(name, fancyFolder + "Star_B") || CompareAssetNames(name, fancyFolder + "Star_C") || CompareAssetNames(name, fancyFolder + "Star_Single")) {
					// Draw over the Fancy star panels
					DrawFancyPanelOverlay(context);
				} else if (CompareAssetNames(name, barsFolder + "MP_Panel_Middle")) {
					// Draw over the Bars middle mana panels
					DrawBarsPanelOverlay(context);
				}
			}
		}

		private static bool CompareAssetNames(string existingName, string compareAssetPath) {
			// This is a helper method for checking if a certain vanilla asset was drawn
			return existingName == Main.Assets.Request<Texture2D>(compareAssetPath).Name;
		}

		private static void DrawClassicFancyOverlay(ResourceOverlayDrawContext context) {
			//Draw over the Classic / Mana stars
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicManaOverlay", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawFancyPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Fancy star panels
			string texture = "ExampleMod/Common/UI/ResourceDisplay/FancyManaOverlay_";

			if (context.resourceNumber == context.snapshot.AmountOfManaStars) {
				//Final panel in the column.  Determine whether it has panels above it
				if (context.resourceNumber == 1) {
					// First and only panel
					texture += "Single";
				} else {
					// Other panels existed above this panel
					texture += "Bottom";
				}
			} else if (context.resourceNumber == 1) {
				// First panel in the column
				texture += "Top";
			} else {
				// Any panel that has a panel above AND below it
				texture += "Middle";
			}

			context.texture = ModContent.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars mana bars
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Fill", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawBarsPanelOverlay(ResourceOverlayDrawContext context) {
			// Draw over the Bars middle life panels
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Panel_Middle", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}
	}
}
