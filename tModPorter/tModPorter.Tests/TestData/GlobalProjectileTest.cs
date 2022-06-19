using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalProjectileTest : GlobalProjectile
{
	public override bool CanDamage(Projectile projectile) { return false; }

	public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough) => true;

	public override bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch) { return true; }

	public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) { return true; }

	public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) { /* Empty */ }

	public override void DrawBehind(Projectile projectile, int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
		drawCacheProjsBehindNPCsAndTiles.Add(index);
		drawCacheProjsBehindNPCs.Add(index);
		drawCacheProjsBehindProjectiles.Add(index);
		drawCacheProjsOverWiresUI.Add(index);
	}
}
