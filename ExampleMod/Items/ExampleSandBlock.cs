using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExampleSandBlock : ModItem
	{
		public override void SetStaticDefaults() => Tooltip.SetDefault("This is a modded sand block.");

		public override void SetDefaults() {
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			//Create the ExampleSand tile
			item.createTile = ModContent.TileType<Tiles.ExampleSand>();
		}

		public override void AddRecipes() {
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>());
			recipe.SetResult(this, 10);
			recipe.AddRecipe();

			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<ExampleWall>(), 4);
			recipe.AddTile(ModContent.TileType<Tiles.ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();

			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<ExamplePlatform>(), 2);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}