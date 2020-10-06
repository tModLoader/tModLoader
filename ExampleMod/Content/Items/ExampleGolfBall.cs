using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleGolfBall : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		
		public override void SetDefaults() {
			item.shoot = ModContent.ProjectileType<ExampleGolfBallProjectile>(); // Determines what projectile is placed on the golf tee.
			item.useStyle = ItemUseStyleID.Swing;
			item.shootSpeed = 12f; // The velocity in pixels the projectile fired by the item will have. Actual velocity depends on the projectile being fired.
			item.width = 18; // The width of the item's hitbox in pixels.
			item.height = 20; // The height of the item's hitbox in pixels.
			item.maxStack = 1; // The maximum number of items that can be contained within a single stack.
			item.UseSound = SoundID.Item1; // The sound that your item makes when used.
			item.useAnimation = 15; // The time span of the using animation for the item.
			item.useTime = 15; // The time span of using the item in frames.
			item.noUseGraphic = true; // If true, the item's sprite will not be visible while the item is in use.
			item.noMelee = true; // If true, the item's using animation will not deal damage. Set to true on most weapons that aren't swords.
			item.value = Item.buyPrice(0, 0, 0, 0); // Value is the number of coins the item is worth (Platinum, Gold, Silver, Copper)
			item.accessory = true; // Whether or not the item is an accessory.
			item.rare = ItemRarityID.Green;
			item.canBePlacedInVanityRegardlessOfConditions = true; // Allows the golf ball to be placed in vanity, despite not having a vanity slot (headSlot, neckSlot etc).
		}
	}
}
