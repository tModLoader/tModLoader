using System;

namespace Terraria.ModLoader.Exceptions;

public class ValueNotTranslationKeyException : Exception
{
	internal bool handled;

	public ValueNotTranslationKeyException(string message) : base(message)
	{
	}
}