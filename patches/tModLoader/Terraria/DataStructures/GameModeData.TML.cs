using Terraria.GameContent.Creative;

namespace Terraria.DataStructures
{
	public partial class GameModeData
	{
		public float GetPureEnemyDamageModifier() {
			float enemyDamageModifier = EnemyDamageMultiplier;

			CreativePowers.DifficultySliderPower difficultySliderPower =
				CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
			if (difficultySliderPower != null && difficultySliderPower.GetIsUnlocked())
				enemyDamageModifier *= difficultySliderPower.StrengthMultiplierToGiveNPCs;
			return enemyDamageModifier;
		}

		public float GetEnemyDamageModifier() {
			float enemyDamageModifier = GetPureEnemyDamageModifier();
			if (Main.getGoodWorld) {
				if (IsJourneyMode) {
					enemyDamageModifier *= 2;
				}
				else {
					enemyDamageModifier += 1;
				}
			}

			return enemyDamageModifier;
		}
		public float GetPureEnemyMaxLifeMultiplier() {
			float enemyMaxLifeMultiplier = EnemyMaxLifeMultiplier;

			CreativePowers.DifficultySliderPower difficultySliderPower =
				CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
			if (difficultySliderPower != null && difficultySliderPower.GetIsUnlocked())
				enemyMaxLifeMultiplier *= difficultySliderPower.StrengthMultiplierToGiveNPCs;
			return enemyMaxLifeMultiplier;
		}

		public float GetEnemyMaxLifeMultiplier() {
			float enemyMaxLifeMultiplier = GetPureEnemyMaxLifeMultiplier();
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

		public float GetEnemyMoneyDropMultiplier() {
			float enemyMoneyDropMultiplier = EnemyMoneyDropMultiplier;
			if (Main.getGoodWorld) {
				if (IsJourneyMode) {
					enemyMoneyDropMultiplier *= 2;
				}
				else {
					enemyMoneyDropMultiplier += 1;
				}
			}

			return enemyMoneyDropMultiplier;
		}
	}
}