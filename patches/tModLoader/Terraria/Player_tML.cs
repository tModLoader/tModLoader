using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		private readonly IDictionary<int, int> crits = new Dictionary<int, int>();
		private readonly IDictionary<int, float> damages = new Dictionary<int, float>();
		private readonly IDictionary<int, float> damageMults = new Dictionary<int, float>();

		internal void ResetDamageClassDictionaries() {
			for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
				crits[i] = 0;
				damages[i] = 0;
				damageMults[i] = 1;
			}
		}

		/// <summary>
		/// Edits the crit for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount that should be added to the crit. Can be negative too.</param>
		public void SetCrit<T>(int changeAmount) where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			if (crits.ContainsKey(damageClassType)) {
				crits[damageClassType] += changeAmount;
			}
		}

		/// <summary>
		/// Edits the damage value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage. Can be negative too.</param>
		public void SetDamage<T>(float changeAmount) where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			if (damages.ContainsKey(damageClassType)) {
				damages[damageClassType] += changeAmount;
			}
		}

		/// <summary>
		/// Edits the damage multiplier's value for the given damage type on this player.
		/// </summary>
		/// <param name="changeAmount">The amount (as a percentage, 0.01f is 1%) that should be added to the damage multiplier. Can be negative too.</param>
		public void SetDamageMult<T>(float changeAmount) where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			if (damageMults.ContainsKey(damageClassType)) {
				damageMults[damageClassType] += changeAmount;
			}
		}

		/// <summary>
		/// Gets the crit stat for this damage type on this player.
		/// </summary>
		public int GetCrit<T>() where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			return GetCrit(damageClassType);
		}

		/// <summary>
		/// Gets the damage stat for this damage type on this player.
		/// </summary>
		public float GetDamage<T>() where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			return GetDamage(damageClassType);
		}

		/// <summary>
		/// Gets the damage multiplier stat for this damage type on this player.
		/// </summary>
		public float GetDamageMult<T>() where T : ModDamageClass {
			int damageClassType = ModContent.DamageClassType<T>();
			return GetDamageMult(damageClassType);
		}

		internal int GetCrit(int damageClassType) => crits.TryGetValue(damageClassType, out int crit) ? crit : 0;
		
		internal float GetDamage(int damageClassType) => damages.TryGetValue(damageClassType, out float damage) ? damage : 0;
		
		internal float GetDamageMult(int damageClassType) => damageMults.TryGetValue(damageClassType, out float damageMult) ? damageMult : 1;
	}
}
