using ExampleMod.Content.Projectiles;
using ExampleMod.Content.Tiles.Furniture;
using ExampleMod.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleYoyo : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded yoyo.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; //The amount of this item that needs to be researched to unlock it in the journey mode duplication menu.

			// These are all related to gamepad controls and don't seem to affect anything else
			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.width = 24; //The width of the item's hitbox.
			Item.height = 24; //The height of the item's hitbox.

			Item.useStyle = ItemUseStyleID.Shoot; //The way the item is used (e.g. swinging, throwing, etc.)
			Item.useTime = 25; //The item's use time in ticks (1 second == 60 ticks)
			Item.useAnimation = 25; //The item's use animation length in ticks (1 second == 60 ticks)
			Item.noMelee = true; //The item's animation won't deal damage if this is set to true.
			Item.noUseGraphic = true; //The item's animation will be invisible if this is set to true.
			Item.UseSound = SoundID.Item1; //The sound that will play when the item is used.

			Item.damage = 40; //The amount of damage the item does to an enemy or player.
			Item.DamageType = DamageClass.Melee; //Sets the item's damage type to melee.
			Item.knockBack = 2.5f; //The amount of knockback the item inflicts.
			Item.crit = 8; //The percent chance for the weapon to deal a critical strike. Defaults to 4.
			Item.channel = true; //Set to true for items that require the attack button to be held out (e.g. yoyos and magic missile weapons)
			Item.rare = ModContent.RarityType<ExampleModRarity>(); //The item's rarity. This changes the color of the item's name.
			Item.value = Item.buyPrice(gold: 1); //The amount of money that the item is can be bought for.

			Item.shoot = ModContent.ProjectileType<ExampleYoyoProjectile>(); //The projectile that the item shoots.
			Item.shootSpeed = 16f; //The velocity of the shot projectile.			
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
