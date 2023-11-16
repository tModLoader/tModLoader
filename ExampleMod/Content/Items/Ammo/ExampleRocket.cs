using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Projectiles;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleRocket : ModItem
	{
		// Rocket Ammo is very weird and does not work like bullets or arrows.
		// Rockets I through IV have four versions: normal Rocket, Grenade, Proximity Mine, and Snowman Rocket.
		// This example is a clone of Rocket I.

		public override void SetStaticDefaults() {
			AmmoID.Sets.IsSpecialist[Type] = true; // This item will benefit from the Shroomite Helmet.
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 14;
			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4f;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(copper: 50);
			Item.consumable = true;
			Item.ammo = AmmoID.Rocket; // Even though we set this as Rocket ammo, it won't work as you'd expect.
			// Item.shoot = ProjectileID.None; // Unlike other ammo types, we do not set a projectile here for Rocket ammo.
		}

		// This is where we tell the game which projectile to spawn when using this rocket as ammo.
		// This specific rocket ammo is like Rocket I's.
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			// If you want your rocket to be a "large blast radius", change this to ProjectileID.GrenadeIII.
			// If you want your rocket to damage tiles, change this to ProjectileID.GrenadeII.
			// If you want your rocket to both damage tiles and have a large blast radius, change this to ProjectileID.GrenadeIV.
			if (weapon.type == ItemID.GrenadeLauncher || type == ProjectileID.GrenadeI) {
				type = ModContent.ProjectileType<ExampleGrenadeProjectile>();
			}
			else if (weapon.type == ItemID.ProximityMineLauncher || type == ProjectileID.ProximityMineI) {
				type = ModContent.ProjectileType<ExampleProximityMineProjectile>();
			}
			else if (weapon.type == ItemID.SnowmanCannon) {
				type = ModContent.ProjectileType<ExampleSnowmanRocketProjectile>();
			}
			else if (weapon.type == ItemID.Celeb2) {
				// We also need to say which type of Celebration Mk2 rockets to use. Change the projectile type to match your rocket.
				// Small explosion, does not damage tiles (Rocket I): ProjectileID.Celeb2Rocket
				// Small explosion, does damage tiles (Rocket II): ProjectileID.Celeb2RocketExplosive
				// Large explosion, does not damage tiles (Rocket III): ProjectileID.Celeb2RocketLarge
				// Large explosion, does damage tiles (Rocket IV): ProjectileID.Celeb2RocketExplosiveLarge
				type = ProjectileID.Celeb2Rocket;
			}
			// The Celebration and Electrosphere Launcher will always use their own projectiles no matter which rocket you use as ammo.
			// So, we don't need to worry about them here.
			else {
				// All other rocket launchers, including ones from other mods, will default to our normal rocket projectile.
				type = ModContent.ProjectileType<ExampleRocketProjectile>();
			}
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
