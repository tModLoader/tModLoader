using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleSandBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			// Set the SandgunAmmoToProjectile to your sandgun projectile
			ItemID.Sets.SandgunAmmoToProjectile[Type] = ModContent.ProjectileType<Projectiles.ExampleSandSandgunProjectile>();
			// This value can be different than your falling block's damage
			ItemID.Sets.SandgunAmmoBonusDamage[Type] = 10;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleSand>());
			Item.width = 12;
			Item.height = 12;
			Item.value = 0;
			Item.ammo = AmmoID.Sand;
			Item.notAmmo = true;
		}
	}
}