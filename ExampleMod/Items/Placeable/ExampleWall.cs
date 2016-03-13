using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExampleWall : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Wall";
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			AddTooltip("This is a modded wall.");
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 7;
			item.useStyle = 1;
			item.consumable = true;
			item.createWall = mod.WallType("ExampleWall");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleBlock");
			recipe.SetResult(this, 4);
			recipe.AddRecipe();
		}
	}
}