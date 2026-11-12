using System;
using Terraria.Localization;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> Uses <see cref="LocalizedText"/> and a chest predicate to define a condition, or adapts <see cref="Condition"/>. Player-related conditions check the player rather than the chest location. </summary>
public sealed class ChestLootCondition : IChestLootCondition
{
	/// <summary> The localized condition description. </summary>
	public LocalizedText Description { get; }
	private readonly Func<Chest, bool> _predicate;

	/// <summary> Creates <see cref="IChestLootCondition"/>. The description is localized and the predicate is evaluated when the rule runs. </summary>
	public ChestLootCondition(LocalizedText description, Func<Chest, bool> predicate)
	{
		Description = description ?? throw new ArgumentNullException(nameof(description));
		_predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
	}

	/// <summary> Creates <see cref="IChestLootCondition"/>. The description is localized and the predicate is evaluated when the rule runs. </summary>
	public ChestLootCondition(string localizationKey, Func<Chest, bool> predicate) : this(Language.GetOrRegister(localizationKey), predicate) { }

	/// <summary> Creates <see cref="IChestLootCondition"/>. The description is localized and the predicate is evaluated when the rule runs. </summary>
	public ChestLootCondition(Condition condition)
	{
		ArgumentNullException.ThrowIfNull(condition);
		Description = condition.Description;
		_predicate = _ => condition.IsMet();
	}

	/// <summary> Checks the chest and current world state. World-generation rules evaluate this only during chest generation. </summary>
	public bool CanDrop(Chest chest)
	{
		ArgumentNullException.ThrowIfNull(chest);
		return _predicate(chest);
	}

	/// <summary> Returns the localized description, or null to omit it. </summary>
	public string GetConditionDescription() => Description?.Value;
}