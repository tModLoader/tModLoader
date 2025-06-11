using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// This is an example gun designed to best demonstrate the various tML hooks that can be used for ammo-related specifications.
	// The ammo wiki guide, https://github.com/tModLoader/tModLoader/wiki/Basic-Ammo, is a good resource for learning how the ammo system works.
	public class ExampleSpecificAmmoGun : ModItem
	{
		public static readonly int FreeAmmoChance1 = 20;
		public static readonly int FreeAmmoChance2 = 63;
		public static readonly int FreeAmmoChance3 = 36;
		public static readonly int AmmoUseDamageBoost = 20;

		private bool consumptionDamageBoost = false;

		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleGun"; // TODO: remove when sprite is made for this

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FreeAmmoChance1, FreeAmmoChance2, FreeAmmoChance3, AmmoUseDamageBoost);

		public override void SetDefaults() {
			// Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.

			// Common Properties
			Item.width = 62; // Hitbox width of the item.
			Item.height = 32; // Hitbox height of the item.
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.Green; // The color that the item's name will be in-game.

			// Use Properties
			Item.useTime = 5; // The item's use time in ticks (60 ticks == 1 second.)
			Item.useAnimation = 15; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			Item.reuseDelay = 5; // The amount of time the item waits between use animations (60 ticks == 1 second.)
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
			Item.autoReuse = true; // Whether or not you can hold click to automatically use it again.
			Item.UseSound = SoundID.Item11;

			// Weapon Properties
			Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
			Item.damage = 20; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
			Item.knockBack = 5f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
			Item.noMelee = true; // So the item's animation doesn't do damage.

			// Gun Properties
			Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
			Item.shootSpeed = 16f; // The speed of the projectile (measured in pixels per frame.)
			Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(2f, -2f);
		}

		public override void UpdateInventory(Player player) {
			consumptionDamageBoost = false;
		}

		public override bool? CanChooseAmmo(Item ammo, Player player) {
			// CanChooseAmmo allows ammo to be chosen or denied independently of the useAmmo field's restrictions.
			// (Its sister hook, CanBeChosenAsAmmo, is called on the ammo, and has the same function.)
			// This returns null by default, which simply picks the ammo based on whether or not ammo.ammo == weapon.useAmmo.
			// Returning true will forcibly allow an ammo to be used; returning false will forcibly deny it.
			// For this example, we'll forcefully deny Cursed Bullets from being used as ammunition, but otherwise make no changes to the ammo pool.
			if (ammo.type == ItemID.CursedBullet)
				return false;

			// This code would allow this weapon to use Arrow ammo. A modder making such a weapon would want to make sure the item tooltip informs the user about these ammo irregularities.
			//if (ammo.ammo == AmmoID.Arrow) {
			//	return true;

			// Oh, and a word of advice: always default to returning null, as per the above.
			// Defaulting to returning true or false may have unintended consequences on what you can or can't use as ammo.
			return null;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player) {
			// CanConsumeAmmo allows ammo to be conserved or consumed depending on various conditions.
			// (Its sister hook, CanBeConsumedAsAmmo, is called on the ammo, and has the same function.)
			// This returns true by default; returning false for any reason will prevent ammo consumption.
			// Note that returning true does NOT allow you to force ammo consumption; this currently requires use of IL editing or detours.

			// For this example, the first shot will have a 20% chance to conserve ammo...
			// ...the second shot will have a 63% chance to conserve ammo...
			// ...and the third shot will have a 36% chance to conserve ammo.
			if (player.ItemUsesThisAnimation == 0)
				return Main.rand.NextFloat() >= FreeAmmoChance1 / 100f;
			else if (player.ItemUsesThisAnimation == 1)
				return Main.rand.NextFloat() >= FreeAmmoChance2 / 100f;
			else if (player.ItemUsesThisAnimation == 2)
				return Main.rand.NextFloat() >= FreeAmmoChance3 / 100f;

			return true;
		}

		public override void OnConsumeAmmo(Item ammo, Player player) {
			// OnConsumeAmmo allows you to make things happen when ammo is successfully consumed.
			// (Its sister hook, OnConsumedAsAmmo, is called on the ammo, and has the same function.)
			// Here, we'll set a bool to true which dictates whether or not the next shot should receive a damage bonus.
			// This makes it so that shots which do consume ammunition gain a damage bonus in exchange for that consumption.
			consumptionDamageBoost = true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (consumptionDamageBoost) {
				damage = damage * (100 + AmmoUseDamageBoost) / 100;
			}
		}
	}
}
