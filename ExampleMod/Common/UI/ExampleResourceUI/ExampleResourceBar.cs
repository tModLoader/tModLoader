using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleResourceUI
{
	// This custom UI will show whenever the player is holding the ExampleCustomResourceWeapon item and will display the player's custom resource amounts that are tracked in ExampleResourcePlayer
	internal class ExampleResourceBar : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private UIText text;
		private UIElement area;
		private UIImage barFrame;
		private Color gradientA;
		private Color gradientB;

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.
			area = new UIElement();
			area.Left.Set(-area.Width.Pixels - 600, 1f); // Place the resource bar to the left of the hearts.
			area.Top.Set(30, 0f); // Placing it just a bit below the top of the screen.
			area.Width.Set(182, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
			area.Height.Set(60, 0f);

			barFrame = new UIImage(ModContent.Request<Texture2D>("ExampleMod/Common/UI/ExampleResourceUI/ExampleResourceFrame")); // Frame of our resource bar
			barFrame.Left.Set(22, 0f);
			barFrame.Top.Set(0, 0f);
			barFrame.Width.Set(138, 0f);
			barFrame.Height.Set(34, 0f);

			text = new UIText("0/0", 0.8f); // text to show stat
			text.Width.Set(138, 0f);
			text.Height.Set(34, 0f);
			text.Top.Set(40, 0f);
			text.Left.Set(0, 0f);

			gradientA = new Color(123, 25, 138); // A dark purple
			gradientB = new Color(187, 91, 201); // A light purple

			area.Append(text);
			area.Append(barFrame);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// This prevents drawing unless we are using an ExampleCustomResourceWeapon
			if (Main.LocalPlayer.HeldItem.ModItem is not ExampleCustomResourceWeapon)
				return;

			base.Draw(spriteBatch);
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<ExampleResourcePlayer>();
			// Calculate quotient
			float quotient = (float)modPlayer.exampleResourceCurrent / modPlayer.exampleResourceMax2; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.

			// Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
			Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 12;
			hitbox.Width -= 24;
			hitbox.Y += 8;
			hitbox.Height -= 16;

			// Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps = (int)((right - left) * quotient);
			for (int i = 0; i < steps; i += 1) {
				// float percent = (float)i / steps; // Alternate Gradient Approach
				float percent = (float)i / (right - left);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
			}
		}

		public override void Update(GameTime gameTime) {
			if (Main.LocalPlayer.HeldItem.ModItem is not ExampleCustomResourceWeapon)
				return;

			var modPlayer = Main.LocalPlayer.GetModPlayer<ExampleResourcePlayer>();
			// Setting the text per tick to update and show our resource values.
			text.SetText(ExampleResourceUISystem.ExampleResourceText.Format(modPlayer.exampleResourceCurrent, modPlayer.exampleResourceMax2));
			base.Update(gameTime);
		}
	}

	// This class will only be autoloaded/registered if we're not loading on a server
	[Autoload(Side = ModSide.Client)]
	internal class ExampleResourceUISystem : ModSystem
	{
		private UserInterface ExampleResourceBarUserInterface;

		internal ExampleResourceBar ExampleResourceBar;

		public static LocalizedText ExampleResourceText { get; private set; }

		public override void Load() {
			ExampleResourceBar = new();
			ExampleResourceBarUserInterface = new();
			ExampleResourceBarUserInterface.SetState(ExampleResourceBar);

			string category = "UI";
			ExampleResourceText ??= Mod.GetLocalization($"{category}.ExampleResource");
		}

		public override void UpdateUI(GameTime gameTime) {
			ExampleResourceBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"ExampleMod: Example Resource Bar",
					delegate {
						ExampleResourceBarUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
