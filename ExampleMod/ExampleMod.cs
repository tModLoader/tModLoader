using ExampleMod.Content;
using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.NPCs;
using System.IO;
using System;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;
using Terraria.ModLoader;
using ExampleMod.Common.Systems;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public const string AssetPath = "ExampleMod/Assets/";
		public static ModKeybind RandomBuffKeybind;
		public static int ExampleCustomCurrencyId;

		public override void Load() {
			RandomBuffKeybind = KeybindLoader.RegisterKeybind(this, "Random Buff", "P");

			// Registers a new custom currency
			ExampleCustomCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.ExampleCustomCurrency(ModContent.ItemType<Content.Items.ExampleItem>(), 999L, "Mods.ExampleMod.Currencies.ExampleCustomCurrency"));
		}

		public override void Unload() {
			RandomBuffKeybind = null;
		}

		public override object Call(params object[] args) {
			// The following code allows other mods to "call" Example Mod data.
			// This allows mod developers to access Example Mod's data without having to set it a reference.
			// Mod calls are not exposed by default, so it will be up to you to publish appropriate calls for your mod, and what values they return.

			// Make sure the call doesn't include anything that could potentially cause exceptions.
			if (args is null) {
				throw new ArgumentNullException(nameof(args), "Arguments cannot be null!");
			}

			if (args.Length == 0) {
				throw new ArgumentException("Arguments cannot be empty!");
			}

			// Make sure that the argument is a string using pattern matching.
			// Since we only need one parameter, we'll take only the first item in the array.
			if (args[0] is string content) {
				// Returns the value provided by downedMinionBoss, if the argument calls for it.
				if (content == "downedMinionBoss") {
					return DownedBossSystem.downedMinionBoss;
				}
				// Returns the value provided by showMinionCount.
				else if (content == "showMinionCount") {
					return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
				}

				// Mod calls can also set data from our mod, as well as retrieve it.
				if (content == "setMinionCount") {
					// We need to make sure the call is provided with a value to set the field to.
					if (args[1] is bool minionSet) {
						Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount = minionSet; // We'll set the value to what the argument provided.
						// Optionally, you can return a value indicating that the assignment was successful.
						// return true;
					}
					else { // If it's not the type we need, we can't continue
						throw new Exception("Expected an argument of type bool when setting minion count, but got type " + args[1].GetType().Name + " instead."); // Tell the developer what type we need, and what we got instead.
					}
				}
			}

			// We can also do this with different data types.
			if (args[0] is int contentInt && contentInt == 4) {
				return ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount;
			}

			// If the arguments provided don't match anything we wanted to return a value for, we'll return null.
			// This value can be anything you would like to provide as a default value.
			return null;
		}

		//TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			ExampleModMessageType msgType = (ExampleModMessageType)reader.ReadByte();

			switch (msgType) {
				// This message syncs ExamplePlayer.exampleLifeFruits
				case ExampleModMessageType.ExamplePlayerSyncPlayer:
					byte playernumber = reader.ReadByte();
					ExampleLifeFruitPlayer examplePlayer = Main.player[playernumber].GetModPlayer<ExampleLifeFruitPlayer>();
					examplePlayer.exampleLifeFruits = reader.ReadInt32();
					// SyncPlayer will be called automatically, so there is no need to forward this data to other clients.
					break;
				case ExampleModMessageType.ExampleTeleportToStatue:
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

	internal enum ExampleModMessageType : byte
	{
		ExamplePlayerSyncPlayer,
		ExampleTeleportToStatue
	}
}