using ExampleMod.Content;
using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.NPCs;
using Steamworks;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public const string AssetPath = "ExampleMod/Assets/";

		public override void AddRecipes() => ExampleRecipes.Load(this);

		public override void Unload() => ExampleRecipes.Unload();

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
					if (Main.npc[reader.ReadByte()].modNPC is ExamplePerson person && person.npc.active) {
						person.StatueTeleport();
					}

					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}

		public override void AddResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
		//Lets you add your own sorting methods in Journey mode's Duplication menu, or change the sorting methods of existing items.
		//This is useful for items with custom damage classes, as well as specific themed items.
			if (item.modItem?.Mod == this) { 
				itemGroup = (ContentSamples.CreativeHelper.ItemGroup)1337; //This number is where the item sort is in relation to any other sorts added by vanilla or mods. To see the vanilla sorting numbers, refer to (insert wiki page here).
			};
			if (item.type == ModContent.ItemType<Content.Items.Placeable.ExampleTorch>()) {
				itemGroup = ContentSamples.CreativeHelper.ItemGroup.Torches; //Vanilla usually matches sorting methods with the right type of item, but sometimes, like with torches, it doesn't. Make sure to set whichever items manually if need be. 
			}
		}
	}
}

internal enum ExampleModMessageType : byte
{
	ExamplePlayerSyncPlayer,
	ExampleTeleportToStatue
}