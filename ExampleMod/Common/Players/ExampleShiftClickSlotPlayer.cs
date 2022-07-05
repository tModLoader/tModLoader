using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Players
{
	// If we hover the cursor over a gel, the cursor style will change.
	// If we shift-click it, it changes its color and rarity.
	// See GelGlobalItem.cs as well, we add a tooltip line for gel to indicate what will happen
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
				// If player is holding shift, use FavoriteStar texture to indicate that a special action will be performed
				if (ItemSlot.ShiftInUse) {
					Main.cursorOverride = CursorOverrideID.FavoriteStar;
					return true; // return true to prevent other things from overriding cursor
				}
			}
			return base.HoverSlot(inventory, context, slot);
		}
	}
}
