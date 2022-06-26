using Terraria;
using Terraria.ModLoader;

namespace tModPorter.Tests.TestData
{
	public class RecipeLoaderTest
	{
		public void Method1(Recipe recipe)
		{
			RecipeLoader.RecipeAvailable(recipe, Main.LocalPlayer);
		}

		public bool Method2(Recipe recipe)
		{
			return RecipeLoader.RecipeAvailable(recipe, Main.LocalPlayer);
		}

		public void Method3(Recipe recipe, Player player)
		{
			RecipeLoader.RecipeAvailable(recipe, player);
		}

		public bool Method4(Recipe recipe, Player player)
		{
			return RecipeLoader.RecipeAvailable(recipe, player);
		}
	}
}
