namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public Modifier damage;
		public Modifier crit;

		public DamageClassData(Modifier damage, Modifier crit) {
			this.damage = damage;
			this.crit = crit;
		}
	}
}
