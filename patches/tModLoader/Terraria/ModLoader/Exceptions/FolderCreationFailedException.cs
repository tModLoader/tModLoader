using System;

namespace Terraria.ModLoader.Exceptions;

class FolderCreationFailedException : Exception
{
	public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#systemunauthorizedaccessexception-access-to-the-path-is-denied";

	public FolderCreationFailedException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
