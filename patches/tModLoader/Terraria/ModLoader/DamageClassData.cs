namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public int crit;
		public float damage;
		public float damageMult;

		public DamageClassData(int crit, float damage, float damageMult) {
			this.crit = crit;
			this.damage = damage;
			this.damageMult = damageMult;
		}
	}
}
