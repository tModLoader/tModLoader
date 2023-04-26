using System;

namespace Terraria.ModLoader.Exceptions;

public class ValueNotTranslationKeyException : Exception
{
	internal string additional;
	internal bool errorOnType;
	internal bool handled;

	public ValueNotTranslationKeyException(string message, bool errorOnType = false) : base(message)
	{
		this.errorOnType = errorOnType;
	}

	public override string Message => $"{base.Message}\n{additional}";

	public override string ToString() => $"{base.ToString()}\n{additional}";
}