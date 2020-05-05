using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.Recipes
{
	public class RecipeBuilder
	{
		private readonly ModRecipe _recipe;


		/// <summary>Creates a new instance with no ingredients and no result.</summary>
		/// <seealso cref="Requires(int,int)"/>
		/// <seealso cref="Build"/>
		public RecipeBuilder()
		{
		}

		/// <summary>Creates a new instance with no ingredients with the given item and stack as a result.</summary>
		/// <param name="mod">The mod who owns the recipe.</param>
		/// <param name="item">The item type.</param>
		/// <param name="stack">The stack.</param>
		/// <seealso cref="Requires(int,int)"/>
		/// <seealso cref="Build"/>
		public RecipeBuilder(Mod mod, int item, int stack = 1)
		{
			_recipe = new ModRecipe(mod);
			_recipe.SetResult(item, stack);
		}

		/// <summary>Creates a new instance with no ingredients with the given item and stack as a result.</summary>
		/// <param name="mod">The mod who owns the recipe.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <seealso cref="Requires(int,int)"/>
		/// <seealso cref="Build"/>
		public RecipeBuilder(Mod mod, string itemName, int stack = 1)
		{
			_recipe = new ModRecipe(mod);
			_recipe.SetResult(mod, itemName, stack);
		}

		/// <summary>Creates a new instance with no ingredients with the given item and stack as a result.</summary>
		/// <param name="item"></param>
		/// <param name="stack"></param>
		/// <seealso cref="Requires(int,int)"/>
		/// <seealso cref="Build"/>
		public RecipeBuilder(ModItem item, int stack = 1)
		{
			_recipe = new ModRecipe(item.mod);
			_recipe.SetResult(item, stack);
		}


		/// <summary>
		/// Adds an ingredient to this recipe with the given item type and stack size.
		/// Ex.: 
		/// <example>recipe.AddIngredient(ItemID.IronAxe)</example>
		/// </summary>
		/// <param name="type">The item type.</param>
		/// <param name="stack">The stack.</param>
		/// <returns></returns>
		public RecipeBuilder Requires(int type, int stack = 1)
		{
			_recipe.AddIngredient(type, stack);

			return this;
		}


		// System.TupleValue is not resolved. Need to import it from a NuGet package ?
		/*public RecipeBuilder Requires(params (short itemId, int stack)[] ingredients)
		{
			for (int i = 0; i < ingredients.Length; i++)
				Requires(ingredients[i].Item1, ingredients[i].Item2);

			return this;
		}*/


		/// <summary>
		/// Adds the specified ingredients to this recipe with the given item types.
		/// Ex.: 
		/// <example>recipe.AddIngredient(ItemID.IronAxe)</example>
		/// </summary>
		/// <param name="type1">The first item type.</param>
		/// <param name="type2">The second item type.</param>
		/// <param name="type3">The third item type.</param>
		/// <param name="types">The remaining item types.</param>
		/// <returns></returns>
		public RecipeBuilder Requires(int type1, int type2, int type3, params int[] types)
		{
			Requires(type1);
			Requires(type2);
			Requires(type3);


			for (int i = 0; i < types.Length; i++)
				Requires(types[i]);


			return this;
		}


		/// <summary>Adds an ingredient to this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.</summary>
		/// <param name="mod">The mod.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <returns></returns>
		/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
		public RecipeBuilder Requires(Mod mod, string itemName, int stack = 1)
		{
			_recipe.AddIngredient(mod ?? _recipe.mod, itemName, stack);

			return this;
		}


		/// <summary>Adds an ingredient to this recipe of the given type of item and stack size.</summary>
		/// <param name="modItem">The item.</param>
		/// <param name="stack">The stack.</param>
		/// <returns></returns>
		public RecipeBuilder Requires(ModItem modItem, int stack = 1)
		{
			_recipe.AddIngredient(modItem, stack);

			return this;
		}


		/// <summary>Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size. Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</summary>
		/// <param name="recipeGroup">The name.</param>
		/// <param name="stack">The stack.</param>
		/// <returns></returns>
		/// <exception cref="RecipeException"></exception>
		public RecipeBuilder Requires(string recipeGroup, int stack = 1)
		{
			_recipe.AddRecipeGroup(recipeGroup, stack);

			return this;
		}


		/// <summary>
		/// Adds one or many required crafting station(s) with the given tile type(s) to the recipe being built.
		/// Ex.:
		/// <example>At(TileID.WorkBenches, TileID.Anvils)</example>
		/// </summary>
		/// <param name="tileTypes"></param>
		/// <returns></returns>
		public RecipeBuilder At(params int[] tileTypes)
		{
			for (int i = 0; i < tileTypes.Length; i++)
				_recipe.AddTile(tileTypes[i]);

			return this;
		}


		public RecipeBuilder RequiresLava() => NeedVar(recipe => recipe.needLava = true);

		public RecipeBuilder RequiresHoney() => NeedVar(recipe => recipe.needHoney = true);

		public RecipeBuilder RequiresWater() => NeedVar(recipe => recipe.needWater = true);

		public RecipeBuilder RequiresSnowBiome() => NeedVar(recipe => recipe.needSnowBiome = true);


		private RecipeBuilder NeedVar(Action<ModRecipe> need)
		{
			need(_recipe);

			return this;
		}


		/// <summary>Adds this recipe to the game. Call this after you have finished setting the result, ingredients, etc.</summary>
		/// <returns></returns>
		/// <exception cref="RecipeException">A recipe without any result has been added.</exception>
		private void Build() => _recipe.AddRecipe();
	}
}