using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable
{
	internal class ExampleLamp : ModItem
	{
		public override void SetDefaults() {
			item.useStyle = ItemUseStyleID.Swing;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 99;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleLamp>();
			item.width = 10;
			item.height = 24;
			item.value = 500;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.WoodenChair)
				.AddIngredient<ExampleBlock>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}