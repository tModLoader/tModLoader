using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items;

/*
The items in this file showcase customizing the decrafting feature of the Shimmer liquid.
By default, Shimmer will tranform crafted items back into their original recipe ingredients.

ShimmerShowcaseCrimsonCorruption showcases crimson and corruption specific shimmer decrafting results.

ShimmerShowcaseCustomShimmerResult showcases both preventing a recipe from being decrafted and specifying a custom shimmer decrafting result.
*/
public class ShimmerShowcaseCrimsonCorruption : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetDefaults() {
		Item.width = 20;
		Item.height = 20;
	}

	public override void AddRecipes() {
		// Many items have multiple recipes. The last added recipe will usually be used for shimmer decrafting.
		// If your recipes are crimson or corruption specific, use CorruptionOnly and CrimsonOnly to indicate which recipe should be used for decrafting with which evil world type.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.RottenChunk)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.Register();

		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.Vertebrae)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.Register();
	}
}

public class ShimmerShowcaseCustomShimmerResult : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetDefaults() {
		Item.width = 20;
		Item.height = 20;
	}

	public override void AddRecipes() {
		// AddCustomShimmerResult can be used to change the decrafting results. Rather that return 1 ExampleItem, decrafting this item will return 1 Rotten Egg and 3 Chain.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.AddCustomShimmerResult(ItemID.RottenEgg)
			.AddCustomShimmerResult(ItemID.Chain, 3)
			.Register();

		// By default, the last added recipe will be used for shimmer decrafting unless crimson or corruption specific recipes are found. We can use DisableShimmer() to tell the game to ignore this recipe and use the above recipe instead.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.PadThai)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.DisableShimmer()
			.Register();
	}
}
