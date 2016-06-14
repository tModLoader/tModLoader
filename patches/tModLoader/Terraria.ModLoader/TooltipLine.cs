using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	public class TooltipLine
	{
		public readonly string mod;
		public readonly string Name;
		public string text;
		public bool isModifier = false;
		public bool isModifierBad = false;
		internal bool oneDropLogo = false;
		public Color? overrideColor = null;

		public TooltipLine(Mod mod, string name, string text)
		{
			this.mod = mod.Name;
			this.Name = name;
			this.text = text;
		}

		internal TooltipLine(string name, string text)
		{
			this.mod = "Terraria";
			this.Name = name;
			this.text = text;
		}
	}
}
