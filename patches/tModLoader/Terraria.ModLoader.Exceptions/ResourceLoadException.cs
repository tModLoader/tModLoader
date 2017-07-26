using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader.Exceptions
{
	class ResourceLoadException : Exception
	{
		public ResourceLoadException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
