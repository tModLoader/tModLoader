using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	internal struct MapEntry
	{
		internal Color color;
		internal string name;
		internal Func<string, int, int, string> getName;

		internal MapEntry(Color color, string name = "")
		{
			this.color = color;
			this.name = name;
			this.getName = sameName;
		}

		internal MapEntry(Color color, string name, Func<string, int, int, string> getName)
		{
			this.color = color;
			this.name = name;
			this.getName = getName;
		}

		private static string sameName(string name, int x, int y)
		{
			return name;
		}
	}
}
