namespace Terraria.ModLoader
{
	/// <summary>
	/// This class provides hooks that control all recipes in the game.
	/// </summary>
	public class GlobalRecipe:ModType
	{
		protected sealed override void Register() {
			ModTypeLookup<GlobalRecipe>.Register(this);
			RecipeHooks.Add(this);
		}

		/// <summary>
		/// Whether or not the conditions are met for the given recipe to be available for the player to use. This hook can be used for conditions unrelated to items or tiles (for example, biome or time).
		/// </summary>
		public virtual bool RecipeAvailable(Recipe recipe) {
			return true;
		}

		/// <summary>
		/// Allows you to make anything happen when the player uses the given recipe. The item parameter is the item the player has just crafted.
		/// </summary>
		/// <param name="item">The item created.</param>
		/// <param name="recipe">The recipe used to create the item.</param>
		public virtual void OnCraft(Item item, Recipe recipe) {
		}
	}
}
