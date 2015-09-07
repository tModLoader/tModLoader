using System;

namespace Terraria.ModLoader.Exceptions
{
	public class MissingResourceException : Exception
	{
		public MissingResourceException()
		{
		}

		public MissingResourceException(string message)
			: base(message)
		{
		}

		public MissingResourceException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
