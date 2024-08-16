using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.Default;

// Test in Multiplayer, suspect there is some issue with synchronization of unloaded slots
public sealed class ModAccessorySlotPlayer : ModPlayer
{
	private const int SharedLoadoutIndex = -1;
	private static AccessorySlotLoader Loader => LoaderManager.Get<AccessorySlotLoader>();

	private readonly Dictionary<string, (int SlotType, bool HasLoadoutSupport)> slots = [];
	private readonly HashSet<int> sharedLoadoutSlotTypes = [];
	private readonly ExEquipmentLoadout sharedLoadout;
	private ExEquipmentLoadout[] exLoadouts;

	// Setting toggle for stack or scroll accessories/npcHousing
	internal bool scrollSlots;
	internal int scrollbarSlotPosition;

	public int SlotCount => slots.Count;
	public int LoadedSlotCount => Loader.TotalCount;
	public int ModdedCurrentLoadoutIndex { get; private set; }

	private ExEquipmentLoadout CurrentLoadout => exLoadouts[ModdedCurrentLoadoutIndex];

	public ModAccessorySlotPlayer()
	{
		foreach (var slot in Loader.list) {
			slots.Add(slot.FullName, (slot.Type, slot.HasEquipmentLoadoutSupport));

			if (!slot.HasEquipmentLoadoutSupport) {
				sharedLoadoutSlotTypes.Add(slot.Type);
				sharedLoadoutSlotTypes.Add(slot.Type + Loader.list.Count);
			}
		}

		sharedLoadout = new ExEquipmentLoadout(SharedLoadoutIndex, SlotCount, new EquipmentLoadout());
	}

	public override void Initialize()
	{
		exLoadouts = Enumerable.Range(0, Player.Loadouts.Length)
			.Select(loadoutIndex => new ExEquipmentLoadout(loadoutIndex, SlotCount, Player.Loadouts[loadoutIndex]))
			.ToArray();
	}

	/// <summary>
	/// Registers an additional equipment loadout. By default, only the vanilla equipment loadouts are supported.
	/// If a mod adds additional loadouts, it must call this method for each one to add loadout support for any
	/// <see cref="ModAccessorySlot"/> instances.
	/// If a player changes to one of the additional loadouts, <see cref="PlayerLoader.OnEquipmentLoadoutSwitched"/> must
	/// be called with the corresponding index for the loadout returned by this method.
	/// </summary>
	/// <param name="loadout">
	/// The additional loadout to be registered. This reference is kept to check for any conflicts with other
	/// accessories if the player tries to equip an accessory. It must therefore point to the actual loadout instance,
	/// which keeps track of any equipped items, for the lifetime of this <see cref="ModAccessorySlotPlayer"/> instance.
	/// </param>
	/// <returns>The loadout index</returns>
	public int RegisterAdditionalEquipmentLoadout(EquipmentLoadout loadout)
	{
		Array.Resize(ref exLoadouts, exLoadouts.Length + 1);
		ExEquipmentLoadout newLoadout = new(exLoadouts.Length - 1, SlotCount, loadout);
		exLoadouts[^1] = newLoadout;

		return newLoadout.LoadoutIndex;
	}

