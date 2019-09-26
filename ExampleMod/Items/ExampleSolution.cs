using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	public class ExampleSolution : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Monochromatic Solution");
			Tooltip.SetDefault("Used by the Clentaminator"
				+ "\nSpreads the example");
		}

		public override void SetDefaults() {
			item.shoot = ProjectileType<Projectiles.ExampleSolution>() - ProjectileID.PureSpray;
			item.ammo = AmmoID.Solution;
			item.width = 10;
			item.height = 12;
			item.value = Item.buyPrice(0, 0, 25, 0);
			item.rare = 3;
			item.maxStack = 999;
			item.consumable = true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 10);
			recipe.SetResult(this, 999);
			recipe.AddRecipe();
		}
	}
}
