using System;
using System.Collections.Generic;
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
		public float mineResist = 1f;
		public int minPick = 0;
		public bool disableSmartCursor = false;
		public int[] adjTiles = new int[0];
		public int closeDoorID = -1;
		public int openDoorID = -1;
		public string chest = "";
		public int chestDrop = 0;
		public string dresser = "";
		public int dresserDrop = 0;
		public bool bed = false;
		public bool torch = false;
		public bool sapling = false;

		public void AddToArray(ref int[] array)
		{
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = Type;
		}

		public void AddMapEntry(Color color, string name = "")
		{
			if (!MapLoader.initialized)
			{
				MapEntry entry = new MapEntry(color, name);
				if (!MapLoader.tileEntries.Keys.Contains(Type))
				{
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		public void AddMapEntry(Color color, string name, Func<string, int, int, string> nameFunc)
		{
			if (!MapLoader.initialized)
			{
				MapEntry entry = new MapEntry(color, name, nameFunc);
				if (!MapLoader.tileEntries.Keys.Contains(Type))
				{
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		public void SetModTree(ModTree tree)
		{
			TileLoader.trees[Type] = tree;
		}

		public void SetModPalmTree(ModPalmTree palmTree)
		{
			TileLoader.palmTrees[Type] = palmTree;
		}

		public void SetModCactus(ModCactus cactus)
		{
			TileLoader.cacti[Type] = cactus;
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void SetDefaults()
		{
		}

		public virtual void PostSetDefaults()
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

		public virtual bool CanKillTile(int i, int j, ref bool blockDamaged)
		{
			return true;
		}

		public virtual void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
		}

		public virtual void KillMultiTile(int i, int j, int frameX, int frameY)
		{
		}

		public virtual bool CanExplode(int i, int j)
		{
			return true;
		}

		public virtual void NearbyEffects(int i, int j, bool closer)
		{
		}

		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
		}

		public virtual bool Dangersense(int i, int j, Player player)
		{
			return false;
		}

		public virtual void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
		}

		public virtual void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
		{
		}

		public virtual void AnimateTile(ref int frame, ref int frameCounter)
		{
		}

		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor)
		{
		}

		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
		}

		public virtual ushort GetMapOption(int i, int j)
		{
			return 0;
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

		public virtual void MouseOverFar(int i, int j)
		{
		}

		public virtual bool AutoSelect(int i, int j, Item item)
		{
			return false;
		}

		public virtual void HitWire(int i, int j)
		{
		}

		public virtual bool Slope(int i, int j)
		{
			return true;
		}

		public virtual bool HasWalkDust()
		{
			return false;
		}

		public virtual void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
		{
		}

		public virtual void ChangeWaterfallStyle(ref int style)
		{
		}

		public virtual int SaplingGrowthType(ref int style)
		{
			return -1;
		}
	}
}
