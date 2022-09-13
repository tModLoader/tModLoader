using ExampleMod.Common.Players;
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
				DrawClassicFancyOverlay(context);
			} else if (context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartPanelsFirstRow) || context.IsSlot(FancyClassicPlayerResourcesDisplaySet.HeartPanelsSecondRow)) {
				string texture = "ExampleMod/Common/UI/ResourceDisplay/FancyLifeOverlay_";

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
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifePanels) && fillResourceNumber >= 1 && fillResourceNumber <= context.snapshot.AmountOfLifeHearts) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Panel_Middle", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.IsSlot(HorizontalBarsPlayerReosurcesDisplaySet.LifeBars)) {
				DrawBarsOverlay(context);
			} else {
				// None of the above cases applied, but this could be a modded resource that uses vanilla textures.
				// So, it would be a good idea to check if certain vanilla resources are being drawn and then draw over them like usual
				string name = context.texture.Name;

				string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
				string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

				if (name == TextureAssets.Heart.Name || name == TextureAssets.Heart2.Name) {
					DrawClassicFancyOverlay(context);
				} else if (name == Main.Assets.Request<Texture2D>(fancyFolder + "Heart_Fill").Name || name == Main.Assets.Request<Texture2D>(fancyFolder + "Heart_Fill_B").Name) {
					DrawClassicFancyOverlay(context);
				} else if (name == Main.Assets.Request<Texture2D>(barsFolder + "HP_Fill").Name || name == Main.Assets.Request<Texture2D>(barsFolder + "HP_Fill_Honey").Name) {
					DrawBarsOverlay(context);
				}
			}
		}

		private static void DrawClassicFancyOverlay(ResourceOverlayDrawContext context) {
			//Draw over the Classic hearts
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicLifeOverlay", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}

		private static void DrawBarsOverlay(ResourceOverlayDrawContext context) {
			context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsLifeOverlay_Fill", AssetRequestMode.ImmediateLoad);
			context.Draw();
		}
	}
}
