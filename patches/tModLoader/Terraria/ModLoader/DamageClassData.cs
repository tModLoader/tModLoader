namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public Modifier damage;
		public Modifier crit;
		public Modifier knockback;

		public DamageClassData(Modifier damage, Modifier crit, Modifier knockback) {
			this.damage = damage;
			this.crit = crit;
			this.knockback = knockback;
		}
	}
}
