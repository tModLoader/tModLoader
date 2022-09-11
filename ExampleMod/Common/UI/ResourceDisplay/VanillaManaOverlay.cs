using ExampleMod.Common.Players;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ResourceDisplay
{
	// The code reponsible for tracking and modifying how many extra mana stars/bars are displayed can be found in Common/Systems/ExampleStatIncreaseSystem.cs
	public class VanillaManaOverlay : ModResourceOverlay
	{
		// Unlike VanillaLifeOverlay, every star is drawn over by this hook.
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			// Only mana resources are replaced
			if (context.DrawSource is not ResourceDrawSource_Mana)
				return;
			
			// Bars panel drawing has two additional elements, so the "resource index" needs to be offset
			int fillResourceNumber = context.DrawSource is ResourceDrawSource_BarsManaPanel ? context.resourceNumber - 1 : context.resourceNumber;
			if (Main.LocalPlayer.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals <= 0)
				return;

			// "context" contains information used to draw the resource
			// If you want to draw directly on top of the vanilla hearts, just replace the texture and have the context draw the new texture

			// NOTE: If you copy this code into PreDrawResource and only want to draw behind the resource and NOT modify it, copy the context into a variable and THEN draw using it:
			/*
			ResourceOverlayDrawContext contextCopy = context;
			contextCopy.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicManaOverlay", AssetRequestMode.ImmediateLoad);
			contextCopy.Draw();
			*/

			if (context.DrawSource is ResourceDrawSource_ClassicMana || context.DrawSource is ResourceDrawSource_FancyMana) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/ClassicStarOverlay", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.DrawSource is ResourceDrawSource_FancyManaPanel) {
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
			} else if (context.DrawSource is ResourceDrawSource_BarsManaPanel && fillResourceNumber >= 1 && fillResourceNumber <= context.snapshot.AmountOfManaStars) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Panel_Middle", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} else if (context.DrawSource is ResourceDrawSource_BarsMana) {
				context.texture = ModContent.Request<Texture2D>("ExampleMod/Common/UI/ResourceDisplay/BarsManaOverlay_Fill", AssetRequestMode.ImmediateLoad);
				context.Draw();
			} 
		}
	}
}
