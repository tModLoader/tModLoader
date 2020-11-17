using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader.Container
{
	public static class ItemStorageUtility
	{
		public static bool Contains(this ItemStorage storage, int type) => storage.Items.Any(item => !item.IsAir && item.type == type);

		public static bool Contains(this ItemStorage storage, Item item) => storage.Items.Any(item.IsTheSameAs);

		/// <summary>
		///     Gets the coin value for a given item handler
		/// </summary>
		public static long CountCoins(this ItemStorage storage) {
			long num = 0L;
			foreach (Item item in storage.Items) {
				if (item.IsAir) continue;

				num += item.type switch {
					ItemID.CopperCoin => item.stack,
					ItemID.SilverCoin => item.stack * 100,
					ItemID.GoldCoin => item.stack * 10000,
					ItemID.PlatinumCoin => item.stack * 1000000,
					_ => 0
				};
			}

			return num;
		}

		public static void DropItems(this ItemStorage storage, Rectangle hitbox) {
			for (int i = 0; i < storage.Length; i++) {
				ref Item item = ref storage.GetItemInSlot(i);
				if (!item.IsAir) {
					Item.NewItem(hitbox, item.type, item.stack, prefixGiven: item.prefix);
					item.TurnToAir();
					storage.OnContentsChanged(i, false);
				}
			}
		}

		/// <summary>
		///     Quick stacks player's items into the ItemStorage
		/// </summary>
		public static void QuickStack(this Player player, ItemStorage storage) {
			for (int i = 49; i >= 10; i--) {
				ref Item inventory = ref player.inventory[i];

				if (!inventory.IsAir && storage.Contains(inventory.type)) storage.InsertItem(ref inventory, true);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

		/// <summary>
		///     Loots ItemStorage's items into player's inventory
		/// </summary>
		public static void LootAll(this Player player, ItemStorage storage) {
			for (int i = 0; i < storage.Length; i++) {
				ref Item item = ref storage.GetItemInSlot(i);
				if (!item.IsAir) {
					item.position = player.Center;
					item.noGrabDelay = 0;

					item = Combine(item.Split().Select(split => player.GetItem(player.whoAmI, split, GetItemSettings.LootAllSettings)));

					storage.OnContentsChanged(i, true);
				}
			}
		}

		/// <summary>
		///     Loots ItemStorage's items into player's inventory
		/// </summary>
		public static void Loot(this Player player, ItemStorage storage, int slot) {
			ref Item item = ref storage.GetItemInSlot(slot);
			if (!item.IsAir) {
				Item n = new Item(item.type);

				int count = Math.Min(item.stack, item.maxStack);
				n.stack = count;
				n.position = player.Center;
				n.noGrabDelay = 0;

				player.GetItem(player.whoAmI, n, GetItemSettings.LootAllSettings);

				item.stack -= count;
				if (item.stack <= 0) item.TurnToAir();

				storage.OnContentsChanged(slot, true);
			}
		}

		/// <summary>
		///     Deposits player's items into the ItemStorage
		/// </summary>
		public static void DepositAll(this Player player, ItemStorage storage) {
			for (int i = 49; i >= 10; i--) {
				ref Item item = ref player.inventory[i];
				if (item.IsAir || item.favorited) continue;
				storage.InsertItem(ref item);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

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