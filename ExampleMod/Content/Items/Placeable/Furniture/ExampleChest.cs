using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleChest : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded chest.");
		}

		public override void SetDefaults() {
			item.width = 26;
			item.height = 22;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.Swing;
			item.consumable = true;
			item.value = 500;
			item.createTile = TileType<Tiles.Furniture.ExampleChest>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Chest)
				.AddIngredient<ExampleBlock>(10)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleChestKey : ModItem
	{
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.GoldenKey);
			item.width = 14;
			item.height = 20;
			item.maxStack = 99;
		}
	}
}