using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class CarKey : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Car Key";
			item.width = 20;
			item.height = 30;
			item.toolTip = "This is a modded mount.";
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.value = 30000;
			item.rare = 2;
			item.useSound = 79;
			item.noMelee = true;
			item.mountType = mod.MountType("Car");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}