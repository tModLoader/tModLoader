using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	// Test in Multiplayer, suspect there is some issue with synchronization of unloaded slots
	public class ModAccessorySlotPlayer : ModPlayer
	{
		public override bool CloneNewInstances => false;
		internal static AccessorySlotLoader Loader => LoaderManager.Get<AccessorySlotLoader>();

		internal int SlotCount() => slots.Count;

		// Arrays for modded accessory slot save/load/usage. Used in DefaultPlayer.
		internal Item[] exAccessorySlot = new Item[2];
		internal Item[] exDyesAccessory = new Item[1];
		internal bool[] exHideAccessory = new bool[1];
		internal Dictionary<string, int> slots = new Dictionary<string, int>();

		// Setting toggle for stack or scroll accessories/npcHousing
		internal bool scrollSlots = true;
		internal int scrollbarSlotPosition = 0;

		public ModAccessorySlotPlayer() {
			foreach (var slot in Loader.list) {
				if (!slot.FullName.StartsWith("Terraria", StringComparison.OrdinalIgnoreCase))
					slots.Add(slot.FullName, slot.Type);
				else
					slots.Add(slot.Name, slot.Type);
			}

			ResizeAccesoryArrays(slots.Count);
		}

		internal void ResizeAccesoryArrays(int newSize) {
			if (newSize < slots.Count) {
				return;
			}

			Array.Resize<Item>(ref exAccessorySlot, 2 * newSize);
			Array.Resize<Item>(ref exDyesAccessory, newSize);
			Array.Resize<bool>(ref exHideAccessory, newSize);

			for (int i = 0; i < newSize; i++) {
				exDyesAccessory[i] = new Item();
				exHideAccessory[i] = false;

				exAccessorySlot[i * 2] = new Item();
				exAccessorySlot[i * 2 + 1] = new Item();
			}
		}

		public override TagCompound Save() {
			return new TagCompound {
				["order"] = slots.Keys.ToList(),
				["items"] = exAccessorySlot.Select(ItemIO.Save).ToList(),
				["dyes"] = exDyesAccessory.Select(ItemIO.Save).ToList(),
				["visible"] = exHideAccessory.ToList()
			};
		}

		public override void Load(TagCompound tag) {
			var order = tag.GetList<string>("order").ToList();
			var items = tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList();
			var dyes = tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList();
			var visible = tag.GetList<bool>("visible").ToList();

			ResizeAccesoryArrays(order.Count);

			for (int i = 0; i < order.Count; i++) {
				// Try finding the slot item goes in to
				if (!slots.TryGetValue(order[i], out int type)) {
					var unloaded = new UnloadedAccessorySlot(Loader.list.Count, order[i]);

					slots.Add(unloaded.Name, unloaded.Type);
					Loader.list.Add(unloaded);
					type = unloaded.Type;
				}

				// Place loaded items in to the correct slot
				exDyesAccessory[type] = dyes[i];
				exHideAccessory[type] = visible[i];
				exAccessorySlot[type] = items[i];
				exAccessorySlot[type + order.Count] = items[i + order.Count];
			}
		}

		// Updates Code:
		/// <summary>
		/// Updates all vanity information on the player for Mod Slots, in a similar fashion to Player.UpdateVisibleAccessories()
		/// Runs On Player Select, so is Player instance sensitive!!!
		/// </summary>
		public override void UpdateVisibleAccessories() {
			var loader = LoaderManager.Get<AccessorySlotLoader>();

			// Handle Player Select Screen rendering for Iterations with the correct player
			if (Player != ModAccessorySlot.Player)
				ModAccessorySlot.Player = Player;

			for (int k = 0; k < SlotCount(); k++) {
				if (loader.ModdedIsAValidEquipmentSlotForIteration(k)) {
					Player.UpdateVisibleAccessories(exAccessorySlot[k], exAccessorySlot[k + SlotCount()], exHideAccessory[k], k, true);
				}
			}

			ModAccessorySlot.Player = Main.LocalPlayer;
		}

		/// <summary>
		/// Mirrors Player.UpdateDyes() for modded slots
		/// Runs On Player Select, so is Player instance sensitive!!!
		/// </summary>
		public override void UpdateDyes() {
			var loader = LoaderManager.Get<AccessorySlotLoader>();

			// Handle Player Select Screen rendering for Iterations with the correct player
			if (Player != ModAccessorySlot.Player)
				ModAccessorySlot.Player = Player;

			for (int i = 0; i < SlotCount() * 2; i++) {
				if (loader.ModdedIsAValidEquipmentSlotForIteration(i)) {
					int num = i % exDyesAccessory.Length;
					Player.UpdateItemDye(i < exDyesAccessory.Length, exHideAccessory[num], exAccessorySlot[i], exDyesAccessory[num]);
				}
			}

			ModAccessorySlot.Player = Main.LocalPlayer;
		}

		/// <summary>
		/// Runs a simplified version of Player.UpdateEquips for the Modded Accessory Slots
		/// </summary>
		public override void UpdateEquips() {
			var loader = LoaderManager.Get<AccessorySlotLoader>();

			// Handle Player Select Screen rendering for Iterations with the correct player
			if (Player != ModAccessorySlot.Player)
				ModAccessorySlot.Player = Player;

			for (int k = 0; k < SlotCount(); k++) {
				if (loader.ModdedIsAValidEquipmentSlotForIteration(k)) {
					loader.CustomUpdateEquips(k);
					
					Item item = exAccessorySlot[k];
					if (MusicLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = MusicLoader.itemToMusic[item.type];
				}
			}

			ModAccessorySlot.Player = Main.LocalPlayer;
		}

		// Death drops code, should run prior to dropping other items in case conditions are used based on player's current equips
		public void DropItems() {
			var loader = LoaderManager.Get<AccessorySlotLoader>();
			var pos = Player.position + Player.Size / 2;
			for (int i = 0; i < SlotCount(); i++) {
				if (loader.ModdedIsAValidEquipmentSlotForIteration(i)) {
					Player.DropItem(pos, ref exAccessorySlot[i]);
					Player.DropItem(pos, ref exAccessorySlot[i + SlotCount()]);
					Player.DropItem(pos, ref exDyesAccessory[i]);
				}
			}
		}

		// The following netcode is adapted from ChickenBone's UtilitySlots:
		public override void clientClone(ModPlayer clientClone) {
			var defaultInv = (ModAccessorySlotPlayer)clientClone;
			for (int i = 0; i < exAccessorySlot.Length; i++)
				defaultInv.exAccessorySlot[i] = exAccessorySlot[i].Clone();
			for (int i = 0; i < exDyesAccessory.Length; i++) {
				defaultInv.exDyesAccessory[i] = exDyesAccessory[i].Clone();
				defaultInv.exHideAccessory[i] = exHideAccessory[i];
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			for (int i = 0; i < exAccessorySlot.Length; i++)
				NetHandler.SendSlot(toWho, Player.whoAmI, i, exAccessorySlot[i]);

			for (int i = 0; i < exDyesAccessory.Length; i++) {
				NetHandler.SendSlot(toWho, Player.whoAmI, i, exDyesAccessory[i]);
				NetHandler.SendVisualState(toWho, Player.whoAmI, i, exHideAccessory[i]);
			}
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			var clientInv = (ModAccessorySlotPlayer)clientPlayer;
			for (int i = 0; i < exAccessorySlot.Length; i++)
				if (exAccessorySlot[i].IsNotTheSameAs(clientInv.exAccessorySlot[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, i, exAccessorySlot[i]);

			for (int i = 0; i < exDyesAccessory.Length; i++) {
				if (exDyesAccessory[i].IsNotTheSameAs(clientInv.exDyesAccessory[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, -i - 1, exDyesAccessory[i]);

				if (exHideAccessory[i] != clientInv.exHideAccessory[i])
					NetHandler.SendVisualState(-1, Player.whoAmI, i, exHideAccessory[i]);
			}
		}

		internal class NetHandler
		{
			public const byte InventorySlot = 1;
			public const byte VisualState = 2;

			public static void SendSlot(int toWho, int plr, int slot, Item item) {
				var p = ModContent.GetInstance<ModLoaderMod>().GetPacket();

				p.Write(InventorySlot);

				if (Main.netMode == 2)
					p.Write((sbyte)plr);

				p.Write((sbyte)slot);

				ItemIO.Send(item, p, true);
				p.Send(toWho, plr);
			}

			private static void HandleSlot(BinaryReader r, int fromWho) {
				if (Main.netMode == 1)
					fromWho = r.ReadByte();

				var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

				sbyte slot = r.ReadSByte();
				var item = ItemIO.Receive(r, true);

				NetHandler.SetSlot(slot, item, dPlayer);

				if (Main.netMode == 2)
					SendSlot(-1, fromWho, slot, item);
			}

			public static void SendVisualState(int toWho, int plr, int slot, bool hideVisual) {
				var p = ModContent.GetInstance<ModLoaderMod>().GetPacket();

				p.Write(VisualState);

				if (Main.netMode == 2)
					p.Write((byte)plr);

				p.Write((sbyte)slot);

				p.Write(hideVisual);
				p.Send(toWho, plr);
			}

			private static void HandleVisualState(BinaryReader r, int fromWho) {
				if (Main.netMode == 1)
					fromWho = r.ReadByte();

				var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

				sbyte slot = r.ReadSByte();
				
				dPlayer.exHideAccessory[slot] = r.ReadBoolean();

				if (Main.netMode == 2)
					SendVisualState(-1, fromWho, slot, dPlayer.exHideAccessory[slot]);
			}

			public static void HandlePacket(BinaryReader r, int fromWho) {
				switch (r.ReadByte()) {
					case InventorySlot:
						HandleSlot(r, fromWho);
						break;
					case VisualState:
						HandleVisualState(r, fromWho);
						break;
				}
			}

			public static void SetSlot(sbyte slot, Item item, ModAccessorySlotPlayer dPlayer) {
				if (slot < 0)
					dPlayer.exDyesAccessory[-(slot + 1)] = item;
				else
					dPlayer.exAccessorySlot[slot] = item;
			}
		}
	}
}
