using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>();

		internal static int Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
			return DamageClasses.IndexOf(damageClass);
		}

		internal static void Unload() {
			DamageClasses.Clear();
		}
	}
}
