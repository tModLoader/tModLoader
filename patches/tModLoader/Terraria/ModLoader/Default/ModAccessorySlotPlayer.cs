using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

// Test in Multiplayer, suspect there is some issue with synchronization of unloaded slots
public sealed class ModAccessorySlotPlayer : ModPlayer
{
	internal static AccessorySlotLoader Loader => LoaderManager.Get<AccessorySlotLoader>();

	private readonly Dictionary<string, int> slots = [];

	private ExEquipmentLoadout[] exLoadouts = [
		new ExEquipmentLoadout(1),
		new ExEquipmentLoadout(2),
		new ExEquipmentLoadout(3),
	];

	internal ExEquipmentLoadout CurrentLoadout => exLoadouts[ModdedCurrentLoadoutIndex];

	// Setting toggle for stack or scroll accessories/npcHousing
	internal bool scrollSlots;
	internal int scrollbarSlotPosition;

	/// <summary>
	/// Gets or sets the modded current loadout index.
	/// </summary>
	public int ModdedCurrentLoadoutIndex { get; set; }

	public int SlotCount => slots.Count;
	public int LoadedSlotCount => Loader.TotalCount;

	public ModAccessorySlotPlayer()
	{
		foreach (var slot in Loader.list) {
			slots.Add(slot.FullName, slot.Type);
		}

		ResetAndSizeAccessoryArrays();
	}

	/// <summary>
	/// Appends an additional equipment loadout. By default, only the three vanilla equipment loadouts are supported.
	/// If a mod adds additional loadouts, it must call this method for each one to add loadout support for any
	/// <see cref="ModAccessorySlot"/> instances.
	/// </summary>
	public void AppendAdditionalEquipmentLoadout()
	{
		Array.Resize(ref exLoadouts, exLoadouts.Length + 1);
		ExEquipmentLoadout newLoadout = new(exLoadouts.Length);
		newLoadout.ResetAndSizeAccessoryArrays(slots.Count);
		exLoadouts[^1] = newLoadout;
	}

