using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override bool PreKill(NPC npc) { return true; /* Empty */ }

	public override void OnKill(NPC npc) { /* Empty */ }

	public override bool SpecialOnKill(NPC npc) { return true; /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
		// instead-expect
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
		// instead-expect
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
	}

#if COMPILE_ERROR
	public override bool CanHitNPC(NPC npc, NPC target)/* tModPorter Suggestion: Return true instead of null */ {
		return null;
	}
#endif

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
	{
	}

	public override void ModifyShop(NPCShop shop) { /* Empty */ }

	public override void HitEffect(NPC npc, NPC.HitInfo hit) { }
	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) { }
	public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) { }
	public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) { }
	public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit) { }
	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) { }
	public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) { }
	public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
#if COMPILE_ERROR
		return false;
#endif
	}
	public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => false;
}