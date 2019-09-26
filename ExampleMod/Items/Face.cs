using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	public class Face : ModItem
	{
		public override void SetStaticDefaults() {
			// See here for help on using Tags: http://terraria.gamepedia.com/Chat#Tags
			Tooltip.SetDefault("How are you feeling today?"
				+ $"\n[c/FF0000:Colors ][c/00FF00:are ][c/0000FF:fun ]and so are items: [i:{item.type}][i:{ItemType<CarKey>()}][i/s123:{ItemID.Ectoplasm}]");

			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(30, 4));
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.value = 100;
			item.rare = 1;
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Here we add a tooltipline that will later be removed, showcasing how to remove tooltips from an item
			var line = new TooltipLine(mod, "Verbose:RemoveMe", "This tooltip won't show in-game");
			tooltips.Add(line);

			line = new TooltipLine(mod, "Face", "I'm feeling just fine!") {
				overrideColor = new Color(100, 100, 255)
			};
			tooltips.Add(line);
			foreach (TooltipLine line2 in tooltips) {
				if (line2.mod == "Terraria" && line2.Name == "ItemName") {
					line2.overrideColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
				}
			}

			// Here we will remove all tooltips whose title end with ':RemoveMe'
			// One like that is added at the start of this method
			tooltips.RemoveAll(l => l.Name.EndsWith(":RemoveMe"));

			// Another method of removal can be done if you know the index of the tooltip:
			//tooltips.RemoveAt(index);

			// You can also remove a specific line, if you have access to that object:
			//tooltips.Remove(tooltipLine);

			// You can also remove a range from the list
			// For example, this would remove the first 4 lines:
			//tooltips.RemoveRange(0, 4);
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}