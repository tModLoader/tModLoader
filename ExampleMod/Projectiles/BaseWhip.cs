using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Enums;

namespace ExampleMod.Projectiles
{
    public static class BaseWhip //This is a helper code for making whips easily.
    {
        public static Vector2 WhipAI(Projectile projectile, float whipLength = 16, bool ignoreTiles = false, int sndgroup = 2, int sound = 39)
        //Whip AI for any whip projectile.
        {
            //Use localAI[1] to track hitting something when using the whip.
            Player player = Main.player[projectile.owner];
            if (projectile.ai[0] == 0f)
            {
                projectile.localAI[1] = player.itemAnimationMax;
                projectile.restrikeDelay = 0;
            }
            float speedIncrease = (float)player.HeldItem.useAnimation / projectile.localAI[1];

            return AI_075(projectile, whipLength * speedIncrease / projectile.MaxUpdates,
                (int)projectile.localAI[1], ignoreTiles, sndgroup, sound);
        }

        private static Vector2 AI_075(Projectile projectile, float swingLength, int swingTime, bool ignoreTiles, int sndgroup, int sound)
        {
            Player player = Main.player[projectile.owner];
            float num = 1.57079637f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, true);

            if (projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = projectile.velocity.ToRotation();
            }
            float num33 = (float)((projectile.localAI[0].ToRotationVector2().X >= 0f) ? 1 : -1);
            if (projectile.ai[1] <= 0f)
            {
                num33 *= -1f;
            }
            Vector2 vector25 = (num33 * (projectile.ai[0] / swingTime * 6.28318548f - 1.57079637f)).ToRotationVector2();
            vector25.Y *= (float)Math.Sin((double)projectile.ai[1]);
            if (projectile.ai[1] <= 0f)
            {
                vector25.Y *= -1f;
            }
            vector25 = vector25.RotatedBy((double)projectile.localAI[0], default(Vector2));
            projectile.ai[0] += 1f / projectile.MaxUpdates;
            if (projectile.ai[0] < swingTime)
            {
                projectile.velocity += swingLength * vector25;
            }
            else
            {
                projectile.Kill();
            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.itemTime = Math.Max(player.itemTime, projectile.restrikeDelay);
            player.itemAnimation = Math.Max(2, projectile.restrikeDelay);
            player.itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));

            Vector2 vector34 = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1)
            {
                vector34.X = (float)player.bodyFrame.Width - vector34.X;
            }
            if (player.gravDir != 1f)
            {
                vector34.Y = (float)player.bodyFrame.Height - vector34.Y;
            }
            vector34 -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
            projectile.Center = player.RotatedRelativePoint(player.position + vector34, true) - projectile.velocity;

            //Colliding with tiles.
            Vector2 endPoint = projectile.position + projectile.velocity * 2f;

            if (projectile.ai[0] > 1 && !ignoreTiles) //Helps with delaying and stopping close tile collision.
            {
                Vector2 prevPoint = projectile.oldPosition + projectile.oldVelocity * 2f;
                if (!Collision.CanHit(endPoint, projectile.width, projectile.height, prevPoint, projectile.width, projectile.height))
                {
                    if (projectile.ai[0] * 2 < projectile.localAI[1])
                    {
                        projectile.restrikeDelay = player.itemAnimationMax - (int)projectile.ai[0] * 2;
                        projectile.ai[0] = Math.Max(1f, projectile.localAI[1] - projectile.ai[0] + 1);
                        projectile.ai[1] *= -0.9f;
                        Main.PlaySound(sndgroup, endPoint, sound);
                        Collision.HitTiles(endPoint, endPoint - prevPoint, 8, 8);
                    }
                }
            }

            return endPoint;
        }
    }
}