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
		/// <summary>
		/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, int)" />. It is much faster.
		/// </summary>
		public static int Count(this ItemStorage storage, int type) => storage.Count(item => !item.IsAir && item.type == type);

		/// <summary>
		/// If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, Item)" />. It is much faster.
		/// </summary>
		public static int Count(this ItemStorage storage, Item item) => storage.Count(item.IsTheSameAs);

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