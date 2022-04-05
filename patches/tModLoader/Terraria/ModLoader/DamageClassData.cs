namespace Terraria.ModLoader
{
	public struct DamageClassData
	{
		public StatModifier damage;
		public StatModifierSimple attackSpeed;
		public int critChance;
		public StatModifier knockback;
		public int armorPen;

		public DamageClassData(StatModifier damage, StatModifierSimple attackSpeed, int critChance, StatModifier knockback, int armorPen) {
			this.damage = damage;
			this.attackSpeed = attackSpeed;
			this.critChance = critChance;
			this.knockback = knockback;
			this.armorPen = armorPen;
		}
	}
}