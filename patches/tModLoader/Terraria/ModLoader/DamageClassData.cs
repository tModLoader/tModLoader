namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public StatModifier damage;
		public int critChance;
		public StatModifier knockback;
		public int armorPen;

		public DamageClassData(StatModifier damage, int critChance, StatModifier knockback, int armorPen) {
			this.damage = damage;
			this.critChance = critChance;
			this.knockback = knockback;
			this.armorPen = armorPen;
		}
	}
}
