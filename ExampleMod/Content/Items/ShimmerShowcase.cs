using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items;

/*
The items in this file showcase customizing the decrafting feature of the Shimmer liquid.
By default, Shimmer will tranform crafted items back into their original recipe ingredients.

ShimmerShowcaseConditions showcases crimson and corruption specific shimmer decrafting results.

ShimmerShowcaseCustomShimmerResult showcases both preventing a recipe from being decrafted and specifying a custom shimmer decrafting result.
*/
public class ShimmerShowcaseConditions : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetDefaults() {
		Item.width = 20;
		Item.height = 20;
	}

	public override void AddRecipes() {
		// Many items have multiple recipes. The last added recipe will usually be used for shimmer decrafting.
		// Recipe conditions may be used to only allow decrafting under certain conditions, here it is used to make the recipes decraftable in only their respective world types
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.RottenChunk)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.AddDecraftCondition(Recipe.Condition.CorruptWorld)
			.Register();

		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.Vertebrae)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.AddDecraftCondition(Recipe.Condition.CrimsonWorld)
			.Register();

		// Or in a specific biome, keep in mind that decraft order is reverse of recipe register, so this desert example has priority over the world evil

		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.Cactus)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.AddDecraftCondition(Recipe.Condition.InDesert)
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

		//Another method of changing priority is using .SetShimmerPriority, setting it to -1 puts it bellow the unset 0 of both others here
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.GoldenDelight)
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.SetShimmerPriority(-1)
			.Register();
	}
}
