using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModProjectileTest : ModProjectile
{
	public void IdentifierTest() {
		Console.Write(Projectile);
		Console.Write(AIType);
		Console.Write(CooldownSlot);
		Console.Write(DrawOffsetX);
		Console.Write(DrawOriginOffsetY);
		Console.Write(DrawOriginOffsetX);
		Console.Write(DrawHeldProjInFrontOfHeldItemAndArms);
	}

	public override bool? CanDamage()/* Suggestion: Return null instead of false */ { return false; }

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) { return true; }

	public override bool PreDrawExtras() { return true; }

	public override bool PreDraw(ref Color lightColor) { return true; }

	public override void PostDraw(Color lightColor) { /* Empty */ }
}
