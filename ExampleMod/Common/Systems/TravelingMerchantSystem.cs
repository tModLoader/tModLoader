using ExampleMod.Content.NPCs;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	public class TravelingMerchantSystem : ModSystem
	{
		public override void PreUpdateWorld() {
			ExampleTravelingMerchant.UpdateTravelingMerchant();
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["shopItems"] = ExampleTravelingMerchant.shopItems;
			if (ExampleTravelingMerchant.spawnTime != double.MaxValue) {
				tag["spawnTime"] = ExampleTravelingMerchant.spawnTime;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.shopItems.AddRange(tag.Get<List<Item>>("shopItems"));
			if (!tag.TryGet("spawnTime", out ExampleTravelingMerchant.spawnTime)) {
				ExampleTravelingMerchant.spawnTime = double.MaxValue;
			}
		}

		public override void ClearWorld() {
			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.spawnTime = double.MaxValue;
		}

		public override void NetSend(BinaryWriter writer) {
			// Note that NetSend is called whenever WorldData packet is sent.
			// We use this so that shop items can easily be synced to joining players
			// We recommend modders avoid sending WorldData too often, or filling it with too much data, lest too much bandwidth be consumed sending redundant data repeatedly
			// Consider sending a custom packet instead of WorldData if you have a significant amount of data to synchronise

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
}
