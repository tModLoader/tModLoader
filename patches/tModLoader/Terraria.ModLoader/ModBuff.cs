using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModBuff
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

		public int Type
		{
			get;
			internal set;
		}

		internal string texture;
		public bool longerExpertDebuff = false;
		public bool canBeCleared = true;

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults()
		{
		}

		public virtual void Update(Player player, ref int buffIndex)
		{
		}

		public virtual void Update(NPC npc, ref int buffIndex)
		{
		}

		public virtual bool ReApply(Player player, int time, int buffIndex)
		{
			return false;
		}

		public virtual bool ReApply(NPC npc, int time, int buffIndex)
		{
			return false;
		}

        public virtual void ModifyBuffTip(ref string tip, ref int rare)
        {
        }
	}
}
