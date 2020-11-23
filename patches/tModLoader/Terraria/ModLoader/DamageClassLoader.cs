using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
			DamageClass.Generic,
			DamageClass.NoScaling,
			DamageClass.Melee,
			DamageClass.Ranged,
			DamageClass.Magic,
			DamageClass.Summon,
			DamageClass.Throwing
		};

		internal static readonly int DefaultDamageClassCount = DamageClasses.Count;

		internal static void Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
		}

		internal static void Unload() {
			DamageClasses.RemoveRange(DefaultDamageClassCount, DamageClasses.Count - DefaultDamageClassCount);
		}
	}
}
