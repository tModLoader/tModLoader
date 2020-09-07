namespace ExampleMod.Content.Items
{
	public class BossItem : ExampleItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Used to craft boss items");
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.Register();
		}
	}
}