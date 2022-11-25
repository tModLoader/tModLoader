using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture
{
	public class ExampleTable : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Table");
			Tooltip.SetDefault("This is a modded table.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ExampleTable>());
			Item.width = 38;
			Item.height = 24;
			Item.value = 150;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.WoodenTable)
				.AddIngredient<ExampleItem>(10)
				.Register();
		}
	}
}
