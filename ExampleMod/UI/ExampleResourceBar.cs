using ExampleMod.Items.ExampleDamageClass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.UI
{
	internal class ExampleResourceBar : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler to do it while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the mod class.
		private UIText text;
		private UIPanel backPanel;
		public UIImage barFrame;
		private Color gradientA;
		private Color gradientB;

		public override void OnInitialize() {
			backPanel = new UIPanel(); // Create a backpanel for all the elements to sit on top of.
			backPanel.Left.Set(-backPanel.Width.Pixels - 600, 1f); // Place the resource bar to the left of the hearts.
			backPanel.Top.Set(30, 0f); // Placing it just a bit below the top of the screen.
			backPanel.Width.Set(138, 0f);
			backPanel.Height.Set(34, 0f);
			backPanel.BackgroundColor = Color.Transparent; // Make it invisible.
			backPanel.BorderColor = Color.Transparent; // Hide the black border that things automatically get.

			barFrame = new UIImage(GetTexture("ExampleMod/UI/ExampleResourceFrame"));
			barFrame.Left.Set(-12, 0f);
			barFrame.Top.Set(-12, 0f);
			barFrame.Width.Set(138, 0f);
			barFrame.Height.Set(34, 0f);

			text = new UIText("0/0", 0.8f); // text to show stat
			text.Width.Set(138, 0f);
			text.Height.Set(34, 0f);
			text.Top.Set(30, 0f);
			text.Left.Set(-15, 0f);

			gradientA = new Color(123, 25, 138); // A dark purple
			gradientB = new Color(187, 91, 201); // A light purple

			backPanel.Append(text);
			backPanel.Append(barFrame);
			Append(backPanel);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// This prevents drawing unless we are using an ExampleDamageItem
			if (!(Main.LocalPlayer.HeldItem.modItem is ExampleDamageItem))
				return;

			base.Draw(spriteBatch);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<ExampleDamagePlayer>();
			// Calculate quotient
			float quotient = (float)modPlayer.exampleResourceCurrent / modPlayer.exampleResourceMax2; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			// Set the offset for the actual gradient bar, in case you need it somewhere else.
			hitbox.X += (int)backPanel.Left.Pixels + 10;
			hitbox.Y += (int)backPanel.Top.Pixels + 2;
			// Set the bounds of the gradient bar, adjusting it with pixel offsets if needed.
			hitbox.Width = (int)backPanel.Width.Pixels - 20;
			hitbox.Height = (int)backPanel.Height.Pixels - 4;
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps = (int)((right - left) * quotient);
			for (int i = 0; i < steps; i += 1) {
				//float percent = (float)i / steps; // Alternate Gradient Approach
				float percent = (float)i / (right - left);
				spriteBatch.Draw(Main.magicPixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
			}
		}
		public override void Update(GameTime gameTime) {
			if (!(Main.LocalPlayer.HeldItem.modItem is ExampleDamageItem))
				return;

			var modPlayer = Main.LocalPlayer.GetModPlayer<ExampleDamagePlayer>();
			// Setting the text per tick to update and show our resource values.
			text.SetText($"Example Resource: {modPlayer.exampleResourceCurrent} / {modPlayer.exampleResourceMax2}");
			base.Update(gameTime);
		}
	}
}
