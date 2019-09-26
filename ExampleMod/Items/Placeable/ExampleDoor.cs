using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Placeable
{
	public class ExampleDoor : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded door.");
		}

		public override void SetDefaults() {
			item.width = 14;
			item.height = 28;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.value = 150;
			item.createTile = TileType<Tiles.ExampleDoorClosed>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.WoodenDoor);
			recipe.AddIngredient(ItemType<ExampleBlock>(), 10);
			recipe.AddTile(TileType<Tiles.ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}