using ExampleMod.Items.ExampleDamageClass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.UI
{
	internal class ExampleResourceBar : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler to do it while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the mod class.
		private UIText _text;
		private UIPanel backPanel;
		public UIImage barFrame = null;
		private Color gradientA;
		private Color gradientB;
		public bool visible = true;

		public override void OnInitialize() {
			backPanel = new UIPanel(); // Create a backpanel for all the elements to sit on top of.
			backPanel.Left.Set(Main.screenWidth / 2f - backPanel.Width.Pixels / 2, 0f); // Center it in the middle of the screen, you can change this to wherever you wish.
			backPanel.Top.Set(50, 0f); // Placing it just a bit below the top of the screen.
			backPanel.Width.Set(138, 0f);
			backPanel.Height.Set(34, 0f);
			backPanel.BackgroundColor = Color.Transparent; // Make it invisible.
			backPanel.BorderColor = Color.Transparent; // Hide the black border that things automatically get.

			barFrame = new UIImage(GetTexture("ExampleMod/UI/ExampleResourceFrame"));
			barFrame.Left.Set(-12, 0f);
			barFrame.Top.Set(-12, 0f);
			barFrame.Width.Set(138, 0f);
			barFrame.Height.Set(34, 0f);

			_text = new UIText("0/0"); // text to show stat
			_text.Width.Set(138, 0f);
			_text.Height.Set(34, 0f);
			_text.Top.Set(30, 0f);
			_text.Left.Set(-15, 0f);

			gradientA = new Color(123, 25, 138); // A dark purple
			gradientB = new Color(187, 91, 201); // A light purple

			backPanel.Append(_text);
			backPanel.Append(barFrame);
			Append(backPanel);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			ExampleDamagePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExampleDamagePlayer>();
			float quotient = 1f;
			// Calculate quotient
			quotient = modPlayer.currentResource / modPlayer.OverallMaximumResource; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
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
			ExampleDamagePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExampleDamagePlayer>();
			// Setting the text per tick to update and show our resource values.
			_text.SetText("Example Resource: " + modPlayer.currentResource + " / " + modPlayer.OverallMaximumResource);
			base.Update(gameTime);
		}
	}
}
