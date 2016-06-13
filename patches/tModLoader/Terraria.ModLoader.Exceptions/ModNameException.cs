using System;

namespace Terraria.ModLoader.Exceptions
{
	public class ModNameException : Exception
	{
		public ModNameException()
		{
		}

		public ModNameException(string message)
			: base(message)
		{
		}

		public ModNameException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
