using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> A queryable chest-loot or content-editing rule resolved and reported by <see cref="ChestLootDatabase"/>. </summary>
public interface IChestLootRule
{
	/// <summary> Applies the rule using the random-number generator supplied by the caller. </summary>
	void TryDrop(Chest chest, UnifiedRandom random);

	/// <summary> Reports <see cref="ChestDropRateInfo"/>, with the parent probability supplied through <paramref name="parentDropRate"/>. </summary>
	void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f);
}