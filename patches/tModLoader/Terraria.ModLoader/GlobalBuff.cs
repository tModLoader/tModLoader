using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader
{
	public class GlobalBuff
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

		public virtual void Update(int type, Player player, ref int buffIndex)
		{
		}

		public virtual void Update(int type, NPC npc, ref int buffIndex)
		{
		}
	}
}
