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

		public StatInheritanceData(float damageInheritance, float useTimeInheritance, float useAnimationInheritance, float useSpeedInheritance, float critChanceInheritance, float knockbackInheritance, float armorPenInheritance) {
			this.damageInheritance = damageInheritance;
			this.useTimeInheritance = useTimeInheritance;
			this.useAnimationInheritance = useAnimationInheritance;
			this.useSpeedInheritance = useSpeedInheritance;
			this.critChanceInheritance = critChanceInheritance;
			this.knockbackInheritance = knockbackInheritance;
			this.armorPenInheritance = armorPenInheritance;
		}

		public static StatInheritanceData Full => new StatInheritanceData(1f, 1f, 1f, 1f, 1f, 1f, 1f);
		public static StatInheritanceData None => new StatInheritanceData(0f, 0f, 0f, 0f, 0f, 0f, 0f);
	}
}
