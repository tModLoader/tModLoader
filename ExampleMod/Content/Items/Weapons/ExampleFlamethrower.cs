using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// This example shows how to create a gel-powered flamethrower that fires multiple shots per use animation while consuming ammo only on the first shot.
	// It also demonstrates preventing underwater use and spawning flames near the muzzle without shooting through walls.
	public class ExampleFlamethrower : ModItem
	{
		public override void SetDefaults() {
			Item.width = 50;
			Item.height = 18;
			Item.damage = 35;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 0.3f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 6; // Fire once every 6 game updates (60 updates = 1 second).
			Item.useAnimation = 30; // Five shots per use animation.
			Item.consumeAmmoOnFirstShotOnly = true; // Only the first shot in each animation consumes gel.
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item34;
			Item.shoot = ModContent.ProjectileType<ExampleFlamethrowerProjectile>();
			Item.shootSpeed = 7f;
			Item.useAmmo = AmmoID.Gel;
			Item.value = Item.buyPrice(gold: 50);
			Item.rare = ItemRarityID.Pink;
		}

		public override bool CanUseItem(Player player) {
			// Vanilla flame weapons prevent use while wet, so weapon matches that check.
			return !player.wet || player.lavaWet;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Start the flames near the muzzle, but don't move their spawn position through a wall.
			Vector2 muzzleOffset = velocity.SafeNormalize(Vector2.UnitX * player.direction) * 30f;
			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0)) {
				position += muzzleOffset;
			}
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
