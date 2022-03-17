using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public struct StatInheritanceData
	{
		public float damageInheritance;
		public float useTimeInheritance;
		public float useAnimationInheritance;
		public float useSpeedInheritance;
		public float critChanceInheritance;
		public float knockbackInheritance;
		public float armorPenInheritance;

		public StatInheritanceData() {
			this.damageInheritance = 1f;
			this.useTimeInheritance = 1f;
			this.useAnimationInheritance = 1f;
			this.useSpeedInheritance = 1f;
			this.critChanceInheritance = 1f;
			this.knockbackInheritance = 1f;
			this.armorPenInheritance = 1f;
		}

		public StatInheritanceData(float damageInheritance = 0f, float useTimeInheritance = 0f, float useAnimationInheritance = 0f, float useSpeedInheritance = 0f, float critChanceInheritance = 0f, float knockbackInheritance = 0f, float armorPenInheritance = 0f) {
			this.damageInheritance = damageInheritance;
			this.useTimeInheritance = useTimeInheritance;
			this.useAnimationInheritance = useAnimationInheritance;
			this.useSpeedInheritance = useSpeedInheritance;
			this.critChanceInheritance = critChanceInheritance;
			this.knockbackInheritance = knockbackInheritance;
			this.armorPenInheritance = armorPenInheritance;
		}
	}

	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static StatInheritanceData[,] statInheritanceCache;
		internal static bool[,] effectInheritanceCache;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
			DamageClass.Default,
			DamageClass.Generic,
			DamageClass.Melee,
			DamageClass.Ranged,
			DamageClass.Magic,
			DamageClass.Summon,
			DamageClass.Throwing
		};

		private static readonly int DefaultClassCount = DamageClasses.Count;

		static DamageClassLoader() {
			RegisterDefaultClasses();
			ResizeArrays();
		}

		internal static int Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
			return DamageClasses.Count - 1;
		}

		internal static void ResizeArrays() {
			RebuildStatInheritanceCache();
			RebuildEffectInheritanceCache();
		}

		internal static void Unload() {
			DamageClasses.RemoveRange(DefaultClassCount, DamageClasses.Count - DefaultClassCount);
		}

		private static void RebuildStatInheritanceCache() {
			statInheritanceCache = new StatInheritanceData[DamageClassCount, DamageClassCount];

			for (int i = 0; i < DamageClasses.Count; i++) {
				for (int j = 0; j < DamageClasses.Count; j++) {
					if (DamageClasses[i] == DamageClasses[j])
						statInheritanceCache[i, j] = new StatInheritanceData();
					else
						statInheritanceCache[i, j] = DamageClasses[i].CheckBaseClassStatInheritance(DamageClasses[j]);
				}
			}
		}

		private static void RebuildEffectInheritanceCache() {
			effectInheritanceCache = new bool[DamageClassCount, DamageClassCount];

			for (int i = 0; i < DamageClasses.Count; i++) {
				for (int j = 0; j < DamageClasses.Count; j++) {
					if (DamageClasses[i] == DamageClasses[j] || DamageClasses[i].CheckClassEffectInheritance(DamageClasses[j]))
						effectInheritanceCache[i, j] = true;
				}
			}
		}

		internal static void RegisterDefaultClasses() {
			int i = 0;
			foreach (var damageClass in DamageClasses) {
				damageClass.Type = i++;
				ContentInstance.Register(damageClass);
				ModTypeLookup<DamageClass>.Register(damageClass);
			}
		}
	}
}
