using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ExampleMod.Items
{
	public class ExampleItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded item.");
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.maxStack = 999;
			item.value = 100;
			item.rare = 1;
		}

		private Vector2 boxSize; // stores the size of our tooltip box
		const int paddingForBox = 10;

		public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			// You can offset the entire tooltip by changing x and y
			// You can actually have the entire tooltip draw somewhere else, x and y is where the tooltip starts drawing

			// Draw a magic box for this tooltip
			// From all tooltips we select their texts
			var texts = lines.Select(z => z.text);
			// Get the longest text, we do this by ordering by length (descending), and then grab the first one
			string longestText = texts.ToList().OrderByDescending(z => z.Length).First();
			// Calculate our widh for the box, which will be the width of the longest text, plus some padding
			int widthForBox = (int)Main.fontMouseText.MeasureString(longestText).X + paddingForBox * 2;
			// Calculate our height for the box, which will be the sum of the text heights, plus some padding
			int heightForBox = (int)texts.ToList().Sum(z => Main.fontMouseText.MeasureString(z).Y) + paddingForBox;
			// Set our boxSize to our calculated size, now we can use this elsewhere too
			boxSize = new Vector2(widthForBox, heightForBox);

			// We will start drawing the box slightly offset to accommodate for padding
			Vector2 drawPosForBox = new Vector2(x - paddingForBox, y - paddingForBox);
			Rectangle drawRectForBox = new Rectangle((int)drawPosForBox.X, (int)drawPosForBox.Y, widthForBox, heightForBox);
			// Draw the magic box
			Main.spriteBatch.Draw(Main.magicPixel, drawRectForBox, Main.mouseTextColorReal);

			return true;
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (!line.oneDropLogo)
			{
				// You are not allowed to change these, modders should use ModifyTooltips to modify them
				//line.text = "you shall not pass...";
				//line.oneDropLogo = false;
				//line.color = Color.AliceBlue;
				//line.overrideColor = Color.AliceBlue;
				//line.isModifier = false;
				//line.isModifierBad = false;
				//line.index = 1;

				// Let's draw the item name centered so it's in the middle, and let's add a form of separator
				string sepText = "-----"; // This is our separator, which will go between the item name and the rest
				float sepHeight = line.font.MeasureString(sepText).Y; // Height of our separator

				// If our line text equals our item name, this is our tooltip line for the item name
				// if (line.text == item.HoverName)
				// What is more accurate to check is the layer name and mod
				if (line.Name == "ItemName" && line.mod == "Terraria") 
					// We check for Terraria so we modify the vanilla tooltip and not a modded one
					// This could be important, in case some mod does a lot of custom work and removes the standard tooltip
					// For tooltip layers, check the documentation for TooltipLine
				{
					// Our offset is half the width of our box, minus the padding of one side
					float boxOffset = boxSize.X / 2 - paddingForBox;
					// The X coordinate where we draw is where the line would draw, plus the box offset,
					// which would place the START of the string at the center, so we subtract half of the line width to center it completely
					float drawX = line.X + boxOffset - line.font.MeasureString(sepText).X / 2;
					float drawY = line.Y + sepHeight / 2;

					// Note how our line object has many properties we can use for drawing
					// Here we draw the separator, note that it'd make more sense to use PostDraw for this, but either will work
					ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.font, sepText,
						new Vector2(drawX, drawY), line.color, line.rotation, line.origin, line.baseScale, line.maxWidth, line.spread);

					// Here we do the same thing as we did for drawX, which will center our ItemName tooltip
					line.X += (int)boxOffset - (int)line.font.MeasureString(line.text).X / 2;
					// yOffset affects the offset that is added every next line, so this will cause the line to come after the separator to be drawn slightly lower
					yOffset = (int)sepHeight / 4;
				}
				else
				{
					// Reset the offset for other lines
					yOffset = 0;
				}
			}
			return true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock);
			recipe.SetResult(this, 999);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("ExampleMod:ExampleItem");
			recipe.SetResult(this, 999);
			recipe.AddRecipe();

			/*
			// Start a new Recipe. (Prepend with "ModRecipe " if 1st recipe in code block.)
			recipe = new ModRecipe(mod);
			// Add a Vanilla Ingredient. 
			// Look up ItemIDs: https://github.com/bluemagic123/tModLoader/wiki/Vanilla-Item-IDs
			// To specify more than one ingredient, use multiple recipe.AddIngredient() calls.
			recipe.AddIngredient(ItemID.DirtBlock);
			// An optional 2nd argument will specify a stack of the item. 
			recipe.AddIngredient(ItemID.Acorn, 10);
			// We can also specify the current item as an ingredient
			recipe.AddIngredient(this, 2);
			// Add a Mod Ingredient. Do not attempt ItemID.EquipMaterial, it's not how it works.
			recipe.AddIngredient(mod, "EquipMaterial", 3);
			// an alternate approach to the above.
			recipe.AddIngredient(mod.ItemType("EquipMaterial"), 3);
			// RecipeGroups allow you create a recipe that accepts items from a group of similar ingredients. For example, all varieties of Wood are in the vanilla "Wood" Group
			recipe.AddRecipeGroup("Wood"); // check here for other vanilla groups: https://github.com/bluemagic123/tModLoader/wiki/ModRecipe#public-void-addrecipegroupstring-name-int-stack--1
			// Here is using a mod recipe group. Check out ExampleMod.AddRecipeGroups() to see how to register a recipe group.
			recipe.AddRecipeGroup("ExampleMod:ExampleItem", 2);
			// To specify a crafting station, specify a tile. Look up TileIDs: https://github.com/bluemagic123/tModLoader/wiki/Vanilla-Tile-IDs
			recipe.AddTile(TileID.WorkBenches);
			// A mod Tile example. To specify more than one crafting station, use multiple recipe.AddTile() calls.
			recipe.AddTile(mod, "ExampleWorkbench");
			// There is a limit of 14 ingredients and 14 tiles to a recipe.
			// Special
			// Water, Honey, and Lava are not tiles, there are special bools for those. Also needSnowBiome. Water also specifies that it works with Sinks.
			recipe.needHoney = true;
			// Set the result of the recipe. You can use stack here too. Since this is in a ModItem class, we can use "this" to specify this item as the result.
			recipe.SetResult(this, 999); // or, for a vanilla result, recipe.SetResult(ItemID.Muramasa);
			// Finish your recipe
			recipe.AddRecipe();
			*/
		}
	}
}
