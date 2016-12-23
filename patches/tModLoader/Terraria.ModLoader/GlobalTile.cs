using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalTile
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

		public void AddToArray(ref int[] array, int type)
		{
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = type;
		}

		public void AddModTree(int soilType, ModTree tree)
		{
			TileLoader.trees[soilType] = tree;
		}

		public void AddModPalmTree(int soilType, ModPalmTree palmTree)
		{
			TileLoader.palmTrees[soilType] = palmTree;
		}

		public void AddModCactus(int soilType, ModCactus cactus)
		{
			TileLoader.cacti[soilType] = cactus;
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

		public virtual void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
		{
		}

		public virtual bool Drop(int i, int j, int type)
		{
			return true;
		}

		public virtual bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			return true;
		}

		public virtual void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
		}

		public virtual bool CanExplode(int i, int j, int type)
		{
			return true;
		}

		public virtual void NearbyEffects(int i, int j, int type, bool closer)
		{
		}

		public virtual void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
		}

		public virtual bool Dangersense(int i, int j, int type, Player player)
		{
			return false;
		}

		public virtual void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects)
		{
		}

		public virtual void AnimateTile()
		{
		}

		public virtual bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor)
		{
		}

		public virtual void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
		}

		public virtual void RandomUpdate(int i, int j, int type)
		{
		}

		public virtual bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			return true;
		}

		public virtual bool CanPlace(int i, int j, int type)
		{
			return true;
		}

		public virtual int[] AdjTiles(int type)
		{
			return new int[0];
		}

		public virtual void RightClick(int i, int j, int type)
		{
		}

		public virtual void MouseOver(int i, int j, int type)
		{
		}

		public virtual void MouseOverFar(int i, int j, int type)
		{
		}

		public virtual bool AutoSelect(int i, int j, int type, Item item)
		{
			return false;
		}

		public virtual bool PreHitWire(int i, int j, int type)
		{
			return true;
		}

		public virtual void HitWire(int i, int j, int type)
		{
		}

		public virtual bool Slope(int i, int j, int type)
		{
			return true;
		}

		public virtual void ChangeWaterfallStyle(int type, ref int style)
		{
		}

		public virtual int SaplingGrowthType(int type, ref int style)
		{
			return -1;
		}
	}
}
