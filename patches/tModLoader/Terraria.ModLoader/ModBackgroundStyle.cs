using System;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public abstract class ModUgBgStyle
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

		public int Slot
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual bool ChooseBgStyle()
		{
			return false;
		}

		public abstract void FillTextureArray(int[] textureSlots);
	}

	public abstract class ModSurfaceBgStyle
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

		public int Slot
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual bool ChooseBgStyle()
		{
			return false;
		}

		public abstract void ModifyFarFades(float[] fades, float transitionSpeed);

		public virtual int ChooseFarTexture()
		{
			return -1;
		}

		public virtual int ChooseMiddleTexture()
		{
			return -1;
		}

		public virtual bool PreDrawCloseBackground(SpriteBatch spriteBatch)
		{
			return true;
		}

		public virtual int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
		{
			return -1;
		}
	}

	public class GlobalBgStyle
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

		public virtual void ChooseUgBgStyle(ref int style)
		{
		}

		public virtual void ChooseSurfaceBgStyle(ref int style)
		{
		}

		public virtual void FillUgTextureArray(int style, int[] textureSlots)
		{
		}

		public virtual void ModifyFarSurfaceFades(int style, float[] fades, float transitionSpeed)
		{
		}
	}
}