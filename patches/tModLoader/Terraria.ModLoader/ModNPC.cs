using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public ModNPC()
		{
			npc = new NPC();
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
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
	}
}
