using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	// Spawns an interactable projectile near the player.
	public class ExampleInteractableProjectileItem : ModItem
	{
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shootSpeed = 4f;
			Item.shoot = ModContent.ProjectileType<ExampleInteractableProjectile>();
			Item.width = 26;
			Item.height = 24;
			Item.UseSound = SoundID.Item59;
			Item.useAnimation = 28;
			Item.useTime = 28;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 2);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
