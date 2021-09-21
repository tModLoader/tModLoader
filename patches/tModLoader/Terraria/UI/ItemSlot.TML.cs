using System;
using Terraria.ModLoader;

namespace Terraria.UI
{
	public partial class ItemSlot
	{
		private static bool AccessorySwap(Player player, Item item, out Item result) {
			//TML: Rewrote ArmorSwap for accessories under the PR #1299 so it was actually readable. No vanilla functionality lost in transition
			accSlotToSwapTo = -1;
			var accLoader = LoaderManager.Get<AccessorySlotLoader>();

			//TML: Check if there is an empty slot available, and if not, track the last available slot
			int num2 = 3;
			for (int i = 3; i < 10; i++) {
				if (player.IsAValidEquipmentSlotForIteration(i)) {
					if (player.armor[i].type == 0 && ItemLoader.CanEquipAccessory(item, i)) {
						accSlotToSwapTo = i - 3;
						break;
					}
				}
			}

			//TML: Check our modded slots
			if (accSlotToSwapTo < 0) {
				for (int i = 0; i < accLoader.list.Count; i++) {
					if (accLoader.ModdedIsAValidEquipmentSlotForIteration(i)) {
						if (AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[i].type == 0 && accLoader.CanAcceptItem(i, item)) {
							accSlotToSwapTo = i + 20;
							break;
						}
					}
				}
			}

			accSlotToSwapTo = Math.Max(accSlotToSwapTo, 0);

			//TML: Check if there is an existing copy of the item in any slot (including vanity)
			// Will also replace wings with wings
			for (int j = 0; j < player.armor.Length; j++) {
				if (item.IsTheSameAs(player.armor[j]) && ItemLoader.CanEquipAccessory(item, j))
					accSlotToSwapTo = j - 3;

				if (j < 10 && item.wingSlot > 0 && player.armor[j].wingSlot > 0 && ItemLoader.CanEquipAccessory(item, j))
					accSlotToSwapTo = j - 3;
			}

			//TML: Do the same check for our modded slots
			for (int j = 0; j < AccessorySlotLoader.ModSlotPlayer.exAccessorySlot.Length; j++) {
				if (item.IsTheSameAs(AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[j]) && accLoader.CanAcceptItem(j, item))
					accSlotToSwapTo = j + 20;

				if (j < accLoader.list.Count && item.wingSlot > 0 && AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[j].wingSlot > 0 && accLoader.CanAcceptItem(j, item))
					accSlotToSwapTo = j + 20;
			}

			if (accSlotToSwapTo >= 20) {
				int num3 = accSlotToSwapTo - 20;
				if (isEquipLocked(AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[num3].type)) {
					result =  item;
					return false;
				}
					

				result = AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[num3].Clone();
				AccessorySlotLoader.ModSlotPlayer.exAccessorySlot[num3] = item.Clone();
			}
			else {
				int num3 = 3 + accSlotToSwapTo;
				if (isEquipLocked(player.armor[num3].type)) {
					result = item;
					return false;
				}
					

				result = player.armor[num3].Clone();
				player.armor[num3] = item.Clone();
			}

			return true;
		}
	}
}
