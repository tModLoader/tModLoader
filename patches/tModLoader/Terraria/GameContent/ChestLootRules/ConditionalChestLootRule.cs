using System;
using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> All <see cref="Conditions"/> must be met to execute <see cref="Rule"/>. Drop-rate reporting preserves condition descriptions. </summary>
public sealed class ConditionalChestLootRule : IChestLootRule
{
	/// <summary> The inner rule executed when all conditions are met. </summary>
	public IChestLootRule Rule { get; }
	/// <summary> The read-only list of conditions checked before execution. All must be met. </summary>
	public IReadOnlyList<IChestLootCondition> Conditions { get; }
	/// <summary> Creates a conditional rule. The condition array is copied and exposed as a read-only list. </summary>
	public ConditionalChestLootRule(IChestLootCondition condition, IChestLootRule rule) : this(rule, condition) { }
	/// <summary> Creates a conditional rule. The condition array is copied and exposed as a read-only list. </summary>
	public ConditionalChestLootRule(IChestLootRule rule, params IChestLootCondition[] conditions)
	{
		Rule = rule ?? throw new ArgumentNullException(nameof(rule));
		ArgumentNullException.ThrowIfNull(conditions);
		if (conditions.Length == 0) throw new ArgumentException(nameof(conditions));
		var copy = (IChestLootCondition[])conditions.Clone();
		foreach (var condition in copy) ArgumentNullException.ThrowIfNull(condition);
		Conditions = Array.AsReadOnly(copy);
	}

	/// <summary> Applies the rule to the chest. Unmet conditions skip the inner rule without consuming its random rolls. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);
		foreach (var condition in Conditions) {
			if (!condition.CanDrop(chest))
				return;
		}
		Rule.TryDrop(chest, random);
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		ArgumentNullException.ThrowIfNull(drops);
		var descriptions = new List<string>();
		foreach (var condition in Conditions) {
			string description = condition.GetConditionDescription();
			if (!string.IsNullOrEmpty(description)) descriptions.Add(description);
		}
		string outerDescription = string.Join("; ", descriptions);
		var conditionalDrops = new List<ChestDropRateInfo>();
		Rule.ReportDropRates(conditionalDrops, parentDropRate);
		foreach (var drop in conditionalDrops) {
			string description = string.IsNullOrEmpty(outerDescription) ? drop.Condition
				: string.IsNullOrEmpty(drop.Condition) ? outerDescription
				: outerDescription + "; " + drop.Condition;
			drops.Add(new ChestDropRateInfo(drop.ItemType, drop.StackMin, drop.StackMax, drop.DropRate, description));
		}
	}
}