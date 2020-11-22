using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

#nullable enable
namespace Terraria.ModLoader.Container
{
	public static partial class ItemStorageUtility
	{
		public static bool Contains(this ItemStorage storage, int type) => storage.Any(item => !item.IsAir && item.type == type);

		public static bool Contains(this ItemStorage storage, Item item) => storage.Any(item.IsTheSameAs);

		/// <summary>
		/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, int)" />. It is much faster.
		/// </summary>
		public static int Count(this ItemStorage storage, int type) => storage.Count(item => !item.IsAir && item.type == type);

		/// <summary>
		/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, Item)" />. It is much faster.
		/// </summary>
		public static int Count(this ItemStorage storage, Item item) => storage.Count(item.IsTheSameAs);

		/// <summary>
		/// Gets if the <paramref name="item" /> can fit in the slot completely.
		/// </summary>
		public static bool CanItemStack(this ItemStorage storage, int slot, Item item) => CanItemStackPartially(storage, slot, item, out int leftOver) && leftOver <= 0;

		/// <summary>
		/// Gets if the <paramref name="item" /> can fit in the slot partially.
		/// </summary>
		/// <param name="leftOver">
		/// The amount of items left over after simulated stacking.
		/// <para />
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.
		/// </param>
		public static bool CanItemStackPartially(this ItemStorage storage, int slot, Item item, out int leftOver) {
			leftOver = 0;
			if (item is null || item.IsAir) {
				return false;
			}

			if (!storage.IsItemValid(slot, item)) {
				return false;
			}

			leftOver = item.stack;
			int size = storage.MaxStackFor(slot, item);
			if (size <= 0) {
				return false;
			}

			Item storageItem = storage.Items[slot];
			if (storageItem.IsAir) {
				leftOver = item.stack - size;
			}
			else {
				if (!ItemStorage.CanItemsStack(storageItem, item)) {
					return false;
				}

				leftOver = (storageItem.stack + item.stack) - size;
			}

			return true;
		}

		/// <summary>
		/// Gets if this item can be inserted completely into the storage.
		/// </summary>
		public static bool CanInsertItem(this ItemStorage storage, object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}

			item = item.Clone();
			for (int i = 0; i < storage.Count; i++) {
				if (storage.CanInteract(i, ItemStorage.Operation.Insert, user) && storage.CanItemStackPartially(i, item, out int leftOver)) {
					item.stack = leftOver;
					if (item.stack <= 0) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets if this item can be inserted, even partially, into the storage.
		/// </summary>
		public static bool CanInsertItemPartially(this ItemStorage storage, object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}

			for (int i = 0; i < storage.Count; i++) {
				if (storage.CanInteract(i, ItemStorage.Operation.Insert, user) && storage.CanItemStackPartially(i, item, out _)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Simulates putting an item into storage and returns if it is feasible.
		/// </summary>
		/// <param name="leftOver">
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.
		/// </param>
		/// <returns>
		/// True if the item was successfully inserted, even partially. False if the item is air, if the slot is already
		/// fully occupied, if the slot rejects the item, or if the slot rejects the user.
		/// </returns>
		public static bool CanInsertItem(this ItemStorage storage, object? user, int slot, Item item, out int leftOver) {
			leftOver = 0;
			if (item == null || item.IsAir)
				return false;

			storage.ValidateSlotIndex(slot);

			return storage.CanInteract(slot, ItemStorage.Operation.Insert, user) && storage.CanItemStackPartially(slot, item, out leftOver);
		}

		/// <summary>
		/// Transfers an item from one item storage to another.
		/// </summary>
		/// <param name="from">The item storage to take from.</param>
		/// <param name="user">The object doing this.</param>
		/// <param name="to">The item storage to send into.</param>
		/// <param name="fromSlot">The slot to take from.</param>
		/// <param name="amount">The amount of items to take from the slot.</param>
		public static void Transfer(this ItemStorage from, object? user, ItemStorage to, int fromSlot, int amount) {
			if (from.RemoveItem(user, fromSlot, out var item, amount)) {
				to.InsertItem(user, ref item);
				from.InsertItem(user, ref item);
			}
		}

		/// <summary>
		/// Drops items from the storage into the rectangle specified.
		/// </summary>
		public static void DropItems(this ItemStorage storage, object? user, Rectangle hitbox) {
			for (int i = 0; i < storage.Count; i++) {
				Item item = storage[i];
				if (!item.IsAir) {
					Item.NewItem(hitbox, item.type, item.stack, prefixGiven: item.prefix);
					storage.RemoveItem(user, i);
				}
			}
		}

		/// <summary>
		/// Quick stacks player's items into the storage.
		/// </summary>
		public static void QuickStack(this Player player, ItemStorage storage) {
			for (int i = 49; i >= 10; i--) {
				Item inventory = player.inventory[i];

				if (!inventory.IsAir && storage.Contains(inventory.type))
					storage.InsertItem(player, ref inventory);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

		/// <summary>
		/// Loots storage's items into a player's inventory
		/// </summary>
		public static void LootAll(this Player player, ItemStorage storage) {
			for (int i = 0; i < storage.Count; i++) {
				Item item = storage[i];
				if (!item.IsAir) {
					item.position = player.Center;
					item.noGrabDelay = 0;

					// bug: wrong logic
					foreach (var split in item.Split()) {
						player.GetItem(player.whoAmI, split, GetItemSettings.LootAllSettings);
					}

					storage.RemoveItem(player, i, out _);
				}
			}
		}

		/// <summary>
		/// Loots storage's items into the player's inventory.
		/// </summary>
		public static void Loot(this Player player, ItemStorage storage, int slot) {
			Item item = storage[slot];
			if (!item.IsAir) {
				Item n = new Item(item.type);

				int count = Math.Min(item.stack, item.maxStack);
				n.stack = count;
				n.position = player.Center;
				n.noGrabDelay = 0;

				// bug: wrong logic
				player.GetItem(player.whoAmI, n, GetItemSettings.LootAllSettings);

				storage.ModifyStackSize(player, slot, -count);
			}
		}

		/// <summary>
		/// Deposits a player's items into storage.
		/// </summary>
		public static void DepositAll(this Player player, ItemStorage storage) {
			for (int i = 49; i >= 10; i--) {
				Item item = player.inventory[i];
				if (item.IsAir || item.favorited) continue;
				storage.InsertItem(player, ref item);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

		/// <summary>
		/// Combines several stacks of items into one stack, disregarding max stack.
		/// </summary>
		public static Item Combine(IEnumerable<Item> items) {
			Item ret = new Item();

			foreach (Item item in items) {
				if (ret.IsAir && !item.IsAir) {
					ret = item.Clone();
					ret.stack = 0;
				}

				if (ret.type == item.type) ret.stack += item.stack;
			}

			return ret;
		}

		/// <summary>
		/// Splits a stack of items into separate stacks that respect max stack.
		/// </summary>
		public static IEnumerable<Item> Split(this Item item) {
			while (item.stack > 0) {
				Item clone = item.Clone();
				int count = Math.Min(item.stack, item.maxStack);
				clone.stack = count;
				yield return clone;

				item.stack -= count;
				if (item.stack <= 0) {
					item.TurnToAir();
					yield break;
				}
			}
		}
	}
}