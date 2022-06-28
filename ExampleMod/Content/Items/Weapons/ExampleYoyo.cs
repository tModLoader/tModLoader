using ExampleMod.Content.Projectiles;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleYoyo : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Shoots out an example yoyo"); // The (English) text shown below your weapon's name. 

			// These are all related to gamepad controls and don't seem to affect anything else.
			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // The number of sacrifices that is required to research the item in Journey Mode.
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot; // How the item behaves when used. Shoot is a very common useStyle.
			Item.width = 24; // The width of the item's hitbox.
			Item.height = 24; // The height of the item's hitbox.
			Item.useAnimation = 25; // All vanilla yoyos have a useAnimation of 25.
			Item.useTime = 25; // All vanilla yoyos have a useTime of 25.
			Item.shootSpeed = 16f; // All vanilla yoyos have a shootSpeed of 16f
			Item.knockBack = 2.5f; // How much knockback the weapon does.
			Item.damage = 9; // How much damage the weapon does.
			Item.rare = ItemRarityID.White; // The rarity of the item. White rarity will burn in lava.

			Item.DamageType = DamageClass.MeleeNoSpeed; // The type of damage the weapon does. MeleeNoSpeed means the item will not scale with attack speed.
			Item.channel = true; // This means the attack key can be held down. 
			Item.noMelee = true; // This makes it so the item doesn't do damage to enemies (the projectile does that).
			Item.noUseGraphic = true; // Makes the item invisible while using it (the projectile is the visible part).

			Item.UseSound = SoundID.Item1; // What sound the item makes when you use it.
			Item.value = Item.sellPrice(silver: 1); // The value of the item. In this case, 1 silver.
			Item.shoot = ModContent.ProjectileType<ExampleYoyoProjectile>(); // Which projectile this item will shoot. We set this to our corresponding projectile.
		}

		// Here is an example of blacklisting certain modifiers. Remove this section for standard vanilla behavior.
		// In this example, we are blacklisting the ones that reduce damage of a melee weapon.
		// Make sure that your item can even receive these prefixes (check the vanilla wiki on prefixes).
		private static readonly int[] unwantedPrefixes = new int[] { PrefixID.Terrible, PrefixID.Dull, PrefixID.Shameful, PrefixID.Annoying, PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy};

		public override bool AllowPrefix(int pre) {
			// return false to make the game reroll the prefix.

			// DON'T DO THIS BY ITSELF:
			// return false;
			// This will get the game stuck because it will try to reroll every time. Instead, make it have a chance to return true.

			if (Array.IndexOf(unwantedPrefixes, pre) > -1) {
				// IndexOf returns a positive index of the element you search for. If not found, it's less than 0.
				// Here we check if the selected prefix is positive (it was found).
				// If so, we found a prefix that we don't want. Reroll.
				return false;
			}

			// Don't reroll
			return true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<ExampleItem>(), 10)
				.AddIngredient(ItemID.WoodYoyo)
				.Register();
		}
	}
}
