using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ExampleDoor : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Door";
			item.width = 14;
			item.height = 28;
			item.maxStack = 99;
			AddTooltip("This is a modded door.");
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.value = 150;
			item.createTile = mod.TileType("ExampleDoorClosed");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.WoodenDoor);
			recipe.AddIngredient(null, "ExampleBlock", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}