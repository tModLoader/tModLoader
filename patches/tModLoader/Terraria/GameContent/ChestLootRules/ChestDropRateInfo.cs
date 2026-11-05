

namespace Terraria.GameContent.ChestLootRules;

/// <summary> Describes a possible chest item stack and its chance when the listed conditions apply. </summary>
public readonly struct ChestDropRateInfo
{
	/// <summary> The item type produced by the rule. </summary>
	public readonly int ItemType;
	/// <summary> The minimum stack size. </summary>
	public readonly int StackMin;
	/// <summary> The maximum stack size. </summary>
	public readonly int StackMax;
	/// <summary> The drop chance when the parent rule and listed conditions apply. </summary>
	public readonly float DropRate;
	/// <summary> Describes seed, chest-content, free-slot, or other generation requirements. </summary>
	public readonly string Condition;

	/// <summary> Creates a drop-rate entry; <paramref name="dropRate"/> is the probability when the listed conditions apply. </summary>
	public ChestDropRateInfo(int itemType, int stackMin, int stackMax, float dropRate, string condition = null)
	{
		ItemType = itemType;
		StackMin = stackMin;
		StackMax = stackMax;
		DropRate = dropRate;
		Condition = condition;
	}
}