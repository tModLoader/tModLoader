using ExampleMod.Content.Projectiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleGolfBall : ModItem
	{
		public override void SetDefaults() {
			item.shoot = ModContent.ProjectileType<ExampleGolfBallProjectile>(); // Determines what projectile is placed on the golf tee.
			item.useStyle = ItemUseStyleID.Swing;
			item.shootSpeed = 12f;
			item.width = 18;
			item.height = 20;
			item.maxStack = 1;
			item.UseSound = SoundID.Item1;
			item.useAnimation = 15;
			item.useTime = 15;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.value = 0;
			item.accessory = true;
			item.rare = ItemRarityID.Green;
			item.canBePlacedInVanityRegardlessOfConditions = true; // Allows the golf ball to be placed in vanity.
		}
	}
}
