using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalProjectileTest : GlobalProjectile
{
	public override bool? CanDamage(Projectile projectile)/* tModPorter Suggestion: Return null instead of false */ { return false; }

	public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

	public override bool PreDrawExtras(Projectile projectile) { return true; }

	public override bool PreDraw(Projectile projectile, ref Color lightColor) { return true; }

	public override void PostDraw(Projectile projectile, Color lightColor) { /* Empty */ }
}
