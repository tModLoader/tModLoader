using System;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public abstract class ModTree
	{
		public virtual int CreateDust()
		{
			return 7;
		}

		public virtual int GrowthFXGore()
		{
			return -1;
		}

		public virtual bool CanDropAcorn()
		{
			return true;
		}

		public abstract int DropWood();

		public abstract Texture2D GetTexture();

		public abstract Texture2D GetTopTextures();

		public abstract Texture2D GetBranchTextures();
	}

	public abstract class ModPalmTree
	{
		public virtual int CreateDust()
		{
			return 215;
		}

		public virtual int GrowthFXGore()
		{
			return -1;
		}

		public abstract int DropWood();

		public abstract Texture2D GetTexture();

		public abstract Texture2D GetTopTextures();
	}

	public abstract class ModCactus
	{
		public abstract Texture2D GetTexture();
	}
}
