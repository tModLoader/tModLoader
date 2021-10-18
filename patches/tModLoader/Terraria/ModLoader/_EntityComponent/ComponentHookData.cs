using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ModLoader
{
	internal static class ComponentHookData<TInterface, TDelegate>
		where TInterface : class
		where TDelegate : Delegate
	{
		public static TDelegate[] DelegatesByComponentId = Array.Empty<TDelegate>();
	}
}
