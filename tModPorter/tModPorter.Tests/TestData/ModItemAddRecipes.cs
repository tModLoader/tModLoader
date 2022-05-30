using Terraria.ID;
using Terraria.ModLoader; 

public class ModItemAddRecipes : ModItem {
	public override void AddRecipes() {
		ModRecipe recipe = new ModRecipe(mod);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(this);
		recipe.AddRecipe();

		Method();
		A.B();
	}
	
	public void Method() { }

	class A {
		public static void B() { }
	}
}