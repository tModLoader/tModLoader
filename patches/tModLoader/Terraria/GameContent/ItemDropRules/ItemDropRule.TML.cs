namespace Terraria.GameContent.ItemDropRules
{
	partial class ItemDropRule
	{
		public static IItemDropRule FewFromOptionsNotScalingWithLuck(int amount, int chanceDenominator, params int[] options) => new FewFromOptionsNotScaledWithLuckDropRule(amount, chanceDenominator, 1, options);
		public static IItemDropRule FewFromOptionsNotScalingWithLuckWithX(int amount, int chanceDenominator, int chanceNumerator, params int[] options) => new FewFromOptionsNotScaledWithLuckDropRule(amount, chanceDenominator, chanceNumerator, options);
		public static IItemDropRule FewFromOptions(int amount, int chanceDenominator, params int[] options) => new FewFromOptionsDropRule(amount, chanceDenominator, 1, options);
		public static IItemDropRule FewFromOptionsWithNumerator(int amount, int chanceDenominator, int chanceNumerator, params int[] options) => new FewFromOptionsDropRule(amount, chanceDenominator, chanceNumerator, options);
		public static IItemDropRule SequentialRules(int chanceDenominator, params IItemDropRule[] rules) => new SequentialRulesRule(chanceDenominator, rules);
		public static IItemDropRule SequentialRulesNotScalingWithLuck(int chanceDenominator, params IItemDropRule[] rules) => new SequentialRulesNotScalingWithLuckRule(chanceDenominator, rules);
	}
}
