using System;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace Terraria.UI;

public partial class ItemSlot
{
	private static bool IsAccessoryContext(int context) => Math.Abs(context) is Context.EquipAccessory or Context.EquipAccessoryVanity;
	internal static bool IsModdedLoadoutShareContext(int context) => context is Context.ModdedAccessorySlot or Context.ModdedVanityAccessorySlot or Context.ModdedDyeSlot;
	internal static bool CanModdedLoadoutShare(int context, int slot) => IsModdedLoadoutShareContext(context) && AccessorySlotLoader.ModSlotPlayer(Main.LocalPlayer).CanLoadoutShare(context, slot);

	/* TODO: Restore functionality in ArmorSwap. Try new approach rather than just fixing compile issues.
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
	*/

	private static void SwapDyeEquip_TML(Player player, Item[] inventory, int context, int slot)
	{
		inventory[slot] = DyeSwap_TML(player, inventory[slot], out var targetDyes, out var targetSlot);
		if (targetSlot < 0)
			return;

		Main.EquipPageSelected = 0;
		AchievementsHelper.HandleOnEquip(player, targetDyes[targetSlot], Context.EquipDye);
		int targetContext = targetDyes == player.dye ? Context.EquipDye : Context.ModdedDyeSlot;
		DisplayTransfer_TwoWay(inventory, context, slot, targetDyes, targetContext, targetSlot);
	}

	private static Item DyeSwap_TML(Player player, Item item, out Item[] targetDyes, out int targetSlot)
	{
		Item[] moddedDyes = AccessorySlotLoader.ModSlotPlayer(player).exDyesAccessory;
		int vanillaSlotCount = player.dye.Length;
		int slotCount = vanillaSlotCount + moddedDyes.Length;
		int candidate = -1;

		// Fill an empty slot before replacing an equipped dye.
		for (int i = 0; i < slotCount; i++) {
			Item dye = i < vanillaSlotCount ? player.dye[i] : moddedDyes[i - vanillaSlotCount];
			if (dye.IsAir && CanSwapDye(player, item, i)) {
				candidate = i;
				break;
			}
		}

		if (candidate < 0) {
			for (int i = 0; i < slotCount; i++) {
				int index = (dyeSwapCounter + i) % slotCount;
				if (CanSwapDye(player, item, index)) {
					candidate = index;
					break;
				}
			}
		}

		if (candidate < 0) {
			targetDyes = player.dye;
			targetSlot = -1;
			return item;
		}

		bool modded = candidate >= vanillaSlotCount;
		targetDyes = modded ? moddedDyes : player.dye;
		targetSlot = modded ? candidate - vanillaSlotCount : candidate;
		SoundEngine.PlaySound(7);
		SwapClearFavorite(ref item, ref targetDyes[targetSlot]);
		dyeSwapCounter = (candidate + 1) % slotCount;
		return item;
	}

	private static bool CanSwapDye(Player player, Item item, int slot)
	{
		return slot < player.dye.Length
			? player.IsItemSlotUnlockedAndUsable(slot)
			: CanEquipInModdedDyeSlot(player, item, slot - player.dye.Length);
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
		/*
		if (isEquipLocked(item.type))
			return true;
		*/

		if (slot != -1) {
			if (!itemCollection[slot].IsNotTheSameAs(item))
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
			if (!item.IsNotTheSameAs(itemCollection[i]))
				return true;
		}

		return !ItemLoader.CanEquipAccessory(player, item, slot >= 20 ? slot - 20 : slot, slot >= 20);
	}

	private static bool CanEquipInModdedAccessorySlot(Item checkItem, int slot, int context)
	{
		if (!checkItem.accessory)
			return false;

		AccessorySlotLoader accessorySlotLoader = LoaderManager.Get<AccessorySlotLoader>();
		return accessorySlotLoader.ModdedIsSpecificItemSlotUnlockedAndUsable(slot, Main.LocalPlayer, accessorySlotLoader.ContextToEnum(context))
			&& accessorySlotLoader.ModSlotCheck(checkItem, slot, context);
	}

	private static bool CanEquipInModdedDyeSlot(Player player, Item item, int slot)
	{
		AccessorySlotLoader loader = LoaderManager.Get<AccessorySlotLoader>();
		return item.dye > 0
			&& loader.ModdedIsSpecificItemSlotUnlockedAndUsable(slot, player, AccessorySlotType.DyeSlot)
			&& loader.CanPlayerAcceptItem(player, slot, item, Context.ModdedDyeSlot);
	}

	private static int GetSlotColorContext(int context) => context switch {
		Context.ModdedAccessorySlot => Context.EquipAccessory,
		Context.ModdedVanityAccessorySlot => Context.EquipAccessoryVanity,
		Context.ModdedDyeSlot => Context.EquipDye,
		_ => context
	};
}
