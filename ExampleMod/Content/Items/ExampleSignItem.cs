using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace ExampleMod.Content.Items
{
	public class ExampleSignItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleSign>(), 0);
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<ExampleSign>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddIngredient(ItemID.DirtBlock, 1) 
				.Register();
		}
	}
}
