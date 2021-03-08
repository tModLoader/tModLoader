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
			Item.damage = 40;
			Item.useStyle = ItemUseStyleID.Shoot; // The use style of your item, UseStyle shoot draws the item sprite in the direction of the cursor.
			Item.useAnimation = 18; // Animation Duration (In ticks).
			Item.useTime = 24; // Usetime Duration (In ticks).
			Item.shootSpeed = 3.7f; // The velocity in pixels per frame the projectile fired by the item will have.
			Item.knockBack = 6.5f; // The force of the knock back. Max value is 20.
			Item.rare = ItemRarityID.Pink; // Rarity/Color of the item you can look at the rest at https://terraria.gamepedia.com/Rarity.
			Item.value = Item.sellPrice(silver: 10); // Value is the number of coins the item is worth.

			Item.DamageType = DamageClass.Melee; // The type of damage this item deals, if it is a weapon. Setting more than one is useless.
			Item.noMelee = true; // Important because the spear is actually a projectile instead of an item. This prevents the melee hitbox of this item, Needed to mention that the projectile also has its own sprite.
			Item.noUseGraphic = true; // Important, it's kind of wired if people see two spears at one time. This prevents the melee animation of this item.
			Item.autoReuse = true; // Most spears don't autoReuse, but it's possible when used in conjunction with CanUseItem()

			Item.UseSound = SoundID.Item1; // The sound that your item makes when used.
			Item.shoot = ProjectileType<ExampleSpearProjectile>(); // The projectile that is fired by the item on use. 
		}

		public override bool CanUseItem(Player player) {
			// Ensures no more than one spear can be thrown out, use this when using autoReuse
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
