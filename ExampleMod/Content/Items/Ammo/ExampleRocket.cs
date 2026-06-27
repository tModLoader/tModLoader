using ExampleMod.Content.Projectiles.Rockets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleRocket : ModItem
	{
		// Rocket Ammo is a little weird and does not work the same as bullets or arrows.
		// Rockets I through IV have four versions: normal Rocket, Grenade, Proximity Mine, and Snowman Rocket.
		// This example is a clone of Rocket I.

		public override void SetStaticDefaults() {
			AmmoID.Sets.IsSpecialist[Type] = true; // This item will benefit from the Shroomite Helmet.

			// This is where we tell the game which projectile to spawn when using this rocket as ammo with certain launchers.
			// This specific rocket ammo is like Rocket I's.
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Type, ModContent.ProjectileType<ExampleRocketProjectile>());
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.GrenadeLauncher].Add(Type, ModContent.ProjectileType<ExampleGrenadeProjectile>());
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.ProximityMineLauncher].Add(Type, ModContent.ProjectileType<ExampleProximityMineProjectile>());
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.SnowmanCannon].Add(Type, ModContent.ProjectileType<ExampleSnowmanRocketProjectile>());
			// We also need to say which type of Celebration Mk2 rockets to use.
			// The Celebration Mk 2 only has four types of rockets. Change the projectile to match your ammo type.
			// Rocket I like   == ProjectileID.Celeb2Rocket
			// Rocket II like  == ProjectileID.Celeb2RocketExplosive
			// Rocket III like == ProjectileID.Celeb2RocketLarge
			// Rocket IV like  == ProjectileID.Celeb2RocketExplosiveLarge
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.Celeb2].Add(Type, ProjectileID.Celeb2Rocket);
			// The Celebration and Electrosphere Launcher will always use their own projectiles no matter which rocket you use as ammo.
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 14;
			Item.damage = 40;
			Item.knockBack = 4f;
			Item.consumable = true;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(copper: 50);
			Item.ammo = AmmoID.Rocket; // The ammo type is Rocket Ammo
			// Unlike other ammo, we don't set Item.shoot to the projectile for rocket ammo due to the logic involved.
			// AmmoID.Sets.SpecificLauncherAmmoProjectileMatches is used to determine the projectile spawned based on the weapon.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(100)
				.AddIngredient(ItemID.RocketI, 100)
				.AddIngredient<ExampleItem>()
				.AddTile(TileID.Anvils)
				.AddCondition(Condition.NpcIsPresent(NPCID.Cyborg))
				.Register();
		}
	}
}
