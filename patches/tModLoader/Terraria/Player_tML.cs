using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		private DamageClassData[] damageData = new DamageClassData[0];

		internal void ResetDamageClassDictionaries() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];
			for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
				damageData[i] = new DamageClassData(4, 0, 1);
			}
		}

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void ModifyCrit<T>(int changeAmount) where T : ModDamageClass => ModifyCrit(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage. Can be negative too.</param>
		public void ModifyDamage<T>(float changeAmount) where T : ModDamageClass => ModifyDamage(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the damage multiplier's value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage multiplier. Can be negative too.</param>
		public void ModifyDamageMult<T>(float changeAmount) where T : ModDamageClass => ModifyDamageMult(ModContent.GetInstance<T>(), changeAmount);

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void ModifyCrit(ModDamageClass damageClass, int changeAmount) {
			if (damageClass is Summon) {
				return;
			}
			ref var data = ref damageData[damageClass.index];
			data.crit += changeAmount;
		}

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage. Can be negative too.</param>
		public void ModifyDamage(ModDamageClass damageClass, float changeAmount) {
			ref var data = ref damageData[damageClass.index];
			data.damage += changeAmount;
		}

		/// <summary>
		/// Edits the damage multiplier's value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage multiplier. Can be negative too.</param>
		public void ModifyDamageMult(ModDamageClass damageClass, float changeAmount) {
			ref var data = ref damageData[damageClass.index];
			data.damageMult += changeAmount;
		}

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit<T>() where T : ModDamageClass => GetCrit(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public float GetDamage<T>() where T : ModDamageClass => GetDamage(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage multiplier stat for this damage type on this player.
		/// </summary>
		public float GetDamageMult<T>() where T : ModDamageClass => GetDamageMult(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit(ModDamageClass damageClass) => damageClass is Summon ? 0 : damageData[damageClass.index].crit; // Special case, summoner class cannot have crits.

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public float GetDamage(ModDamageClass damageClass) => damageData[damageClass.index].damage;

		/// <summary>
		/// Gets the damage multiplier stat for this damage type on this player.
		/// </summary>
		public float GetDamageMult(ModDamageClass damageClass) => damageData[damageClass.index].damageMult;
	}
}
