using Terraria.GameContent.Creative;

namespace Terraria.DataStructures
{
	public partial class GameModeData
	{
		/// <summary>
		/// Returns the damage modifier with journey mode applied
		/// </summary>
		public float EnemyDamageModifierWithPowersApplied
		{
			get
			{
				float enemyDamageModifier = EnemyDamageMultiplier;

				if (IsJourneyMode)
				{
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
		public float EffectiveEnemyDamageModifier
		{
			get
			{
				float enemyDamageModifier = EnemyDamageModifierWithPowersApplied;
				if (Main.getGoodWorld)
				{
					if (IsJourneyMode)
					{
						//The normal formula is (EnemyDamageMultiplier + 1) * 4/3 * difficultySliderPower.StrengthMultiplierToGiveNPCs
						//As the difficultySliderPower is already applied, we need to multiply (EnemyDamageMultiplier + 1) to get the correct value
						enemyDamageModifier *= 2;
					}
					else
					{
						enemyDamageModifier += 1;
					}
					enemyDamageModifier *= 1.25f;
				}

				return enemyDamageModifier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with journey mode applied
		/// </summary>
		public float EnemyMaxLifeMultiplierWithPowersApplied
		{
			get
			{
				float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplier;

				if (IsJourneyMode)
				{
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
		public float EffectiveEnemyMaxLifeMultiplier
		{
			get
			{
				float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplierWithPowersApplied;
				if (Main.getGoodWorld)
				{
					if (IsJourneyMode)
					{
						//The normal formula is (EnemyDamageMultiplier + 1) * difficultySliderPower.StrengthMultiplierToGiveNPCs
						//As the difficultySliderPower is already applied, we need to multiply (EnemyDamageMultiplier + 1) to get the correct value
						enemyMaxLifeMultiplier *= 2;
					}
					else
					{
						enemyMaxLifeMultiplier += 1;
					}
				}

				return enemyMaxLifeMultiplier;
			}
		}

		/// <summary>
		/// Returns the damage modifier with for the worthy applied
		/// </summary>
		public float EffectiveEnemyMoneyDropMultiplier
		{
			get
			{
				float enemyMoneyDropMultiplier = EnemyMoneyDropMultiplier;
				if (Main.getGoodWorld)
				{
					enemyMoneyDropMultiplier += 1;
				}

				return enemyMoneyDropMultiplier;
			}
		}
	}
}