using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Projectiles;
 
namespace ExampleMod.Projectiles.Minions      
{
    public class ExampleOverheadMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 32; 
            projectile.height = 32;   
            projectile.hostile = false;  
            projectile.friendly = false; //false so the minion itself won't damage enemies on contact.
            projectile.ignoreWater = true; //does not get slowed in water.    
            projectile.penetrate = -1; 
            projectile.tileCollide = false; //does not collide with tiles.
            projectile.minion = true; //deals summon damage.
            projectile.alpha = 90; //slightly transparent.
            projectile.minionSlots = 1f; //takes up a minion slot.
        }
 
        public override void Kill(int timeLeft) //on projectile death
        {
 
            Main.PlaySound(2, projectile.Center, 8); //play a sound 
 
            for (int i = 0; i < 20; i++) 
            {
                //spawn dusts on death
                int dust = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 16, projectile.velocity.X * 1.2f, projectile.velocity.Y * 1.2f, 160, default(Color), 3.50f);   
                Main.dust[dust].noGravity = true; //dust is not affected by gravity.
                Main.dust[dust].velocity *= 2.5f;
            }
        }
 
        public override void AI()
        {       
            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active) {
				player.ClearBuff(ModContent.BuffType<Buffs.ExampleOverheadMinionBuff>()); //handling with the summon buff.
			}
			if (player.HasBuff(ModContent.BuffType<Buffs.ExampleOverheadMinionBuff>())) {
				projectile.timeLeft = 2;
			}
            projectile.Center = player.Center - new Vector2(0, 60); //projectile is always above your head.
            
            for (int i = 0; i < 200; i++) //this code below handles the aiming and shooting of the minion.
            {
                NPC target = Main.npc[i];
 
                
                float shootToX = target.position.X + (float)target.width * 0.5f - projectile.Center.X; 
                float shootToY = target.position.Y + (float)target.height * 0.5f - projectile.Center.Y;
                float distance = (float)System.Math.Sqrt((double)(shootToX * shootToX + shootToY * shootToY));
 
                
                if (distance < 400f && !target.friendly && target.active && target.CanBeChasedBy()) //range is 400 pixels. 
                {
                    if (projectile.ai[0] > 60f) //Fires every second. (60 ticks)
                    {
                        
                        distance = 1.6f / distance;
 
                        
                        shootToX *= distance * 3;
                        shootToY *= distance * 3;
                        int damage = projectile.damage;                  
                        
                        Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, shootToX, shootToY, mod.ProjectileType("ExampleOverheadMinionProjectile"), damage, 4f, Main.myPlayer, 0f, 0f); 
                        Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 28); //28 is the sound
                        projectile.ai[0] = 0f;
                    }
                }
            }
            projectile.ai[0] += 1f;
 
        }
    }
}
