using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		private DamageClassData[] damageData = new DamageClassData[0];

		internal void ResetDamageClassDictionaries() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];
			for (int i = 0; i < damageData.Length; i++) {
				damageData[i] = new DamageClassData(4, DamageModifier.One); // Default values from vanilla - 4 crit, 0 add, 1x mult.
			}
		}

		public void AddDamageModifier<T>(DamageModifier changeAmount) where T : DamageClass => AddDamageModifier(ModContent.GetInstance<T>(), changeAmount);

		public void AddDamageModifier(DamageClass damageClass, DamageModifier changeAmount) {
			ref var data = ref damageData[damageClass.index];
			data.damage &= changeAmount;
		}

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void AddCrit<T>(int changeAmount) where T : DamageClass => AddCrit(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the damage, as a damage modifier.</param>
		public void AddDamage<T>(float changeAmount) where T : DamageClass => AddDamage(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the damage, as a damage modifier.</param>
		public void AddDamageMult<T>(float changeAmount) where T : DamageClass => AddDamageMult(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void AddCrit(DamageClass damageClass, int changeAmount) {
			if (damageClass is Summon) {
				return;
			}
			ref var data = ref damageData[damageClass.index];
			data.crit += changeAmount;
		}

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the damage for this class.</param>
		public void AddDamage(DamageClass damageClass, float changeAmount) {
			ref var data = ref damageData[damageClass.index];
			data.damage += changeAmount;
		}

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the damage multiplier for this class.</param>
		public void AddDamageMult(DamageClass damageClass, float changeAmount) {
			ref var data = ref damageData[damageClass.index];
			data.damage *= changeAmount;
		}

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit<T>() where T : DamageClass => GetCrit(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public DamageModifier GetDamage<T>() where T : DamageClass => GetDamage(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit(DamageClass damageClass) => damageClass is Summon ? 0 : damageData[damageClass.index].crit; // Special case, summoner class cannot have crits.

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public DamageModifier GetDamage(DamageClass damageClass) => damageData[damageClass.index].damage;
	}
}
