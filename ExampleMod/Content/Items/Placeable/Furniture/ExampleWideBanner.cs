using ExampleMod.Content.Tiles.Furniture;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture;

/// <summary>
/// Item that places <see cref="ExampleWideBannerTile"/>
/// </summary>
public class ExampleWideBanner : ModItem
{
	public override void SetDefaults() {
		Item.DefaultToPlaceableTile(ModContent.TileType<ExampleWideBannerTile>());
		Item.value = Terraria.Item.buyPrice(copper: 10);
	}

	// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.Register();
	}
}