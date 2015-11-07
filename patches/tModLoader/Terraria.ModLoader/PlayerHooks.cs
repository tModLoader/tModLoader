using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

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

		internal static void OnHitNPC(Player player, float x, float y, Entity victim)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				if (CanHitNPC(player, (NPC)victim))
				{
					modPlayer.OnHitNPC(x, y, victim);
				}
			}
		}

		internal static bool CanHitNPC(Player player, NPC npc)
		{
			bool flag = true;
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				if (!modPlayer.CanHitNPC(npc))
				{
					flag = false;
				}
			}
			return flag;
		}

		internal static void UpdateBiomes(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateBiomes();
			}
		}

		internal static void UpdateBiomeVisuals(Player player, string biomeName, bool inZone, Vector2 activationSource)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateBiomeVisuals(biomeName, inZone, activationSource);
			}
		}

		internal static void UpdateDead(Player player)
		{
			foreach (ModPlayer modPlayer in player.modPlayers)
			{
				modPlayer.UpdateDead();
			}
		}
	}
}
