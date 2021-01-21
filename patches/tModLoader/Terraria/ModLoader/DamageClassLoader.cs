using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static bool[,] countsAs;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>();

		internal static int Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
			return DamageClasses.Count - 1;
		}

		internal static void ResizeArrays() {
			RebuildCountsAsCache();

			foreach (var dc in DamageClasses)
				dc.RebuildBenefitCache();
		}

		private static void RebuildCountsAsCache() {
				countsAs = new bool[DamageClassCount, DamageClassCount];
			for (int i = 0; i < DamageClasses.Count; i++) {
				for (int j = 0; j < DamageClasses.Count; j++) {
					if (DamageClasses[i] == DamageClasses[j] || DamageClasses[i].CountsAs(DamageClasses[j]))
						countsAs[i, j] = true;
				}
			}
		}

		internal static void Unload() {
			DamageClasses.Clear();
		}
	}
}
