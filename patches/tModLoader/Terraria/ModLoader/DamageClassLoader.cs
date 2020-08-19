using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class DamageClassLoader
	{
		public static Melee Melee { get; private set; } = new Melee();
		public static Ranged Ranged { get; private set; } = new Ranged();
		public static Magic Magic { get; private set; } = new Magic();
		public static Summon Summon { get; private set; } = new Summon();

		public static int DamageClassCount => damageClasses.Count;

		internal static List<ModDamageClass> damageClasses = new List<ModDamageClass>() {
			Melee,
			Ranged,
			Magic,
			Summon
		};

		internal static int defaultDamageClassCount = damageClasses.Count;

		internal static int Add(ModDamageClass damageClass) {
			damageClasses.Add(damageClass);
			return damageClasses.Count - 1;
		}

		internal static void Unload() {
			damageClasses.RemoveRange(defaultDamageClassCount, damageClasses.Count - defaultDamageClassCount);
		}
	}
}
