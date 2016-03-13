using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExampleWorkbench : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Workbench";
			item.width = 28;
			item.height = 14;
			item.maxStack = 99;
			AddTooltip("This is a modded workbench.");
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.value = 150;
			item.createTile = mod.TileType("ExampleWorkbench");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.WorkBench);
			recipe.AddIngredient(null, "ExampleBlock", 10);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}