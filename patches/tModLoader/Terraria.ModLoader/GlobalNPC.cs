using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public virtual bool PreNPCLoot(NPC npc)
		{
			return true;
		}

		public virtual void NPCLoot(NPC npc)
		{
		}

		public virtual bool CanHitPlayer(NPC npc, Player target)
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
	}
}
