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

		internal static void PreUpdate(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PreUpdate();
			}
		}

		internal static void SetControls(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.SetControls();
			}
		}

		internal static void PreUpdateBuffs(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PreUpdateBuffs();
			}
		}

		internal static void PostUpdateBuffs(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostUpdateBuffs();
			}
		}

		internal static void PostUpdateEquips(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostUpdateEquips();
			}
		}

		internal static void PostUpdateMiscEffects(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostUpdateMiscEffects();
			}
		}

		internal static void PostUpdateRunSpeeds(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostUpdateRunSpeeds();
			}
		}

		internal static void PostUpdate(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostUpdate();
			}
		}

		internal static void FrameEffects(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.FrameEffects();
			}
		}

		internal static bool PreHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref string deathText)
		{
			bool flag = true;
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				if (!modPlayer.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage,
					    ref playSound, ref genGore, ref deathText))
				{
					flag = false;
				}
			}
			return flag;
		}

		internal static void Hurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.Hurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		internal static void PostHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostHurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		internal static bool PreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genGore, ref string deathText)
		{
			bool flag = true;
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				if (!modPlayer.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref deathText))
				{
					flag = false;
				}
			}
			return flag;
		}

		internal static void Kill(Player player, double damage, int hitDirection, bool pvp, string deathText)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.Kill(damage, hitDirection, pvp, deathText);
			}
		}

		internal static bool PreItemCheck(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				if (!modPlayer.PreItemCheck())
				{
					return false;
				}
			}
			return true;
		}

		internal static void PostItemCheck(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.PostItemCheck();
			}
		}

		internal static void GetWeaponDamage(Player player, Item item, ref int damage)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.GetWeaponDamage(item, ref damage);
			}
		}

		internal static void GetWeaponKnockback(Player player, Item item, ref float knockback)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.GetWeaponKnockback(item, ref knockback);
			}
		}

		internal static void OnHitNPC(Player player, Item item, NPC target, int damage, float knockBack, bool crit)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.OnHitNPC(item, target, damage, knockBack, crit);
			}
		}

		internal static void OnHitByNPC(Player player, NPC npc, int damage, bool crit)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.OnHitByNPC(npc, damage, crit);
			}
		}

		internal static void CatchFish(Player player, Item fishingRod, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
		{
			int i = 0;
			while (i < 58)
			{
				if (player.inventory[i].stack > 0 && player.inventory[i].bait > 0)
				{
					break;
				}
				i++;
			}
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.CatchFish(fishingRod, player.inventory[i], liquidType, poolSize, worldLayer, questFish, ref caughtType, ref junk);
			}
		}

		internal static void GetFishingLevel(Player player, Item fishingRod, Item bait, ref int fishingLevel)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.GetFishingLevel(fishingRod, bait, ref fishingLevel);
			}
		}

		internal static void AnglerQuestReward(Player player, float rareMultiplier, List<Item> rewardItems)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.AnglerQuestReward(rareMultiplier, rewardItems);
			}
		}

		internal static void GetDyeTraderReward(Player player, List<int> rewardPool)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.GetDyeTraderReward(rewardPool);
			}
		}
	}
}
