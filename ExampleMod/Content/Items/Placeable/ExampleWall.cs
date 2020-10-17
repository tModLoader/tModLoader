using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleWall : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded wall.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
		}

		public override void SetDefaults() {
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 7;
			item.useStyle = ItemUseStyleID.Swing;
			item.consumable = true;
			item.createWall = ModContent.WallType<Walls.ExampleWall>(); // The ID of the wall that this item should place when used. ModContent.WallType<T>() method returns an integer ID of the wall provided to it through its generic type argument (the type in angle brackets).

		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<ExampleBlock>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
