using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleTooltipsItem : ModItem
	{
		public override void SetStaticDefaults() {
			// See here for help on using Tags: http://terraria.gamepedia.com/Chat#Tags
			Tooltip.SetDefault("How are you feeling today?"
				+ $"\n[c/FF0000:Colors ][c/00FF00:are ][c/0000FF:fun ]and so are items: [i:{Item.type}][i:{ModContent.ItemType<ExampleMountItem>()}][i/s123:{ItemID.Ectoplasm}]");

			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(30, 4));
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Here we add a tooltipline that will later be removed, showcasing how to remove tooltips from an item
			var line = new TooltipLine(Mod, "Verbose:RemoveMe", "This tooltip won't show in-game");
			tooltips.Add(line);

			line = new TooltipLine(Mod, "Face", "I'm feeling just fine!") {
				overrideColor = new Color(100, 100, 255)
			};
			tooltips.Add(line);

			// Here we give the item name a rainbow effect.
			foreach (TooltipLine line2 in tooltips) {
				if (line2.mod == "Terraria" && line2.Name == "ItemName") {
					line2.overrideColor = Main.DiscoColor;
				}
			}

			// Here we will remove all tooltips whose title end with ':RemoveMe'
			// One like that is added at the start of this method
			tooltips.RemoveAll(l => l.Name.EndsWith(":RemoveMe"));

			// Another method of removal can be done if you know the index of the tooltip:
			//tooltips.RemoveAt(index);

			// You can also remove a specific line, if you have access to that object:
			//tooltips.Remove(tooltipLine);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}