using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	// This is the item that summons ExampleSentry.
	public class ExampleSentryItem : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
		}

		public override void SetDefaults() {
			Item.damage = 50;
			Item.DamageType = DamageClass.Summon;
			Item.sentry = true;
			Item.mana = 10;
			Item.width = 26;
			Item.height = 28;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 3;
			Item.value = Item.buyPrice(gold: 30);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item83;
			Item.shoot = ModContent.ProjectileType<ExampleSentry>();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = Main.MouseWorld;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			bool canPlaceInAir = false;
			// This is just to let modders experiment with a sentry that places anywhere and one that snaps to the ground.
			if (player.direction == 1) {
				canPlaceInAir = true;
			}

			(int i, int j) = position.ToTileCoordinates(); // position is Main.MouseWorld from ModifyShootStats

			if (!canPlaceInAir) {
				// This code will "snap" the sentry to the floor. This is the Queen Spider Staff and Staff of Frost Hydra approach.
				// This loop travels down until it finds a solid tile to rest on.
				while (j < Main.maxTilesY - 10) {
					if (WorldGen.SolidTile2(i, j) || WorldGen.SolidTile2(i - 1, j) || WorldGen.SolidTile2(i + 1, j)) {
						break;
					}
					j++;
				}
				j--; // Move back up to the empty space right above the found solid tile

				// Spawn immediately over the tile.
				// Depending on the height of the projectile you may need to adjust this, but it works as is for this projectile's height.
				position = new Vector2(Main.MouseWorld.X, j * 16);
			}
			else {
				position.Y -= 15; // Adjust in-air spawn to spawn with bottom at cursor.
			}

			// Spawn the sentry projectile at the calculated location.
			Projectile sentryProjectile = Projectile.NewProjectileDirect(source, position, canPlaceInAir ? Vector2.Zero : new Vector2(0f, 15f), type, damage, knockback, Main.myPlayer, ai2: canPlaceInAir ? 0 : 1);

			// originalDamage facilitates the Projectile.ContinuouslyUpdateDamageStats feature inherent to sentries and minions.
			sentryProjectile.originalDamage = Item.damage;

			// Kills older sentry projectiles according do player.maxTurrets
			player.UpdateMaxTurrets();

			return false;
		}
	}
}
