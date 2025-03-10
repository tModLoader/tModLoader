using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ExampleMod.Content.Items
{
	// Showcases various tooltip drawing hooks. For manipulating tooltips, see ExampleTooltipsItem.cs instead.
	// The code in this example uses the available tooltip drawing hooks to do the following:
	// 1. Center the item name tooltip line
	// 2. Draw "-----" below the item name
	// 3. Draw a custom icon before one of the tooltip lines.
	// 4. Draw a custom item tooltip background replacing the normal background (2 examples)
	public class ExampleDrawTooltips : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.AnnouncementBox}";

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}

		private Vector2 boxSize; // stores the size of our tooltip box
		private const int paddingForBox = 10;

		public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, Vector2 textAreaSize, ref int x, ref int y, ref bool boxDrawn) {
			boxSize = textAreaSize;

			// We are using PreDrawTooltip to draw a custom background. If the user has disabled the "Hover Text Boxes" setting, we will skip our custom drawing to respect that.
			if (!Main.SettingsEnabled_OpaqueBoxBehindTooltips) {
				return true;
			}

			// We set boxDrawn to true to tell GlobalItem.PreDrawTooltip methods and vanilla code that a background has already been drawn so they shouldn't draw an additional box.
			boxDrawn = true;

			// 2 separate examples to show multiple approaches.
			if (Main.LocalPlayer.direction == 1) {
				// When facing right, we will show a simple background example that is exactly the same as the vanilla except for color. We do not need to do any other adjustments.
				int paddingX = 14;
				int paddingY = 9;
				Utils.DrawInvBG(Main.spriteBatch, new Rectangle(x - paddingX, y - paddingY, (int)textAreaSize.X + paddingX * 2, (int)textAreaSize.Y + paddingY + paddingY / 2), new Color(123, 25, 81, 255) * 0.925f);
			}
			else {
				// When facing left, we will show a custom background example. 
				// We will draw the box slightly offset from textAreaSize to accommodate for padding
				int paddingX = 4;
				int paddingY = 4;
				Rectangle drawRectForBox = new Rectangle(x - paddingX, y - paddingY, (int)textAreaSize.X + paddingX * 2, (int)textAreaSize.Y + paddingY * 2);

				// Draw the custom background
				var customTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Backgrounds/ExampleBiomeUnderground3").Value;
				Main.spriteBatch.Draw(customTexture, drawRectForBox, Color.White);
			}
			return true;
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			// You are not allowed to change these here, modders should use ModifyTooltips to modify them
			// line.Text = "you shall not pass...";
			// line.OneDropLogo = false;
			// line.Color = Color.AliceBlue;
			// line.OverrideColor = Color.AliceBlue;
			// line.IsModifier = false;
			// line.IsModifierBad = false;
			// line.Index = 1;
			// line.OffsetTextSize = new Vector2(100, 0);

			// if (line.FullName == "Terraria/ItemName") { <-- You might prefer this approach
			if (line.Mod == "Terraria" && line.Name == "ItemName") {
				// Let's draw the item name centered so it's in the middle, and let's add a form of separator
				string sepText = "-----"; // This is our separator, which will go between the item name and the rest. This example is text, but custom drawing is also an option.

				// Our offset is half the width of our box, minus the padding of one side
				float boxOffset = boxSize.X / 2 - paddingForBox;
				// The X coordinate where we draw is where the line would draw, plus the box offset,
				// which would place the START of the string at the center, so we subtract half of the line width to center it completely
				float drawX = line.X + boxOffset - line.Font.MeasureString(sepText).X / 2;
				float drawY = line.Y + 20;

				// Note how our line object has many properties we can use for drawing
				// Here we draw the separator, note that PostDraw could be used for this, but either will work
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, sepText,
					new Vector2(drawX, drawY), line.Color, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

				// Here we do the same thing as we did for drawX, which will center our ItemName tooltip
				line.X += (int)boxOffset - (int)line.Font.MeasureString(line.Text).X / 2;
				// yOffset affects the offset that is added to the next line, so this will cause the line to come after the separator to be drawn slightly lower
				yOffset = 15; // This matches the OffsetTextSize.Y we assigned in ModifyTooltips
			}
			else if (line.Mod == "Terraria" && line.Name == "Tooltip1") {
				// In this example, we draw a heart icon in front of the 2nd tooltip line.
				Main.spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(line.X, line.Y), Color.White);
				line.X += 30; // move the text over to accommodate the icon. This matches the OffsetTextSize.X we assigned in ModifyTooltips
			}
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			foreach (TooltipLine line in tooltips) {
				if (line.Mod == "Terraria" && line.Name == "ItemName") {
					line.OffsetTextSize = new Vector2(0, 15); // Make space for "----" separator
				}

				if (line.Mod == "Terraria" && line.Name == "Tooltip1") {
					line.OffsetTextSize = new Vector2(30, 0); // Make space for custom icon
				}
			}
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
