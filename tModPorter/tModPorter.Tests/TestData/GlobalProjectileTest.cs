using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalProjectileTest : GlobalProjectile
{
	public override bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch) { return true; }

	public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) { return true; }

	public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) { /* Empty */ }
}
