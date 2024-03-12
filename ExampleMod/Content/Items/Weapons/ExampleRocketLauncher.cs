using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Projectiles;
using ExampleMod.Content.Items.Ammo;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleRocketLauncher : ModItem {
		public override void SetStaticDefaults() {
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

			//AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type].Add(ModContent.ItemType<ExampleRocket>(), ModContent.ProjectileType<ExampleRocketProjectile>());
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useAmmo = AmmoID.Rocket;
			Item.width = 50;
			Item.height = 20;
			Item.shoot = ProjectileID.RocketI;
			Item.UseSound = SoundID.Item11;
			Item.damage = 55;
			Item.shootSpeed = 5f;
			Item.noMelee = true;
			Item.value = Item.buyPrice(0, 40);
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Yellow;
			Item.DamageType = DamageClass.Ranged;
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 2f);
		}
	}
}