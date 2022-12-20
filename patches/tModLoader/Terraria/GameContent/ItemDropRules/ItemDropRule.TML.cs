using System;
using System.Threading.Channels;
using Terraria.ModLoader;

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
		public static IItemDropRule Coins(long value, bool withRandomBonus) => new CoinsRule(value, withRandomBonus);
		public static IItemDropRule CoinsBasedOnNPCValue(int npcId) {
			var npc = new NPC();
			npc.SetDefaults(npcId);
			// TODO, support dynamic NPC value, expert/master scaling etc. Not sure the best way to display/handle it.
			return Coins((long)npc.value, withRandomBonus: true);
		}
		public static IItemDropRule AlwaysAtleastOneSuccess(params IItemDropRule[] rules) => new AlwaysAtleastOneSuccessDropRule(rules);

		// overloads
		public static IItemDropRule Common(int itemId, Fraction fraction, int minimumDropped = 1, int maximumDropped = 1) => Common(itemId, fraction.Denominator, minimumDropped, maximumDropped, fraction.Numerator);
		public static IItemDropRule Common<T>(int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) where T : ModItem => Common(ModContent.ItemType<T>(), chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
		public static IItemDropRule Common<T>(Fraction fraction, int minimumDropped = 1, int maximumDropped = 1) where T : ModItem => Common(ModContent.ItemType<T>(), fraction.Denominator, minimumDropped, maximumDropped, fraction.Numerator);
		public static IItemDropRule BossBag<T>() where T : ModItem => BossBag(ModContent.ItemType<T>());
		public static IItemDropRule BossBagByCondition<T>(IItemDropRuleCondition condition) where T : ModItem => BossBagByCondition(condition, ModContent.ItemType<T>());
		public static IItemDropRule ExpertGetsRerolls(int itemId, Fraction chance, int expertRerolls) => ExpertGetsRerolls(itemId, chance.Denominator, expertRerolls, chance.Numerator);
		public static IItemDropRule ExpertGetsRerolls<T>(int chanceDenominator, int expertRerolls, int chanceNumerator = 1) where T : ModItem => ExpertGetsRerolls(ModContent.ItemType<T>(), chanceDenominator, expertRerolls, chanceNumerator);
		public static IItemDropRule ExpertGetsRerolls<T>(Fraction chance, int expertRerolls) where T : ModItem => ExpertGetsRerolls(ModContent.ItemType<T>(), chance.Denominator, expertRerolls, chance.Numerator);
		public static IItemDropRule MasterModeCommonDrop<T>() where T : ModItem => MasterModeCommonDrop(ModContent.ItemType<T>());
		public static IItemDropRule MasterModeDropOnAllPlayers(int itemId, Fraction chance) => MasterModeDropOnAllPlayers(itemId, chance.Denominator, chance.Numerator);
		public static IItemDropRule MasterModeDropOnAllPlayers<T>(int chanceDenominator = 1, int chanceNumerator = 1) where T : ModItem => MasterModeDropOnAllPlayers(ModContent.ItemType<T>(), chanceDenominator, chanceNumerator);
		public static IItemDropRule MasterModeDropOnAllPlayers<T>(Fraction chance) where T : ModItem => MasterModeDropOnAllPlayers(ModContent.ItemType<T>(), chance.Denominator, chance.Numerator);
		public static IItemDropRule WithRerolls(int itemId, int rerolls, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) => WithRerolls(itemId, rerolls, chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule WithRerolls<T>(int rerolls, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) where T : ModItem => WithRerolls(ModContent.ItemType<T>(), rerolls, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
		public static IItemDropRule WithRerolls<T>(int rerolls, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) where T : ModItem => WithRerolls(ModContent.ItemType<T>(), rerolls, chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule ByCondition(IItemDropRuleCondition condition, int itemId, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) => ByCondition(condition, itemId, chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule ByCondition<T>(IItemDropRuleCondition condition, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) where T : ModItem => ByCondition(condition, ModContent.ItemType<T>(), chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
		public static IItemDropRule ByCondition<T>(IItemDropRuleCondition condition, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) where T : ModItem => ByCondition(condition, ModContent.ItemType<T>(), chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule NotScalingWithLuck(int itemId, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) => NotScalingWithLuck(itemId, chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule NotScalingWithLuck<T>(int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) where T : ModItem => NotScalingWithLuck(ModContent.ItemType<T>(), chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
		public static IItemDropRule NotScalingWithLuck<T>(Fraction chance, int minimumDropped = 1, int maximumDropped = 1) where T : ModItem => NotScalingWithLuck(ModContent.ItemType<T>(), chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule OneFromOptionsNotScalingWithLuckWithX(Fraction chance, params int[] options) => OneFromOptionsNotScalingWithLuckWithX(chance.Denominator, chance.Numerator, options);
		public static IItemDropRule OneFromOptionsWithNumerator(Fraction chance, params int[] options) => OneFromOptionsWithNumerator(chance.Denominator, chance.Numerator, options);
		public static IItemDropRule NormalvsExpert<T>(int chanceDenominatorInNormal, int chanceDenominatorInExpert) where T : ModItem => NormalvsExpert(ModContent.ItemType<T>(), chanceDenominatorInNormal, chanceDenominatorInExpert);
		public static IItemDropRule NormalvsExpert(int itemId, int chanceDenominatorInNormal, int chanceNumeratorInNormal, int chanceDenominatorInExpert, int chanceNumeratorInExpert) => new DropBasedOnExpertMode(Common(itemId, chanceDenominatorInNormal, 1, 1, chanceNumeratorInNormal), Common(itemId, chanceDenominatorInExpert, 1, 1, chanceNumeratorInExpert));
		public static IItemDropRule NormalvsExpert(int itemId, Fraction chanceInNormal, Fraction chanceInExpert) => NormalvsExpert(itemId, chanceInNormal.Denominator, chanceInNormal.Numerator, chanceInExpert.Denominator, chanceInExpert.Numerator);
		public static IItemDropRule NormalvsExpert<T>(int chanceDenominatorInNormal, int chanceNumeratorInNormal, int chanceDenominatorInExpert, int chanceNumeratorInExpert) where T : ModItem => NormalvsExpert(ModContent.ItemType<T>(), chanceDenominatorInNormal, chanceNumeratorInNormal, chanceDenominatorInExpert, chanceNumeratorInExpert);
		public static IItemDropRule NormalvsExpert<T>(Fraction chanceInNormal, Fraction chanceInExpert) where T : ModItem => NormalvsExpert(ModContent.ItemType<T>(), chanceInNormal, chanceInExpert);
		public static IItemDropRule Food(int itemId, Fraction chance, int minimumDropped = 1, int maximumDropped = 1) => Food(itemId, chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule Food<T>(int chanceDenominator, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) where T : ModItem => Food(ModContent.ItemType<T>(), chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
		public static IItemDropRule Food<T>(Fraction chance, int minimumDropped = 1, int maximumDropped = 1) where T : ModItem => Food(ModContent.ItemType<T>(), chance.Denominator, minimumDropped, maximumDropped, chance.Numerator);
		public static IItemDropRule StatusImmunityItem(int itemId, Fraction chance) => StatusImmunityItem(itemId, chance.Denominator, chance.Numerator);
		public static IItemDropRule StatusImmunityItem<T>(int dropsOutOfX, int chanceNumerator = 1) where T : ModItem => ExpertGetsRerolls(ModContent.ItemType<T>(), dropsOutOfX, chanceNumerator);
		public static IItemDropRule StatusImmunityItem<T>(Fraction chance) where T : ModItem => ExpertGetsRerolls(ModContent.ItemType<T>(), chance.Denominator, chance.Numerator);
	}
}
