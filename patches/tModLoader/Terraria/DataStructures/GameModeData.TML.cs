using Terraria.GameContent.Creative;

namespace Terraria.DataStructures
{
	public partial class GameModeData
	{
		/// <summary>
		/// 
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

		public float EffectiveEnemyDamageModifier
		{
			get
			{
				float enemyDamageModifier = EnemyDamageModifierWithPowersApplied;
				if (Main.getGoodWorld)
				{
					if (IsJourneyMode)
					{
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

		public float EffectiveEnemyMaxLifeMultiplier
		{
			get
			{
				float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplierWithPowersApplied;
				if (Main.getGoodWorld)
				{
					if (IsJourneyMode)
					{
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

		public float EffectiveEnemyMoneyDropMultiplier
		{
			get
			{
				float enemyMoneyDropMultiplier = EnemyMoneyDropMultiplier;
				if (Main.getGoodWorld)
				{
					if (IsJourneyMode)
					{
						enemyMoneyDropMultiplier *= 2;
					}
					else
					{
						enemyMoneyDropMultiplier += 1;
					}
				}

				return enemyMoneyDropMultiplier;
			}
		}
	}
}