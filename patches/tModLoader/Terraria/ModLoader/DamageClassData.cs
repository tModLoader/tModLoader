namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public DamageClass damageClass;
		public Modifier damage;
		public int crit;
		public Modifier knockback;

		public DamageClassData(DamageClass damageClass, Modifier damage, int crit, Modifier knockback) {
			this.damageClass = damageClass;
			this.damage = damage;
			this.crit = crit;
			this.knockback = knockback;
		}
	}
}
