using ExampleMod.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Weapons
{
	public class ExampleSpear : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded spear");
		}

		public override void SetDefaults() {
			// Common Properties
			item.width = 32; // The item texture's width.
			item.height = 32; // The item texture's height.
			item.scale = 1f; // The item texture's scale.
			item.rare = ItemRarityID.Pink; // Assign this item a rarity level of Pink
			item.value = Item.sellPrice(silver: 10); // The number and type of coins item can be sold for to an NPC

			// Use Properties
			item.useStyle = ItemUseStyleID.HoldingOut; // How you use the item (swinging, holding out, etc.)
			item.useAnimation = 18; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			item.useTime = 24; // The length of the item's use time in ticks (60 ticks == 1 second.)
			item.UseSound = SoundID.Item1; // The sound that this item plays when used.
			item.autoReuse = true; // Whether or not you can hold click to automatically use it again. Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()		

			// Weapon Properties
			item.damage = 40;
			item.knockBack = 6.5f;	
			item.noUseGraphic = true; // If true, the item's sprite will not be visible while the item is in use. This is true because a spear is technically a projectile so we do not want to show a second spear while using.

			// Melee Properties
			item.melee = true;
			item.noMelee = true; // Whether or not the item's animation will do damage Important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.

			// Projectile Properties
			item.shootSpeed = 3.7f; // The speed of the projectile (measured in pixels per frame.)
			item.shoot = ProjectileType<ExampleSpearProjectile>(); //The projectile that is fired from this weapon
		}

		public override bool CanUseItem(Player player) {
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[item.shoot] < 1;
		}
		
		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
