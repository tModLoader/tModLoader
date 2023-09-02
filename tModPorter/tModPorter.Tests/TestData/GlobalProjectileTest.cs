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

	public override bool? SingleGrappleHook(int type, Player player) { return null; }

	public override void ModifyDamageScaling(Projectile projectile, ref float damageScale) { }
	public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit) { }

	public override void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvp(Projectile projectile, Player target, int damage, bool crit) { }
	public override void Kill(Projectile projectile, int timeLeft) { }
}