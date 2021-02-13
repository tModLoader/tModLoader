using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Projectiles;

namespace ExampleMod.Items.Weapons
{
	public class ExampleSolarEruption : ModItem
    {
		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Example Solar Eruption");
			Tooltip.SetDefault("Less powerful than the Solar Eruption, but more explosively potent");
        }

        public override void SetDefaults()
        {
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAnimation = 60;
			item.useTime = 60;
			item.shootSpeed = 7f;
			item.knockBack = 4f;
			item.UseSound = SoundID.Item116;
			item.shoot = ModContent.ProjectileType<ExampleSolarEruptionProjectile>();
			item.value = Item.sellPrice(silver: 5);
			item.noMelee = true;
			item.noUseGraphic = true;
			item.channel = true;
			item.autoReuse = true;
			item.melee = true;
			item.damage = 12;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			// How far out the inaccuracy of the shot chain can be.
			float radius = 90f;
			// Sets ai[1] to the following value to determine the firing direction.
			// The smaller the value of NextFloat(), the more accurate the shot will be. The larger, the less accurate. This changes depending on your radius.
			// NextBool().ToDirectionInt() will have a 50% chance to make it negative instead of positive.
			// The Solar Eruption uses this calculation: Main.rand.NextFloat(0f, 0.5f) * Main.rand.NextBool().ToDirectionInt() * MathHelper.ToRadians(90f);
			float direction = Main.rand.NextFloat(0.25f, 1f) * Main.rand.NextBool().ToDirectionInt() * MathHelper.ToRadians(radius);
			Projectile projectile = Projectile.NewProjectileDirect(player.RotatedRelativePoint(player.MountedCenter), new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 0f, direction);
			// Extra logic for the chain to adjust to item stats, unlike the Solar Eruption.
			if (projectile.modProjectile is ExampleSolarEruptionProjectile modItem)
            {
				modItem.firingSpeed = item.shootSpeed * 2f;
				modItem.firingAnimation = item.useAnimation;
				modItem.firingTime = item.useTime;
            }
			return false;
        }
    }
}