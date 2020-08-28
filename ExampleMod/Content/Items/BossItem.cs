using Terraria.GameContent.Creative;

namespace ExampleMod.Content.Items
{
	public class BossItem : ExampleItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Used to craft boss items");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type] = 100;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.Register();
		}
	}
}