using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override bool PreNPCLoot(NPC npc) { return true; /* Empty */ }

	public override void NPCLoot(NPC npc) { /* Empty */ }

	public override bool SpecialNPCLoot(NPC npc) { return true; /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
	}

	public override bool? CanHitNPC(NPC npc, NPC target) {
		return null;
	}

	public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
	{
	}

	public override void SetupShop(int type, Chest shop, ref int nextSlot) { /* Empty */ }

	public override void HitEffect(NPC npc, int hitDirection, double damage) { }
	public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit) { }
	public override void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit) { }
	public override void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit) { }
	public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) { }
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) { }
	public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
		return false;
	}
	public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref float damageMultiplier, ref Rectangle npcHitbox) => false;
	public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset) { }
	public override void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness) {
		closeness = 20;
	}
}