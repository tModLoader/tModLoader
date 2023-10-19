using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

// Test in Multiplayer, suspect there is some issue with synchronization of unloaded slots
public sealed class ModAccessorySlotPlayer : ModPlayer
{
	internal static AccessorySlotLoader Loader => LoaderManager.Get<AccessorySlotLoader>();

	// Arrays for modded accessory slot save/load/usage. Used in DefaultPlayer.
	internal Item[] exAccessorySlot;
	internal Item[] exDyesAccessory;
	internal bool[] exHideAccessory;
	internal Dictionary<string, int> slots = new Dictionary<string, int>();

	// Setting toggle for stack or scroll accessories/npcHousing
	internal bool scrollSlots;
	internal int scrollbarSlotPosition;

	public int SlotCount => slots.Count;
	public int LoadedSlotCount => Loader.TotalCount;

	public ModAccessorySlotPlayer()
	{
		foreach (var slot in Loader.list) {
			slots.Add(slot.FullName, slot.Type);
		}

		ResetAndSizeAccessoryArrays();
	}

	internal void ResetAndSizeAccessoryArrays()
	{
		int size = slots.Count;
		exAccessorySlot = new Item[2 * size];
		exDyesAccessory = new Item[size];
		exHideAccessory = new bool[size];

		for (int i = 0; i < size; i++) {
			exDyesAccessory[i] = new Item();
			exHideAccessory[i] = false;

			exAccessorySlot[i * 2] = new Item();
			exAccessorySlot[i * 2 + 1] = new Item();
		}
	}

	public override void SaveData(TagCompound tag)
	{
		// TODO, might be nice to only save acc slots which have something in them... particularly if they're unloaded. Otherwise old unloaded slots just bloat the array with empty entries forever
		tag["order"] = slots.Keys.ToList();
		tag["items"] = exAccessorySlot.Select(ItemIO.Save).ToList();
		tag["dyes"] = exDyesAccessory.Select(ItemIO.Save).ToList();
		tag["visible"] = exHideAccessory.ToList();
	}

	public override void LoadData(TagCompound tag)
	{
		// Scan the saved slot names and add ids for any unloaded slots
		var order = tag.GetList<string>("order").ToList();
		foreach (var name in order) {
			if (!slots.ContainsKey(name))
				slots.Add(name, slots.Count);
		}

		ResetAndSizeAccessoryArrays();


		var items = tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList();
		var dyes = tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList();
		var visible = tag.GetList<bool>("visible").ToList();

		for (int i = 0; i < order.Count; i++) {
			int type = slots[order[i]];

			// Place loaded items in to the correct slot
			exDyesAccessory[type] = dyes[i];
			exHideAccessory[type] = visible[i];
			exAccessorySlot[type] = items[i];
			exAccessorySlot[type + SlotCount] = items[i + order.Count];
		}
	}

