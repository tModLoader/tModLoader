using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleRocketLauncher : ModItem {
		public override void SetStaticDefaults() {

			// Define which ammo types correspond to which projectile types. This example is just like the Rocket Launcher.
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Add(Type, new Dictionary<int, int> {
				{ ItemID.RocketI, ProjectileID.RocketI },
				{ ItemID.RocketII, ProjectileID.RocketII },
				{ ItemID.RocketIII, ProjectileID.RocketIII },
				{ ItemID.RocketIV, ProjectileID.RocketIV },
				{ ItemID.ClusterRocketI, ProjectileID.ClusterRocketI },
				{ ItemID.ClusterRocketII, ProjectileID.ClusterRocketII },
				{ ItemID.MiniNukeI, ProjectileID.MiniNukeRocketI },
				{ ItemID.MiniNukeII, ProjectileID.MiniNukeRocketII },
				{ ItemID.DryRocket, ProjectileID.DryRocket },
				{ ItemID.WetRocket, ProjectileID.WetRocket },
				{ ItemID.LavaRocket, ProjectileID.LavaRocket },
				{ ItemID.HoneyRocket, ProjectileID.HoneyRocket }
			});
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