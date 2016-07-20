using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Liquid;

namespace Terraria.ModLoader
{
	public abstract class ModWaterStyle
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
		internal string blockTexture;

		public virtual bool Autoload(ref string name, ref string texture, ref string blockTexture)
		{
			return mod.Properties.Autoload;
		}

		public virtual bool ChooseWaterStyle()
		{
			return false;
		}

		public abstract int ChooseWaterfallStyle();

		public abstract int GetSplashDust();

		public abstract int GetDropletGore();

		public virtual void LightColorMultiplier(ref float r, ref float g, ref float b)
		{
			r = 0.88f;
			g = 0.96f;
			b = 1.015f;
		}

		public virtual Color BiomeHairColor()
		{
			return new Color(28, 216, 94);
		}
	}

	public class ModWaterfallStyle
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

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void AddLight(int i, int j)
		{
		}

		public virtual void ColorMultiplier(ref float r, ref float g, ref float b, float a)
		{
		}
	}
}
