using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.WorldBuilding;

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

		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(biomes, hook.method).Select(p => p.index).ToArray();
			}
		}

		// Internal boilerplate

		internal static void SetupPlayer(Player player) {
			player.modBiomeFlags = new bool[biomes.Count];
		}

		public static void UpdateBiomes(Player player) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				biomes[i].Update(player, ref player.modBiomeFlags[i]);
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

		private static HookList HookUpdateBiomeVisuals = AddHook<Action<Player>>(b => b.UpdateBiomeVisuals);

		public static void UpdateBiomeVisuals(Player player) {
			foreach (int index in HookUpdateBiomeVisuals.arr) {
				biomes[index].UpdateBiomeVisuals(player);
			}
		}

		private delegate void DelegateModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight);
		private static HookList HookModifyWorldGenTasks = AddHook<DelegateModifyWorldGenTasks>(b => b.ModifyWorldGenTasks);

		public static void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
			foreach (var index in HookModifyWorldGenTasks.arr) {
				biomes[index].ModifyWorldGenTasks(tasks, ref totalWeight);
			}
		}
	}
}
