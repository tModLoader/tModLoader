using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// Rocket launchers are special because they typically have ammo-specific variant projectiles.
	// ExampleRocketLauncher will inherit the variants specified by the Rocket Launcher weapon
	public class ExampleRocketLauncher : ModItem
	{
		public override void SetStaticDefaults() {
			// This line lets ExampleRocketLauncher act like a normal RocketLauncher in regard to any variant projectiles
			// corresponding to ammo that aren't specifically populated in SpecificLauncherAmmoProjectileMatches below.
			AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;

			// SpecificLauncherAmmoProjectileMatches can be used to provide specific projectiles for specific ammo items.
			// This example dictates that when RocketIII ammo is used, this weapon will fire the Meowmere projectile.
			// This is purely to show off this capability, typically SpecificLauncherAmmoProjectileFallback is all
			// that is needed for an "upgrade". A completely custom rocket launcher would instead specify new and
			// unique projectiles for all possible rocket ammo.
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Add(Type, new Dictionary<int, int> {
				{ ItemID.RocketIII, ProjectileID.Meowmere },
			});

			// Note that some rocket launchers, like Celebration and Electrosphere Launcher, will always
			// use their own projectiles no matter which rocket is used as ammo.
			// This type of behavior can be implemented in ModifyShootStats
		}

		public override void SetDefaults() {
			Item.DefaultToRangedWeapon(ProjectileID.RocketI, AmmoID.Rocket, singleShotTime: 30, shotVelocity: 5f, hasAutoReuse: true);
			Item.width = 50;
			Item.height = 20;
			Item.damage = 55;
			Item.knockBack = 4f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.buyPrice(gold: 40);
			Item.rare = ItemRarityID.Yellow;
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 2f); // Moves the position of the weapon in the player's hand.
		}
	}
}