using System;

namespace Terraria.ModLoader.Exceptions;

class GetLoadableTypesException : Exception
{
	public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#extendsfrommod";

	public GetLoadableTypesException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
