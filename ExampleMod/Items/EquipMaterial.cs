using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class EquipMaterial : ExampleItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Equipment Item");
			Tooltip.SetDefault("Used to craft equipment");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}