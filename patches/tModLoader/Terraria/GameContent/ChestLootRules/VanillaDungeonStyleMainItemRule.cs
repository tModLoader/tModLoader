using System;
using System.Collections.Generic;
using Terraria.Utilities;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal sealed class VanillaDungeonStyleMainItemRule : IChestLootRule
{
	/// <summary> Applies this rule to the chest contents. </summary>
	public void TryDrop(Chest chest, UnifiedRandom random)
	{
		ChestLootRuleUtilities.ValidateArguments(chest, random);

		chest.AddItem(RollItem(random, Main.tenthAnniversaryWorld));
	}

	/// <summary> Appends <see cref="ChestDropRateInfo"/> entries to <paramref name="drops"/>, scaled by the parent probability. Reporting does not evaluate conditions. </summary>
	public void ReportDropRates(List<ChestDropRateInfo> drops, float parentDropRate = 1f)
	{
		drops.Add(new ChestDropRateInfo(848, 1, 1, parentDropRate / 9f, "Dungeon-style house chest main item outside tenth-anniversary worlds."));
		drops.Add(new ChestDropRateInfo(857, 1, 1, parentDropRate * 4f / 9f, "Dungeon-style house chest main item outside tenth-anniversary worlds."));
		drops.Add(new ChestDropRateInfo(857, 1, 1, parentDropRate * 5f / 9f, "Dungeon-style house main item in tenth-anniversary worlds."));
		drops.Add(new ChestDropRateInfo(934, 1, 1, parentDropRate * 4f / 9f, "Dungeon-style house chest main item."));
	}

	internal static int RollItem(UnifiedRandom random, bool tenthAnniversaryWorld)
	{
		int option = random.Next(3);
		if (option == 0)
			option = random.Next(3);

		if (tenthAnniversaryWorld && option == 0)
			option = 1;

		return option switch {
			0 => 848,
			1 => 857,
			2 => 934,
			_ => throw new InvalidOperationException("The dungeon-style main-item roll returned an invalid option.")
		};
	}
}