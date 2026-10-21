using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.GameContent.ChestLootRules;

/// <summary> An internal vanilla chest-generation rule or drop-rate reporting implementation. </summary>
internal static class VanillaDungeonBiomeMainItemRule
{
	internal static void ReportDropRates(int chestID, List<ChestDropRateInfo> drops)
	{
		switch (chestID) {
			case ChestID.LockedJungle:
				drops.Add(new ChestDropRateInfo(1156, 1, 1, 1f, "Main item in a standard-world jungle biome dungeon chest."));
				break;
			case ChestID.LockedCorruption:
				drops.Add(new ChestDropRateInfo(1571, 1, 1, 1f, "Main item in a corruption biome dungeon chest when selected by the vanilla biome-chest sequence."));
				break;
			case ChestID.LockedCrimson:
				drops.Add(new ChestDropRateInfo(1569, 1, 1, 1f, "Main item in a crimson biome dungeon chest when selected by the vanilla biome-chest sequence."));
				break;
			case ChestID.LockedHallowed:
				drops.Add(new ChestDropRateInfo(1260, 1, 1, 1f, "Main item in a hallowed biome dungeon chest."));
				break;
			case ChestID.LockedIce:
				drops.Add(new ChestDropRateInfo(1572, 1, 1, 1f, "Main item in a snow biome dungeon chest."));
				break;
			case ChestID.LockedDungeonDesert:
				drops.Add(new ChestDropRateInfo(4607, 1, 1, 1f, "Main item in a desert biome dungeon chest."));
				break;
		}
	}

	internal static void GetDungeonBiomeChestInfo(int sequenceIndex, int chestCount, bool crimson, out ushort chestTileType, out int chestStyle, out int mainItemType)
	{
		chestTileType = TileID.Containers;
		chestStyle = 0;
		mainItemType = ItemID.None;

		switch (sequenceIndex) {
			case 0:
				chestStyle = 23;
				mainItemType = 1156;
				break;
			case 1:
			case 5 when chestCount > 5:
				bool useCorruptionChest = sequenceIndex == 1 ? !crimson : crimson;
				chestStyle = useCorruptionChest ? 24 : 25;
				mainItemType = useCorruptionChest ? 1571 : 1569;
				break;
			case 2:
				chestStyle = 26;
				mainItemType = 1260;
				break;
			case 3:
				chestStyle = 27;
				mainItemType = 1572;
				break;
			case 4:
				chestTileType = TileID.Containers2;
				chestStyle = 13;
				mainItemType = 4607;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
		}
	}
}