using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalProjectileTest : GlobalProjectile
{
	public override bool? CanDamage(Projectile projectile)/* tModPorter Suggestion: Return null instead of true */ { return false; }

	public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

	public override bool PreDrawExtras(Projectile projectile) { return true; }

	public override bool PreDraw(Projectile projectile, ref Color lightColor) { return true; }

	public override void PostDraw(Projectile projectile, Color lightColor) { /* Empty */ }

	public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		// not-yet-implemented
		behindNPCsAndTiles.Add(index);
		behindNPCs.Add(index);
		behindProjectiles.Add(index);
		overWiresUI.Add(index);
		// instead-expect
#if COMPILE_ERROR
		drawCacheProjsBehindNPCsAndTiles.Add(index);
		drawCacheProjsBehindNPCs.Add(index);
		drawCacheProjsBehindProjectiles.Add(index);
		drawCacheProjsOverWiresUI.Add(index);
#endif
	}

#if COMPILE_ERROR
	public override bool? SingleGrappleHook(int type, Player player)/* tModPorter Note: Removed. In SetStaticDefaults, use ProjectileID.Sets.SingleGrappleHook[type] = true if you previously had this method return true */ { return null; }
#endif

#if COMPILE_ERROR // duplicate method
	public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }
#endif
	public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }
	public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) { }
	public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) { }

#if COMPILE_ERROR
	public override void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)/* tModPorter Note: Removed. Use ModifyHitPlayer and check modifiers.PvP */ { }
	public override void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)/* tModPorter Note: Removed. Use OnHitPlayer and check info.PvP */ { }
#endif
	public override void OnKill(Projectile projectile, int timeLeft) { }
}