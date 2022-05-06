using Terraria.GameContent.Creative;

namespace Terraria.DataStructures
{
	public partial record struct GameModeData
	{
		/// <summary>
		/// Returns the damage modifier with journey mode applied
		/// </summary>
		public float EnemyDamageModifierWithPowersApplied {
			get {
				float enemyDamageModifier = EnemyDamageMultiplier;

				if (IsJourneyMode) {
					CreativePowers.DifficultySliderPower difficultySliderPower =
						CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
					enemyDamageModifier *= difficultySliderPower.StrengthMultiplierToGiveNPCs;
				}

				return enemyDamageModifier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with journey mode and for the worthy applied
		/// </summary>
		public float EffectiveEnemyDamageModifier {
			get {
				//The normal formula is (EnemyDamageMultiplier + 1) * 4/3 * difficultySliderPower.StrengthMultiplierToGiveNPCs
				float enemyDamageModifier = EnemyDamageModifierWithPowersApplied;
				if (Main.getGoodWorld) {
					if (IsJourneyMode) {
						enemyDamageModifier *= 2;
					}
					else {
						enemyDamageModifier += 1;
					}

					enemyDamageModifier *= 4f/3f;
				}

				return enemyDamageModifier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with journey mode applied
		/// </summary>
		public float EnemyMaxLifeMultiplierWithPowersApplied {
			get {
				float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplier;

				if (IsJourneyMode) {
					CreativePowers.DifficultySliderPower difficultySliderPower =
						CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
					enemyMaxLifeMultiplier *= difficultySliderPower.StrengthMultiplierToGiveNPCs;
				}

				return enemyMaxLifeMultiplier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with journey mode and for the worthy applied
		/// </summary>
		public float EffectiveEnemyMaxLifeMultiplier {
			get {
				//The normal formula is (EnemyMaxLifeMultiplier + 1) * difficultySliderPower.StrengthMultiplierToGiveNPCs
				float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplierWithPowersApplied;
				if (Main.getGoodWorld) {
					if (IsJourneyMode) {
						enemyMaxLifeMultiplier *= 2;
					}
					else {
						enemyMaxLifeMultiplier += 1;
					}
				}

				return enemyMaxLifeMultiplier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with for the worthy applied
		/// </summary>
		public float EffectiveEnemyMoneyDropMultiplier {
			get {
				float enemyMoneyDropMultiplier = EnemyMoneyDropMultiplier;
				if (Main.getGoodWorld) {
					enemyMoneyDropMultiplier += 1;
				}

				return enemyMoneyDropMultiplier;
			}
		}
	}
}