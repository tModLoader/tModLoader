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
			Item.shoot = ModContent.ProjectileType<ExampleGolfBallProjectile>(); // Determines what projectile is placed on the golf tee.
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shootSpeed = 12f; // The velocity in pixels the projectile fired by the item will have. Actual velocity depends on the projectile being fired.
			Item.width = 18; // The width of the item's hitbox in pixels.
			Item.height = 20; // The height of the item's hitbox in pixels.
			Item.maxStack = 1; // The maximum number of items that can be contained within a single stack.
			Item.UseSound = SoundID.Item1; // The sound that your item makes when used.
			Item.useAnimation = 15; // The time span of the using animation for the Item.
			Item.useTime = 15; // The time span of using the item in frames.
			Item.noUseGraphic = true; // If true, the item's sprite will not be visible while the item is in use.
			Item.noMelee = true; // If true, the item's using animation will not deal damage. Set to true on most weapons that aren't swords.
			Item.value = Item.buyPrice(0, 0, 0, 0); // Value is the number of coins the item is worth (Platinum, Gold, Silver, Copper)
			Item.accessory = true; // Whether or not the item is an accessory.
			Item.rare = ItemRarityID.Green;
			Item.canBePlacedInVanityRegardlessOfConditions = true; // Allows the golf ball to be placed in vanity, despite not having a vanity slot (headSlot, neckSlot etc).
		}
	}
}
