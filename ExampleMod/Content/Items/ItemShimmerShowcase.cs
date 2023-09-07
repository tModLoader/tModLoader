using ExampleMod.Content.NPCs;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items;

/*
The items in this file showcase customizing the decrafting feature of the Shimmer liquid.
By default, Shimmer will transform crafted items back into their original recipe ingredients.

ShimmerShowcaseConditions showcases the ability to use conditions to change when shimmer transformations can happen, this includes crimson and corruption recipe variants.

ShimmerShowcaseCustomShimmerResult showcases both preventing a recipe from being decrafted and specifying a custom shimmer decrafting result.

To use Shimmer to transform an item into another item instead of decrafting an item, simply set "ItemID.Sets.ShimmerTransformToItem[Type] = ItemType here;" in SetStaticDefaults. ExampleMusicBox and ExampleTorch show examples of this. For the same including the same functionality normal decrafting allows, see the below class.

Also note that critter items (Item.makeNPC > 0) will also not attempt to decraft, but will instead transform into the NPC that the Item.makeNPC transforms into. NPCID.Sets.ShimmerTransformToNPC sets which NPC an NPC will transform into, see PartyZombie and ExampleCustomAISlimeNPC for examples of this.
*/

public class ItemShimmerShowcaseConditions : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";
	public override void SetDefaults() {
		Item.width = Item.height = 20;
		Item.maxStack = Item.CommonMaxStack;
	}

	public override void AddRecipes() {
		// Many items have multiple recipes. The first added recipe will usually be used for shimmer decrafting. Recipe decraft conditions may be used
		// to only allow decrafting under certain conditions, the first recipe found that satisfies all of it's decraft conditions will be used.
		// Therefore, this desert-specific example has priority over the world evil examples registered after it.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.Cactus)
			.AddTile<ExampleWorkbench>()
			.AddDecraftCondition(Condition.InDesert)
			.Register();

		// In these 2 examples, decraft conditions are used to make the recipes decraftable only in their respective world types
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.RottenChunk)
			.AddTile<ExampleWorkbench>()
			.AddDecraftCondition(Condition.CorruptWorld)
			.Register();

		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.Vertebrae)
			.AddTile<ExampleWorkbench>()
			.AddDecraftCondition(Condition.CrimsonWorld)
			.Register();

		// Finally, the ApplyConditionsAsDecraftConditions method can be used to quickly mirror any crafting conditions onto the decrafting conditions.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.BeeHive)
			.AddCondition(Condition.InHive)
			.ApplyConditionsAsDecraftConditions()
			.Register();
	}
}

public class ItemShimmerShowcaseCustomShimmerResult : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetDefaults() {
		Item.width = Item.height = 20;
		Item.maxStack = Item.CommonMaxStack;
	}

	public override void AddRecipes() {
		// By default, the first added recipe will be used for shimmer decrafting. We can use DisableDecraft() to tell the game to ignore this recipe
		// and use the below recipe instead.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddIngredient(ItemID.PadThai)
			.AddTile<ExampleWorkbench>()
			.DisableDecraft()
			.Register();

		// AddCustomShimmerResult can be used to change the decrafting results. Rather that return 1 ExampleItem, decrafting this item will return 1
		// Rotten Egg and 3 Chain.
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddTile<ExampleWorkbench>()
			.AddCustomShimmerResult(ItemID.RottenEgg)
			.AddCustomShimmerResult(ItemID.Chain, 3)
			.Register();
	}
}

public class ItemShimmerShowcaseModShimmer : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetStaticDefaults() {
		ShimmerTransformation transformation = CreateShimmerTransformation() // Using CreateShimmerTransformation we create an instance of ShimmerTransformation
			.AddCondition(Condition.Christmas) // Just like Recipe we can apply condition and item results
			.AddItemResult(ItemID.Present, 2);
		transformation.Register();

		CreateShimmerTransformation()
			.AddModItemResult<ExampleDye>(30)
			// Changes the value of the first result to a random number inclusive between 20 - 40
			.AddModifyShimmerCallBack((ShimmerTransformation transformation, IModShimmerable source) => transformation.Results[0] = transformation.Results[0] with { Count = Main.rand.Next(20, 41) })
			.AddModNPCResult<ExampleZombieThief>(2) // Unlike recipe we can spawn NPCs here, of any count, under conditions
			.AddCoinLuckResult(2) // We can also add coin luck
			.SortBefore(transformation)
			.Register();
	}

	public override void SetDefaults() {
		Item.width = Item.height = 20;
		Item.maxStack = Item.CommonMaxStack;
	}
}