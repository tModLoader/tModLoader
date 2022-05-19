using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleToilet : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Toilet");
			Tooltip.SetDefault("This is a modded toilet.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleToilet>());
			Item.value = 150;
			Item.maxStack = 99;
			Item.width = 16;
			Item.height = 24;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Toilet)
				.AddIngredient<ExampleItem>(10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
