using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModAccessorySlot1 : ModAccessorySlot
	{
		// If the class is empty, everything will default to a basic vanilla slot.
	}

	public class ExampleCustomLocationAndTextureSlot : ModAccessorySlot
	{
		// We will place the slot to be at the center of the map, making the decision not to follow the internal UI handling
		public override int XLoc => Main.screenWidth / 2;
		public override int YLoc => 3 * Main.screenHeight / 4;

		//TODO: Add an example of custom texture application in the following override.
		//public override void DrawModded(Item[] inv, int context, int slot, Vector2 position) => base.DrawModded(inv, context, slot, position);
	}

	public class ExampleModWingSlot : ModAccessorySlot
	{
		public override bool SlotCanAcceptItem(Item checkItem) {
			if (checkItem.wingSlot > 0) // if is Wing, then can go in slot
				return true;

			return false; // Otherwise nothing in slot
		}

		public override bool IsSlotValid() {
			if (Main.LocalPlayer.armor[0].headSlot >= 0) // if player is wearing a helmet, because flight safety
				return true; // Then Display Slot

			return false; // Don't display slot
		}

		// Overrides the default behaviour to show an accessory slot when there is an item in it despite not meeting condition in IsSlotValid()
		public override bool IsSlotVisibleButNotValid() {
			return false; // We set to false to just not display if not valid.
		}
	}
}
