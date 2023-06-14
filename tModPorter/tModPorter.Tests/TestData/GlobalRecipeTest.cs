using Terraria;
using Terraria.ModLoader;

public abstract class GlobalRecipeTest : GlobalRecipe
{
	public override bool RecipeAvailable(Recipe recipe) {
		return true;
	}

	public override void OnCraft(Item item, Recipe recipe) {
	}

	public override void ConsumeItem(Recipe recipe, int type, ref int amount) {
	}
}