using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.NPCs;
using System.IO;
using Terraria;

namespace ExampleMod
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	partial class ExampleMod
	{
		internal enum MessageType : byte
		{
			ExamplePlayerSyncPlayer,
			ExampleTeleportToStatue
		}

		// Override this method to handle network packets sent for this mod.
		//TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				// This message syncs ExamplePlayer.exampleLifeFruits
				case MessageType.ExamplePlayerSyncPlayer:
					byte playernumber = reader.ReadByte();
					ExampleLifeFruitPlayer examplePlayer = Main.player[playernumber].GetModPlayer<ExampleLifeFruitPlayer>();
					examplePlayer.exampleLifeFruits = reader.ReadInt32();
					// SyncPlayer will be called automatically, so there is no need to forward this data to other clients.
					break;
				case MessageType.ExampleTeleportToStatue:
					if (Main.npc[reader.ReadByte()].ModNPC is ExamplePerson person && person.NPC.active) {
						person.StatueTeleport();
					}

					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}
}