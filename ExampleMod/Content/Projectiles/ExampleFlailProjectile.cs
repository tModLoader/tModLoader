using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ExampleMod.Content.Dusts;

namespace ExampleMod.Content.Projectiles     //We need this to basically indicate the folder where it is to be read from, so you the texture will load correctly
{
    public class ExampleFlailProjectile : ModProjectile
    {
        bool alreadyHitATile = false; //checks if the projectile already hit a tile, used for the afterimage trail
        int shootTimer = 12; //how much time the projectile can go before retracting (speed and shootTimer will set the flail's range)
        int tilecounter = 0; //how much tiles this projectile has hit when not in line of sight
        // The folder path to the flail chain sprite
        private const string ChainTexturePath = "ExampleMod/Content/Projectiles/ExampleFlailProjectileChain";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Flail");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;    //The length of old position to be recorded
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24; //the width of your projectile
            Projectile.height = 24; //the height of your projectile
            Projectile.friendly = true; //deals damage to enemies
            Projectile.penetrate = -1; //infinite pierce
            Projectile.tileCollide = true; //collides with tiles and thus calls OnTileCollide
            Projectile.ignoreWater = true; //does not get slowed down in water
            Projectile.DamageType = DamageClass.Melee; //deals melee damage
            Projectile.usesLocalNPCImmunity = true; //used for hit cooldown changes in the ai hook
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            //kill the projectile if the player dies or gets crowd controlled
            if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
            {
                Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
            {
                Projectile.Kill();
                return;
            }
            if (tilecounter >= 3) //forces the flail to retract once it hits three bounces that are not in line of sight, to deal with the flail being stuck behind a wall
            {
                Projectile.tileCollide = false;
                Projectile.ai[0] = 4;
            }
            //dusts
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1f);
            dust.noGravity = true;
            dust.velocity.X = MathHelper.Lerp(-0.5f, 0.5f, (float)Main.rand.NextDouble());
            dust.velocity.Y = MathHelper.Lerp(-0.5f, 0.5f, (float)Main.rand.NextDouble());

