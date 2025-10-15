using ExampleMod.Common.Players;
using ExampleMod.Common.Systems;
using ExampleMod.Content.CustomModType;
using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.Items.Weapons;
using ExampleMod.Content.NPCs;
using ExampleMod.Content.TileEntities;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	// The class is partial to organize similar code together to clarify what is related.
	// This class extends from the Mod class as seen in ExampleMod.cs. Make sure to extend from the mod class, ": Mod", in your own code if using this file as a template for you mods Mod class.
	partial class ExampleMod
	{
		internal enum MessageType : byte
		{
			ExampleStatIncreasePlayerSync,
			ExampleTeleportToStatue,
			ExampleDodge,
			ExampleTownPetUnlockOrExchange,
			ExampleResourceEffect,
			StartVictoryPose,
			CancelVictoryPose,
			SendCustomUseStylePlayerDirection,
		}

		// Override this method to handle network packets sent for this mod.
		// TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				// This message syncs ExampleStatIncreasePlayer.exampleLifeFruits and ExampleStatIncreasePlayer.exampleManaCrystals
				case MessageType.ExampleStatIncreasePlayerSync:
					byte playerNumber = reader.ReadByte();
					ExampleStatIncreasePlayer examplePlayer = Main.player[playerNumber].GetModPlayer<ExampleStatIncreasePlayer>();
					examplePlayer.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the other clients
						examplePlayer.SyncPlayer(-1, whoAmI, false);
					}
					break;
				case MessageType.ExampleTeleportToStatue:
					if (Main.npc[reader.ReadByte()].ModNPC is ExamplePerson person && person.NPC.active) {
						person.StatueTeleport();
					}

					break;
				case MessageType.ExampleDodge:
					ExampleDamageModificationPlayer.HandleExampleDodgeMessage(reader, whoAmI);
					break;
				case MessageType.ExampleTownPetUnlockOrExchange:
					// Call a custom function that we made in our License item.
					ExampleTownPetLicense.ExampleTownPetUnlockOrExchangePet(ref ExampleTownPetSystem.boughtExampleTownPet, ModContent.NPCType<Content.NPCs.TownPets.ExampleTownPet>(), ModContent.GetInstance<ExampleTownPetLicense>().GetLocalizationKey("LicenseExampleTownPetUse"));
					break;
				case MessageType.ExampleResourceEffect:
					ExampleResourcePlayer.HandleExampleResourceEffectMessage(reader, whoAmI);
					break;
				case MessageType.StartVictoryPose:
					VictoryPosePlayer.HandleStartVictoryPoseMessage(reader, whoAmI);
					break;
				case MessageType.SendCustomUseStylePlayerDirection:
					ExampleCustomUseStylePlayer.ReceiveDirection(reader, whoAmI);
					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}
}