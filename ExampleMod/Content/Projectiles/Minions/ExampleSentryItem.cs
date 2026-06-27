using Microsoft.Xna.Framework;
using System;
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

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			bool canPlaceInAir = false;
			// This is just to let modders experiment with a sentry that places anywhere and one that snaps to the ground.
			if (player.direction == 1) {
				canPlaceInAir = true;
			}

			position = Main.MouseWorld;
			player.LimitPointToPlayerReachableArea(ref position);
			int halfProjectileHeight = (int)Math.Ceiling(ContentSamples.ProjectilesByType[type].height / 2f);

			if (!canPlaceInAir) {
				// This code will "snap" the sentry to the floor.
				// FindSentryRestingSpot returns the coordinates for the sentry to be placed on solid ground below the cursor position.
				player.FindSentryRestingSpot(type, out int worldX, out int worldY, out int pushYUp);
				position = new Vector2(worldX, worldY - halfProjectileHeight);

				// If, for some reason, you need custom placement logic (extra wide, hanging from the ceiling, etc), the following can be used as a guide for implementing that:
				/*
				// This loop travels down until it finds a solid tile to rest on.
				(int i, int j) = position.ToTileCoordinates();
				while (j < Main.maxTilesY - 10) {
					// This code checks a 3 tile wide area, this will need to be adjusted if the sentry's with is larger than 48.
					if (WorldGen.SolidTile2(i, j) || WorldGen.SolidTile2(i - 1, j) || WorldGen.SolidTile2(i + 1, j)) {
						break;
					}
					j++;
				}

				position = new Vector2(i * 16 + 8, j * 16 - halfProjectileHeight);
				// Also, replace "i * 16 + 8" with "position.X" if you don't want the sentry to "snap" to the center of tiles like the newer Tavernkeep sentries do.
				*/
			}
			else {
				position.Y -= halfProjectileHeight; // Adjust in-air option to spawn with bottom at cursor.
			}

			// Spawn the sentry projectile at the calculated location.
			Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, Main.myPlayer, ai2: canPlaceInAir ? 0 : 1);

			// Kills older sentry projectiles according to player.maxTurrets
			player.UpdateMaxTurrets();

			return false;
		}
	}
}
