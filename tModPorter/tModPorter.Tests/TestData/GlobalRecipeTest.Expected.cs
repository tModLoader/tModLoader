using Terraria;
using Terraria.ModLoader;

#if COMPILE_ERROR
public abstract class GlobalRecipeTest : GlobalRecipe
{
	public override bool RecipeAvailable(Recipe recipe)/* tModPorter Note: Removed. Use Recipe.AddCondition */ {
		return true;
	}

	public override void OnCraft(Item item, Recipe recipe)/* tModPorter Note: Removed. Use Recipe.AddOnCraftCallback or GlobalItem.OnCreate */ {
	}

	public override void ConsumeItem(Recipe recipe, int type, ref int amount)/* tModPorter Note: Removed. Use Recipe.AddConsumeItemCallback */ {
	}
}
#endif