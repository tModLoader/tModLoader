using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleSpear : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("An example spear");
		}

		public override void SetDefaults() { 
			item.damage = 40; // The damage inflicted by the item.
			item.useStyle = ItemUseStyleID.Shoot; // The use style of your item.
			item.useAnimation = 18; // The time span of the using animation for the item. 
			item.useTime = 24; // The time span of using the item in frames.
			item.shootSpeed = 3.7f; // The velocity in pixels the projectile fired by the item will have.
			item.knockBack = 6.5f; // The force of the knock back. Max value is 20.
			item.width = 32; // The width of the item's hitbox in pixels.
			item.height = 32; // The height of the item's hitbox in pixels.
			item.scale = 1f; // The size multiplier of the item's sprite while the item is being used. Also increases range for melee weapons.
			item.rare = ItemRarityID.Pink; // Rarity/Color of the item you can look at the rest at https://terraria.gamepedia.com/Rarity.
			item.value = Item.sellPrice(silver: 10); // Value is the number of coins the item is worth.

			item.melee = true; //Whether or not your item is part of the melee class.
			item.noMelee = true; // Important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item.
			item.noUseGraphic = true; // Important, it's kind of wired if people see two spears at one time. This prevents the melee animation of this item.
			item.autoReuse = true; // Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()

			item.UseSound = SoundID.Item1; // The sound that your item makes when used.
			item.shoot = ProjectileType<ExampleSpearProjectile>(); // The projectile that is fired by the item on use. 
		}

		public override bool CanUseItem(Player player) {
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[item.shoot] < 1;
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
