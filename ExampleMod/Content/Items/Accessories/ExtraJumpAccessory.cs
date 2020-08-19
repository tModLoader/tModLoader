using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExtraJumpAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fire in a bottle");
			//TODO add other jumps
			//Tooltip.SetDefault("Grants you three more jumps. On the last jump, you can keep jumping if you face left");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 26;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 1);
			item.rare = ItemRarityID.Orange;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// Jumps are usually executed in alphabetic order
			//player.EnableDoubleJump<ExampleDoubleJump1>();
			//player.EnableDoubleJump<ExampleDoubleJump2>();
			//player.EnableDoubleJump<ExampleDoubleJump3>();
			// Uncomment this to see the simple extra jump example
			// player.EnableDoubleJump<SimpleExtraJump>();
			player.EnableExtraJump<ExtraJumps.SimpleExtraJump>();
		}

		public override void AddRecipes() {
			//ModRecipe recipe = new ModRecipe(mod);
			//recipe.AddIngredient(ItemID.CloudinaBottle);
			//recipe.AddIngredient(ItemID.SandstorminaBottle);
			//recipe.AddIngredient(ItemID.BlizzardinaBottle);
			//recipe.AddTile(TileID.TinkerersWorkbench);
			//recipe.SetResult(this);
			//recipe.AddRecipe();
			CreateRecipe()
				.AddIngredient<ExampleItem>(20)
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
		}
	}
}
