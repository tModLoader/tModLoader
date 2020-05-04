using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	public class BossItem : ExampleItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Used to craft boss items");
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}