            Vector2 mountedCenter = player.MountedCenter;
            bool shouldOwnerHitCheck = false;
            float speed = 18f; //how fast the projectile can move
            float maxStretchLength = 600f; //how far the projectile's chain can stretch before being forced to retract
            float deceleration = 3f;
            float retractSpeed = 12f; //how fast the projectile comes back to you
            float alsoSuperRetractSpeed = 6f;
            float superRetractSpeed = 16f; //how fast the projectile comes back to you when being forced to retract
            float retractSpeedMult = 1f;
            float alsoRetractSpeed = 10f;
            int restingChainLength = 60;
            int hitspeedResting = 8; //how often your flail hits when resting on the ground, or retracting
            int hitspeedSpinning = 15; //how often your flail hits when spinning
            int hitspeed = 10; //how often your flail hits when moving
            int fallingTimer = shootTimer + 5;
            float meleeSpeed = player.meleeSpeed;
            float mSpeedMult = 1f / meleeSpeed;
            speed *= mSpeedMult;
            retractSpeedMult *= mSpeedMult;
            alsoRetractSpeed *= mSpeedMult;
            deceleration *= mSpeedMult;
            retractSpeed *= mSpeedMult;
            alsoSuperRetractSpeed *= mSpeedMult;
            superRetractSpeed *= mSpeedMult;
            float rangeMult = speed * (float)shootTimer;
            float range = rangeMult + 160f;
            Projectile.localNPCHitCooldown = hitspeedResting;
            switch ((int)Projectile.ai[0])
            {
                case 0: //Projectile.ai[0] = 0; (Spinning mode)
                    {
                        shouldOwnerHitCheck = true;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            Vector2 origin = mountedCenter;
                            Vector2 mouseWorld = Main.MouseWorld;
                            Vector2 value3 = origin.DirectionTo(mouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                            player.ChangeDir((value3.X > 0f) ? 1 : (-1));
                            if (!player.channel) //If the player releases then change to moving forward mode
                            {
                                Projectile.tileCollide = true;
                                Projectile.ai[0] = 1f;
                                Projectile.ai[1] = 0f;
                                Projectile.velocity = value3 * speed + player.velocity;
                                Projectile.Center = mountedCenter;
                                Projectile.netUpdate = true;
                                Projectile.localNPCHitCooldown = hitspeed;
                                break;
                            }
                        }
                        Projectile.localAI[1] += 1f;
                        Vector2 value4 = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (Projectile.localAI[1] / 60f) * (float)player.direction);
                        value4.Y *= 0.8f;
                        if (value4.Y * player.gravDir > 0f)
                        {
                            value4.Y *= 0.5f;
                        }
                        Projectile.Center = mountedCenter + value4 * 30f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.localNPCHitCooldown = hitspeedSpinning; //set the hit speed to the spinning hit speed
                        Projectile.tileCollide = false;
                        break;
                    }
                case 1: //Projectile.ai[0] = 1; (Moving forward)
                    {
                        bool surpassedShootTimer = Projectile.ai[1]++ >= (float)shootTimer;
                        surpassedShootTimer |= Projectile.Distance(mountedCenter) >= maxStretchLength;
                        if (player.controlUseItem) //if tap again then move to fall to the ground mode
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                            break;
                        }
                        if (surpassedShootTimer) //if shootTimer is surpassed then move to transition mode
                        {
                            Projectile.ai[0] = 2f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.3f;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        Projectile.localNPCHitCooldown = hitspeed;
                        break;
                    }
                case 2: //Projectile.ai[0] = 2; (Forward to backward transition)
                    {
                        Vector2 value2 = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= retractSpeed)
                        {
                            Projectile.Kill();
                            return;
                        }
                        if (player.controlUseItem) //if tap again then move to fall to the ground mode
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                        }
                        else
                        {
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(value2 * retractSpeed, deceleration);
                            player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        }
                        break;
                    }
                case 3: //Projectile.ai[0] = 3; (Retracting mode)
                    {
                        if (!player.controlUseItem)
                        {
                            Projectile.ai[0] = 4f; //Move to super retracting mode if the player taps
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            break;
                        }
                        float currentChainLength = Projectile.Distance(mountedCenter);
                        Projectile.tileCollide = Projectile.ai[1] == 1f;
                        bool flag3 = currentChainLength <= rangeMult;
                        if (flag3 != Projectile.tileCollide)
                        {
                            Projectile.tileCollide = flag3;
                            Projectile.ai[1] = (Projectile.tileCollide ? 1 : 0);
                            Projectile.netUpdate = true;
                        }
                        if (currentChainLength > (float)restingChainLength)
                        {

                            if (currentChainLength >= rangeMult)
                            {
                                Projectile.velocity *= 0.5f;
                                Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * alsoRetractSpeed, alsoRetractSpeed);
                            }
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * alsoRetractSpeed, retractSpeedMult);
                        }
                        else
                        {
                            if (Projectile.velocity.Length() < 6f)
                            {
                                Projectile.velocity.X *= 0.96f;
                                Projectile.velocity.Y += 0.2f;
                            }
                            if (player.velocity.X == 0f)
                            {
                                Projectile.velocity.X *= 0.96f;
                            }
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 4: //Projectile.ai[0] = 4; (Super retracting mode)
                    {
                        Projectile.tileCollide = false;
                        Vector2 vector = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= superRetractSpeed)
                        {
                            Projectile.Kill(); //kill the projectile if too far away
                            return;
                        }
                        Projectile.velocity *= 0.98f;

                        Projectile.velocity = Projectile.velocity.MoveTowards(vector * superRetractSpeed, alsoSuperRetractSpeed);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(vector, value) < 0f)
                        {
                            Projectile.Kill(); //kill the projectile if too far away
                            return;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 5: //Projectile.ai[0] = 5; (Fall to the ground mode part 1)
                    if (Projectile.ai[1]++ >= (float)fallingTimer)
                    {
                        Projectile.ai[0] = 6f; //If fallingTimer is surpassed then move to part 2 falling mode
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.localNPCHitCooldown = hitspeed;
                        Projectile.velocity.Y += 0.6f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
                case 6: //Projectile.ai[0] = 6; (Fall to the ground mode part 2)
                    if (!player.controlUseItem || Projectile.Distance(mountedCenter) > range)
                    {
                        Projectile.ai[0] = 4f; //If the player releases, move to super retracting mode
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.velocity.Y += 0.8f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
            }
            Projectile.direction = ((Projectile.velocity.X > 0f) ? 1 : (-1));
            Projectile.spriteDirection = Projectile.direction;
            Projectile.ownerHitCheck = shouldOwnerHitCheck;
                if (Projectile.velocity.Length() > 1f)
                {
                    //This check is used if your flail's projectile is shaped like it points one direction
                    if(player.direction == 1)
					{
					    if (Projectile.velocity.X > 0f)
					    {
						Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.ToRadians(90f);
					    }
					    else if (Projectile.velocity.X < 0f)
					    {
						Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
					    }
					}
					else
					{
						if (Projectile.velocity.X > 0f)
					    {
						Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
					    }
					    else if (Projectile.velocity.X < 0f)
					    {
						Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.ToRadians(90f);
					    }
					}
                    //Use this instead of the other one if your flail's projectile does not point in a direction (ball shaped)
                    //Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f;
                }
                else
                {
                    Projectile.rotation += Projectile.velocity.X * 0.1f;
                }
            Projectile.timeLeft = 2; //Makes sure the flail doesn't die (good when the flail is resting on the ground)
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2); //Add a delay so the player can't button mash the flail
            player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
            if (Projectile.Center.X < mountedCenter.X)
            {
                player.itemRotation += (float)Math.PI;
            }
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

            //Put your extra stuff here (shooting projectiles like flower pow)

        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[Projectile.owner];
            // This custom OnTileCollide code makes the projectile bounce off tiles at 1/5th the original speed, and plays sound and spawns dust if the projectile was going fast enough.
            bool shouldMakeSound = false;
            if (!Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, player.position, player.width, player.height) && Projectile.ai[0] != 6f) //if projectile isn't in line of sight or in falling on the ground mode then start desperate retraction
            {
                if (oldVelocity.X != Projectile.velocity.X)
                {
                    if (Math.Abs(oldVelocity.X) > 4f)
                    {
                        shouldMakeSound = true;
                    }

                    Projectile.position.X += Projectile.velocity.X;
                    Projectile.velocity.X = -oldVelocity.X * 0.05f;
                }

                if (oldVelocity.Y != Projectile.velocity.Y)
                {
                    if (Math.Abs(oldVelocity.Y) > 4f)
                    {
                        shouldMakeSound = true;
                    }

                    Projectile.position.Y += Projectile.velocity.Y;
                    Projectile.velocity.Y = -oldVelocity.Y * 0.05f;
                }
                tilecounter++;
            }
            else //if projectile is in line of sight, do normal bouncing
            {
                if (oldVelocity.X != Projectile.velocity.X)
                {
                    if (Math.Abs(oldVelocity.X) > 4f)
                    {
                        shouldMakeSound = true;
                    }

                    Projectile.position.X += Projectile.velocity.X;
                    Projectile.velocity.X = -oldVelocity.X * 0.2f;
                }

                if (oldVelocity.Y != Projectile.velocity.Y)
                {
                    if (Math.Abs(oldVelocity.Y) > 4f)
                    {
                        shouldMakeSound = true;
                    }

                    Projectile.position.Y += Projectile.velocity.Y;
                    Projectile.velocity.Y = -oldVelocity.Y * 0.2f;
                }
            }
            alreadyHitATile = true; //the projectile hit a tile, so set bool to true

            if (shouldMakeSound)
            {
                // if we should play the sound..
                Projectile.netUpdate = true;
                for (int k = 0; k < 8; k++) //emit dusts
                {
                    int dustint = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 4, 4, ModContent.DustType<Sparkle>());
                    Main.dust[dustint].noGravity = true;
                    Dust dust = Main.dust[dustint];
                    dust.velocity *= 1.5f;
                    dust = Main.dust[dustint];
                    dust.scale *= 1.5f;
                }
                // Play the sound
                SoundEngine.PlaySound(SoundID.Dig, (int)Projectile.position.X, (int)Projectile.position.Y);
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var player = Main.player[Projectile.owner];

            Vector2 mountedCenter = player.MountedCenter;
            Texture2D chainTexture = (Texture2D)ModContent.Request<Texture2D>(ChainTexturePath);

            var drawPosition = Projectile.Center;
            var remainingVectorToPlayer = mountedCenter - drawPosition;

            float rotation = remainingVectorToPlayer.ToRotation() - MathHelper.PiOver2;

            if (Projectile.alpha == 0)
            {
                int direction = -1;

                if (Projectile.Center.X < mountedCenter.X)
                    direction = 1;

                player.itemRotation = (float)Math.Atan2(remainingVectorToPlayer.Y * direction, remainingVectorToPlayer.X * direction);
            }

            // This while loop draws the chain texture from the projectile to the player, looping to draw the chain texture along the path
            while (true)
            {
                float length = remainingVectorToPlayer.Length();

                // Once the remaining length is small enough, we terminate the loop
                if (length < 25f || float.IsNaN(length))
                    break;

                // drawPosition is advanced along the vector back to the player by 16 pixels
                // 16 comes from the height of ExampleFlailProjectileChain.png and the spacing that we desired between links
                drawPosition += remainingVectorToPlayer * 16 / length;
                remainingVectorToPlayer = mountedCenter - drawPosition;

                // Finally, we draw the texture at the coordinates using the lighting information of the tile coordinates of the chain section
                Color color = Lighting.GetColor((int)drawPosition.X / 16, (int)(drawPosition.Y / 16f));
                Main.spriteBatch.Draw(chainTexture, drawPosition - Main.screenPosition, null, color, rotation, chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            if (Projectile.ai[0] == 1 && Projectile.ai[1] > 5 && !alreadyHitATile) //add a trail when moving forward, like most flails do (don't add trail if already hit a tile)
            {
                Vector2 drawOrigin = new Vector2(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Width * 0.5f, Projectile.height * 0.5f);
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, SpriteEffects.None, 0f);
                }
            }

            return true;
        }
    }
}