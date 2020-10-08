using ExampleMod.Items.Abomination;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	public class ElementalPurge : ModItem
	{
		public override void SetDefaults() {
			item.width = 22;
			item.height = 32;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.rare = ItemRarityID.Red;
			item.value = Item.buyPrice(0, 20, 0, 0);
			item.createTile = ModContent.TileType<Tiles.ElementalPurge>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<VoidMonolith>());
			recipe.AddIngredient(ModContent.ItemType<ElementResidue>());
			recipe.AddTile(TileID.CrystalBall);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}