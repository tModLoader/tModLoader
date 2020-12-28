using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ExampleMod.Common.GlobalProjectiles;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleModifiedProjectilesItem : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleShootingSword";
		public override void SetDefaults() {
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.Shoot;
			item.autoReuse = true;
			item.damage = 20;
			item.DamageType = DamageClass.Ranged;
			item.width = 32;
			item.height = 32;
			item.shoot = 10;
			// This Ammo is nonspecific. I want to modify what it shoots, however.
			item.useAmmo = AmmoID.Bullet;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			// NewProjectile returns the index of the projectile it creates in the NewProjectile array.
			// Here we are using it to gain access to the projectile object.
			int projectileID = Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
			Projectile projectile = Main.projectile[projectileID];

			ExampleProjectileModifications globalProjectile = projectile.GetGlobalProjectile<ExampleProjectileModifications>();
			// For more context, see ExampleProjectileModifications.cs
			globalProjectile.SetTrail(Color.Green);
			globalProjectile.sayTimesHitOnThirdHit = true;
			globalProjectile.applyBuffOnHit = true;

			// We do not want vanilla to spawn a duplicate projectile.
			return false;
		}
	}
}
