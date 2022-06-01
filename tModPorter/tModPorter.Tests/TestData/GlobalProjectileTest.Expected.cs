using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalProjectileTest : GlobalProjectile
{
	public override bool PreDrawExtras(Projectile projectile) { return true; }

	public override bool PreDraw(Projectile projectile, ref Color lightColor) { return true; }

	public override void PostDraw(Projectile projectile, Color lightColor) { /* Empty */ }
}
