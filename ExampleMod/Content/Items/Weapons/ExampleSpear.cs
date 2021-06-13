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
			item.rare = ItemRarityID.Pink; // Assign this item a rarity level of Pink
			item.value = Item.sellPrice(silver: 10); // The number and type of coins item can be sold for to an NPC

			// Use Properties
			item.useStyle = ItemUseStyleID.HoldingOut; // How you use the item (swinging, holding out, etc.)
			item.useAnimation = 18; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			item.useTime = 24; // The length of the item's use time in ticks (60 ticks == 1 second.)
			item.UseSound = SoundID.Item1; // The sound that this item plays when used.
			item.autoReuse = true; // Allows the player to hold click to automatically use the item again. Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()		

			// Weapon Properties
			item.damage = 40;
			item.knockBack = 6.5f;	
			item.noUseGraphic = true; // When true, the item's sprite will not be visible while the item is in use. This is true because the spear projectile is what's shown so we do not want to show the spear sprite as well.

			// Melee Properties
			item.melee = true;
			item.noMelee = true; // Allows the item's animation to do damage. This is important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.

			// Projectile Properties
			item.shootSpeed = 3.7f; // The speed of the projectile measured in pixels per frame.
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
