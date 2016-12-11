using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalNPC
	{
		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults(NPC npc)
		{
		}

		public virtual void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
		{
		}

		public virtual void ResetEffects(NPC npc)
		{
		}

		public virtual bool PreAI(NPC npc)
		{
			return true;
		}

		public virtual void AI(NPC npc)
		{
		}

		public virtual void PostAI(NPC npc)
		{
		}

		public virtual void FindFrame(NPC npc, int frameHeight)
		{
		}

		public virtual void HitEffect(NPC npc, int hitDirection, double damage)
		{
		}

		public virtual void UpdateLifeRegen(NPC npc, ref int damage)
		{
		}

		public virtual bool CheckActive(NPC npc)
		{
			return true;
		}

		public virtual bool CheckDead(NPC npc)
		{
			return true;
		}

		public virtual bool PreNPCLoot(NPC npc)
		{
			return true;
		}

		public virtual void NPCLoot(NPC npc)
		{
		}

		public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			return true;
		}

		public virtual void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
		{
		}

		public virtual bool? CanHitNPC(NPC npc, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item)
		{
			return null;
		}

		public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			return null;
		}

		public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
		}

		public virtual void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
		}

		public virtual bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			return true;
		}

		public virtual void BossHeadSlot(NPC npc, ref int index)
		{
		}

		public virtual void BossHeadRotation(NPC npc, ref float rotation)
		{
		}

		public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
		{
		}

		public virtual Color? GetAlpha(NPC npc, Color drawColor)
		{
			return null;
		}

		public virtual void DrawEffects(NPC npc, ref Color drawColor)
		{
		}

		public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			return true;
		}

		public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
		}

		public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
		{
			return null;
		}

		public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
		}

		public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY)
		{
		}

		public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
		}

		public virtual void SpawnNPC(int npc, int tileX, int tileY)
		{
		}

		public virtual void GetChat(NPC npc, ref string chat)
		{
		}

		public virtual void SetupShop(int type, Chest shop, ref int nextSlot)
		{
		}

		public virtual void SetupTravelShop(int[] shop, ref int nextSlot)
		{
		}

		public virtual void BuffTownNPC(ref float damageMult, ref int defense)
		{
		}

		public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
		{
		}

		public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
		{
		}

		public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
		{
		}

		public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset)
		{
		}

		public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
		{
		}

		public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
		{
		}

		public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
		{
		}

		public virtual void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness)
		{
		}

		public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset)
		{
		}
	}
}
