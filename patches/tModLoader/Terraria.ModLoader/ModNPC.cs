using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModNPC
	{
		//add modNPC property to Terraria.NPC (internal set)
		//set modNPC to null at beginning of Terraria.NPC.SetDefaults
		public NPC npc
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}

		internal string texture;
		public int aiType = 0;
		public int animationType = 0;
		public int bossBag = -1;

		public ModNPC()
		{
			npc = new NPC();
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void AutoloadHead(ref string headTexture, ref string bossHeadTexture)
		{
		}

		internal void SetupNPC(NPC npc)
		{
			ModNPC newNPC = (ModNPC)Activator.CreateInstance(GetType());
			newNPC.npc = npc;
			npc.modNPC = newNPC;
			newNPC.mod = mod;
			newNPC.SetDefaults();
		}

		public virtual void SetDefaults()
		{
		}

		public virtual bool PreAI()
		{
			return true;
		}

		public virtual void AI()
		{
		}

		public virtual void PostAI()
		{
		}

		public virtual void FindFrame(int frameHeight)
		{
		}

		public virtual void HitEffect(int hitDirection, double damage)
		{
		}

		public virtual bool PreNPCLoot()
		{
			return true;
		}

		public virtual void NPCLoot()
		{
		}

		public virtual void BossLoot(ref string name, ref int potionType)
		{
		}

		public virtual bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return true;
		}

		public virtual void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPlayer(Player target, int damage, bool crit)
		{
		}

		public virtual bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByItem(Player player, Item item)
		{
			return null;
		}

		public virtual void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByProjectile(Projectile projectile)
		{
			return null;
		}

		public virtual void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
		}

		public virtual bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			return true;
		}

		public virtual void BossHeadSlot(ref int index)
		{
		}

		public virtual void BossHeadRotation(ref float rotation)
		{
		}

		public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
		{
		}
	}
}
