using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Weapons;
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
		}

		public override void SetDefaults() {
			Item.height = 12;
			Item.width = 12;
		}

		public override bool OnPickup(WorldItem item, Player player) {
			// When the item is picked up, heal the player's ExampleResource stat and spawn and sync the corresponding CombatText
			player.GetModPlayer<ExampleResourcePlayer>().HealExampleResource(ExampleResourceHealAmount);

			// We need to play this ourselves since we are returning false meaning it won't play automatically.
			SoundEngine.PlaySound(SoundID.Grab, player.Center);

			// We return false to prevent the item from going into the players inventory.
			return false;
		}

		// Since ItemID.Sets.IsAPickup is true, we don't need to override the ItemSpace hook to allow picking up the item when inventory is full

		// We can override CanPickup to prevent attempting to pick up this item when at max ExampleResource, but hearts and stars do not do this so we won't either.

		// In the case of mana stars, the pickup *from a distance* is prevented entirely based on some conditions
		public override bool GrabStyle(WorldItem item, Player player) {
			// The item should only be picked up from a distance if at least one of these are true:
			// - The player is using a resource magnet
			// - The player is holding a class item
			// - The player is not at maximum resource
			bool canPickup = false;
			var modPlayer = player.GetModPlayer<ExampleResourcePlayer>();
			if (modPlayer.exampleResourceMagnet) {
				canPickup = true;
			}
			else if (modPlayer.exampleResourceCurrent < modPlayer.exampleResourceMax2) {
				canPickup = true;
			}
			// If you have a custom damage class, you would do this check here
			// else if (player.HeldItem.CountsAsClass(DamageClass.Magic)) {
			// 	canPickup = true;
			// }
			// Instead, for showcasing that it works, we check the ExampleCustomResourceWeapon directly
			else if (player.HeldItem.type == ModContent.ItemType<ExampleCustomResourceWeapon>()) {
				canPickup = true;
			}

			// Returning true indicates that we want to skip the vanilla grab style. Since we don't do anything extra here, the grabbing is simply prevented
			// Do note that it does not prevent pickup, only the pull effect from a short distance. If the player touches the item, it still gets picked up
			return !canPickup;
		}

		// GrabRange can be used to implement effects similar to Heartreach potion or Celestial Magnet.
		public override void GrabRange(WorldItem item, Player player, ref int grabRange) {
			if (player.GetModPlayer<ExampleResourcePlayer>().exampleResourceMagnet) {
				grabRange += ExampleResourcePlayer.exampleResourceMagnetGrabRange;
			}
		}
	}
}
