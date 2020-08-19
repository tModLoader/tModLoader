using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => damageClasses.Count;

		internal static List<ModDamageClass> damageClasses = new List<ModDamageClass>();

		internal static bool DamageClassExists(ModDamageClass damageClass) => damageClass == null ? false : damageClasses.Contains(damageClass);

		internal static void Add(ModDamageClass damageClass) {
			damageClasses.Add(damageClass);
		}

		internal static int GetIndex<T>() => damageClasses.FindIndex(d => d.GetType() == typeof(T));

		internal static int GetIndex(ModDamageClass damageClass) => damageClasses.FindIndex(d => d == damageClass);

		internal static void Unload() {
			damageClasses.Clear();
		}
	}
}