	// Updates Code:
	/// <summary>
	/// Updates functional slot visibility information on the player for Mod Slots, in a similar fashion to Player.UpdateVisibleAccessories()
	/// </summary>
	public override void UpdateVisibleAccessories()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(k, Player)) {
				Player.UpdateVisibleAccessories(exAccessorySlot[k], exHideAccessory[k], k, true);
			}
		}
	}

	/// <summary>
	/// Updates vanity slot information on the player for Mod Slots, in a similar fashion to Player.UpdateVisibleAccessories()
	/// </summary>
	public override void UpdateVisibleVanityAccessories()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(k, Player)) {
				var vanitySlot = k + SlotCount;
				if (!Player.ItemIsVisuallyIncompatible(exAccessorySlot[vanitySlot]))
					Player.UpdateVisibleAccessory(vanitySlot, exAccessorySlot[vanitySlot], true);
			}
		}
	}

	/// <summary>
	/// Mirrors Player.UpdateDyes() for modded slots
	/// Runs On Player Select, so is Player instance sensitive!!!
	/// </summary>
	public void UpdateDyes(bool socialSlots)
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		// Called manually, this method does not override ModPlayer.UpdateDyes.
		int start = socialSlots ? SlotCount : 0;
		int end  = socialSlots ? SlotCount * 2 : SlotCount;

		for (int i = start; i < end; i++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player)) {
				int num = i % exDyesAccessory.Length;
				Player.UpdateItemDye(i < exDyesAccessory.Length, exHideAccessory[num], exAccessorySlot[i], exDyesAccessory[num]);
			}
		}
	}

	/// <summary>
	/// Runs a simplified version of Player.UpdateEquips for the Modded Accessory Slots
	/// </summary>
	public override void UpdateEquips()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(k, Player)) {
				loader.CustomUpdateEquips(k, Player);
			}
		}
	}

	// Death drops code, should run prior to dropping other items in case conditions are used based on player's current equips
	public void DropItems(IEntitySource itemSource)
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();
		var pos = Player.position + Player.Size / 2;
		for (int i = 0; i < SlotCount; i++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player)) {
				Player.DropItem(itemSource, pos, ref exAccessorySlot[i]);
				Player.DropItem(itemSource, pos, ref exAccessorySlot[i + SlotCount]);
				Player.DropItem(itemSource, pos, ref exDyesAccessory[i]);
			}
		}
	}

	// The following netcode is adapted from ChickenBones' UtilitySlots:
	public override void CopyClientState(ModPlayer targetCopy)
	{
		var defaultInv = (ModAccessorySlotPlayer)targetCopy;
		for (int i = 0; i < LoadedSlotCount; i++) {
			exAccessorySlot[i].CopyNetStateTo(defaultInv.exAccessorySlot[i]);
			exAccessorySlot[i + SlotCount].CopyNetStateTo(defaultInv.exAccessorySlot[i + LoadedSlotCount]);
			exDyesAccessory[i].CopyNetStateTo(defaultInv.exDyesAccessory[i]);
			defaultInv.exHideAccessory[i] = exHideAccessory[i];
		}
	}

	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
		for (int i = 0; i < LoadedSlotCount; i++) {
			NetHandler.SendSlot(toWho, Player.whoAmI, i, exAccessorySlot[i]);
			NetHandler.SendSlot(toWho, Player.whoAmI, i + LoadedSlotCount, exAccessorySlot[i + SlotCount]);
			NetHandler.SendSlot(toWho, Player.whoAmI, -i - 1, exDyesAccessory[i]);
			NetHandler.SendVisualState(toWho, Player.whoAmI, i, exHideAccessory[i]);
		}
	}

	public override void SendClientChanges(ModPlayer clientPlayer)
	{
		var clientInv = (ModAccessorySlotPlayer)clientPlayer;
		for (int i = 0; i < LoadedSlotCount; i++) {
			if (exAccessorySlot[i].IsNetStateDifferent(clientInv.exAccessorySlot[i]))
				NetHandler.SendSlot(-1, Player.whoAmI, i, exAccessorySlot[i]);

			if (exAccessorySlot[i + SlotCount].IsNetStateDifferent(clientInv.exAccessorySlot[i + LoadedSlotCount]))
				NetHandler.SendSlot(-1, Player.whoAmI, i + LoadedSlotCount, exAccessorySlot[i + SlotCount]);

			if (exDyesAccessory[i].IsNetStateDifferent(clientInv.exDyesAccessory[i]))
				NetHandler.SendSlot(-1, Player.whoAmI, -i - 1, exDyesAccessory[i]);

			if (exHideAccessory[i] != clientInv.exHideAccessory[i])
				NetHandler.SendVisualState(-1, Player.whoAmI, i, exHideAccessory[i]);
		}
	}

	internal static class NetHandler
	{
		public const byte InventorySlot = 1;
		public const byte VisualState = 2;

		public const byte Server = 2;
		public const byte Client = 1;
		public const byte SP = 0;

		public static void SendSlot(int toWho, int plr, int slot, Item item)
		{
			new AccessorySlotInventorySlotNetPacket {
				Player = (byte)plr,
				Slot = (sbyte)slot,
				Item = item
			}.Send(toWho, plr);
		}

		public static void SendVisualState(int toWho, int plr, int slot, bool hideVisual)
		{
			new AccessorySlotVisualStateNetPacket {
				Player = (byte)plr,
				Slot = (sbyte)slot,
				HideVisual = hideVisual
			}.Send(toWho, plr);
		}

		public static void SetSlot(sbyte slot, Item item, ModAccessorySlotPlayer dPlayer)
		{
			if (slot < 0)
				dPlayer.exDyesAccessory[-(slot + 1)] = item;
			else
				dPlayer.exAccessorySlot[slot] = item;
		}
	}
}
