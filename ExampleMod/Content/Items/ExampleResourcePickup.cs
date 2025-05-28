using ExampleMod.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// This class showcases a "pickup". Also known as a power-up.
	// Pickup refers to items that don't enter then inventory when picked up, but rather have some other effect when obtained.
	// Pickups usually provide resources to the player, such as hearts providing life or stars providing mana. Nebula armor boosters are another example.
	// This example drops from enemies when Example Resource is low, similar to how hearts and stars only drop if the player is lacking health or mana.
	// See ExampleResourcePickupGlobalNPC for the item drop code.
	public class ExampleResourcePickup : ModItem
	{
		public static readonly int ExampleResourceHealAmount = 50;

		public override LocalizedText Tooltip => LocalizedText.Empty;

		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			ItemID.Sets.IsAPickup[Type] = true;
			ItemID.Sets.ItemSpawnDecaySpeed[Type] = 4;
		}

		public override void SetDefaults() {
			Item.height = 12;
			Item.width = 12;
		}

		public override bool OnPickup(Player player) {
			// When the item is picked up, heal the player's ExampleResource stat and spawn and sync the corresponding CombatText
			player.GetModPlayer<ExampleResourcePlayer>().HealExampleResource(ExampleResourceHealAmount);

			// We need to play this ourselves since we are returning false meaning it won't play automatically.
			SoundEngine.PlaySound(SoundID.Grab, player.Center);

			// We return false to prevent the item from going into the players inventory.
			return false;
		}

		// Since ItemID.Sets.IsAPickup is true, we don't need to override the ItemSpace hook to allow picking up the item when inventory is full

		// We can override CanPickup to prevent attempting to pick up this item when at max ExampleResource, but hearts and stars do not do this so we won't either.

		// GrabRange can be used to implement effects similar to Heartreach potion or Celestial Magnet.
		public override void GrabRange(Player player, ref int grabRange) {
			if (player.GetModPlayer<ExampleResourcePlayer>().exampleResourceMagnet) {
				grabRange += ExampleResourcePlayer.exampleResourceMagnetGrabRange;
			}
		}
	}
}
