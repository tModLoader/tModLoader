using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExamplePlatform : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded platform.");
		}

		public override void SetDefaults()
		{
			item.width = 8;
			item.height = 10;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType("ExamplePlatform");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleBlock");
			recipe.SetResult(this, 2);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.AddRecipe();
		}
	}
}