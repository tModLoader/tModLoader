using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public struct StatInheritanceData
	{
		public static readonly StatInheritanceData Full = new StatInheritanceData(1f, 1f, 1f, 1f, 1f);
		public static readonly StatInheritanceData None = new StatInheritanceData(0f, 0f, 0f, 0f, 0f);

		public float damageInheritance;
		public float attackSpeedInheritance;
		public float critChanceInheritance;
		public float knockbackInheritance;
		public float armorPenInheritance;

		public StatInheritanceData(float damageInheritance, float attackSpeedInheritance, float critChanceInheritance, float knockbackInheritance, float armorPenInheritance) {
			this.damageInheritance = damageInheritance;
			this.attackSpeedInheritance = attackSpeedInheritance;
			this.critChanceInheritance = critChanceInheritance;
			this.knockbackInheritance = knockbackInheritance;
			this.armorPenInheritance = armorPenInheritance;
		}
	}
}
