using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class RecipeHooks
	{
		internal static readonly IList<GlobalRecipe> globalRecipes = new List<GlobalRecipe>();

		internal static void Add(GlobalRecipe modWorld)
		{
			globalRecipes.Add(modWorld);
		}

		internal static void Unload()
		{
			globalRecipes.Clear();
		}

		public static bool RecipeAvailable(Recipe recipe)
		{
			ModRecipe modRecipe = recipe as ModRecipe;
			if (modRecipe != null && !modRecipe.RecipeAvailable())
			{
				return false;
			}
			foreach (GlobalRecipe globalRecipe in globalRecipes)
			{
				if (!globalRecipe.RecipeAvailable(recipe))
				{
					return false;
				}
			}
			return true;
		}

		public static void OnCraft(Item item, Recipe recipe)
		{
			ModRecipe modRecipe = recipe as ModRecipe;
			if (modRecipe != null)
			{
				modRecipe.OnCraft(item);
			}
			foreach (GlobalRecipe globalRecipe in globalRecipes)
			{
				globalRecipe.OnCraft(item, recipe);
			}
		}
	}
}