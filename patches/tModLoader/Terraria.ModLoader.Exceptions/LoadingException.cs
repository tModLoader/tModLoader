using System;

namespace Terraria.ModLoader.Exceptions
{
	internal class LoadingException : Exception
	{
		public readonly Mod mod;

		public LoadingException(Mod mod, string message, Exception inner) : base(message, inner)
		{
			this.mod = mod;
		}
	}
}
