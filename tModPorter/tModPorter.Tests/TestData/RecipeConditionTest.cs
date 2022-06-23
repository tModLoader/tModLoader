using Terraria;
using Terraria.Localization;

namespace tModPorter.Tests.TestData
{
	public class RecipeConditionTest
	{
		public void Method1()
		{
			Recipe.Condition condition1 = new(NetworkText.FromLiteral("Test"), r => Main.LocalPlayer.ZoneGraveyard);
		}

		public void Method2(Recipe recipe)
		{
			recipe.AddCondition(NetworkText.FromLiteral("Test"), r => Main.LocalPlayer.ZoneGraveyard);
		}

		public void Method3()
		{
			Recipe.Condition condition1 = new(NetworkText.FromLiteral("Test"), _ => Main.LocalPlayer.ZoneGraveyard);
		}

		public void Method4(Recipe recipe)
		{
			recipe.AddCondition(NetworkText.FromLiteral("Test"), _ => Main.LocalPlayer.ZoneGraveyard);
		}

		public void Method5(Recipe.Condition condition, Recipe recipe)
		{
			condition.RecipeAvailable(recipe);
		}

		public bool Method6(Recipe.Condition condition, Recipe recipe)
		{
			return condition.RecipeAvailable(recipe);
		}

		public void Method7(Recipe.Condition condition, Recipe recipe, Player player)
		{
			condition.RecipeAvailable(recipe);
		}

		public bool Method8(Recipe.Condition condition, Recipe recipe, Player player)
		{
			return condition.RecipeAvailable(recipe);
		}
	}
}
