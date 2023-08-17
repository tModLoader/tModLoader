using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(NPC);
		Console.Write(AIType);
		Console.Write(AnimationType);
		Console.Write(Music);
		Console.Write(SceneEffectPriority);
		Console.Write(DrawOffsetY);
		Console.Write(Banner);
		Console.Write(BannerItem);
#if COMPILE_ERROR
		Console.Write(bossBag/* tModPorter Note: Removed. Spawn the treasure bag alongside other loot via npcLoot.Add(ItemDropRule.BossBag(type)) */);
#endif
	}

	public override bool PreKill() { return true; /*empty*/ }

	public override void OnKill() { /*empty*/ }

	public override bool SpecialOnKill() { return true; /* Empty */ }

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		Vector2 screen = screenPos - Vector2.One * 6f;
		// instead-expect
#if COMPILE_ERROR
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
#endif
		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		Vector2 screen = screenPos - Vector2.One * 6f;
		// instead-expect
#if COMPILE_ERROR
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
#endif
	}

	public override bool CanTownNPCSpawn(int numTownNPCs)/* tModPorter Suggestion: Copy the implementation of NPC.SpawnAllowed_Merchant in vanilla if you to count money, and be sure to set a flag when unlocked, so you don't count every tick. */ => false;

#if COMPILE_ERROR
	// not-yet-implemented
	public override string[] AltTextures/* tModPorter Suggestion: Create a ITownNPCProfile, in its GetTextureNPCShouldUse, check for npc.altTexture to return the texture you want. Then, use TownNPCProfile hook to return an instance of that ITownNPCProfile */ => new string[0];
	// instead-expect
	public override string[] AltTextures => new string[0];

	public override List<string> SetNPCNameList()/* tModPorter Suggestion: Return a list of names */ { return "Name"; }

	public override bool CanHitNPC(NPC target)/* tModPorter Suggestion: Return true instead of null */ {
		return null;
	}
#endif

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
	{
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName) { /* Empty */ }
	public override void ModifyActiveShop(string shopName, Item[] items) { }

	public override void HitEffect(NPC.HitInfo hit) { }
	public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }
	public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) { }
	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }
	public override void OnHitNPC(NPC target, NPC.HitInfo hit) { }
	public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }
	public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }
	public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
#if COMPILE_ERROR
		return false;
#endif
	}
	public void HitMemberRename(NPC npc) {
		var hit = npc.CalculateHitInfo(0, 0);
		hit.Knockback = 2;
	}
	public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => false;
	public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }
	public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)/* tModPorter Note: closeness is now horizontalHoldoutOffset, use 'horizontalHoldoutOffset = Main.DrawPlayerItemPos(1f, itemtype) - originalClosenessValue' to adjust to the change. See docs for how to use hook with an item type. */ {
#if COMPILE_ERROR
		closeness = 10;
#endif
	}

	public override void SetStaticDefaults() {
#if COMPILE_ERROR
		NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, new NPCDebuffImmunityData {
			SpecificallyImmuneTo = new int[] {
				BuffID.Poisoned
			}
		});
#endif

		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
	}
}