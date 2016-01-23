using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExampleSeeds : ModItem
	{
		public override void SetDefaults()
		{
			item.autoReuse = true;
			item.name = "Example Seeds";
			item.useTurn = true;
			item.useStyle = 1;
			item.useAnimation = 15;
			item.useTime = 10;
			item.maxStack = 99;
			item.consumable = true;
			item.placeStyle = 0;
			item.width = 12;
			item.height = 14;
			item.value = 80;
			item.createTile = mod.TileType("ExamplePlant");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleBlock", 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}