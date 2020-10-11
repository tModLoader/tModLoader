namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public int crit;
		public Modifier damage;

		public DamageClassData(int crit, Modifier damage) {
			this.crit = crit;
			this.damage = damage;
		}
	}
}
