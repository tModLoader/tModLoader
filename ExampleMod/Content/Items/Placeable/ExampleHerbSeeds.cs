using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleHerbSeeds : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Herb Seeds");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleHerb>());
			Item.width = 12;
			Item.height = 14;
			Item.value = 80;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<ExampleBlock>(), 1)
				.Register();
		}
	}
}
