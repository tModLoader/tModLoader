using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => damageClasses.Count;

		internal static List<ModDamageClass> damageClasses = new List<ModDamageClass>();

		internal static bool DamageTypeExists(int type) => type >= 0 && type < DamageClassCount;

		internal static int Add(ModDamageClass damageClass) {
			damageClasses.Add(damageClass);
			return damageClasses.Count - 1;
		}

		internal static ModDamageClass GetDamageClass(int type) => damageClasses[type];

		internal static void Unload() {
			damageClasses.Clear();
		}
	}
}
