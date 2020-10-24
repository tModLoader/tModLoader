using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleWorkbench : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded workbench.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			item.createTile = ModContent.TileType<Tiles.Furniture.ExampleWorkbench>(); //This sets the id of the tile that this item should place when used.

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

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.WorkBench)
				.AddIngredient<ExampleItem>(10)
				.Register();
		}
	}
}