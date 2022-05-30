using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // Vector2, Color
// Blank

public class ModProjectileTest : ModProjectile
{
	public void IdentifierTest() {
		Console.Write(Projectile);
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) { return true; }

	public override bool PreDraw(ref Color lightColor) { return true; }

	public override void PostDraw(Color lightColor) { /* Empty */ }
}
