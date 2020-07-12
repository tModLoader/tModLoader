using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(this);
			recipe.SetResult(ItemID.WingsSolar);
			recipe.AddRecipe();
		}
	}
}