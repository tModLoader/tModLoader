using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleSandBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;

			// Set the SandgunAmmoProjectileData to your sandgun projectile with a bonus damage of 10
			ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ModContent.ProjectileType<Projectiles.ExampleSandBallGunProjectile>(), 10);
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleSand>());
			Item.width = 12;
			Item.height = 12;
			Item.ammo = AmmoID.Sand;
			// Item.shoot and Item.damage are not used for sand ammo by convention. They would result in undesireable item tooltips.
			// ItemID.Sets.SandgunAmmoProjectileData is used instead.
			Item.notAmmo = true;
		}

		public override void AddRecipes() {
			CreateRecipe(10)
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}