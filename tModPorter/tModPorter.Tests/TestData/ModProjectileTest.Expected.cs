using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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
#if COMPILE_ERROR
		Console.Write(DrawHeldProjInFrontOfHeldItemAndArms/* tModPorter Note: Removed. Replace with Projectile.drawLayer = ProjectileDrawLayerID.HeldProjOverHand; */);
#endif
	}

	public override void SetStaticDefaults()
	{
#if COMPILE_ERROR
		ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY/* tModPorter Note: Removed. AI() should use master.RotatedRelativePoint(master.MountedCenter + ...) to position held projectiles */[Type] = true;
		ProjectileID.Sets.DontAttachHideToAlpha/* tModPorter Note: Removed. Now true by default. See Projectile.usesOwnerLight and Projectile.drawLayer for more details. */[Type] = true;
#endif
	}

	public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */ { return false; }

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) { return true; }

	public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ { return true; }

	public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ { return true; }

	public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ { /* Empty */ }

#if COMPILE_ERROR
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)/* tModPorter Note: Removed. Set Projectile.drawLayer instead */ {
		// not-yet-implemented
		behindNPCsAndTiles.Add(index);
		behindNPCs.Add(index);
		behindProjectiles.Add(index);
		overWiresUI.Add(index);
		// instead-expect
		drawCacheProjsBehindNPCsAndTiles.Add(index);
		drawCacheProjsBehindNPCs.Add(index);
		drawCacheProjsBehindProjectiles.Add(index);
		drawCacheProjsOverWiresUI.Add(index);
	}
#endif

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
