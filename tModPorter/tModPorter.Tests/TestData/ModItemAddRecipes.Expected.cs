using Terraria.ID;
using Terraria.ModLoader; 

public class ModItemAddRecipes : ModItem {
	public override void AddRecipes() {
		CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 10)
			.AddTile(TileID.WorkBenches)
			.Register();

		Method();
		A.B();
	}
	
	public void Method() { }

	class A {
		public static void B() { }
	}
}