using System;
using System.Collections.Generic;
using Terraria;
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

	public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */ { return false; }

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) { return true; }

	public override bool PreDrawExtras() { return true; }

	public override bool PreDraw(ref Color lightColor) { return true; }

	public override void PostDraw(Color lightColor) { /* Empty */ }

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
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
	public override bool? SingleGrappleHook(Player player)/* tModPorter Note: Removed. In SetStaticDefaults, use ProjectileID.Sets.SingleGrappleHook[Type] = true if you previously had this method return true */ { return null; }
#endif

#if COMPILE_ERROR // duplicate method
	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }
#endif
	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }
	public override void OnHitPlayer(Player target, Player.HurtInfo info) { }

#if COMPILE_ERROR
	public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)/* tModPorter Note: Removed. Use ModifyHitPlayer and check modifiers.PvP */ { }
	public override void OnHitPvp(Player target, int damage, bool crit)/* tModPorter Note: Removed. Use OnHitPlayer and check info.PvP */ { }
#endif
	public override void OnKill(int timeLeft) { }

#if COMPILE_ERROR
	public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)/* tModPorter Note: Removed. Use ModItem.ModifyFishingLine */ { }
#endif
}
