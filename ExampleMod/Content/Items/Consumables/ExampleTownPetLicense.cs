using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExampleMod.Content.NPCs.TownPets;
using ExampleMod.Common.Systems;

namespace ExampleMod.Content.Items.Consumables
{
	public class ExampleTownPetLicense : ModItem
	{
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.UseSound = SoundID.Item92;
			Item.width = 28;
			Item.height = 28;
			Item.maxStack = Item.CommonMaxStack;
			Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 5));
		}

		public override void OnConsumeItem(Player player) {
			int npcType = ModContent.NPCType<ExampleTownPet>(); // The NPC Type for the Town Pet.
			if (player.whoAmI == Main.myPlayer && player.itemAnimation > 0) {
				// Only do something if the License hasn't been used before or the Town Pet exists in the world.
				if (!ExampleTownPetSystem.boughtExampleTownPet || NPC.AnyNPCs(npcType)) {
					player.ApplyItemTime(Item); // Make it so the player uses the item for the useAnimation.
					ExampleTownPetUnlockOrExchangePet(ref ExampleTownPetSystem.boughtExampleTownPet, npcType, "Mods.ExampleMod.UI.LicenseExampleTownPetUse"); // Modified NPC.UnlockOrExchangePet method.
				}
			}
		}
		public override bool? UseItem(Player player) {
			// Only consume the item if it is going to do something.
			if (!ExampleTownPetSystem.boughtExampleTownPet || NPC.AnyNPCs(ModContent.NPCType<ExampleTownPet>())) {
				return true;
			}
			return null;
		}

		/// <summary>
		/// <br>The vanilla method NPC.UnlockOrExchangePet will not work for our modded Town Pets because the NetMessage only works with vanilla NPCs.</br>
		/// <br>This version uses a ModPacket for that instead.</br>
		/// </summary>
		/// <param name="petBoughtFlag">The bool that determines if the License has been used once. It doesn't really have anything to do with buying.</param>
		/// <param name="npcType">The NPC Type for the Town Pet.</param>
		/// <param name="textKeyForLicense">The localization path for when the License has been used for the first time.</param>
		public static void ExampleTownPetUnlockOrExchangePet(ref bool petBoughtFlag, int npcType, string textKeyForLicense) {
			Color color = new(50, 255, 130); // Chat message color.
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				if (!petBoughtFlag || NPC.AnyNPCs(npcType)) {
					// Send the ModPacket if used by a player in multiplayer so that other players can receive the change, too.
					// The ModPacket is handled in ExampleMod.Networking.cs
					ModPacket packet = ModContent.GetInstance<ExampleMod>().GetPacket();
					packet.Write((byte)ExampleMod.MessageType.ExampleTownPetUnlockOrExchange);
					packet.Send();
				}
			}
			else if (!petBoughtFlag) {
				petBoughtFlag = true; // the bool that is set and saved in our ModSystem class.
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(textKeyForLicense), color); // Send the chat message.
				NetMessage.TrySendData(MessageID.WorldData); // Sync the change for everyone.
			}
			else if (NPC.RerollVariationForNPCType(npcType)) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.PetExchangeSuccess"), color);
			}
			else {
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.PetExchangeFail"), color);
			}
		}
	}
}