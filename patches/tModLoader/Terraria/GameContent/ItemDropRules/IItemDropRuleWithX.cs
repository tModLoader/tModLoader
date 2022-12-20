namespace Terraria.GameContent.ItemDropRules
{
	// added by tml
	public interface IItemDropRuleWithChance {
		float Chance {
			get {
				float chance = 1.0f;
				if (this is IItemDropRuleWithNumerator ruleWithNumerator) {
					chance = ruleWithNumerator.Numerator / (float)ruleWithNumerator.Denominator;
				}
				else if (this is IItemDropRuleWithDenominator ruleWithDenominator) {
					chance /= ruleWithDenominator.Denominator;
				}
				return chance;
			}
		}
	}
	public interface IItemDropRuleWithDenominator : IItemDropRuleWithChance {
		int Denominator { get; }
	}
	public interface IItemDropRuleWithNumerator : IItemDropRuleWithDenominator {
		int Numerator { get; }
	}
}
