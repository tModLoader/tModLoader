using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	// This projectiles merely flies towards a position and then dies. We use it to signify kill points for TEScoreBoard.
	internal class ScorePoint : ModProjectile
	{
		public override void SetDefaults() {
			projectile.width = 8;
			projectile.height = 8;
			projectile.hostile = true;
			projectile.alpha = 255;
			projectile.ignoreWater = true;
			projectile.timeLeft = 3600;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			projectile.extraUpdates = 2;
		}

		public override void AI() {
			// Since projectiles have 2 ai slots, and I don't want to do manual syncing of an extra variable, here I use the HalfVector2 and ReinterpretCast.FloatAsUInt to get a Vector2 from 1 float variable instead of 2 like normal.
			Vector2 target = new HalfVector2() { PackedValue = ReLogic.Utilities.ReinterpretCast.FloatAsUInt(projectile.ai[0]) }.ToVector2();

			Rectangle targetRectangle = new Rectangle((int)target.X - 4, (int)target.Y - 4, 8, 8);
			if (projectile.Hitbox.Intersects(targetRectangle)) {
				projectile.Kill();
				return;
			}
			Vector2 targetDirection = new Vector2(target.X, target.Y) - projectile.Center;
			projectile.velocity = Vector2.Normalize(targetDirection) * 5f;
			// Using the player's index, which we passed into ai[1], we can differentiate kills by assigning a hue to the dust we spawn
			float hue = (int)projectile.ai[1] % 6 / 6f;
			Dust.QuickDust(projectile.Center, Main.hslToRgb(hue, 1f, 0.5f));
		}
	}
}
