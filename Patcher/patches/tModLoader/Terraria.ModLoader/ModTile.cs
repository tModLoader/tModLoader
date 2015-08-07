using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class ModTile
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
		public int animationFrameHeight = 0;
		public Color? mapColor = null;
		public string mapName = "";
		public float mineResist = 1f;
		public int minPick = 0;
		public int[] adjTiles = new int[0];

		public void AddToArray(ref int[] array)
		{
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = Type;
		}

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

		public virtual void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
		{
		}

		public virtual bool Drop(int i, int j)
		{
			return true;
		}

		public virtual void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
		}

		public virtual void KillMultiTile(int i, int j, int frameX, int frameY)
		{
		}

		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
		}

		public virtual void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
		}

		public virtual void AnimateTile(ref int frame, ref int frameCounter)
		{
		}

		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
		}

		public virtual Color? MapColor(int i, int j)
		{
			return mapColor;
		}

		public virtual string MapName(int frameX, int frameY)
		{
			return mapName;
		}

		public virtual void RandomUpdate(int i, int j)
		{
		}

		public virtual bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			return true;
		}

		public virtual bool CanPlace(int i, int j)
		{
			return true;
		}

		public virtual void RightClick(int i, int j)
		{
		}

		public virtual void MouseOver(int i, int j)
		{
		}

		public virtual void HitWire(int i, int j)
		{
		}

		public virtual bool Slope(int i, int j)
		{
			return true;
		}
	}
}
