using System;
using Terraria.Localization;

namespace Terraria.GameContent.ItemDropRules;

#nullable enable

public record SimpleItemDropRuleCondition(LocalizedText Description, Func<bool> Predicate, ShowItemDropInUI ShowItemDropInUI, bool ShowConditionInUI = true) : IItemDropRuleCondition
{
	public bool CanDrop(DropAttemptInfo info) => Predicate();

	public bool CanShowItemDropInUI() => ShowItemDropInUI switch {
		ShowItemDropInUI.Always => true,
		ShowItemDropInUI.Never => false,
		ShowItemDropInUI.WhenConditionSatisfied => Predicate(),
		_ => throw new NotImplementedException()
	};

	public string? GetConditionDescription() => ShowConditionInUI ? Description.Value : null;
}

public enum ShowItemDropInUI
{
	Always,
	Never,
	WhenConditionSatisfied
}

public static class Extensions
{
	public static SimpleItemDropRuleCondition ToDropCondition(this Condition condition, ShowItemDropInUI showItemDropInUI, bool showConditionInUI = true) =>
		new(Language.GetText("Bestiary_ItemDropConditions.SimpleCondition").WithFormatArgs(condition.Description), condition.Predicate, showItemDropInUI, showConditionInUI);
}