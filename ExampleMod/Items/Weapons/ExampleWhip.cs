using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ExampleMod.Items.Weapons
{
    public class ExampleWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Whip");
            Tooltip.SetDefault("Only deals critical hits at the tip\nCritical strike chance boosts critical damage");
        }
        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 21;
            item.useTime = 22;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;

            item.melee = true;
            item.damage = 11;
            item.crit = 11; //Critcal damage increased instead of critical strike chance.
            item.knockBack = 5f;
            item.shoot = mod.ProjectileType("ExampleWhipProj");
            item.shootSpeed = 1f;

            item.rare = 0;
            item.value = Item.sellPrice(0, 25); //Platinum coins is 0 and 25 is gold coins.
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0,
                Main.rand.Next(-150, 150) * 0.001f * player.gravDir); //Whip swinging code for the whip to attack enemies.
            return false;
        }
    }
}