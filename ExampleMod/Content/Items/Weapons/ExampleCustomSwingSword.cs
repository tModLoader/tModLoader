using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// ExampleCustomSwingSword is an example of a sword with a custom swing using a held projectile
	// This is great if you want to make melee weapons with complex swing behavior
	// A separate Example, ExampleCustomUseStyleWeapon, showcases implementing a custom swing using custom use style code rather than a held projectile.
	public class ExampleCustomSwingSword : ModItem
	{
		public int attackType = 0; // keeps track of which attack it is
		public int comboExpireTimer = 0; // we want the attack pattern to reset if the weapon is not used for certain period of time

		public override void SetDefaults() {
			// Common Properties
			Item.width = 46;
			Item.height = 48;
			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.Green;

			// Use Properties
			// Note that useTime and useAnimation for this item don't actually affect the behavior because the held projectile handles that.
			// Each attack takes a different amount of time to execute
			// Conforming to the item useTime and useAnimation makes it much harder to design
			// It does, however, affect the item tooltip, so don't leave it out.
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;

			// Weapon Properties
			Item.knockBack = 7;  // The knockback of your sword, this is dynamically adjusted in the projectile code.
			Item.autoReuse = true; // This determines whether the weapon has autoswing
			Item.damage = 62; // The damage of your sword, this is dynamically adjusted in the projectile code.
			Item.DamageType = DamageClass.Melee; // Deals melee damage
			Item.noMelee = true;  // This makes sure the item does not deal damage from the swinging animation
			Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand

			// Projectile Properties
			Item.shoot = ModContent.ProjectileType<ExampleCustomSwingProjectile>(); // The sword as a projectile
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Using the shoot function, we override the swing projectile to set ai[0] (which attack it is)
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
			attackType = (attackType + 1) % 2; // Increment attackType to make sure next swing is different
			comboExpireTimer = 0; // Every time the weapon is used, we reset this so the combo does not expire
			return false; // return false to prevent original projectile from being shot
		}

		public override void UpdateInventory(Player player) {
			if (comboExpireTimer++ >= 120) // after 120 ticks (== 2 seconds) in inventory, reset the attack pattern
				attackType = 0;
		}

		public override bool MeleePrefix() {
			return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}