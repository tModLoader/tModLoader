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
 		tag["shopItems"] = ExampleTravelingMerchant.shopItems;
		if (ExampleTravelingMerchant.spawnTime != double.MaxValue) {
			tag["ExampleTravelingMerchantSpawnTime"] = ExampleTravelingMerchant.spawnTime;
		}
	}

	public override void LoadWorldData(TagCompound tag) {
        ExampleTravelingMerchant.shopItems.Clear();
        ExampleTravelingMerchant.shopItems.AddRange(tag.Get<List<Item>>("shopItems"));
  		if (!tag.TryGet("ExampleTravelingMerchantSpawnTime", out ExampleTravelingMerchant.spawnTime)) {
			ExampleTravelingMerchant.spawnTime = double.MaxValue;
		}
	}

    public override void ClearWorld() {
        ExampleTravelingMerchant.shopItems.Clear();
        ExampleTravelingMerchant.spawnTime = double.MaxValue;
    }
 
    public override void NetSend(BinaryWriter writer) {
        writer.Write(ExampleTravelingMerchant.shopItems.Count);
        foreach (Item item in ExampleTravelingMerchant.shopItems) {
            ItemIO.Send(item, writer, writeStack: true);
        }
    }

    public override void NetReceive(BinaryReader reader) {
        ExampleTravelingMerchant.shopItems.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++) {
            ExampleTravelingMerchant.shopItems.Add(ItemIO.Receive(reader, readStack: true));
        }
    }
}
