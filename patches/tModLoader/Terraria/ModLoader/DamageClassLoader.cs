using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static bool[,] countsAs;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
			DamageClass.Generic,
			DamageClass.NoScaling,
			DamageClass.Melee,
			DamageClass.Ranged,
			DamageClass.Magic,
			DamageClass.Summon,
			DamageClass.Throwing
		};

		private static readonly int DefaultClassCount = DamageClasses.Count;

		static DamageClassLoader() {
			RegisterDefaultClasses();
			RebuildCountsAsCache();
		}

		internal static int Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
			return DamageClasses.Count - 1;
		}

		internal static void ResizeArrays() {
			RegisterDefaultClasses();
			RebuildCountsAsCache();

			foreach (var dc in DamageClasses)
				dc.RebuildBenefitCache();
		}

		internal static void Unload() {
			DamageClasses.RemoveRange(DefaultClassCount - 1, DamageClasses.Count - DefaultClassCount);
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

		private static void RegisterDefaultClasses() {
			for (int i = 0; i < DefaultClassCount; i++) {
				var damageClass = DamageClasses[i];

				damageClass.Type = i;

				ContentInstance.Register(damageClass);
				ModTypeLookup<DamageClass>.RegisterInternal(damageClass, damageClass.Name, damageClass.FullName, throwOnDuplicate: false);
			}
		}
	}
}
