namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public Modifier damage;
		public int crit;
		public Modifier knockback;

		public DamageClassData(Modifier damage, int crit, Modifier knockback) {
			this.damage = damage;
			this.crit = crit;
			this.knockback = knockback;
		}
	}
}
