using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Localization;
using System;

namespace ExampleMod.Projectiles
{
    public class ExampleWhipProj : ModProjectile
    {
		public const int handleHeight = 24;
		public const int chainHeight = 14;
		public const int partHeight = 14;
		public const int tipHeight = 16;
		public const float whipLength = 20f;
        public const bool doubleCritWindow = true;
        public const bool ignoreLighting = true;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Whip Projectile");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.scale = 1.2f;
            projectile.aiStyle = 75;
            projectile.penetrate = -1;

            projectile.alpha = 50;
            projectile.hide = true;
            projectile.extraUpdates = 3;
            projectile.light = 0.3f;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Vector2 endPos = ExampleBaseWhip.WhipAI(projectile, whipLength); //This is from ExampleBaseWhip's code to make your whip function well without any problems with AI problems.
        }
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ExampleBaseWhip.ModifyHitAny(projectile, ref damage, ref knockback, ref crit);
		}
		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			ExampleBaseWhip.ModifyHitAny(projectile, ref damage, ref crit);
		}
		public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
		{
			ExampleBaseWhip.ModifyHitAny(projectile, ref damage, ref crit);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return ExampleBaseWhip.Colliding(projectile, targetHitbox);
		}

		public override bool? CanCutTiles()
		{
			return ExampleBaseWhip.CanCutTiles(projectile);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return ExampleBaseWhip.PreDraw(projectile, handleHeight, chainHeight, partHeight, tipHeight, 12, ignoreLighting, doubleCritWindow);
		}
	}
}