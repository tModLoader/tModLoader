using Microsoft.Xna.Framework;
using System;
using Terraria.Localization;

namespace Terraria.ModLoader;

public struct MapEntry
{
	internal Color color;
	internal LocalizedText name;
	internal Func<string, int, int, string> getName;

	internal MapEntry(Color color, LocalizedText name = null, Func<string, int, int, string> getName = null)
	{
		this.color = color;
		this.name = name == null ? LocalizedText.Empty : name;
		
		this.getName = getName == null ? sameName : getName;
	}

	private static string sameName(string name, int x, int y)
	{
		return name;
	}
}
