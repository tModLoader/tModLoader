using System;
using System.IO;

namespace Terraria.ModLoader.Exceptions;

public class CustomModDataException : IOException
{
	public readonly string modName;

	public CustomModDataException(Mod mod, string message, Exception inner) : base(message, inner)
	{
		modName = mod.Name ?? "Terraria";
	}
}
