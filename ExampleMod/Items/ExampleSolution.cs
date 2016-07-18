using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleSolution : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Monochromatic Solution";
			item.shoot = mod.ProjectileType("ExampleSolution") - ProjectileID.PureSpray;
			item.ammo = ItemID.GreenSolution;
			item.width = 10;
			item.height = 12;
			item.value = Item.buyPrice(0, 0, 25, 0);
			item.rare = 3;
			item.maxStack = 999;
			item.toolTip = "Used by the Clentaminator";
			item.toolTip2 = "Spreads the example";
			item.consumable = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.SetResult(this, 999);
			recipe.AddRecipe();
		}
	}
}
