using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class BossItem : ExampleItem
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.name = "Boss Item";
			item.toolTip = "Used to craft boss items";
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