using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Players
{
	// If we shift-click a gel, it changes its color.
	public class ExampleShiftClickSlotPlayer : ModPlayer
	{
		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			// Apply our changes if this item is in inventory and is gel
			if (context == ItemSlot.Context.InventoryItem && inventory[slot].type == ItemID.Gel) {
				inventory[slot].color = Main.DiscoColor; // Change the color of the item into a "random" color
				inventory[slot].rare = Main.rand.Next(ItemRarityID.Count); // Random rarity
				SoundEngine.PlaySound(SoundID.Item4); // Play mana crystal using sound

				// Block vanilla code so the item will not be picked up when it is clicked.
				return true;
			}
			return base.ShiftClickSlot(inventory, context, slot);
		}

		// Here we override the cursor style
		public override bool HoverSlot(Item[] inventory, int context, int slot) {
			// Apply our changes if this item is in inventory and is gel
			if (context == ItemSlot.Context.InventoryItem && inventory[slot].type == ItemID.Gel) {
				// If player is holding shift, use FavoriteStar texture, otherwise use CameraLight texture.
				Main.cursorOverride = ItemSlot.ShiftInUse ? CursorOverrideID.FavoriteStar : CursorOverrideID.CameraLight;

				// Block vanilla overriding.
				return true;
			}
			return base.HoverSlot(inventory, context, slot);
		}
	}
}
