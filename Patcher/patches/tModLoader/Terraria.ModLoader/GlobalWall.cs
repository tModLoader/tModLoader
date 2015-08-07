using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalWall
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

		public virtual void SetDefaults()
		{
		}

		public virtual bool KillSound(int i, int j, int type)
		{
			return true;
		}

		public virtual void NumDust(int i, int j, int type, bool fail, ref int num)
		{
		}

		public virtual bool CreateDust(int i, int j, int type, ref int dustType)
		{
			return true;
		}

		public virtual bool Drop(int i, int j, int type, ref int dropType)
		{
			return true;
		}

		public virtual void KillWall(int i, int j, int type, ref bool fail)
		{
		}

		public virtual void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
		}

		public virtual void RandomUpdate(int i, int j, int type)
		{
		}

		public virtual bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
		}
	}
}
