using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader.Container
{
	public static class ItemHandlerUtility
	{
		/// <summary>
		///     Quick stacks player's items into the ItemHandler
		/// </summary>
		public static void QuickStack(this Player player, ItemHandler handler) {
			for (int i = 49; i >= 10; i--) {
				ref Item inventory = ref player.inventory[i];

				if (!inventory.IsAir && handler.Contains(inventory.type)) handler.InsertItem(ref inventory);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

		/// <summary>
		///     Loots ItemHandler's items into player's inventory
		/// </summary>
		public static void LootAll(this Player player, ItemHandler handler) {
			for (int i = 0; i < handler.Slots; i++) {
				ref Item item = ref handler.GetItemInSlot(i);
				if (!item.IsAir) {
					item.position = player.Center;

					item = Combine(item.Split().Select(split => player.GetItem(player.whoAmI, split, GetItemSettings.LootAllSettings)));

					handler.OnContentsChanged?.Invoke(i, true);
				}
			}
		}

		/// <summary>
		///     Loots ItemHandler's items into player's inventory
		/// </summary>
		public static void Loot(this Player player, ItemHandler handler, int slot) {
			ref Item item = ref handler.GetItemInSlot(slot);
			if (!item.IsAir) {
				Item n = new Item(item.type);

				int count = Math.Min(item.stack, item.maxStack);
				n.stack = count;
				n.position = player.Center;
				player.GetItem(player.whoAmI, n, GetItemSettings.LootAllSettings);

				item.stack -= count;
				if (item.stack <= 0) item.TurnToAir();

				handler.OnContentsChanged?.Invoke(slot, true);
			}
		}

		/// <summary>
		///     Deposits player's items into the ItemHandler
		/// </summary>
		public static void DepositAll(this Player player, ItemHandler handler) {
			for (int i = 53; i >= 10; i--) {
				ref Item item = ref player.inventory[i];
				if (item.IsAir || item.favorited) continue;
				handler.InsertItem(ref item);
			}

			SoundEngine.PlaySound(SoundID.Grab);
		}

		public static Item Combine(IEnumerable<Item> items) {
			List<Item> list = items.ToList();

			Item ret = new Item();

			foreach (Item item in list) {
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