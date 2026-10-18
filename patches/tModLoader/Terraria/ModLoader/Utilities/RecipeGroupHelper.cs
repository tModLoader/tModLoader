using System;
using System.Reflection;
using Terraria.ID;

namespace Terraria.ModLoader.Utilities;

internal static class RecipeGroupHelper
{
	internal static void ResetRecipeGroups()
	{
		RecipeGroup.recipeGroups.Clear();
	//	RecipeGroup.recipeGroupIDs.Clear();
		RecipeGroup.nextRecipeGroupIndex = 0;
	}

	internal static void AddRecipeGroups()
	{
		var addRecipeGroupsMethod = typeof(Mod).GetMethod(nameof(Mod.AddRecipeGroups), BindingFlags.Instance | BindingFlags.Public)!;
		foreach (Mod mod in ModLoader.Mods) {
			try {
				addRecipeGroupsMethod.Invoke(mod, Array.Empty<object>());
				SystemLoader.AddRecipeGroups(mod);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
		}
	}
}
