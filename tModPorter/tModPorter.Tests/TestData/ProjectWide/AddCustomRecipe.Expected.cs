using Terraria.ID;
using Terraria.ModLoader;

public class AddCustomRecipe
{
	public void AddRecipes(Mod mod, ModItem item) {
#if COMPILE_ERROR
		var recipe = new CustomRecipe(mod, item.Type, 5);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.AddRecipe();
#endif
	}
}