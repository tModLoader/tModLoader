using System;

namespace Terraria.ModLoader.Exceptions;

class ResourceLoadException : Exception
{
	public ResourceLoadException(string message, Exception inner = null)
		: base(message, inner)
	{
	}
}
