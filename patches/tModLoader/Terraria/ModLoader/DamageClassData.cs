namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public StatModifier damage;
		public int critChance;
		public StatModifier knockback;

		public DamageClassData(StatModifier damage, int critChance, StatModifier knockback) {
			this.damage = damage;
			this.critChance = critChance;
			this.knockback = knockback;
		}
	}
}
