using ExampleMod.Content.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModAccessorySlot1 : ModAccessorySlot
	{
		// If the class is empty, everything will default to a basic vanilla slot.
	}

	public class ExampleModAccessorySlot2 : ModAccessorySlot
	{
		// If the class is empty, everything will default to a basic vanilla slot.
	}

	public class ExampleModWingSlot : ModAccessorySlot
	{
		public override bool LimitWhatCanGoInSlot(Item checkItem) {
			if (checkItem.wingSlot > 0) // if is Wing, then can go in slot
				return true;

			return false; // Otherwise nothing in slot
		}

		public override bool CanUseSlot() {
			if (Main.LocalPlayer.armor[0].headSlot >= 0) // if player is wearing a helmet, because flight safety
				return true; // Then Display Slot

			return false; // Don't display slot
		}
	}
}