	public override void SaveData(TagCompound tag)
	{
		// TODO, might be nice to only save acc slots which have something in them... particularly if they're unloaded. Otherwise old unloaded slots just bloat the array with empty entries forever
		tag["loadout"] = ModdedCurrentLoadoutIndex;
		tag["order"] = slots.Keys.ToList();

		sharedLoadout.SaveData(tag);

		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			equipmentLoadout.SaveData(tag);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		ModdedCurrentLoadoutIndex = tag.TryGet("loadout", out int loadoutIndex) ? loadoutIndex : Player.CurrentLoadoutIndex;

		// Scan the saved slot names and add ids for any unloaded slots
		var order = tag.GetList<string>("order").ToList();
		foreach (var name in order) {
			if (!slots.ContainsKey(name))
				slots.Add(name, (slots.Count, true));
		}

		IReadOnlyList<SlotInfo> noLongerSharedSlots = sharedLoadout.LoadData(tag, order, slots);

		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			equipmentLoadout.LoadData(tag, order, slots);

			if (equipmentLoadout.LoadoutIndex == 0) {
				AddPreviouslySharedItemsToLoadout(noLongerSharedSlots, equipmentLoadout);
			}
		}
	}

	private void AddPreviouslySharedItemsToLoadout(
		IReadOnlyList<SlotInfo> noLongerSharedSlots,
		ExEquipmentLoadout equipmentLoadout)
	{
		foreach (SlotInfo slotInfo in noLongerSharedSlots) {
			equipmentLoadout.ExDyesAccessory[slotInfo.Slot] = slotInfo.Dye;
			equipmentLoadout.ExAccessorySlot[slotInfo.Slot + SlotCount] = slotInfo.VanityItem;
			equipmentLoadout.ExAccessorySlot[slotInfo.Slot] = slotInfo.Accessory;
			equipmentLoadout.ExHideAccessory[slotInfo.Slot] = slotInfo.HideAccessory;
		}
	}

	/// <summary>
	/// Returns a list of all items including items from non-modded slots for the loadout with the given <paramref name="loadoutIndex"/>.
	/// The list contains items from vanilla slots first, followed by items from modded slots.
	/// Do not modify this array to set an item for a loadout, it will have no effect.
	/// </summary>
	/// <param name="loadoutIndex">The loadout index</param>
	/// <returns>A list of all items including items from non-modded slots for the loadout with the given <paramref name="loadoutIndex"/></returns>
	internal Item[] GetAllAccessoriesForLoadout(int loadoutIndex)
	{
		return [
			..(loadoutIndex == ModdedCurrentLoadoutIndex ? Player.armor : exLoadouts[loadoutIndex].LoadoutReference.Armor),
			..GetAllModSlotAccessoriesForLoadout(loadoutIndex),
		];
	}

	/// <summary>
	/// Returns a list of all items including items from non-modded slots for the current loadout.
	/// The list contains items from vanilla slots first, followed by items from modded slots.
	/// Do not modify this array to set an item for a loadout, it will have no effect.
	/// </summary>
	/// <returns>A list of all items including items from non-modded slots for the current loadout.</returns>
	internal Item[] GetAllAccessoriesForCurrentLoadout() => GetAllAccessoriesForLoadout(ModdedCurrentLoadoutIndex);

	/// <summary>
	/// Returns a list of all items from mod slots including from shared slots for the loadout with the given <paramref name="loadoutIndex"/>.
	/// Do not modify this array to set an item for a loadout, it will have no effect.
	/// </summary>
	/// <returns>A list of all items from mod slots including from shared slots for the loadout with the given <paramref name="loadoutIndex"/></returns>
	internal Item[] GetAllModSlotAccessoriesForLoadout(int loadoutIndex)
	{
		Item[] result = new Item[sharedLoadout.ExAccessorySlot.Length];

		for (int slot = 0; slot < result.Length; slot++) {
			ExEquipmentLoadout currentLoadout = GetLoadoutBySlot(slot, loadoutIndex);
			result[slot] = currentLoadout.ExAccessorySlot[slot];
		}

		return result;
	}

	/// <summary>
	/// Returns a list of all items from mod slots including from shared slots for the current loadout.
	/// Do not modify this array to set an item for a loadout, it will have no effect.
	/// </summary>
	/// <returns>A list of all items from mod slots including from shared slots for the current loadout.</returns>
	internal Item[] GetAllModSlotAccessoriesForCurrentLoadout() => GetAllModSlotAccessoriesForLoadout(ModdedCurrentLoadoutIndex);

	/// <summary>
	/// Gets the dyes for the current loadout. This method returns a copy of the original array with
	/// any dyes from shared slots added. The returned array should therefore not be modified.
	/// </summary>
	/// <returns>
	/// The accessories for the current loadout. This array can not be used to set dyes for slots.
	/// Use <see cref="SetDyeItemForCurrentLoadout"/> instead.
	/// </returns>
	internal Item[] GetAllModSlotDyesForCurrentLoadout()
	{
		Item[] result = new Item[SlotCount];

		for (int slot = 0; slot < CurrentLoadout.ExDyesAccessory.Length; slot++) {
			ExEquipmentLoadout loadout = GetLoadoutBySlot(slot);
			result[slot] = loadout.ExDyesAccessory[slot];
		}

		return result;
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
				UpdateVisibleAccessories(CurrentLoadout, k);
				UpdateVisibleAccessories(sharedLoadout, k);
			}
		}

		void UpdateVisibleAccessories(ExEquipmentLoadout loadout, int slot)
		{
			Player.UpdateVisibleAccessories(loadout.ExAccessorySlot[slot], loadout.ExHideAccessory[slot], slot, true);
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

				UpdateVisibleVanityAccessories(CurrentLoadout, vanitySlot);
				UpdateVisibleVanityAccessories(sharedLoadout, vanitySlot);
			}
		}

		void UpdateVisibleVanityAccessories(ExEquipmentLoadout loadout, int vanitySlot)
		{
			if (!Player.ItemIsVisuallyIncompatible(loadout.ExAccessorySlot[vanitySlot])) {
				Player.UpdateVisibleAccessory(vanitySlot, loadout.ExAccessorySlot[vanitySlot], true);
			}
		}
	}

	public override void UpdateDyes()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(k, Player)) {
				UpdateDyes(CurrentLoadout, k);
				UpdateDyes(sharedLoadout, k);
			}
		}

		void UpdateDyes(ExEquipmentLoadout loadout, int slot)
		{
			Player.UpdateItemDye(true, loadout.ExHideAccessory[slot], loadout.ExAccessorySlot[slot], loadout.ExDyesAccessory[slot]);
			Player.UpdateItemDye(false, loadout.ExHideAccessory[slot], loadout.ExAccessorySlot[SlotCount + slot], loadout.ExDyesAccessory[slot]);
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
		int end = socialSlots ? SlotCount * 2 : SlotCount;

		for (int i = start; i < end; i++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player)) {
				UpdateDyes(CurrentLoadout, i);
				UpdateDyes(sharedLoadout, i);
			}
		}

		void UpdateDyes(ExEquipmentLoadout loadout, int slot)
		{
			int num = slot % loadout.ExDyesAccessory.Length;
			Player.UpdateItemDye(
				slot < loadout.ExDyesAccessory.Length,
				loadout.ExHideAccessory[num],
				loadout.ExAccessorySlot[slot],
				loadout.ExDyesAccessory[num]);
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
				foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts.Concat([sharedLoadout])) {
					Player.DropItem(itemSource, pos, ref equipmentLoadout.ExAccessorySlot[i]);
					Player.DropItem(itemSource, pos, ref equipmentLoadout.ExAccessorySlot[i + SlotCount]);
					Player.DropItem(itemSource, pos, ref equipmentLoadout.ExDyesAccessory[i]);
				}
			}
		}
	}

	// The following netcode is adapted from ChickenBones' UtilitySlots:
	public override void CopyClientState(ModPlayer targetCopy)
	{
		var defaultInv = (ModAccessorySlotPlayer)targetCopy;

		for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
			CopyState(exLoadouts[loadoutIndex], defaultInv.exLoadouts[loadoutIndex]);
		}

		CopyState(sharedLoadout, defaultInv.sharedLoadout);

		void CopyState(ExEquipmentLoadout equipmentLoadout, ExEquipmentLoadout targetEquipmentLoadout)
		{
			for (int i = 0; i < LoadedSlotCount; i++) {
				equipmentLoadout.ExAccessorySlot[i].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i]);
				equipmentLoadout.ExAccessorySlot[i + SlotCount].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i + LoadedSlotCount]);
				equipmentLoadout.ExDyesAccessory[i].CopyNetStateTo(targetEquipmentLoadout.ExDyesAccessory[i]);
				targetEquipmentLoadout.ExHideAccessory[i] = equipmentLoadout.ExHideAccessory[i];
			}
		}
	}

	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
		foreach (var equipmentLoadout in exLoadouts) {
			Sync(equipmentLoadout);
		}

		Sync(sharedLoadout);

		void Sync(ExEquipmentLoadout loadout)
		{
			for (int slot = 0; slot < LoadedSlotCount; slot++) {
				NetHandler.SendSlot(toWho, Player.whoAmI, loadout.LoadoutIndex, slot, loadout.ExAccessorySlot[slot]);
				NetHandler.SendSlot(toWho, Player.whoAmI, loadout.LoadoutIndex, slot + LoadedSlotCount, loadout.ExAccessorySlot[slot + SlotCount]);
				NetHandler.SendSlot(toWho, Player.whoAmI, loadout.LoadoutIndex, -slot - 1, loadout.ExDyesAccessory[slot]);
				NetHandler.SendVisualState(toWho, Player.whoAmI, loadout.LoadoutIndex, slot, loadout.ExHideAccessory[slot]);
			}
		}
	}

	public override void SendClientChanges(ModPlayer clientPlayer)
	{
		var clientInv = (ModAccessorySlotPlayer)clientPlayer;
		for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
			SendClientChanges(exLoadouts[loadoutIndex], clientInv.exLoadouts[loadoutIndex]);
		}

		SendClientChanges(sharedLoadout, clientInv.sharedLoadout);

		void SendClientChanges(ExEquipmentLoadout equipmentLoadout, ExEquipmentLoadout clientEquipmentLoadout)
		{
			for (int slot = 0; slot < LoadedSlotCount; slot++) {
				if (equipmentLoadout.ExAccessorySlot[slot].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[slot]))
					NetHandler.SendSlot(-1, Player.whoAmI, equipmentLoadout.LoadoutIndex, slot, equipmentLoadout.ExAccessorySlot[slot]);

				if (equipmentLoadout.ExAccessorySlot[slot + SlotCount].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[slot + LoadedSlotCount]))
					NetHandler.SendSlot(-1, Player.whoAmI, equipmentLoadout.LoadoutIndex, slot + LoadedSlotCount, equipmentLoadout.ExAccessorySlot[slot + SlotCount]);

				if (equipmentLoadout.ExDyesAccessory[slot].IsNetStateDifferent(clientEquipmentLoadout.ExDyesAccessory[slot]))
					NetHandler.SendSlot(-1, Player.whoAmI, equipmentLoadout.LoadoutIndex, -slot - 1, equipmentLoadout.ExDyesAccessory[slot]);

				if (equipmentLoadout.ExHideAccessory[slot] != clientEquipmentLoadout.ExHideAccessory[slot])
					NetHandler.SendVisualState(-1, Player.whoAmI, equipmentLoadout.LoadoutIndex, slot, equipmentLoadout.ExHideAccessory[slot]);
			}
		}
	}

	public override void OnEquipmentLoadoutSwitched(int loadoutIndex)
	{
		ModdedCurrentLoadoutIndex = loadoutIndex;
	}

	internal Item GetFunctionalItemForCurrentLoadout(int slotType)
	{
		return GetLoadoutBySlot(slotType).ExAccessorySlot[slotType];
	}

	internal void SetFunctionalItemForCurrentLoadout(int slotType, Item item)
	{
		GetLoadoutBySlot(slotType).ExAccessorySlot[slotType] = item;
	}

	internal void SetAccessoryForCurrentLoadout(int slotType, Item item)
	{
		GetLoadoutBySlot(slotType).ExAccessorySlot[slotType] = item;
	}

	internal Item GetVanityItemForCurrentLoadout(int slotType)
	{
		return GetLoadoutBySlot(slotType).ExAccessorySlot[slotType + SlotCount];
	}

	internal void SetVanityItemForCurrentLoadout(int slotType, Item item)
	{
		GetLoadoutBySlot(slotType).ExAccessorySlot[slotType + SlotCount] = item;
	}

	internal Item GetDyeItemForCurrentLoadout(int slotType)
	{
		return GetLoadoutBySlot(slotType).ExDyesAccessory[slotType];
	}

	internal void SetDyeItemForCurrentLoadout(int slotType, Item item)
	{
		GetLoadoutBySlot(slotType).ExDyesAccessory[slotType] = item;
	}

	internal bool GetHideAccessoryForCurrentLoadout(int slotType)
	{
		return GetLoadoutBySlot(slotType).ExHideAccessory[slotType];
	}

	internal void SetHideAccessoryForCurrentLoadout(int slotType, bool hide)
	{
		GetLoadoutBySlot(slotType).ExHideAccessory[slotType] = hide;
	}

	/// <summary>
	/// Returns the loadout which keeps track of items equipped for the given <paramref name="slot"/>.
	/// Items in shared accessory slots are kept in a separate loadout instance which makes this method necessary.
	/// </summary>
	/// <param name="slot">The slot index/type</param>
	/// <param name="loadoutIndex">
	/// Optional loadout index that is used to retrieve the loadout if <paramref name="slot"/> does not point to
	/// the shared loadout. If this is not set, <see cref="ModdedCurrentLoadoutIndex"/> is used.
	/// </param>
	/// <returns>The loadout for the given slot.</returns>
	internal ExEquipmentLoadout GetLoadoutBySlot(int slot, int? loadoutIndex = null)
	{
		return IsSharedSlot(slot)
			? sharedLoadout
			: exLoadouts[loadoutIndex ?? ModdedCurrentLoadoutIndex];
	}

	internal ExEquipmentLoadout GetLoadout(int loadoutIndex)
	{
		return loadoutIndex == SharedLoadoutIndex
			? sharedLoadout
			: exLoadouts[loadoutIndex];
	}

	/// <summary>
	/// Checks if <paramref name="checkItem"/> can be equipped in <paramref name="slot"/> without
	/// conflicting with any other currently equipped items.
	/// </summary>
	/// <param name="checkItem">The item for which to check if it can be equipped.</param>
	/// <param name="slot">The slot into which the item should be equipped.</param>
	/// <returns>a<see langword="true"/> if the item can be equipped in <paramref name="slot"/>; otherwise <see langword="false"/></returns>
	internal bool CanItemBeEquippedInSlot(Item checkItem, int slot)
	{
		if (IsSharedSlot(slot)) {
			return exLoadouts.All(loadout =>
				!ItemSlot.AccCheck_ForLocalPlayer(GetAllAccessoriesForLoadout(loadout.LoadoutIndex), checkItem, slot + Player.armor.Length));
		}

		return !ItemSlot.AccCheck_ForLocalPlayer(GetAllAccessoriesForLoadout(ModdedCurrentLoadoutIndex), checkItem, slot + Player.armor.Length);
	}

	private bool IsSharedSlot(int slotType)
	{
		return sharedLoadoutSlotTypes.Contains(slotType);
	}

	internal sealed class ExEquipmentLoadout
	{
		private readonly string identifier;

		public ExEquipmentLoadout(int loadoutIndex, int slotCount, EquipmentLoadout loadoutReference)
		{
			this.LoadoutIndex = loadoutIndex;
			this.LoadoutReference = loadoutReference;
			this.identifier = $"loadout_{loadoutIndex}";

			this.ResetAndSizeAccessoryArrays(slotCount);
		}

		public int LoadoutIndex { get; }

		public EquipmentLoadout LoadoutReference { get; }

		public Item[] ExAccessorySlot { get; private set; } = [];

		public Item[] ExDyesAccessory { get; private set; } = [];

		public bool[] ExHideAccessory { get; private set; } = [];

		private string ItemsTagKey => $"items_{this.identifier}";

		private string DyesTagKey => $"dyes_{this.identifier}";

		private string AccessoryHiddenTagKey => $"hidden_{this.identifier}";

		public void SaveData(TagCompound tag)
		{
			tag[this.ItemsTagKey] = ExAccessorySlot.Select(ItemIO.Save).ToList();
			tag[this.DyesTagKey] = ExDyesAccessory.Select(ItemIO.Save).ToList();
			tag[this.AccessoryHiddenTagKey] = ExHideAccessory.ToList();
		}

		/// <summary>
		/// Loads data for this loadout and updates this instance accordingly.
		/// Returns a collection of <see cref="SlotInfo"/> objects for slots, which are not added to the loadout,
		/// because <see cref="ModAccessorySlot.HasEquipmentLoadoutSupport"/> changed since the last save.
		/// </summary>
		/// <param name="tag">The <see cref="TagCompound"/> from which to load the data</param>
		/// <param name="order">Saved slot names in order.</param>
		/// <param name="slots">Slot name to slot info mapping.</param>
		/// <returns>
		/// A collection of <see cref="SlotInfo"/> objects for slots, which are not added to the loadout,
		/// because <see cref="ModAccessorySlot.HasEquipmentLoadoutSupport"/> changed since the last save.
		/// </returns>
		public IReadOnlyList<SlotInfo> LoadData(
			TagCompound tag,
			List<string> order,
			Dictionary<string, (int SlotType, bool HasLoadoutSupport)> slots)
		{
			List<SlotInfo> result = [];
			IList<Item> items;
			IList<Item> dyes;
			IList<bool> visible;

			this.ResetAndSizeAccessoryArrays(slots.Count);

			// Preserve backwards compatibility if data is stored in format pre loadout support
			if (tag.TryGet("items", out IList<TagCompound> itemsTags)) {
				if (LoadoutIndex is not 0 and not SharedLoadoutIndex) {
					return result;
				}

				items = itemsTags.Select(ItemIO.Load).ToList();
				dyes = tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList();
				visible = tag.GetList<bool>("visible");
			}
			else {
				items = tag.GetList<TagCompound>(this.ItemsTagKey).Select(ItemIO.Load).ToList();
				dyes = tag.GetList<TagCompound>(this.DyesTagKey).Select(ItemIO.Load).ToList();
				visible = tag.GetList<bool>(this.AccessoryHiddenTagKey).ToList();
			}

			for (int i = 0; i < order.Count; i++) {
				(int type, bool hasLoadoutSupport) = slots[order[i]];
				bool loadoutSupportSettingOfSlotChanged = LoadoutIndex == SharedLoadoutIndex && hasLoadoutSupport
				                                          || LoadoutIndex != SharedLoadoutIndex && !hasLoadoutSupport;

				Item dye = dyes.ElementAtOrDefault(i) ?? new Item();
				Item accessory = items.ElementAtOrDefault(i) ?? new Item();
				Item vanityItem = items.ElementAtOrDefault(i + order.Count) ?? new Item();
				bool isHidden = visible.ElementAtOrDefault(i);

				if (loadoutSupportSettingOfSlotChanged) {
					result.Add(new SlotInfo {
						Slot = type,
						Dye = dye,
						VanityItem = vanityItem,
						Accessory = accessory,
						HideAccessory = isHidden,
					});
					continue;
				}

				ExDyesAccessory[type] = dye;
				ExAccessorySlot[type + slots.Count] = vanityItem;
				ExAccessorySlot[type] = accessory;
				ExHideAccessory[type] = isHidden;
			}

			return result;
		}

		private void ResetAndSizeAccessoryArrays(int size)
		{
			ExAccessorySlot = new Item[2 * size];
			ExDyesAccessory = new Item[size];
			ExHideAccessory = new bool[size];

			for (int i = 0; i < size; i++) {
				ExDyesAccessory[i] = new Item();
				ExHideAccessory[i] = false;

				ExAccessorySlot[i * 2] = new Item();
				ExAccessorySlot[i * 2 + 1] = new Item();
			}
		}
	}

	internal sealed record SlotInfo
	{
		public int Slot { get; init; }

		public Item Dye { get; init; }

		public Item VanityItem { get; init; }

		public Item Accessory { get; init; }

		public bool HideAccessory { get; init; }
	}

	internal static class NetHandler
	{
		public const byte InventorySlot = 1;
		public const byte VisualState = 2;

		public const byte Server = 2;
		public const byte Client = 1;
		public const byte SP = 0;

		public static void SendSlot(int toWho, int plr, int loadout, int slot, Item item)
		{
			var p = ModLoaderMod.GetPacket(ModLoaderMod.AccessorySlotPacket);

			p.Write(InventorySlot);

			if (Main.netMode == Server)
				p.Write((byte)plr);

			p.Write((sbyte)loadout);
			p.Write((sbyte)slot);

			ItemIO.Send(item, p, true);
			p.Send(toWho, plr);
		}

		private static void HandleSlot(BinaryReader r, int fromWho)
		{
			if (Main.netMode == Client)
				fromWho = r.ReadByte();

			var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

			sbyte loadout = r.ReadSByte();
			sbyte slot = r.ReadSByte();
			var item = ItemIO.Receive(r, true);

			SetSlot(loadout, slot, item, dPlayer);

			if (Main.netMode == 2)
				SendSlot(-1, fromWho, loadout, slot, item);
		}

		public static void SendVisualState(int toWho, int plr, int loadout, int slot, bool hideVisual)
		{
			var p = ModLoaderMod.GetPacket(ModLoaderMod.AccessorySlotPacket);

			p.Write(VisualState);

			if (Main.netMode == Server)
				p.Write((byte)plr);

			p.Write((sbyte)loadout);
			p.Write((sbyte)slot);

			p.Write(hideVisual);
			p.Send(toWho, plr);
		}

		private static void HandleVisualState(BinaryReader r, int fromWho)
		{
			if (Main.netMode == Client)
				fromWho = r.ReadByte();

			var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

			sbyte loadout = r.ReadSByte();
			sbyte slot = r.ReadSByte();

			ExEquipmentLoadout equipmentLoadout = dPlayer.GetLoadout(loadout);
			equipmentLoadout.ExHideAccessory[slot] = r.ReadBoolean();

			if (Main.netMode == Server)
				SendVisualState(-1, fromWho, loadout, slot, equipmentLoadout.ExHideAccessory[slot]);
		}

		public static void HandlePacket(BinaryReader r, int fromWho)
		{
			switch (r.ReadByte()) {
				case InventorySlot:
					HandleSlot(r, fromWho);
					break;
				case VisualState:
					HandleVisualState(r, fromWho);
					break;
			}
		}

		public static void SetSlot(sbyte loadout, sbyte slot, Item item, ModAccessorySlotPlayer dPlayer)
		{
			ExEquipmentLoadout equipmentLoadout = dPlayer.GetLoadout(loadout);

			if (slot < 0)
				equipmentLoadout.ExDyesAccessory[-(slot + 1)] = item;
			else
				equipmentLoadout.ExAccessorySlot[slot] = item;
		}
	}
}