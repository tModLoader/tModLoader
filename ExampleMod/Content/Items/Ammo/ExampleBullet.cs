using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleBullet : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded bullet ammo."); // The item's description, can be set to whatever you want.
		}

		public override void SetDefaults() {
			item.damage = 12; // The damage for projectiles isn't actually 12, it actually is the damage combined with the projectile and the item together.
			item.ranged = true;
			item.width = 8;
			item.height = 8;
			item.maxStack = 999;
			item.consumable = true; // This marks the item as consumable, making it automatically be consumed when it's used as ammunition, or something else, if possible.
			item.knockBack = 1.5f;
			item.value = 10;
			item.rare = ItemRarityID.Green;
			item.shoot = ProjectileType<Projectiles.ExampleBullet>(); //The projectile that weapons fire when using this item as ammunition.
			item.shootSpeed = 16f; // The speed of the projectile.
			item.ammo = AmmoID.Bullet; // The ammo class this ammo belongs to.
		}
		// Refer to ExampleItem.cs for how to create recipes.
		public override void AddRecipes() {
			CreateRecipe(50)
				.AddIngredient(ItemID.MusketBall, 50)
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
