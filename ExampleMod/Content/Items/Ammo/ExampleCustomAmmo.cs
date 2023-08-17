using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Ammo
{
	// This Example class demonstrates how to make your own weapon ammo.
	// Used by ExampleCustomAmmoGun
	public class ExampleCustomAmmo : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}

		public override void SetDefaults() {
			Item.width = 14; // The width of item hitbox
			Item.height = 14; // The height of item hitbox

			Item.damage = 8; // The damage for projectiles isn't actually 8, it actually is the damage combined with the projectile and the item together
			Item.DamageType = DamageClass.Ranged; // What type of damage does this ammo affect?

			Item.maxStack = Item.CommonMaxStack; // The maximum number of items that can be contained within a single stack
			Item.consumable = true; // This marks the item as consumable, making it automatically be consumed when it's used as ammunition, or something else, if possible
			Item.knockBack = 2f; // Sets the item's knockback. Ammunition's knockback added together with weapon and projectiles.
			Item.value = Item.sellPrice(0, 0, 1, 0); // Item price in copper coins (can be converted with Item.sellPrice/Item.buyPrice)
			Item.rare = ItemRarityID.Yellow; // The color that the item's name will be in-game.
			Item.shoot = ModContent.ProjectileType<Projectiles.ExampleHomingProjectile>(); // The projectile that weapons fire when using this item as ammunition.

			Item.ammo = Item.type; // Important. The first item in an ammo class sets the AmmoID to its type
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		// Here we create recipe for 999/ExampleCustomAmmo stack from 1/ExampleItem
		public override void AddRecipes() {
			CreateRecipe(999)
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
