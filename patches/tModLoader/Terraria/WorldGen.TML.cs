using System;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class WorldGen
	{
		public static WorldGenLocals GenLocalValues;
		internal static event Action UpdateGenLocals;

		internal static void InvokeGenLocalUpdate() {
			UpdateGenLocals?.Invoke();
		}
	}
}
