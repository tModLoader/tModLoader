namespace Terraria.ModLoader
{
	internal struct DamageClassData
	{
		public int crit;
		public DamageModifier damage;

		public DamageClassData(int crit, DamageModifier damage) {
			this.crit = crit;
			this.damage = damage;
		}
	}
}
