using Terraria;
using Terraria.Localization;

namespace tModPorter.Tests.TestData
{
	public class RecipeConditionTest
	{
		public void Method1()
		{
			Recipe.Condition condition1 = new(NetworkText.FromLiteral("Test"), (r, player) => player.ZoneGraveyard);
		}

		public void Method2(Recipe recipe)
		{
			recipe.AddCondition(NetworkText.FromLiteral("Test"), (r, player) => player.ZoneGraveyard);
		}

		public void Method3()
		{
			Recipe.Condition condition1 = new(NetworkText.FromLiteral("Test"), (_, player) => player.ZoneGraveyard);
		}

		public void Method4(Recipe recipe)
		{
			recipe.AddCondition(NetworkText.FromLiteral("Test"), (_, player) => player.ZoneGraveyard);
		}

		public void Method5(Recipe.Condition condition, Recipe recipe)
		{
			condition.RecipeAvailable(recipe, Main.LocalPlayer);
		}

		public bool Method6(Recipe.Condition condition, Recipe recipe)
		{
			return condition.RecipeAvailable(recipe, Main.LocalPlayer);
		}

		public void Method7(Recipe.Condition condition, Recipe recipe, Player player)
		{
			condition.RecipeAvailable(recipe, player);
		}

		public bool Method8(Recipe.Condition condition, Recipe recipe, Player player)
		{
			return condition.RecipeAvailable(recipe, player);
		}
	}
}
