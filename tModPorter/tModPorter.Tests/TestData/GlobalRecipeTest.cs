using Terraria;
using Terraria.ModLoader;

namespace tModPorter.Tests.TestData
{
	public class GlobalRecipeTest : GlobalRecipe
	{
		public override bool RecipeAvailable(Recipe recipe) {
			return true;
		}
	}
}
