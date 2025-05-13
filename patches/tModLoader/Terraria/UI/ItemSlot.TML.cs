using System;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Terraria.UI;

public partial class ItemSlot
{
	private static bool AccessorySwap(Player player, Item item, ref Item result)
	{
		//TML: Rewrote ArmorSwap for accessories under the PR #1299 so it was actually readable. No vanilla functionality lost in transition
		accSlotToSwapTo = -1;
		var accLoader = LoaderManager.Get<AccessorySlotLoader>();
		var accessories = AccessorySlotLoader.ModSlotPlayer(player).exAccessorySlot;

		//TML: Check if there is an empty slot available in functional slots, and if not, track the last available slot
		for (int i = 3; i < 10; i++) {
			if (player.IsItemSlotUnlockedAndUsable(i)) {
				if (player.armor[i].type == 0 && ItemLoader.CanEquipAccessory(item, i, false)) {
					accSlotToSwapTo = i - 3;
					break;
				}
			}
		}

		//TML: Check our modded functional slots
		if (accSlotToSwapTo < 0) {
			for (int i = 0; i < accessories.Length / 2; i++) {
				if (accLoader.ModdedIsSpecificItemSlotUnlockedAndUsable(i, player, vanity: false)) {
					if (accessories[i].type == 0 && accLoader.CanAcceptItem(i, item, (int)Context.ModdedAccessorySlot) && ItemLoader.CanEquipAccessory(item, i, true)) {
						accSlotToSwapTo = i + 20;
						break;
					}
				}
			}
		}

		accLoader.ModifyDefaultSwapSlot(item, ref accSlotToSwapTo);

		//TML: Check if there is an existing copy of the item in any slot (including vanity)
		// Will also replace wings with wings
		for (int j = 0; j < player.armor.Length; j++) {
			if (item.IsTheSameAs(player.armor[j]) && ItemLoader.CanEquipAccessory(item, j, false))
				accSlotToSwapTo = j - 3;

			if (j < 10 && (item.wingSlot > 0 && player.armor[j].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(player.armor[j], item)) && ItemLoader.CanEquipAccessory(item, j, false))
				accSlotToSwapTo = j - 3;
		}

		//TML: Do the same check for our modded slots
		for (int j = 0; j < accessories.Length; j++) {
			if (item.IsTheSameAs(accessories[j]) && accLoader.CanAcceptItem(j, item, j < accessories.Length / 2 ? (int)Context.ModdedAccessorySlot : (int)Context.ModdedVanityAccessorySlot) && ItemLoader.CanEquipAccessory(item, j, true))
				accSlotToSwapTo = j + 20;

			if (j < accLoader.list.Count && (item.wingSlot > 0 && accessories[j].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(accessories[j], item))
				&& accLoader.CanAcceptItem(j, item, j < accessories.Length / 2 ? (int)Context.ModdedAccessorySlot : (int)Context.ModdedVanityAccessorySlot) && ItemLoader.CanEquipAccessory(item, j, true))
				accSlotToSwapTo = j + 20;
		}

		// No slot found, and it can't go in slot zero, than return
		if (accSlotToSwapTo == -1 && !ItemLoader.CanEquipAccessory(item, 0, false))
			return false;

		accSlotToSwapTo = Math.Max(accSlotToSwapTo, 0);
		if (accSlotToSwapTo >= 20) {
			int num3 = accSlotToSwapTo - 20;
			if (isEquipLocked(accessories[num3].type)) {
				result =  item;
				return false;
			}

			if (!accLoader.ModSlotCheck(item, num3, Context.ModdedAccessorySlot)) {
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
	private static Item ModSlotDyeSwap(Item item, out bool success)
	{
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

		if (dyeSlotCount >= msPlayer.SlotCount) {
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

	// Copy of Acc check, but runs hooks which take the local player as a context.
	/// <inheritdoc cref="AccCheck_ForPlayer(Player, Item[], Item, int)"/>
	internal static bool AccCheck_ForLocalPlayer(Item[] itemCollection, Item item, int slot) => AccCheck_ForPlayer(Main.LocalPlayer, itemCollection, item, slot);

	/// <summary>
	/// Checks if placing <paramref name="item"/> into index <paramref name="slot"/> of <paramref name="itemCollection"/> works or not. <paramref name="itemCollection"/> corresponds to the <paramref name="player"/>'s accessories. 
	/// <br/><br/> This is a replacement for <see cref="AccCheck(Item[], Item, int)"/> that takes into account player-specific checks and hooks.
	/// <br/><br/> Returns true if can't equip and false if placing the accessory is ok. 
	/// </summary>
	internal static bool AccCheck_ForPlayer(Player player, Item[] itemCollection, Item item, int slot)
	{
		if (isEquipLocked(item.type))
			return true;

		if (slot != -1) {
			if (itemCollection[slot].IsTheSameAs(item))
				return false;

			if (itemCollection[slot].wingSlot > 0 && item.wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(player, itemCollection[slot], item))
				return !ItemLoader.CanEquipAccessory(player, item, slot >= 20 ? slot - 20 : slot, slot >= 20);
		}

		var modSlotPlayer = AccessorySlotLoader.ModSlotPlayer(player);
		var modCount = modSlotPlayer.SlotCount;
		bool targetVanity = slot >= 20 && (slot >= modCount + 20) || slot < 20 && slot >= 10;

		for (int i = targetVanity ? 13 : 3; i < (targetVanity ? 20 : 10); i++) {
			if (!targetVanity && item.wingSlot > 0 && itemCollection[i].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(player, itemCollection[i], item))
				return true;
		}

		for (int i = (targetVanity ? modCount : 0) + 20; i < (targetVanity ? modCount * 2 : modCount) + 20; i++) {
			if (!targetVanity && item.wingSlot > 0 && itemCollection[i].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(player, itemCollection[i], item))
				return true;
		}

		for (int i = 0; i < itemCollection.Length; i++) {
			if (item.IsTheSameAs(itemCollection[i]))
				return true;
		}

		return !ItemLoader.CanEquipAccessory(player, item, slot >= 20 ? slot - 20 : slot, slot >= 20);
	}
}
