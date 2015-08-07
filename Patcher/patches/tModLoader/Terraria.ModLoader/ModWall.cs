using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModWall
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

		public ushort Type
		{
			get;
			internal set;
		}

		internal string texture;
		public int soundType = 0;
		public int soundStyle = 1;
		public int dustType = 0;
		public int drop = 0;
		public Color? mapColor = null;
		public string mapName = "";

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults()
		{
		}

		public virtual bool KillSound(int i, int j)
		{
			return true;
		}

		public virtual void NumDust(int i, int j, bool fail, ref int num)
		{
		}

		public virtual bool CreateDust(int i, int j, ref int type)
		{
			type = dustType;
			return true;
		}

		public virtual bool Drop(int i, int j, ref int type)
		{
			type = drop;
			return true;
		}

		public virtual void KillWall(int i, int j, ref bool fail)
		{
		}

		public virtual Color? MapColor(int i, int j)
		{
			return mapColor;
		}

		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
		}

		public virtual void RandomUpdate(int i, int j)
		{
		}

		public virtual void AnimateWall(ref byte frame, ref byte frameCounter)
		{
		}

		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
		}
	}
}
