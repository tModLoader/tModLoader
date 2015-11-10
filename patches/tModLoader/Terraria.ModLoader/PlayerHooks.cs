using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader
{
	internal static class PlayerHooks
	{
		private static readonly IList<ModPlayer> players = new List<ModPlayer>();
		private static readonly IDictionary<string, IDictionary<string, int>> indexes = new Dictionary<string, IDictionary<string, int>>();

		internal static void Add(ModPlayer player)
		{
			string mod = player.mod.Name;
			if (!indexes.ContainsKey(mod))
			{
				indexes[mod] = new Dictionary<string, int>();
			}
			indexes[mod][player.Name] = players.Count;
			players.Add(player);
		}

		internal static void Unload()
		{
			players.Clear();
			indexes.Clear();
		}

		internal static void SetupPlayer(Player player)
		{
			foreach (ModPlayer modPlayer in players)
			{
				ModPlayer newPlayer = modPlayer.Clone();
				newPlayer.player = player;
				player.modPlayers.Add(newPlayer);
			}
		}

		internal static ModPlayer GetModPlayer(Player player, Mod mod, string name)
		{
			IDictionary<string, int> modIndexes = indexes[mod.Name];
			if (!modIndexes.ContainsKey(name))
			{
				return null;
			}
			return player.modPlayers[modIndexes[name]];
		}

		internal static void ResetEffects(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.ResetEffects();
			}
		}

		internal static void UpdateDead(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateDead();
			}
		}

		internal static IList<Item> SetupStartInventory(Player player)
		{
			IList<Item> items = new List<Item>();
			Item item = new Item();
			item.SetDefaults("Copper Shortsword");
			item.Prefix(-1);
			items.Add(item);
			item = new Item();
			item.SetDefaults("Copper Pickaxe");
			item.Prefix(-1);
			items.Add(item);
			item = new Item();
			item.SetDefaults("Copper Axe");
			item.Prefix(-1);
			items.Add(item);
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.SetupStartInventory(items);
			}
			IDictionary<int, int> counts = new Dictionary<int, int>();
			foreach (Item item0 in items)
			{
				if (item0.maxStack > 1)
				{
					if (!counts.ContainsKey(item0.netID))
					{
						counts[item0.netID] = 0;
					}
					counts[item0.netID] += item0.stack;
				}
			}
			int k = 0;
			while (k < items.Count)
			{
				bool flag = true;
				int id = items[k].netID;
				if (counts.ContainsKey(id))
				{
					items[k].stack = counts[id];
					if (items[k].stack > items[k].maxStack)
					{
						items[k].stack = items[k].maxStack;
					}
					counts[id] -= items[k].stack;
					if (items[k].stack <= 0)
					{
						items.RemoveAt(k);
						flag = false;
					}
				}
				if (flag)
				{
					k++;
				}
			}
			return items;
		}

		internal static void SetStartInventory(Player player, IList<Item> items)
		{
			if (items.Count <= 50)
			{
				for (int k = 0; k < items.Count; k++)
				{
					player.inventory[k] = items[k];
				}
			}
			else
			{
				for (int k = 0; k < 49; k++)
				{
					player.inventory[k] = items[k];
				}
				Item bag = new Item();
				bag.SetDefaults(ModLoader.GetMod("ModLoader").ItemType("StartBag"));
				for (int k = 49; k < items.Count; k++)
				{
					((StartBag)bag.modItem).AddItem(items[k]);
				}
				player.inventory[49] = bag;
			}
		}

		internal static void SetStartInventory(Player player)
		{
			SetStartInventory(player, SetupStartInventory(player));
		}

		internal static void UpdateBiomes(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateBiomes();
			}
		}

		internal static void UpdateBiomeVisuals(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateBiomeVisuals();
			}
		}

		internal static void UpdateBadLifeRegen(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateBadLifeRegen();
			}
		}

		internal static void UpdateLifeRegen(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateLifeRegen();
			}
		}

		internal static void NaturalLifeRegen(Player player, ref float regen)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.NaturalLifeRegen(ref regen);
			}
		}
	}
}
