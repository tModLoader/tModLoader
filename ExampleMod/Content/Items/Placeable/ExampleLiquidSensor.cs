using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleLiquidSensor : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
		}

		public override void SetDefaults() {
			Item.createTile = ModContent.TileType<Tiles.ExampleLiquidSensor>();
			Item.width = 16;
			Item.height = 16;
			Item.rare = ItemRarityID.Blue;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.placeStyle = 0;
			Item.mech = true;
		}

		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Cog, 5);
			recipe.AddIngredient(ModContent.ItemType<MagicExampleLiquidDropper>());
			recipe.AddIngredient(ItemID.Wire);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
