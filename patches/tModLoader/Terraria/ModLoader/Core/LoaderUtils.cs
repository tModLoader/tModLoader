using System;

namespace Terraria.ModLoader.Core
{
	internal static class LoaderUtils
	{
		public static void ReloadSets(Type setsType) {
			setsType.TypeInitializer.Invoke(null, null);

			foreach (var type in setsType.GetNestedTypes()) {
				if (type.IsAbstract && type.IsSealed) { //IsStatic
					type.TypeInitializer?.Invoke(null, null);
				}
			}
		}
	}
}
