using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class EquipMaterial : ExampleItem
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.name = "Equipment Item";
			item.toolTip = "Used to craft equipment";
			item.toolTip2 = "";
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