	private void ResetAndSizeAccessoryArrays()
	{
		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			equipmentLoadout.ResetAndSizeAccessoryArrays(slots.Count);
		}
	}

	public override void SaveData(TagCompound tag)
	{
		// TODO, might be nice to only save acc slots which have something in them... particularly if they're unloaded. Otherwise old unloaded slots just bloat the array with empty entries forever
		tag["loadout"] = ModdedCurrentLoadoutIndex;
		tag["order"] = slots.Keys.ToList();

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
				slots.Add(name, slots.Count);
		}

		ResetAndSizeAccessoryArrays();

		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			equipmentLoadout.LoadData(tag, order, slots);
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
				Player.UpdateVisibleAccessories(CurrentLoadout.ExAccessorySlot[k], CurrentLoadout.ExHideAccessory[k], k, true);
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

				if (!Player.ItemIsVisuallyIncompatible(CurrentLoadout.ExAccessorySlot[vanitySlot]))
					Player.UpdateVisibleAccessory(vanitySlot, CurrentLoadout.ExAccessorySlot[vanitySlot], true);
			}
		}
	}

	public override void UpdateDyes()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(k, Player)) {
				Player.UpdateItemDye(true, CurrentLoadout.ExHideAccessory[k], CurrentLoadout.ExAccessorySlot[k], CurrentLoadout.ExDyesAccessory[k]);
				Player.UpdateItemDye(false, CurrentLoadout.ExHideAccessory[k], CurrentLoadout.ExAccessorySlot[SlotCount + k], CurrentLoadout.ExDyesAccessory[k]);
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
		int end = socialSlots ? SlotCount * 2 : SlotCount;

		for (int i = start; i < end; i++) {
			if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player)) {
				foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
					int num = i % equipmentLoadout.ExDyesAccessory.Length;
					Player.UpdateItemDye(i < equipmentLoadout.ExDyesAccessory.Length,
						equipmentLoadout.ExHideAccessory[num], equipmentLoadout.ExAccessorySlot[i],
						equipmentLoadout.ExDyesAccessory[num]);
				}
			}
		}
	}

	public override void OnEquipmentLoadoutSwitched(int loadoutIndex)
	{
		ModdedCurrentLoadoutIndex = loadoutIndex;
	}

	public Item GetFunctionalItemForLoadout(int loadout, int slotType)
	{
		return exLoadouts[loadout].ExAccessorySlot[slotType];
	}

	public void SetFunctionalItemForLoadout(int loadout, int slotType, Item item)
	{
		exLoadouts[loadout].ExAccessorySlot[slotType] = item;
	}

	public Item GetVanityItemForLoadout(int loadout, int slotType)
	{
		return exLoadouts[loadout].ExAccessorySlot[slotType + slots.Count];
	}

	public void SetVanityItemForLoadout(int loadout, int slotType, Item item)
	{
		exLoadouts[loadout].ExAccessorySlot[slotType + slots.Count] = item;
	}

	public Item GetDyeItemForLoadout(int loadout, int slotType)
	{
		return exLoadouts[loadout].ExDyesAccessory[slotType];
	}

	public void SetDyeItemForLoadout(int loadout, int slotType, Item item)
	{
		exLoadouts[loadout].ExDyesAccessory[slotType] = item;
	}

	public bool GetHideAccessoryForLoadout(int loadout, int slotType)
	{
		return exLoadouts[loadout].ExHideAccessory[slotType];
	}

	public void SetHideAccessoryForLoadout(int loadout, int slotType, bool hide)
	{
		exLoadouts[loadout].ExHideAccessory[slotType] = hide;
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
				foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
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
		for (int i = 0; i < LoadedSlotCount; i++) {
			for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
				ExEquipmentLoadout equipmentLoadout = exLoadouts[loadoutIndex];
				ExEquipmentLoadout targetEquipmentLoadout = defaultInv.exLoadouts[loadoutIndex];
				equipmentLoadout.ExAccessorySlot[i].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i]);
				equipmentLoadout.ExAccessorySlot[i + SlotCount].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i + LoadedSlotCount]);
				equipmentLoadout.ExDyesAccessory[i].CopyNetStateTo(targetEquipmentLoadout.ExDyesAccessory[i]);
				targetEquipmentLoadout.ExHideAccessory[i] = equipmentLoadout.ExHideAccessory[i];
			}
		}
	}

	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
		for (int i = 0; i < LoadedSlotCount; i++) {
			for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
				ExEquipmentLoadout equipmentLoadout = exLoadouts[loadoutIndex];

				NetHandler.SendSlot(toWho, Player.whoAmI, loadoutIndex, i, equipmentLoadout.ExAccessorySlot[i]);
				NetHandler.SendSlot(toWho, Player.whoAmI, loadoutIndex, i + LoadedSlotCount, equipmentLoadout.ExAccessorySlot[i + SlotCount]);
				NetHandler.SendSlot(toWho, Player.whoAmI, loadoutIndex, -i - 1, equipmentLoadout.ExDyesAccessory[i]);
				NetHandler.SendVisualState(toWho, Player.whoAmI, loadoutIndex, i, equipmentLoadout.ExHideAccessory[i]);
			}
		}
	}

	public override void SendClientChanges(ModPlayer clientPlayer)
	{
		var clientInv = (ModAccessorySlotPlayer)clientPlayer;
		for (int i = 0; i < LoadedSlotCount; i++) {
			for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
				ExEquipmentLoadout equipmentLoadout = exLoadouts[loadoutIndex];
				ExEquipmentLoadout clientEquipmentLoadout = clientInv.exLoadouts[loadoutIndex];

				if (equipmentLoadout.ExAccessorySlot[i].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, loadoutIndex, i, equipmentLoadout.ExAccessorySlot[i]);

				if (equipmentLoadout.ExAccessorySlot[i + SlotCount].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[i + LoadedSlotCount]))
					NetHandler.SendSlot(-1, Player.whoAmI, loadoutIndex, i + LoadedSlotCount, equipmentLoadout.ExAccessorySlot[i + SlotCount]);

				if (equipmentLoadout.ExDyesAccessory[i].IsNetStateDifferent(clientEquipmentLoadout.ExDyesAccessory[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, loadoutIndex, -i - 1, equipmentLoadout.ExDyesAccessory[i]);

				if (equipmentLoadout.ExHideAccessory[i] != clientEquipmentLoadout.ExHideAccessory[i])
					NetHandler.SendVisualState(-1, Player.whoAmI, loadoutIndex, i, equipmentLoadout.ExHideAccessory[i]);
			}
		}
	}


	internal record ExEquipmentLoadout
	{
		private readonly int loadoutNumber;
		private readonly string identifier;

		public ExEquipmentLoadout(int loadoutNumber)
		{
			this.loadoutNumber = loadoutNumber;
			this.identifier = $"loadout_{loadoutNumber}";
		}

		public Item[] ExAccessorySlot { get; private set; } = [];

		public Item[] ExDyesAccessory { get; private set; } = [];

		public bool[] ExHideAccessory { get; private set; } = [];

		public void ResetAndSizeAccessoryArrays(int size)
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

		public void SaveData(TagCompound tag)
		{
			tag[$"items_{this.identifier}"] = ExAccessorySlot.Select(ItemIO.Save).ToList();
			tag[$"dyes_{this.identifier}"] = ExDyesAccessory.Select(ItemIO.Save).ToList();
			tag[$"visible_{this.identifier}"] = ExHideAccessory.ToList();
		}

		public void LoadData(TagCompound tag, List<string> order, Dictionary<string, int> slots)
		{
			// Preserve backwards compatibility if data is stored in format pre loadout support
			var items = tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList();
			var dyes = tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList();
			var visible = tag.GetList<bool>("visible");

			if (items.Count > 0 || dyes.Count > 0 || visible.Count > 0) {
				// only add the items to loadout 1 if initially loaded with backwards compatibility
				if (loadoutNumber != 1) {
					return;
				}
			}
			else {
				items = tag.GetList<TagCompound>($"items_{this.identifier}").Select(ItemIO.Load).ToList();
				dyes = tag.GetList<TagCompound>($"dyes_{this.identifier}").Select(ItemIO.Load).ToList();
				visible = tag.GetList<bool>($"visible_{this.identifier}").ToList();
			}

			for (int i = 0; i < order.Count; i++) {
				int type = slots[order[i]];

				if (i < dyes.Count) {
					ExDyesAccessory[type] = dyes[i];
				}

				if (i < visible.Count) {
					ExHideAccessory[type] = visible[i];
				}

				if (i < items.Count) {
					ExAccessorySlot[type] = items[i];
				}

				if ((i + order.Count) < items.Count) {
					ExAccessorySlot[type + slots.Count] = items[i + order.Count];
				}
			}
		}
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

			dPlayer.exLoadouts[loadout].ExHideAccessory[slot] = r.ReadBoolean();

			if (Main.netMode == Server)
				SendVisualState(-1, fromWho, loadout, slot, dPlayer.exLoadouts[loadout].ExHideAccessory[slot]);
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
			if (slot < 0)
				dPlayer.exLoadouts[loadout].ExDyesAccessory[-(slot + 1)] = item;
			else
				dPlayer.exLoadouts[loadout].ExAccessorySlot[slot] = item;
		}
	}
}