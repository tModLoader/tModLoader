using System;

namespace Terraria.ModLoader.Exceptions;

public class ValueNotTranslationKeyException : Exception
{
	internal string additional;

	public ValueNotTranslationKeyException(string message) : base(message)
	{
	}

	public override string Message => $"{base.Message}\n{additional}";

	public override string ToString() => $"{base.ToString()}\n{additional}";
}