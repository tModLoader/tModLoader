using System;

namespace Terraria.ModLoader.Exceptions;

public class ValueNotTranslationKeyException : Exception
{
	public ValueNotTranslationKeyException(string message) : base(message)
	{
		HelpLink = "https://github.com/tModLoader/tModLoader/pull/3302";
	}
}