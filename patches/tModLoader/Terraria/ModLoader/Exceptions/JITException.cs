using System;

namespace Terraria.ModLoader.Exceptions
{
	public class JITException : Exception
	{
		public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/JIT-Mod-Assemblies-Error";
		
		public JITException(string message)	: base(message) {
		}
	}
}
