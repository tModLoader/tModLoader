using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	// This file showcases the concept of piercing.
	// This file also shows an animated projectile
	// This file also shows advanced drawing to center the drawn projectile correctly
	/*
	
	NPC.immune determines if an npc can be hit by a item or projectile owned by a particular player (it is an array, each slot corresponds to different players (whoAmI))
	NPC.immune is decremented towards 0 every update
	Melee items set NPC.immune to player.itemAnimation, which starts at item.useAnimation and decrements towards 0

	Projectiles, however, provide mechanisms for custom immunity.
	1. MaxPenetrate = 1: A projectile with penetrate set to 1 will hit regardless of the npc's immunity counters
		Ex: Wooden Arrow. 
	2. No code and penetrate > 1 or -1: npc.immune[owner] will be set to 10. 
		The NPC will be hit if not immune and will become immune to all damage for 10 ticks
		Ex: Unholy Arrow
	3. Override OnHitNPC: If not immune, when it hits it manually set an immune other than 10
		Ex: Arkhalis: Sets it to 5
		Ex: Sharknado Minion: Sets to 20
		Video: https://gfycat.com/DisloyalImprobableHoatzin Notice how Sharknado minion hits prevent Arhalis hits for a brief moment.
	4. projectile.usesIDStaticNPCImmunity and projectile.idStaticNPCHitCooldown: Specifies that a type of projectile has a shared immunity timer for each npc.
		Use this if you want other projectiles a chance to damage, but don't want the same projectile type to hit an npc rapidly.
		Ex: Ghastly Glaive is the only one who uses this.
	5. projectile.usesLocalNPCImmunity and projectile.localNPCHitCooldown: Specifies the projectile manages it's own immunity timers for each npc
		Use this if you want the multiple projectiles of the same type to have a chance to attack rapidly, but don't want a single projectile to hit rapidly. A -1 value prevents the same projectile from ever hitting the npc again.
		Ex: Lightning Aura sentries use this. (localNPCHitCooldown = 3, but other code controls how fast the projectile itself hits) 
			Overlapping Auras all have a chance to hit after each other even though they share the same ID.

	Try the above by uncommenting out the respective bits of code in the projectile below.
	*/

	internal class ExampleAnimatedPierce : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults() {
			projectile.width = 40;
			projectile.height = 40;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
			projectile.alpha = 255;
			projectile.penetrate = -1;

			//1: projectile.penetrate = 1; // Will hit even if npc is currently immune to player
			//2a: projectile.penetrate = -1; // Will hit and unless 3 is use, set 10 ticks of immunity
			//2b: projectile.penetrate = 3; // Same, but max 3 hits before dying
			//5: projectile.usesLocalNPCImmunity = true;
			//5a: projectile.localNPCHitCooldown = -1; // 1 hit per npc max
			//5b: projectile.localNPCHitCooldown = 20; // o
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			//3a: target.immune[projectile.owner] = 20;
			//3b: target.immune[projectile.owner] = 5;
		}

		public override Color? GetAlpha(Color lightColor) {
			//return Color.White;
			return new Color(255, 255, 255, 0) * (1f - (float)projectile.alpha / 255f);
		}

		public override void AI() {
			projectile.ai[0] += 1f;
			if (projectile.ai[0] > 50f) {
				// Fade out
				projectile.alpha += 25;
				if (projectile.alpha > 255) {
					projectile.alpha = 255;
				}
			}
			else {
				// Fade in
				projectile.alpha -= 25;
				if (projectile.alpha < 100) {
					projectile.alpha = 100;
				}
			}
			// Slow down
			projectile.velocity *= 0.98f;
			// Loop through the 4 animation frames, spending 5 ticks on each.
			if (++projectile.frameCounter >= 5) {
				projectile.frameCounter = 0;
				if (++projectile.frame >= 4) {
					projectile.frame = 0;
				}
			}
			// Kill this projectile after 1 second
			if (projectile.ai[0] >= 60f) {
				projectile.Kill();
			}
			projectile.direction = projectile.spriteDirection = projectile.velocity.X > 0f ? 1 : -1;
			projectile.rotation = projectile.velocity.ToRotation();
			if (projectile.velocity.Y > 16f) {
				projectile.velocity.Y = 16f;
			}
			// Since our sprite has an orientation, we need to adjust rotation to compensate for the draw flipping.
			if (projectile.spriteDirection == -1) {
				projectile.rotation += MathHelper.Pi;
			}
		}

		// Some advanced drawing because the texture image isn't centered or symetrical.
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			Texture2D texture = Main.projectileTexture[projectile.type];
			int frameHeight = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
			int startY = frameHeight * projectile.frame;
			Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
			Vector2 origin = sourceRectangle.Size() / 2f;
			origin.X = (float)(projectile.spriteDirection == 1 ? sourceRectangle.Width - 20 : 20);

			Color drawColor = projectile.GetAlpha(lightColor);
			Main.spriteBatch.Draw(texture,
				projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY),
				sourceRectangle, drawColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);

			return false;
		}
	}

	internal class ExamplePierce : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.NebulaBlaze;

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.NebulaBlaze);
			item.mana = 3;
			item.damage = 3;
			item.shoot = ProjectileType<ExampleAnimatedPierce>();
		}
	}
}
