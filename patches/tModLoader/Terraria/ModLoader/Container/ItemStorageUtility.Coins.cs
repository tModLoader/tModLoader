#nullable enable

using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.Container
{
	public static partial class ItemStorageUtility
	{
		private static Dictionary<int, Item> CoinItems;

		static ItemStorageUtility() {
			CoinItems = new Dictionary<int, Item>();

			for (int i = 0; i < 4; i++) {
				CoinItems.Add(ItemID.CopperCoin + i, new Item(ItemID.CopperCoin + i));
			}
		}

		/// <summary>
		/// Gets the coin value for a storage.
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

		/// <summary>
		/// Removes coins from the storage
		/// </summary>
		/// <returns>Returns false if amount is lt or eq 0 or fails to fit change</returns>
		public static bool RemoveCoins(this ItemStorage storage, object? user, ref long amount) {
			if (amount <= 0)
				return false;

			Dictionary<int, List<int>> slotsEmptyForCoin = new Dictionary<int, List<int>> {
				{ ItemID.CopperCoin, new List<int>() },
				{ ItemID.SilverCoin, new List<int>() },
				{ ItemID.GoldCoin, new List<int>() },
				{ ItemID.PlatinumCoin, new List<int>() }
			};

			for (int i = 0; i < storage.Count; i++) {
				CheckEmptySlotForValidCoins(i);
			}

			List<int> slotCoins = new List<int>();
			for (int i = 0; i < storage.Count; i++) {
				if (storage[i].IsACoin) {
					slotCoins.Add(i);
				}
			}

			Dictionary<int, Item> dictionary = new Dictionary<int, Item>();
			bool result = true;
			while (amount > 0) {
				// Decrement existing coins without splitting				
				for (int i = 0, coinValue = 1000000; i < 4; i++, coinValue /= 100) {
					if (amount < coinValue)
						continue;

					foreach (int slotCoin in slotCoins) {
						if (storage[slotCoin].type == 74 - i) {
							long toRemove = amount / coinValue;
							dictionary[slotCoin] = storage[slotCoin].Clone();
							if (toRemove < storage[slotCoin].stack) {
								storage[slotCoin].stack -= (int)toRemove;
							}
							else {
								storage[slotCoin].SetDefaults();

								CheckEmptySlotForValidCoins(slotCoin);
							}

							amount -= coinValue * (dictionary[slotCoin].stack - storage[slotCoin].stack);
						}
					}
				}

				if (amount <= 0)
					return true;

				if (slotsEmptyForCoin.Any(pair => pair.Value.Count > 0)) {
					foreach (KeyValuePair<int, List<int>> pair in slotsEmptyForCoin) {
						pair.Value.Sort((a, b) => b.CompareTo(a));
					}

					int changedIndex = -1;
					for (int i = 0, coinValue = 10000; i < storage.Count; i++) {
						for (int j = 0; j < 3; j++, coinValue /= 100) {
							if (amount < coinValue)
								continue;

							foreach (int slotCoin in slotCoins) {
								if (storage[slotCoin].type != 74 - j || storage[slotCoin].stack <= 0)
									continue;

								if (--storage[slotCoin].stack <= 0) {
									storage[slotCoin].SetDefaults();

									CheckEmptySlotForValidCoins(slotCoin);
								}

								if (!TryPopFirst(slotsEmptyForCoin[73 - j], out int index))
									goto Fail;

								dictionary[index] = storage[index].Clone();
								storage[index].SetDefaults(73 - j);
								storage[index].stack = 100;
								changedIndex = index;

								break;
							}
						}

						for (int l = 0; l < 2; l++) {
							if (changedIndex != -1)
								continue;

							foreach (int slotCoin in slotCoins) {
								if (storage[slotCoin].type != 73 + l || storage[slotCoin].stack <= 0)
									continue;

								if (--storage[slotCoin].stack <= 0) {
									storage[slotCoin].SetDefaults();

									CheckEmptySlotForValidCoins(slotCoin);
								}

								if (!TryPopFirst(slotsEmptyForCoin[72 + l], out int index))
									goto Fail;

								dictionary[index] = storage[index].Clone();
								storage[index].SetDefaults(72 + l);
								storage[index].stack = 100;
								changedIndex = index;

								break;
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

				Fail:

				// Return the storage into original state
				foreach (KeyValuePair<int, Item> pair in dictionary) {
					storage[pair.Key] = pair.Value.Clone();
				}

				result = false;
				break;
			}

			// note: any OnChanged events have to be performed here

			return result;

			void CheckEmptySlotForValidCoins(int slot) {
				Item item = storage[slot];
				if (!item.IsAir) return;

				foreach (KeyValuePair<int, List<int>> pair in slotsEmptyForCoin) {
					if (storage.IsItemValid(slot, CoinItems[pair.Key])) {
						pair.Value.Add(slot);
					}
				}
			}

			static bool TryPopFirst<T>(List<T> list, out T value) {
				if (list.Count <= 0) {
					value = default;
					return false;
				}

				value = list[0];
				list.RemoveAt(0);
				return true;
			}
		}

		/// <summary>
		/// Inserts coins to the storage
		/// </summary>
		/// <returns>Returns false if amount is lt or eq 0 or fails to fit change</returns>
		public static bool InsertCoins(this ItemStorage storage, object? user, long amount) {
			if (amount <= 0)
				return false;

			Item[] old = new Item[storage.Count];
			for (int i = 0; i < storage.Count; i++) {
				old[i] = storage[i].Clone();
			}

			// platinum coin
			while (amount >= 1000000) {
				int emptyIndex = -1;
				for (int i = storage.Count - 1; i >= 0; i--) {
					if (emptyIndex == -1 && storage[i].IsAir && storage.IsItemValid(i, CoinItems[ItemID.PlatinumCoin]))
						emptyIndex = i;

					while (storage[i].type == ItemID.PlatinumCoin && storage[i].stack < storage.MaxStackFor(i, storage[i]) && amount >= 1000000) {
						storage[i].stack++;
						amount -= 1000000;
						DoCoins(i);
						if (storage[i].stack == 0 && emptyIndex == -1)
							emptyIndex = i;
					}
				}

				if (amount >= 1000000) {
					if (emptyIndex == -1)
						goto Fail;

					storage[emptyIndex].SetDefaults(ItemID.PlatinumCoin);
					amount -= 1000000;
				}
			}

			// gold coin
			while (amount >= 10000) {
				int emptyIndex = -1;
				for (int i = storage.Count - 1; i >= 0; i--) {
					if (emptyIndex == -1 && storage[i].IsAir && storage.IsItemValid(i, CoinItems[ItemID.GoldCoin]))
						emptyIndex = i;

					while (storage[i].type == ItemID.GoldCoin && storage[i].stack < storage.MaxStackFor(i, storage[i]) && amount >= 10000) {
						storage[i].stack++;
						amount -= 10000;
						DoCoins(i);
						if (storage[i].stack == 0 && emptyIndex == -1)
							emptyIndex = i;
					}
				}

				if (amount >= 10000) {
					if (emptyIndex == -1)
						goto Fail;

					storage[emptyIndex].SetDefaults(ItemID.GoldCoin);
					amount -= 10000;
				}
			}

			// silver coin
			while (amount >= 100) {
				int emptyIndex = -1;
				for (int i = storage.Count - 1; i >= 0; i--) {
					if (emptyIndex == -1 && storage[i].IsAir && storage.IsItemValid(i, CoinItems[ItemID.SilverCoin]))
						emptyIndex = i;

					while (storage[i].type == ItemID.SilverCoin && storage[i].stack < storage.MaxStackFor(i, storage[i]) && amount >= 100) {
						storage[i].stack++;
						amount -= 100;
						DoCoins(i);
						if (storage[i].stack == 0 && emptyIndex == -1)
							emptyIndex = i;
					}
				}

				if (amount >= 100) {
					if (emptyIndex == -1)
						goto Fail;

					storage[emptyIndex].SetDefaults(ItemID.SilverCoin);
					amount -= 100;
				}
			}

			// copper coin
			while (amount >= 1) {
				int emptyIndex = -1;
				for (int i = storage.Count - 1; i >= 0; i--) {
					if (emptyIndex == -1 && storage[i].IsAir && storage.IsItemValid(i, CoinItems[ItemID.CopperCoin]))
						emptyIndex = i;

					while (storage[i].type == ItemID.CopperCoin && storage[i].stack < storage.MaxStackFor(i, storage[i]) && amount >= 1) {
						storage[i].stack++;
						amount--;
						DoCoins(i);
						if (storage[i].stack == 0 && emptyIndex == -1)
							emptyIndex = i;
					}
				}

				if (amount >= 1) {
					if (emptyIndex == -1)
						goto Fail;

					storage[emptyIndex].SetDefaults(ItemID.CopperCoin);
					amount--;
				}
			}

			return true;

			Fail:
			for (int j = 0; j < storage.Count; j++) {
				storage[j] = old[j].Clone();
			}

			return false;

			void DoCoins(int slot) {
				if (storage[slot].stack != 100 || (storage[slot].type != ItemID.CopperCoin && storage[slot].type != ItemID.SilverCoin && storage[slot].type != ItemID.GoldCoin))
					return;

				// replace 100 coin stack with greater value
				// note: we don't know if this is a valid slot for the coins
				storage[slot].SetDefaults(storage[slot].type + 1);
				for (int i = 0; i < storage.Count; i++) {
					if (storage[i].IsTheSameAs(storage[slot]) && i != slot && storage[i].type == storage[slot].type && storage[i].stack < storage.MaxStackFor(i, storage[i])) {
						storage[i].stack++;

						storage[slot].SetDefaults();
						storage[slot].active = false;
						storage[slot].TurnToAir();
						DoCoins(i);
					}
				}
			}
		}
	}
}