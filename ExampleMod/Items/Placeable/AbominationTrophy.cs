using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class AbominationTrophy : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Abomination Trophy";
			item.width = 30;
			item.height = 30;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.value = 50000;
			item.createTile = mod.TileType("BossTrophy");
			item.placeStyle = 0;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleBlock", 10);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}