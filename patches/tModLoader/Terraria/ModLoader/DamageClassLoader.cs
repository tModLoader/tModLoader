using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static int DamageClassCount => DamageClasses.Count;

		internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
			DamageClass.Melee,
			DamageClass.Ranged,
			DamageClass.Magic,
			DamageClass.Summon
		};

		internal static readonly int DefaultDamageClassCount = DamageClasses.Count;

		internal static int Add(DamageClass damageClass) {
			DamageClasses.Add(damageClass);
			return DamageClasses.Count - 1;
		}

		internal static void Unload() {
			DamageClasses.RemoveRange(DefaultDamageClassCount, DamageClasses.Count - DefaultDamageClassCount);
		}
	}
}
