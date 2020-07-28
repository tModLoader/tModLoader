using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleBar : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityMaterials[item.type] = 59; // Influences the inventory sort order. 59 is PlatinumBar, higher is more valuable.
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.value = 750; // Reminder: each time you increase the value by one adds 1 copper to the sell price. (1 = 1 copper, 100 = 1 silver, 1000 = 1 gold, 10000 = 1 platinum)
			item.useStyle = ItemUseStyleID.Swing;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleBar>(); // Place the tile that is put inside the <>. Modded tiles will use this format -> 'TileType<TileName>();'.
			item.placeStyle = 0;
		}
		// Check ExampleItem.cs to see how recipes work.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleOre>(4)
				.AddTile(TileID.Furnaces)
				.Register();
		}
	}
}
