using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class Face : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Face";
			item.width = 20;
			item.height = 20;
			item.toolTip = "How are you feeling today?";
			item.value = 100;
			item.rare = 1;
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override DrawAnimation GetAnimation()
		{
			return new DrawAnimationVertical(30, 4);
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
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