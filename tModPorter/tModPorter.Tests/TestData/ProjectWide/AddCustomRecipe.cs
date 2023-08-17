using Terraria.ID;
using Terraria.ModLoader;

public class AddCustomRecipe
{
	public void AddRecipes(Mod mod, ModItem item) {
		var recipe = new CustomRecipe(mod);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(item, 5);
		recipe.AddRecipe();
	}
}