using ExampleMod.Content.Tiles.Furniture;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleWallAdvanced : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is an advanced modded wall.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
		}

		public override void SetDefaults() {
			Item.DefaultToPlacableWall((ushort)ModContent.WallType<Walls.ExampleWallAdvanced>());
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<ExampleBlock>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
