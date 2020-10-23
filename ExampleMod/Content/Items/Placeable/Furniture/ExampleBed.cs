using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleBed : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded bed.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 20;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.Swing;
			item.consumable = true;
			item.value = 2000;
			item.createTile = ModContent.TileType<Tiles.Furniture.ExampleBed>();
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}