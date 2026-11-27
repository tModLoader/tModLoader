using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Items;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleWhip : ModItem
	{
		public static readonly int ExampleWhipTagDamage = 5;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ExampleWhipTagDamage);

		public override void SetStaticDefaults() {
			// Here is where we define how much TagDamage the whip does.
			// TagDuration and CritChance can be modified, too.
			// For more customizability, see Example Whip Advanced's tag effects.
			ItemID.Sets.UniqueTagEffects[Type] = new WhipTagEffect() { TagDamage = ExampleWhipTagDamage };
		}

		public override void SetDefaults() {
			// This method quickly sets the whip's properties.
			// Mouse over to see its parameters.
			Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectile>(), 20, 2, 4);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(gold: 1);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// This gives some visual variance on how fast the whip swinging animation plays out.
			// This has no effect on the actual collision.
			float swingDirection = 0.6f + (0.4f * Main.rand.NextFloat());
			// 1/3 of the time, swing the whip from the bottom to top instead of from top to bottom.
			// The Dark Harvest is the only whip that doesn't have the chance of swinging from the bottom up.
			if (Main.rand.NextBool(3)) {
				swingDirection *= -2.5f;
			}
			// Set swingDirection to 1f for the pre-1.4.5 behavior.

			// If the Snake Band spawns this whip, no item time is applied.
			// However, the whip projectile AI applies the life time from the item time.
			// So, spawn the secondary whip with a value set in Projectile.ai[2] with its life time.
			float secondaryWhipTime = 0f;
			if (Item.IsPerformingFreeUse) {
				// Set the velocity of the secondary whip to be a little different from the primary whip.
				velocity = velocity.RotatedByRandom(MathHelper.Pi / 10f);

				// Secondary whips are set to double this item's animation time.
				// The 1000f is important for designating that the whip is a secondary whip and is corrected in Projectile.GetWhipSettings().
				secondaryWhipTime = 1000f + player.itemAnimationMax * 2f;

				// Reduce the damage and knockback of the secondary whip.
				float secondaryWhipDamageMultiplier = 0.3f; // -70% damage
				float secondaryWhipKnockbackMultiplier = 0.25f; // -75% knockback
				damage = (int)MathHelper.Max(damage * secondaryWhipDamageMultiplier, 3); // Make the secondary whip deal at least 3 damage.
				knockback *= secondaryWhipKnockbackMultiplier;
			}

			// Spawn the whip projectile.
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: 0f, ai1: swingDirection, ai2: secondaryWhipTime);

			// If the whip is a primary whip (not spawned by the Snake Band), keep track of how many swings we've done.
			if (!Item.IsPerformingFreeUse) {
				player.repeatWhipSwings++;
				player.repeatWhipsResetCooldown = 120;
			}

			return false; // Return false because we've already spawned the projectile.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		// Makes the whip receive melee prefixes
		public override bool MeleePrefix() {
			return true;
		}
	}
}
