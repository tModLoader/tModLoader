using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleDOTAccessory : ModItem
	{
		public override string Texture => "Terraria/Images/Item_49";

		public override void SetDefaults() {
			Item.width = Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 10);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ExampleDOTPlayer>().amplifyDebuffs = true;
		}

		public override void AddRecipes() {
			CreateRecipe().AddIngredient<ExampleItem>(10).AddTile(TileID.WorkBenches).Register();
		}
	}
}
