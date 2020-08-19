using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => damageClasses.Count;

		internal static List<ModDamageClass> damageClasses = new List<ModDamageClass>();

		internal static bool DamageClassExists(ModDamageClass damageClass) => damageClass == null ? false : damageClasses.Contains(damageClass);

		internal static int Add(ModDamageClass damageClass) {
			damageClasses.Add(damageClass);
			return damageClasses.Count - 1;
		}

		internal static void Unload() {
			damageClasses.Clear();
		}
	}
}
