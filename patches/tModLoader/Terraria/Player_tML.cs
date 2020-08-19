using System;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		private DamageClassData[] damageData = new DamageClassData[0];

		internal void ResetDamageClassDictionaries() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];
			for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
				damageData[i] = new DamageClassData(0, 0, 1);
			}
		}

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void SetCrit<T>(int changeAmount) where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			if (ContainsIndex(damageData, damageClassType)) {
				ref var data = ref damageData[damageClassType];
				data.crit += changeAmount;
			}
		}

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage. Can be negative too.</param>
		public void SetDamage<T>(float changeAmount) where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			if (ContainsIndex(damageData, damageClassType)) {
				ref var data = ref damageData[damageClassType];
				data.damage += changeAmount;
			}
		}

		/// <summary>
		/// Edits the damage multiplier's value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage multiplier. Can be negative too.</param>
		public void SetDamageMult<T>(float changeAmount) where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			if (ContainsIndex(damageData, damageClassType)) {
				ref var data = ref damageData[damageClassType];
				data.damageMult += changeAmount;
			}
		}

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit<T>() where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			return GetCrit(damageClassType);
		}

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public float GetDamage<T>() where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			return GetDamage(damageClassType);
		}

		/// <summary>
		/// Gets the damage multiplier stat for this damage type on this player.
		/// </summary>
		public float GetDamageMult<T>() where T : ModDamageClass {
			int damageClassType = DamageClassLoader.GetIndex<T>();
			return GetDamageMult(damageClassType);
		}

		internal int GetCrit(int damageClassType) => damageData[damageClassType].crit;
		
		internal float GetDamage(int damageClassType) => damageData[damageClassType].damage;
		
		internal float GetDamageMult(int damageClassType) => damageData[damageClassType].damageMult;

		private bool ContainsIndex(Array array, int index) => index >= 0 && index <= array.Length - 1;
	}

	public struct DamageClassData
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
