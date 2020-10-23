using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleBullet : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Bullet"); //The English name of the projectile
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5; //The length of old position to be recorded
			ProjectileID.Sets.TrailingMode[projectile.type] = 0; //The recording mode
		}

		public override void SetDefaults() {
			projectile.width = 8; //The width of projectile hitbox
			projectile.height = 8; //The height of projectile hitbox
			projectile.aiStyle = 1; //The ai style of the projectile, please reference the source code of Terraria
			projectile.friendly = true; //Can the projectile deal damage to enemies?
			projectile.hostile = false; //Can the projectile deal damage to the player?
			projectile.DamageType = DamageClass.Ranged; //Is the projectile shoot by a ranged weapon?
			projectile.penetrate = 5; //How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
			projectile.timeLeft = 600; //The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
			projectile.alpha = 255; //The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
			projectile.light = 0.5f; //How much light emit around the projectile
			projectile.ignoreWater = true; //Does the projectile's speed be influenced by water?
			projectile.tileCollide = true; //Can the projectile collide with tiles?
			projectile.extraUpdates = 1; //Set to above 0 if you want the projectile to update multiple time in a frame
			aiType = ProjectileID.Bullet; //Act exactly like default Bullet
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			//If collide with tile, reduce the penetrate.
			//So the projectile can reflect at most 5 times
			projectile.penetrate--;
			if (projectile.penetrate <= 0) {
				projectile.Kill();
			}
			else {
				Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, projectile.position);

				// If the projectile hits the left or right side of the tile, reverse the X velocity
				if (Math.Abs(projectile.velocity.X - oldVelocity.X) > float.Epsilon) {
					projectile.velocity.X = -oldVelocity.X;
				}

				// If the projectile hits the top or bottom side of the tile, reverse the Y velocity
				if (Math.Abs(projectile.velocity.Y - oldVelocity.Y) > float.Epsilon) {
					projectile.velocity.Y = -oldVelocity.Y;
				}
			}

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Main.instance.LoadProjectile(projectile.type);
			Texture2D texture = TextureAssets.Projectile[projectile.type].Value;

			//Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, projectile.height * 0.5f);
			for (int k = 0; k < projectile.oldPos.Length; k++) {
				Vector2 drawPos = (projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, projectile.gfxOffY);
				Color color = projectile.GetAlpha(lightColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
				spriteBatch.Draw(texture, drawPos, null, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);
			}

			return true;
		}

		public override void Kill(int timeLeft) {
			// This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, projectile.position);
		}
	}
}