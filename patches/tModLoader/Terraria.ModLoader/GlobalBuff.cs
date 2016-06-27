using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

		public virtual bool ReApply(int type, Player player, int time, int buffIndex)
		{
			return false;
		}

		public virtual bool ReApply(int type, NPC npc, int time, int buffIndex)
		{
			return false;
		}

		public virtual void ModifyBuffTip(int type, ref string tip, ref int rare)
		{
		}

		public virtual void CustomBuffTipSize(string buffTip, List<Vector2> sizes)
		{
		}

		public virtual void DrawCustomBuffTip(string buffTip, SpriteBatch spriteBatch, int originX, int originY)
		{
		}
	}
}
