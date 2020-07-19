using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleWall : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded wall.");
		}

		public override void SetDefaults() {
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 7;
			item.useStyle = ItemUseStyleID.Swing;
			item.consumable = true;
			item.createWall = WallType<Walls.ExampleWall>();
		}

		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<ExampleBlock>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}