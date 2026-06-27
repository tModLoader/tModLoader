using ExampleMod.Common.Systems;
using ExampleMod.Content.NPCs.TownPets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	public class ExampleTownPetLicense : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}

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

		public override bool? UseItem(Player player) {
			// Only do something if the License hasn't been used before or the Town Pet exists in the world.
			int npcType = ModContent.NPCType<ExampleTownPet>(); // The NPC Type for the Town Pet.
			if (player.ItemAnimationJustStarted && (!ExampleTownPetSystem.boughtExampleTownPet || NPC.AnyNPCs(npcType))) {
				if (player.whoAmI == Main.myPlayer) {
					ExampleTownPetUnlockOrExchangePet(ref ExampleTownPetSystem.boughtExampleTownPet, npcType, this.GetLocalizationKey("LicenseExampleTownPetUse")); // Modified NPC.UnlockOrExchangePet method.
				}
				return true;
			}
			return false;
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