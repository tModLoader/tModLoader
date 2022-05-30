using System;
using Terraria.ModLoader;
// Blank

public class ModProjectileTest : ModProjectile
{
	public void IdentifierTest() {
		Console.Write(projectile);
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) { return true; }
}
