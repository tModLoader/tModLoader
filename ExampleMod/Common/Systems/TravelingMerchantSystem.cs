using ExampleMod.Content.NPCs;
using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems;

public class TravelingMerchantSystem : ModSystem
{
	public override void PreUpdateWorld() {
		ExampleTravelingMerchant.UpdateTravelingMerchant();
	}

	public override void SaveWorldData(TagCompound tag) {
		if (ExampleTravelingMerchant.spawnTime != double.MaxValue) {
			tag["ExampleTravelingMerchantSpawnTime"] = ExampleTravelingMerchant.spawnTime;
		}
	}

	public override void LoadWorldData(TagCompound tag) {
		if (!tag.TryGet("ExampleTravelingMerchantSpawnTime", out ExampleTravelingMerchant.spawnTime)) {
			ExampleTravelingMerchant.spawnTime = double.MaxValue;
		}
	}
}