using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleBullet : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded bullet ammo.");
		}

		public override void SetDefaults() {
			item.damage = 12;
			item.ranged = true;
			item.width = 8;
			item.height = 8;
			item.maxStack = 999;
			item.consumable = true; //You need to set the item consumable so that the ammo would automatically consumed
			item.knockBack = 1.5f;
			item.value = 10;
			item.rare = ItemRarityID.Green;
			item.shoot = ProjectileType<Projectiles.ExampleBullet>(); //The projectile shoot when your weapon using this ammo
			item.shootSpeed = 16f; //The speed of the projectile
			item.ammo = AmmoID.Bullet; //The ammo class this ammo belongs to.
		}

		public override void AddRecipes() {
			CreateRecipe(50)
				.AddIngredient(ItemID.MusketBall, 50)
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}