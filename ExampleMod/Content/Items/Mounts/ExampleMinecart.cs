using ExampleMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Mounts
{
	public class ExampleMinecart : ModItem
	{
		public override void SetDefaults() {
			Item.mountType = ModContent.MountType<ExampleMinecartMount>();
			Item.width = 34;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Minecart)
				.AddIngredient(ModContent.ItemType<ExampleItem>(), 15)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
