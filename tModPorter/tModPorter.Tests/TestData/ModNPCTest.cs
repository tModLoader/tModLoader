using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(npc);
		Console.Write(aiType);
		Console.Write(animationType);
		Console.Write(music);
		Console.Write(musicPriority);
		Console.Write(drawOffsetY);
		Console.Write(banner);
		Console.Write(bannerItem);
		Console.Write(bossBag);
	}

	public override bool PreNPCLoot() { return true; /*empty*/ }

	public override void NPCLoot() { /*empty*/ }

	public override bool SpecialNPCLoot() { return true; /* Empty */ }

	public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
	}

	public override bool CanTownNPCSpawn(int numTownNPCs, int money) => false;

	public override string[] AltTextures => new string[0];

	public override string TownNPCName() { return "Name"; }

	public override bool? CanHitNPC(NPC target) {
		return null;
	}

	public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
	{
	}

	public override void OnChatButtonClicked(bool firstButton, ref bool shop) { /* Empty */ }
	public override void SetupShop(Chest shop, ref int nextSlot) { }

	public override void HitEffect(int hitDirection, double damage) { }
	public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) { }
	public override void OnHitPlayer(Player target, int damage, bool crit) { }
	public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit) { }
	public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit) { }
	public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) { }
	public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) { }
	public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
		return false;
	}
	public void HitMemberRename(NPC npc) {
		var hit = npc.SimpleStrike(0, 0);
		hit.KnockBack = 2;
	}
	public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref float damageMultiplier, ref Rectangle npcHitbox) => false;
	public override void DrawTownAttackSwing(ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset) { }
	public override void DrawTownAttackGun(ref float scale, ref int item, ref int closeness) {
		closeness = 10;
	}

	public override void SetStaticDefaults() {
		NPCID.Sets.DebuffImmunitySets.Add(Type, new NPCDebuffImmunityData {
			SpecificallyImmuneTo = new int[] {
				BuffID.Poisoned
			}
		});

		NPCID.Sets.ShimmerImmunity[Type] = true;
	}
}