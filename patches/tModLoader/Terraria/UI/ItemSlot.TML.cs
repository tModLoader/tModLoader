using System;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Terraria.UI
{
	public partial class ItemSlot
	{
		private static bool AccessorySwap(Player player, Item item, ref Item result) {
			//TML: Rewrote ArmorSwap for accessories under the PR #1299 so it was actually readable. No vanilla functionality lost in transition
			accSlotToSwapTo = -1;
			var accLoader = LoaderManager.Get<AccessorySlotLoader>();
			var accessories = AccessorySlotLoader.ModSlotPlayer(player).exAccessorySlot;

			//TML: Check if there is an empty slot available in functional slots, and if not, track the last available slot
			for (int i = 3; i < 10; i++) {
				if (player.IsAValidEquipmentSlotForIteration(i)) {
					if (player.armor[i].type == 0 && ItemLoader.CanEquipAccessory(item, i, false)) {
						accSlotToSwapTo = i - 3;
						break;
					}
				}
			}

			//TML: Check our modded functional slots
			if (accSlotToSwapTo < 0) {
				for (int i = 0; i < accessories.Length / 2; i++) {
					if (accLoader.ModdedIsAValidEquipmentSlotForIteration(i)) {
						if (accessories[i].type == 0 && accLoader.CanAcceptItem(i, item) && ItemLoader.CanEquipAccessory(item, i, true)) {
							accSlotToSwapTo = i + 20;
							break;
						}
					}
				}
			}

			accLoader.ModifyDefaultSwapSlot(item, ref accSlotToSwapTo);

			accSlotToSwapTo = Math.Max(accSlotToSwapTo, 0);

			//TML: Check if there is an existing copy of the item in any slot (including vanity)
			// Will also replace wings with wings
			for (int j = 0; j < player.armor.Length; j++) {
				if (item.IsTheSameAs(player.armor[j]) && ItemLoader.CanEquipAccessory(item, j, false))
					accSlotToSwapTo = j - 3;

				if (j < 10 && item.wingSlot > 0 && player.armor[j].wingSlot > 0 && ItemLoader.CanEquipAccessory(item, j, false))
					accSlotToSwapTo = j - 3;
			}

			//TML: Do the same check for our modded slots
			for (int j = 0; j < accessories.Length; j++) {
				if (item.IsTheSameAs(accessories[j]) && accLoader.CanAcceptItem(j, item) && ItemLoader.CanEquipAccessory(item, j, true))
					accSlotToSwapTo = j + 20;

				if (j < accLoader.list.Count && item.wingSlot > 0 && accessories[j].wingSlot > 0 && accLoader.CanAcceptItem(j, item) && ItemLoader.CanEquipAccessory(item, j, true))
					accSlotToSwapTo = j + 20;
			}

			if (accSlotToSwapTo >= 20) {
				int num3 = accSlotToSwapTo - 20;
				if (isEquipLocked(accessories[num3].type)) {
					result =  item;
					return false;
				}
					

				result = accessories[num3].Clone();
				accessories[num3] = item.Clone();
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

		/// <summary>
		/// Alters the ItemSlot.DyeSwap method for modded slots; 
		/// Unfortunately, I (Solxan) couldn't ever get ItemSlot.DyeSwap invoked so pretty sure this and its vanilla code is defunct.
		/// Here in case someone proves my statement wrong later.
		/// </summary>
		private static Item ModSlotDyeSwap(Item item, out bool success) {
			Item item2 = item;
			var msPlayer = AccessorySlotLoader.ModSlotPlayer(Main.LocalPlayer);
			int dyeSlotCount = 0;
			var dyes = msPlayer.exDyesAccessory;

			for (int i = 0; i < dyeSlotCount; i++) {
				if (dyes[i].type == 0) {
					dyeSlotCount = i;
					break;
				}
			}

			if (dyeSlotCount >= msPlayer.SlotCount()) {
				success = false;
				return item2;
			}

			item2 = dyes[dyeSlotCount].Clone();
			dyes[dyeSlotCount] = item.Clone();

			SoundEngine.PlaySound(7);
			Recipe.FindRecipes();
			success = true;
			return item2;
		}
	}
}
