namespace Terraria.ModLoader
{
	public struct ItemSpeedMultipliers
	{
		public StatModifier useTime;
		public StatModifier useAnimation;
		public StatModifier useSpeed;

		public ItemSpeedMultipliers(StatModifier useTime, StatModifier useAnimation, StatModifier useSpeed) {
			this.useTime = useTime;
			this.useAnimation = useAnimation;
			this.useSpeed = useSpeed;
		}
	}

	public struct DamageClassData
	{
		public StatModifier damage;
		public ItemSpeedMultipliers attackSpeed;
		public int critChance;
		public StatModifier knockback;
		public int armorPen;

		public DamageClassData(StatModifier damage, ItemSpeedMultipliers attackSpeed, int critChance, StatModifier knockback, int armorPen) {
			this.damage = damage;
			this.attackSpeed = attackSpeed;
			this.critChance = critChance;
			this.knockback = knockback;
			this.armorPen = armorPen;
		}
	}
}