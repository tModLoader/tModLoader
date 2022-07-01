using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModProjectileTest : ModProjectile
{
	public void IdentifierTest() {
		Console.Write(projectile);
		Console.Write(aiType);
		Console.Write(cooldownSlot);
		Console.Write(drawOffsetX);
		Console.Write(drawOriginOffsetY);
		Console.Write(drawOriginOffsetX);
		Console.Write(drawHeldProjInFrontOfHeldItemAndArms);
	}

	public override bool CanDamage() { return false; }

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) { return true; }

	public override bool PreDrawExtras(SpriteBatch spriteBatch) { return true; }

	public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) { return true; }

	public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) { /* Empty */ }

	public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
		drawCacheProjsBehindNPCsAndTiles.Add(index);
		drawCacheProjsBehindNPCs.Add(index);
		drawCacheProjsBehindProjectiles.Add(index);
		drawCacheProjsOverWiresUI.Add(index);
	}
}
