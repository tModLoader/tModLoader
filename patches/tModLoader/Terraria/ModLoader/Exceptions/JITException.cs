using System;

namespace Terraria.ModLoader.Exceptions;

public class JITException : Exception
{
	public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/JIT-Exception";
	
	public JITException(string message)	: base(message)
	{
	}
}
