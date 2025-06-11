using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

// Test in Multiplayer, suspect there is some issue with synchronization of unloaded slots
public sealed class ModAccessorySlotPlayer : ModPlayer
{
	private static AccessorySlotLoader Loader => LoaderManager.Get<AccessorySlotLoader>();

	// Arrays for modded accessory slot save/load/usage. Used in DefaultPlayer.
	/// <summary> <see cref="ModAccessorySlot"/> corollary to the accessory and vanity slots of <see cref="Player.armor"/>. </summary>
	internal Item[] exAccessorySlot;
	/// <summary> <see cref="ModAccessorySlot"/> corollary to <see cref="Player.dye"/>. </summary>
	internal Item[] exDyesAccessory;
	/// <summary> <see cref="ModAccessorySlot"/> corollary to <see cref="Player.hideVisibleAccessory"/>. </summary>
	internal bool[] exHideAccessory;
	/// <summary> All slots that can potentially show for this user. First the loaded ModAccessorySlots, followed by any unloaded slots. </summary>
	private readonly Dictionary<string, int> slots = [];
	/// <summary> Which slots are shared, indexed by ModAccessorySlot.Type </summary>
	private bool[] sharedLoadoutSlotTypes;
	/// <inheritdoc cref="ExEquipmentLoadout"/>
	private ExEquipmentLoadout[] exLoadouts;

	/// <summary> Holds items from a saved <see cref="ModAccessorySlot"/> that changed to not <see cref="ModAccessorySlot.HasEquipmentLoadoutSupport"/> ("shared") and would otherwise be lost. Will be returned to the player when entering a world. </summary>
	private List<Item> extraItems = new();

	// Setting toggle for stack or scroll accessories/npcHousing
	internal bool scrollSlots;
	internal int scrollbarSlotPosition;

	/// <summary>
	/// Total modded slots to show, including UnloadedAccessorySlot at the end.
	/// On the local instance of a ModAccessorySlotPlayer, this might have extra entries not present on remote clients.
	/// </summary>
	public int SlotCount => slots.Count;
	/// <summary>
	/// Total loaded modded slots.
	/// This does not include UnloadedAccessorySlot entries. This value is used for network syncing since this will be consistent between clients.
	/// </summary>
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
		int size = SlotCount;
		exAccessorySlot = new Item[2 * size];
		exDyesAccessory = new Item[size];
		exHideAccessory = new bool[size];
		sharedLoadoutSlotTypes = new bool[size];

		for (int i = 0; i < size; i++) {
			exDyesAccessory[i] = new Item();
			exHideAccessory[i] = false;

			exAccessorySlot[i * 2] = new Item();
			exAccessorySlot[i * 2 + 1] = new Item();
		}

