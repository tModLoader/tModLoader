using System;
using Terraria.ID;

namespace Terraria;

public partial class Chest
{
	// TML: #ModdedChestSizes. Newly placed chests get the slot count set for their tile type in TileID.Sets.ChestSlotCount.
	private static Chest CreatePlacedChest(int index, int x, int y)
	{
		Chest chest = CreateWorldChest(index, x, y);
		int slotCount = Math.Clamp(TileID.Sets.ChestSlotCount[Framing.GetTileSafely(x, y).TileType], 1, AbsoluteMaxItemsWeCanEverReachInAChestForNow);
		if (chest.maxItems != slotCount)
			chest.Resize(slotCount);

		return chest;
	}
}
