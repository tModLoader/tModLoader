using System;

namespace Terraria.ModLoader.Exceptions
{
	public class MissingResourceException : Exception
	{
		public override string HelpLink => "https://github.com/blushiemagic/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#terrariamodloadermodgettexturestring-name-error";

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
