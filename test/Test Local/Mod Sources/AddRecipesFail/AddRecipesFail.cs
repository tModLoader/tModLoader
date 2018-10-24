using Terraria.ModLoader;
using System;

namespace AddRecipesFail
{
	class AddRecipesFail : Mod
	{
		public override void AddRecipes() {
			throw new Exception("AddRecipes Failed");
		}
	}
}
