using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleBar : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityMaterials[item.type] = 59; // influences the inventory sort order. 59 is PlatinumBar, higher is more valuable.
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.value = 750; // reminder: each time you increase the value by one is adding 1 copper. (1 = 1 copper, 100 = 1 silver, 1000 = 1 gold, 10000 = 1 platinum)
			item.useStyle = ItemUseStyleID.Swing;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleBar>(); // place this tile
			item.placeStyle = 0;
		}
		// check ExampleItem.cs to see how recipes work
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleOre>(4)
				.AddTile(TileID.Furnaces)
				.Register();
		}
	}
}
