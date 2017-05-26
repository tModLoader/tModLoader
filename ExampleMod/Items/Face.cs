using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class Face : ModItem
	{
		public override void SetStaticDefaults()
		{
			// See here for help on using Tags: http://terraria.gamepedia.com/Chat#Tags
			Tooltip.SetDefault("How are you feeling today?"
				+ string.Format("\n[c/FF0000:Colors ][c/00FF00:are ][c/0000FF:fun ]and so are items: [i:{0}][i:{1}][i/s123:{2}]", item.type, mod.ItemType<CarKey>(), ItemID.Ectoplasm));

			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(30, 4));
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.value = 100;
			item.rare = 1;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			TooltipLine line = new TooltipLine(mod, "Face", "I'm feeling just fine!");
			line.overrideColor = new Color(100, 100, 255);
			tooltips.Add(line);
			foreach (TooltipLine line2 in tooltips)
			{
				if (line2.mod == "Terraria" && line2.Name == "ItemName")
				{
					line2.overrideColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
				}
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}