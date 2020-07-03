namespace Terraria.ModLoader
{
	/// <summary>
	/// This class provides hooks that control all recipes in the game.
	/// </summary>
	public class GlobalRecipe
	{
		/// <summary>
		/// The mod which added this GlobalRecipe.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this GlobaRecipe.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// Allows you to automatically load a GlobalRecipe instead of using Mod.AddGlobalRecipe. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload, and to change the default internal name.
		/// </summary>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
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
