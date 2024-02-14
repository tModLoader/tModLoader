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

	public override bool? SingleGrappleHook(Player player) { return null; }

	public override void ModifyDamageScaling(ref float damageScale) { }
	public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) { }
	public override void OnHitPlayer(Player target, int damage, bool crit) { }

	public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvp(Player target, int damage, bool crit) { }
	public override void Kill(int timeLeft) { }

	public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor) { }
}