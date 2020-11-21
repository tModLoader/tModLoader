using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

#nullable enable
namespace Terraria.ModLoader.Container
{
	public static class ItemStorageUtility
	{
		public static bool Contains(this ItemStorage storage, int type) => storage.Any(item => !item.IsAir && item.type == type);

		public static bool Contains(this ItemStorage storage, Item item) => storage.Any(item.IsTheSameAs);

		/// <summary>
		///     If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, int)" />. It is much
		///     faster.
		/// </summary>
		public static int Count(this ItemStorage storage, int type) => storage.Count(item => !item.IsAir && item.type == type);

		/// <summary>
		///     If you need to check if a storage contains an item, use <see cref="Contains(ItemStorage, Item)" />. It is much
		///     faster.
		/// </summary>
		public static int Count(this ItemStorage storage, Item item) => storage.Count(item.IsTheSameAs);

		/// <summary>
		/// Gets if this item can be inserted completely into the storage.
		/// </summary>
		public static bool CanInsert(this ItemStorage storage, object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}
			item = item.Clone();
			for (int i = 0; i < storage.Count; i++) {
				if (!storage.CanInteract(i, ItemStorage.Operation.Input, user)) {
					return false;
				}

				if (storage.CanItemStackPartially(i, item, out int leftOver)) {
					item.stack = leftOver;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets if this item can be inserted, even partially, into the storage.
		/// </summary>
		public static bool CanInsertPartially(this ItemStorage storage, object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}
			for (int i = 0; i < storage.Count; i++) {
				if (!storage.CanInteract(i, ItemStorage.Operation.Input, user)) {
					return false;
				}

				if (storage.CanItemStackPartially(i, item, out int leftOver)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		///     Gets the coin value for a storage.
		/// </summary>
		public static long CountCoins(this ItemStorage storage) {
			long num = 0L;
			foreach (Item item in storage) {
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

		// private static Item[] CoinsSplit(long count) {
		// 	Item[] array = new Item[4];
		// 	long coinsAdded = 0L;
		// 	long currentCoin = 1000000L;
		// 	for (int i = 3; i >= 0; i--) {
		// 		array[i] = new Item(ItemID.CopperCoin + i) { stack = (int)((count - coinsAdded) / currentCoin) };
		// 		coinsAdded += array[i].stack * currentCoin;
		// 		currentCoin /= 100;
		// 	}
		//
		// 	return array;
		// }

		public static bool RemoveCoins(this ItemStorage storage, object? user, ref long amount) {
			if (amount <= 0) return false;
			
			Dictionary<int, List<int>> slotsEmptyForCoin = new Dictionary<int, List<int>> {
				{ ItemID.CopperCoin, new List<int>() },
				{ ItemID.SilverCoin, new List<int>() },
				{ ItemID.GoldCoin, new List<int>() },
				{ ItemID.PlatinumCoin, new List<int>() }
			};
			foreach (KeyValuePair<int, List<int>> pair in slotsEmptyForCoin) {
				Item coin = new Item(pair.Key);

				for (int i = 0; i < storage.Count; i++) {
					Item item = storage[i];
					if (!item.IsAir) continue;

					if (storage.IsInsertValid(i, coin)) {
						pair.Value.Add(i);
					}
				}
			}

			List<int> slotCoins = new List<int>();
			for (int i = 0; i < storage.Count; i++) {
				if (storage[i].IsACoin) {
					slotCoins.Add(i);
				}
			}

			Dictionary<int, Item> dictionary = new Dictionary<int, Item>();
			bool result = false;
			while (amount > 0) {
				long coinValue = 1000000L;
				for (int i = 0; i < 4; i++) {
					if (amount >= coinValue) {
						foreach (int slotCoin in slotCoins) {
							if (storage[slotCoin].type == 74 - i) {
								long toRemove = amount / coinValue;
								dictionary[slotCoin] = storage[slotCoin].Clone();
								if (toRemove < storage[slotCoin].stack) {
									storage[slotCoin].stack -= (int)toRemove;
								}
								else {
									storage[slotCoin].SetDefaults();
									slotsEmptyForCoin[74 - i].Add(slotCoin);
								}

								amount -= coinValue * (dictionary[slotCoin].stack - storage[slotCoin].stack);
							}
						}
					}

					coinValue /= 100;
				}

				if (amount <= 0)
					continue;

				if (slotsEmptyForCoin.Count > 0) {
					foreach (KeyValuePair<int, List<int>> pair in slotsEmptyForCoin) {
						pair.Value.Sort((a, b) => b.CompareTo(a));
					}

					int changedIndex = -1;
					for (int j = 0; j < storage.Count; j++) {
						coinValue = 10000L;
						for (int k = 0; k < 3; k++) {
							if (amount >= coinValue) {
								foreach (int slotCoin in slotCoins) {
									if (storage[slotCoin].type == 74 - k && storage[slotCoin].stack >= 1) {
										if (--storage[slotCoin].stack <= 0) {
											storage[slotCoin].SetDefaults();
											slotsEmptyForCoin[74 - k].Add(slotCoin);
										}

										int index = PopFirst(slotsEmptyForCoin[73 - k]);

										dictionary[index] = storage[index].Clone();
										storage[index].SetDefaults(73 - k);
										storage[index].stack = 100;
										changedIndex = index;

										break;
									}
								}
							}

							if (changedIndex != -1)
								break;

							coinValue /= 100;
						}

						for (int l = 0; l < 2; l++) {
							if (changedIndex != -1)
								continue;

							foreach (int slotCoin in slotCoins) {
								if (storage[slotCoin].type == 73 + l && storage[slotCoin].stack >= 1) {
									if (--storage[slotCoin].stack <= 0) {
										storage[slotCoin].SetDefaults();
										slotsEmptyForCoin[73 + l].Add(slotCoin);
									}

									int index = PopFirst(slotsEmptyForCoin[72 + l]);

									dictionary[index] = storage[index].Clone();
									storage[index].SetDefaults(72 + l);
									storage[index].stack = 100;
									changedIndex = index;

									break;
								}
							}
						}

						if (changedIndex != -1) {
							slotCoins.Add(changedIndex);
							break;
						}
					}

					foreach (KeyValuePair<int, List<int>> pair in slotsEmptyForCoin) {
						pair.Value.Sort((a, b) => b.CompareTo(a));
					}

					continue;
				}

				foreach (KeyValuePair<int, Item> pair in dictionary) {
					Item item = pair.Value;
					storage.InsertItem(user, pair.Key, ref item);
				}

				result = true;
				break;
			}

			return !result;

			static T PopFirst<T>(List<T> list) {
				T temp = list[0];
				list.RemoveAt(0);
				return temp;
			}
		}

		public static bool InsertCoins(this ItemStorage storage, object? user, long amount) {
			// if (amount < 0) return false;
			// long storageCoinCount = storage.CountCoins();
			//
			// Item[] coins = CoinsSplit(storageCoinCount + amount);
			//
			// // check if the storage will fit new coins
			// var cloned = storage.Clone();
			// for (int i = 0; i < cloned.Count; i++) {
			// 	if (cloned[i].IsACoin) {
			// 		cloned[i].TurnToAir();
			// 	}
			// }
			//
			// bool flag = true;
			// for (int i = 0; i < coins.Length; i++) {
			// 	if (coins[i].IsAir) continue;
			//
			// 	flag &= cloned.CanInsert(user, coins[i]);
			// }
			//
			// if (!flag) {
			// 	return false;
			// }
			// // -----
			//
			// for (int i = 0; i < storage.Count; i++) {
			// 	if (storage[i].IsACoin) {
			// 		storage[i].TurnToAir();
			// 	}
			// }
			//
			// for (int i = 0; i < coins.Length; i++) {
			// 	if (coins[i].IsAir) continue;
			//
			// 	storage.InsertItem(user, ref coins[i]);
			// }

			return true;
		}

		/// <summary>
		///     Transfers an item from one item storage to another.
		/// </summary>
		/// <param name="from">The item storage to take from.</param>
		/// <param name="user">The object doing this.</param>
		/// <param name="to">The item storage to send into.</param>
		/// <param name="fromSlot">The slot to take from.</param>
		/// <param name="amount">The amount of items to take from the slot.</param>
		public static void Transfer(this ItemStorage from, object? user, ItemStorage to, int fromSlot, int amount) {
			if (from.RemoveItem(user, fromSlot, out var item, amount)) {
				to.InsertItem(user, ref item);
				from.InsertItemStartingFrom(user, fromSlot, 1, ref item);
			}
		}

		/// <summary>
		///     Drops items from the storage into the rectangle specified.
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
		///     Quick stacks player's items into the storage.
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
		///     Loots storage's items into a player's inventory
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
		///     Loots storage's items into the player's inventory.
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
		///     Deposits a player's items into storage.
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
		///     Combines several stacks of items into one stack, disregarding max stack.
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
		///     Splits a stack of items into separate stacks that respect max stack.
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