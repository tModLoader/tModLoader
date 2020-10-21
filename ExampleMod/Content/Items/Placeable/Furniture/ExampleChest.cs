using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleChest : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded chest.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
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
			item.createTile = ModContent.TileType<Tiles.Furniture.ExampleChest>();
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleChestKey : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3; //Biome keys usually take 1 item to research instead.
		}
		
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.GoldenKey);
			item.width = 14;
			item.height = 20;
			item.maxStack = 99;
		}
	}
}
