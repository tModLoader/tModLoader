using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent; //This lets us access methods (like ItemType) from ModContent without having to type its name.

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleWorkbench : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded workbench.");
		}

		public override void SetDefaults() {
			item.createTile = TileType<Tiles.Furniture.ExampleWorkbench>(); //This sets the id of the tile that this item should place when used.

			item.width = 28; //The item texture's width
			item.height = 14; //The item texture's height

			item.useTurn = true;
			item.autoReuse = true;
			item.useStyle = ItemUseStyleID.Swing;
			item.useTime = 10;
			item.useAnimation = 15;

			item.maxStack = 99;
			item.consumable = true;
			item.value = 150;
		}

		public override void AddRecipes() {
			new ModRecipe(mod)
				.AddIngredient(ItemID.WorkBench)
				.AddIngredient(ItemType<ExampleItem>(), 10)
				.SetResult(this)
				.Build();
		}
	}
}