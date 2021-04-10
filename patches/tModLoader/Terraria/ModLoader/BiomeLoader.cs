using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace Terraria.ModLoader
{
	public static class BiomeLoader
	{
		internal static readonly IList<ModBiome> biomes = new List<ModBiome>();

		private class HookList
		{
			public int[] arr;
			public readonly MethodInfo method;

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModBiome, F>> func) {
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		internal static void Add(ModBiome biome) {
			biome.index = biomes.Count;
			biomes.Add(biome);
		}

		internal static void Unload() {
			biomes.Clear();
		}

		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(biomes, hook.method).Select(p => p.index).ToArray();
			}
		}

		// Internal boilerplate

		internal static void SetupPlayer(Player player) {
			player.modBiomeFlags = new System.Collections.BitArray(biomes.Count);
		}

		public static void UpdateBiomes(Player player) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				bool prev = player.modBiomeFlags[i];
				bool value = player.modBiomeFlags[i] = biomes[i].IsBiomeActive(player);

				if (!prev && value)
					biomes[i].OnEnter(player);
				else if (!value && prev)
					biomes[i].OnLeave(player);

				if (value)
					biomes[i].OnInBiome(player);
			}
		}

		public static bool CustomBiomesMatch(Player player, Player other) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				if (player.modBiomeFlags[i] ^ other.modBiomeFlags[i])
					return false;
			}
			return true;
		}

		public static void CopyCustomBiomesTo(Player player, Player other) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				other.modBiomeFlags[i] = player.modBiomeFlags[i];
			}
		}

		public static void SendCustomBiomes(Player player, BinaryWriter writer) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				writer.Write(player.modBiomeFlags[i]);
			}
		}

		public static void ReceiveCustomBiomes(Player player, BinaryReader reader) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				player.modBiomeFlags[i] = reader.ReadBoolean();
			}
		}

		// Hooks

		private static HookList HookPostUpdateBiome = AddHook<Action<Player>>(b => b.BiomeVisuals);

		public static void PostUpdateBiome(Player player) {
			foreach (int index in HookPostUpdateBiome.arr) {
				biomes[index].BiomeVisuals(player);
			}
		}

		private static HookList HookModifyShopPrices = AddHook<Action<HelperInfo, ShopHelper>>(b => b.ModifyShopPrices);

		public static void ModifyShopPrices(HelperInfo info, ShopHelper s) {
			foreach (int index in HookModifyShopPrices.arr) {
				if (info.player.modBiomeFlags[index]) {
					biomes[index].ModifyShopPrices(info, s);
				}
			}
		}

		public const int VanillaPrimaryBiomeCount = 11;

		public static int GetPrimaryModBiome(Player player, out AVFXPriority priority) {
			int index = 0, weight = 0;
			priority = AVFXPriority.None;

			for (int i = 0; i < biomes.Count; i++) {
				bool active = player.modBiomeFlags[i] && biomes[i].IsPrimaryBiome;
				int tst = biomes[i].GetCorrWeight(player);
				if (active && tst > weight) {
					index = i + VanillaPrimaryBiomeCount;
					priority = biomes[i].Priority;
					weight = tst;
				}
			}

			return index;
		}
	}
}
