using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleClock : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded clock.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type] = 1;
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
			item.createTile = TileType<Tiles.Furniture.ExampleClock>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.GrandfatherClock)
				.AddIngredient<ExampleBlock>(10)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}