using System;
using Terraria.Localization;

namespace Terraria.GameContent.ItemDropRules;

public record SimpleItemDropRuleCondition(LocalizedText Description, Func<bool> Predicate, ShowItemDropInUI ShowItemDropInUI) : IItemDropRuleCondition
{
	public SimpleItemDropRuleCondition(Condition Condition, ShowItemDropInUI ShowItemDropInUI) : this(Condition.Description, Condition.Predicate, ShowItemDropInUI) { }

	public bool CanDrop(DropAttemptInfo info) => Predicate();

	public bool CanShowItemDropInUI() => ShowItemDropInUI switch {
		ShowItemDropInUI.Always => true,
		ShowItemDropInUI.Never => false,
		ShowItemDropInUI.WhenConditionSatisifed => Predicate(),
		_ => throw new NotImplementedException()
	};

	public string GetConditionDescription() => Description?.Value;
}

public enum ShowItemDropInUI
{
	Always,
	Never,
	WhenConditionSatisifed
}

public static class Extensions
{
	public static SimpleItemDropRuleCondition ToDropCondition(this Condition condition, ShowItemDropInUI showItemDropInUI) => new(condition, showItemDropInUI);
}