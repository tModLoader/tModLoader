

namespace Terraria.GameContent.ChestLootRules;

/// <summary> A chest-rule condition used by <see cref="ConditionalChestLootRule"/>. </summary>
public interface IChestLootCondition
{
	/// <summary> Checks whether the chest satisfies the condition, as evaluated by <see cref="ConditionalChestLootRule.TryDrop"/>. </summary>
	bool CanDrop(Chest chest);

	/// <summary> Returns the localized condition description for <see cref="IChestLootRule.ReportDropRates"/>; may return null. </summary>
	string GetConditionDescription();
}