		foreach (var slot in Loader.list) {
			if (!slot.HasEquipmentLoadoutSupport) {
				sharedLoadoutSlotTypes[slot.Type] = true;
			}
		}
	}

	public override void Initialize()
	{
		exLoadouts = Enumerable.Range(0, Player.Loadouts.Length)
			.Select(loadoutIndex => new ExEquipmentLoadout(loadoutIndex, SlotCount, Player.Loadouts[loadoutIndex]))
			.ToArray();
	}

	public override void SaveData(TagCompound tag)
	{
		// TODO, might be nice to only save acc slots which have something in them... particularly if they're unloaded. Otherwise old unloaded slots just bloat the array with empty entries forever
		tag["order"] = slots.Keys.ToList();
		tag["items"] = exAccessorySlot.Select(ItemIO.Save).ToList();
		tag["dyes"] = exDyesAccessory.Select(ItemIO.Save).ToList();
		tag["visible"] = exHideAccessory.ToList(); // Note: "visible" is backwards, should be "hidden". True values are hidden.

		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			equipmentLoadout.SaveData(tag);
		}
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

		foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
			var extraItemsFromLoadout = equipmentLoadout.LoadData(tag, order, slots, sharedLoadoutSlotTypes);
			extraItems.AddRange(extraItemsFromLoadout);
		}

		for (int i = 0; i < order.Count; i++) {
			int type = slots[order[i]];

			// Place loaded items in to the correct slot
			exDyesAccessory[type] = dyes[i];
			exHideAccessory[type] = visible[i];
			exAccessorySlot[type] = items[i];
			exAccessorySlot[type + SlotCount] = items[i + order.Count];
		}
	}

	public override void OnEnterWorld()
	{
		// Need to enter world with disabled slots.
		DetectConflictsWithSharedSlots();

		if (extraItems.Count == 0)
			return;

		foreach (var item in extraItems) {
			Player.QuickSpawnItem(null, item);
		}
		Main.NewText(Language.GetTextValue("tModLoader.ModAccessorySlotNoLongerSharedItemsRemoved"));
		extraItems.Clear();
	}

	// Updates Code:
	/// <summary>
	/// Updates functional slot visibility information on the player for Mod Slots, in a similar fashion to Player.UpdateVisibleAccessories()
	/// </summary>
	public override void UpdateVisibleAccessories()
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();

		for (int k = 0; k < SlotCount; k++) {
			if (loader.ModdedIsSpecificItemSlotUnlockedAndUsable(k, Player, vanity: false)) {
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
			if (loader.ModdedIsSpecificItemSlotUnlockedAndUsable(k, Player, vanity: true)) {
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
		int end = socialSlots ? SlotCount * 2 : SlotCount;

		for (int i = start; i < end; i++) {
			if (loader.ModdedIsSpecificItemSlotUnlockedAndUsable(i, Player, vanity: socialSlots)) {
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
			if (loader.ModdedIsSpecificItemSlotUnlockedAndUsable(k, Player, vanity: false)) {
				loader.CustomUpdateEquips(k, Player);
			}
		}
	}

	// Death drops code, should run prior to dropping other items in case conditions are used based on player's current equips
	public void DropItems(IEntitySource itemSource)
	{
		var loader = LoaderManager.Get<AccessorySlotLoader>();
		for (int i = 0; i < SlotCount; i++) {
			// Drop all items, even if not ModdedIsItemSlotUnlockedAndUsable, to match vanilla behavior.
			Player.TryDroppingSingleItem(itemSource, exAccessorySlot[i]);
			Player.TryDroppingSingleItem(itemSource, exAccessorySlot[i + SlotCount]);
			Player.TryDroppingSingleItem(itemSource, exDyesAccessory[i]);

			foreach (ExEquipmentLoadout equipmentLoadout in exLoadouts) {
				Player.TryDroppingSingleItem(itemSource, equipmentLoadout.ExAccessorySlot[i]);
				Player.TryDroppingSingleItem(itemSource, equipmentLoadout.ExAccessorySlot[i + SlotCount]);
				Player.TryDroppingSingleItem(itemSource, equipmentLoadout.ExDyesAccessory[i]);
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

		for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
			CopyState(exLoadouts[loadoutIndex], defaultInv.exLoadouts[loadoutIndex]);
		}

		void CopyState(ExEquipmentLoadout equipmentLoadout, ExEquipmentLoadout targetEquipmentLoadout)
		{
			for (int i = 0; i < LoadedSlotCount; i++) {
				equipmentLoadout.ExAccessorySlot[i].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i]);
				equipmentLoadout.ExAccessorySlot[i + SlotCount].CopyNetStateTo(targetEquipmentLoadout.ExAccessorySlot[i + LoadedSlotCount]);
				equipmentLoadout.ExDyesAccessory[i].CopyNetStateTo(targetEquipmentLoadout.ExDyesAccessory[i]);
			}
		}
	}

	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
		// Send currently equipped items.
		// Note that LoadedSlotCount and SlotCount are cleverly used to skip over unloaded slots, unloaded slots only exist on the local client.
		for (int i = 0; i < LoadedSlotCount; i++) {
			NetHandler.SendSlot(toWho, Player.whoAmI, i, exAccessorySlot[i]);
			NetHandler.SendSlot(toWho, Player.whoAmI, i + LoadedSlotCount, exAccessorySlot[i + SlotCount]);
			NetHandler.SendSlot(toWho, Player.whoAmI, -i - 1, exDyesAccessory[i]);
			NetHandler.SendVisualState(toWho, Player.whoAmI, i, exHideAccessory[i]);
		}

		foreach (var equipmentLoadout in exLoadouts) {
			Sync(equipmentLoadout);
		}

		void Sync(ExEquipmentLoadout loadout)
		{
			for (int slot = 0; slot < LoadedSlotCount; slot++) {
				NetHandler.SendSlot(toWho, Player.whoAmI, slot, loadout.ExAccessorySlot[slot], (sbyte)loadout.LoadoutIndex);
				NetHandler.SendSlot(toWho, Player.whoAmI, slot + LoadedSlotCount, loadout.ExAccessorySlot[slot + SlotCount], (sbyte)loadout.LoadoutIndex);
				NetHandler.SendSlot(toWho, Player.whoAmI, -slot - 1, loadout.ExDyesAccessory[slot], (sbyte)loadout.LoadoutIndex);
			}
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

		for (int loadoutIndex = 0; loadoutIndex < exLoadouts.Length; loadoutIndex++) {
			SendClientChanges(exLoadouts[loadoutIndex], clientInv.exLoadouts[loadoutIndex]);
		}

		void SendClientChanges(ExEquipmentLoadout equipmentLoadout, ExEquipmentLoadout clientEquipmentLoadout)
		{
			for (int slot = 0; slot < LoadedSlotCount; slot++) {
				if (equipmentLoadout.ExAccessorySlot[slot].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[slot]))
					NetHandler.SendSlot(-1, Player.whoAmI, slot, equipmentLoadout.ExAccessorySlot[slot], (sbyte)equipmentLoadout.LoadoutIndex);

				if (equipmentLoadout.ExAccessorySlot[slot + SlotCount].IsNetStateDifferent(clientEquipmentLoadout.ExAccessorySlot[slot + LoadedSlotCount]))
					NetHandler.SendSlot(-1, Player.whoAmI, slot + LoadedSlotCount, equipmentLoadout.ExAccessorySlot[slot + SlotCount], (sbyte)equipmentLoadout.LoadoutIndex);

				if (equipmentLoadout.ExDyesAccessory[slot].IsNetStateDifferent(clientEquipmentLoadout.ExDyesAccessory[slot]))
					NetHandler.SendSlot(-1, Player.whoAmI, -slot - 1, equipmentLoadout.ExDyesAccessory[slot], (sbyte)equipmentLoadout.LoadoutIndex);
			}
		}
	}

	public override void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex)
	{
		exLoadouts[oldLoadoutIndex].Swap(this);
		exLoadouts[loadoutIndex].Swap(this);

		if (Player.whoAmI == Main.myPlayer) {
			if (Main.netMode != NetmodeID.SinglePlayer) {
				CopyClientState(Main.clientPlayer.GetModPlayer<ModAccessorySlotPlayer>());

				for (int i = 0; i < LoadedSlotCount; i++) {
					NetHandler.SendVisualState(-1, Player.whoAmI, i, exHideAccessory[i]);
				}
			}

			DetectConflictsWithSharedSlots();
		}
	}

	private void DetectConflictsWithSharedSlots() // Only called on local client.
	{
		// Handle conflicts that arise from supporting shared slots.
		bool anyConflict = false;
		for (int i = 0; i < exAccessorySlot.Length / 2; i++) {
			if (IsSharedSlot(i)) {
				anyConflict |= Loader.IsAccessoryInConflict(Player, exAccessorySlot[i], i, Terraria.UI.ItemSlot.Context.ModdedAccessorySlot);
				anyConflict |= Loader.IsAccessoryInConflict(Player, exAccessorySlot[i + SlotCount], i + SlotCount, Terraria.UI.ItemSlot.Context.ModdedVanityAccessorySlot);
			}
		}

		if (anyConflict) {
			Main.NewText(Language.GetTextValue("tModLoader.SharedAccessorySlotConflictMessage"));
			// TODO: Main.NewText doesn't work with ModPlayer.OnEnterWorld in multiplayer since the chat is cleared during the joining process. This will need to be fixed and also affects ExampleMod usages of OnEnterWorld as well.
		}
	}

	// If we allow HasEquipmentLoadoutSupport to differ per-client, this might not be suitable without extra changes.
	internal bool IsSharedSlot(int slotType)
	{
		return sharedLoadoutSlotTypes[slotType % SlotCount];
	}

	internal bool SharedSlotHasLoadoutConflict(int slotType, bool vanitySlot)
	{
		if (!IsSharedSlot(slotType))
			return false;

		// Handle conflicts that arise from supporting shared slots.
		int functional = slotType % SlotCount;
		int vanity = functional + SlotCount;

		if (vanitySlot) {
			return Loader.IsAccessoryInConflict(Player, exAccessorySlot[vanity], vanity, Terraria.UI.ItemSlot.Context.ModdedVanityAccessorySlot);
		}
		else {
			return Loader.IsAccessoryInConflict(Player, exAccessorySlot[functional], functional, Terraria.UI.ItemSlot.Context.ModdedAccessorySlot);
		}
	}

	// Extended Loadout, contains Item instances for ModAccessorySlot for a specific loadout index.
	/// <summary>
	/// <see cref="ExEquipmentLoadout"/> holds loadout items for loadouts not in use. <see cref="ModAccessorySlot"/> corollary to <see cref="EquipmentLoadout"/>.
	/// Note that when a loadout is selected, the items in <see cref="ExEquipmentLoadout"/> are swapped into <see cref="exAccessorySlot"/>, etc. and the loadout is left with empty Item instances. The active loadout items are stored on the player and the <see cref="ExEquipmentLoadout"/> is left empty, this is the same approach used for vanilla loadouts as well (<see cref="Player.Loadouts"/>).
	/// </summary>
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

		/// <inheritdoc cref="ModAccessorySlotPlayer.exAccessorySlot"/>
		public Item[] ExAccessorySlot { get; private set; } = [];

		/// <inheritdoc cref="ModAccessorySlotPlayer.exDyesAccessory"/>
		public Item[] ExDyesAccessory { get; private set; } = [];

		/// <inheritdoc cref="ModAccessorySlotPlayer.exHideAccessory"/>
		public bool[] ExHideAccessory { get; private set; } = [];

		public void SaveData(TagCompound tag)
		{
			tag[identifier] = new TagCompound {
				["items"] = ExAccessorySlot.Select(ItemIO.Save).ToList(),
				["dyes"] = ExDyesAccessory.Select(ItemIO.Save).ToList(),
				["hidden"] = ExHideAccessory.ToList(),
			};
		}

		/// <summary>
		/// Loads data for this loadout and updates this instance accordingly.
		/// Returns a collection of <see cref="Item"/>s, which were not added to the loadout,
		/// because <see cref="ModAccessorySlot.HasEquipmentLoadoutSupport"/> changed since the last save.
		/// </summary>
		/// <param name="tag">The <see cref="TagCompound"/> from which to load the data</param>
		/// <param name="order">Saved slot names in order.</param>
		/// <param name="slots">Slot name to slot info mapping.</param>
		/// <param name="sharedLoadoutSlotTypes">Slots that don't have loadout support</param>
		public IReadOnlyList<Item> LoadData(
			TagCompound tag,
			List<string> order,
			Dictionary<string, int> slots,
			bool[] sharedLoadoutSlotTypes)
		{
			List<Item> itemsToDrop = [];

			this.ResetAndSizeAccessoryArrays(slots.Count);

			// Saves from before this feature.
			if (!tag.ContainsKey(identifier))
				return itemsToDrop;

			tag = tag.GetCompound(identifier);
			var items = tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList();
			var dyes = tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList();
			var visible = tag.GetList<bool>("hidden").ToList();

			for (int i = 0; i < order.Count; i++) {
				int type = slots[order[i]];

				Item dye = dyes[i];
				Item accessory = items[i];
				Item vanityItem = items[i + order.Count];
				bool isHidden = visible[i];

				// If this slot doesn't have loadout support, the items shouldn't exist in any loadout at all (they will be on the player from the start). They will be returned to the player in OnEnterWorld, allowing the player to choose the correct solution to this conflict.
				if (sharedLoadoutSlotTypes[type]) {
					if (!accessory.IsAir)
						itemsToDrop.Add(accessory);
					if (!vanityItem.IsAir)
						itemsToDrop.Add(vanityItem);
					if (!dye.IsAir)
						itemsToDrop.Add(dye);
					continue;
				}

				ExDyesAccessory[type] = dye;
				ExAccessorySlot[type + slots.Count] = vanityItem;
				ExAccessorySlot[type] = accessory;
				ExHideAccessory[type] = isHidden;
			}

			return itemsToDrop;
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

		internal void Swap(ModAccessorySlotPlayer modAccessorySlotPlayer)
		{
			Item[] armor = modAccessorySlotPlayer.exAccessorySlot;
			for (int i = 0; i < armor.Length; i++) {
				if (modAccessorySlotPlayer.IsSharedSlot(i))
					continue;
				Utils.Swap(ref armor[i], ref ExAccessorySlot[i]);
			}

			Item[] dye = modAccessorySlotPlayer.exDyesAccessory;
			for (int j = 0; j < dye.Length; j++) {
				if (modAccessorySlotPlayer.IsSharedSlot(j))
					continue;
				Utils.Swap(ref dye[j], ref ExDyesAccessory[j]);
			}

			bool[] hideVisibleAccessory = modAccessorySlotPlayer.exHideAccessory;
			for (int k = 0; k < hideVisibleAccessory.Length; k++) {
				if (modAccessorySlotPlayer.IsSharedSlot(k))
					continue;
				Utils.Swap(ref hideVisibleAccessory[k], ref ExHideAccessory[k]);
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

		public static void SendSlot(int toWho, int plr, int slot, Item item, sbyte loadout = -1)
		{
			var p = ModLoaderMod.GetPacket(ModLoaderMod.AccessorySlotPacket);

			p.Write(InventorySlot);

			if (Main.netMode == Server)
				p.Write((byte)plr);

			p.Write(loadout);
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
				SendSlot(-1, fromWho, slot, item, loadout);
		}

		public static void SendVisualState(int toWho, int plr, int slot, bool hideVisual)
		{
			// Note: vanilla only syncs the current visibility, not the loadouts individually.
			// Vanilla sets 16 bits all at once in a single packet. We do a packet per slot, which seems really inefficient. We could change it to SendVisualStates.

			var p = ModLoaderMod.GetPacket(ModLoaderMod.AccessorySlotPacket);

			p.Write(VisualState);

			if (Main.netMode == Server)
				p.Write((byte)plr);

			p.Write((sbyte)slot);

			p.Write(hideVisual);
			p.Send(toWho, plr);
		}

		private static void HandleVisualState(BinaryReader r, int fromWho)
		{
			if (Main.netMode == Client)
				fromWho = r.ReadByte();

			var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

			sbyte slot = r.ReadSByte();

			dPlayer.exHideAccessory[slot] = r.ReadBoolean();

			if (Main.netMode == Server)
				SendVisualState(-1, fromWho, slot, dPlayer.exHideAccessory[slot]);
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
			if (loadout == -1) {
				if (slot < 0)
					dPlayer.exDyesAccessory[-(slot + 1)] = item;
				else
					dPlayer.exAccessorySlot[slot] = item;
			}
			else {
				ExEquipmentLoadout equipmentLoadout = dPlayer.exLoadouts[loadout];

				if (slot < 0)
					equipmentLoadout.ExDyesAccessory[-(slot + 1)] = item;
				else
					equipmentLoadout.ExAccessorySlot[slot] = item;
			}
		}
	}
}
