namespace Terraria.GameContent.ItemDropRules;

partial class ItemDropRule
{
	public static IItemDropRule FewFromOptionsNotScalingWithLuck(int amount, int chanceDenominator, params int[] options) => new FewFromOptionsNotScaledWithLuckDropRule(amount, chanceDenominator, 1, options);
	public static IItemDropRule FewFromOptionsNotScalingWithLuckWithX(int amount, int chanceDenominator, int chanceNumerator, params int[] options) => new FewFromOptionsNotScaledWithLuckDropRule(amount, chanceDenominator, chanceNumerator, options);
	public static IItemDropRule FewFromOptions(int amount, int chanceDenominator, params int[] options) => new FewFromOptionsDropRule(amount, chanceDenominator, 1, options);
	public static IItemDropRule FewFromOptionsWithNumerator(int amount, int chanceDenominator, int chanceNumerator, params int[] options) => new FewFromOptionsDropRule(amount, chanceDenominator, chanceNumerator, options);
	public static IItemDropRule SequentialRules(int chanceDenominator, params IItemDropRule[] rules) => new SequentialRulesRule(chanceDenominator, rules);
	public static IItemDropRule SequentialRulesNotScalingWithLuck(int chanceDenominator, params IItemDropRule[] rules) => new SequentialRulesNotScalingWithLuckRule(chanceDenominator, rules);
	public static IItemDropRule SequentialRulesNotScalingWithLuckWithNumerator(int chanceDenominator, int chanceNumerator, params IItemDropRule[] rules) => new SequentialRulesNotScalingWithLuckRule(chanceDenominator, chanceNumerator, rules);
	public static IItemDropRule Coins(long value, bool withRandomBonus) => new CoinsRule(value, withRandomBonus);
	public static IItemDropRule CoinsBasedOnNPCValue(int npcId)
	{
		var npc = new NPC();
		npc.SetDefaults(npcId);
		// TODO, support dynamic NPC value, expert/master scaling etc. Not sure the best way to display/handle it.
		return Coins((long)npc.value, withRandomBonus: true);
	}
	public static IItemDropRule AlwaysAtleastOneSuccess(params IItemDropRule[] rules) => new AlwaysAtleastOneSuccessDropRule(rules);
	public static IItemDropRule NotScalingWithLuckWithNumerator(int itemId, int chanceDenominator = 1, int chanceNumerator = 1, int minimumDropped = 1, int maximumDropped = 1) => new CommonDropNotScalingWithLuck(itemId, chanceDenominator, chanceNumerator, minimumDropped, maximumDropped);
}
