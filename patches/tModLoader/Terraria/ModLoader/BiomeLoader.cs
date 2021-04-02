using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.Audio;
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

		private static HookList HookPostUpdateBiome = AddHook<Action<Player>>(b => b.PostUpdateBiome);

		public static void PostUpdateBiome(Player player) {
			foreach (int index in HookPostUpdateBiome.arr) {
				biomes[index].PostUpdateBiome(player);
			}
		}

		private static HookList HookModifyShopPrices = AddHook<Action<HelperInfo, ShopHelper>>(b => b.ModifyShopPrices);

		public static void ModifyShopPrices(HelperInfo h, ShopHelper s) {
			foreach (int index in HookModifyShopPrices.arr) {
				biomes[index].ModifyShopPrices(h, s);
			}
		}

		public struct BiomeAtmosphere {
			internal Func<IAudioTrack> music;
			internal Func<Texture2D> mapBG;
			internal Func<Texture2D> worldBG;
			internal byte weight;
		}

		public static bool SetBiomeAtmosphere(Player player) {
			BiomeAtmosphere result;
			var weight = 0;

			for (int i = 0; i < biomes.Count; i++ ) {
				var tst = biomes[i].GetBiomeAtmosphere(player);
				if (tst.weight > weight) {
					result = tst;
					weight = tst.weight;
				}
			}

			if (weight == 0)
				return false;


			//TODO: Do some stuff to actually load textures, music

			return true;
		}

		internal struct BiomeWeight
		{
			internal Predicate<Player> isActiveCondition;
			internal byte weight;

			internal BiomeWeight(Predicate<Player> isActiveCondition, byte weight) {
				this.isActiveCondition = isActiveCondition;
				this.weight = weight;
			}
		}

		internal static List<BiomeWeight> vanillaPrimaryBiomes = new List<BiomeWeight>() { 
			// Weighted and ordered per Player.GetPrimaryBiome()
			new BiomeWeight(_ => true, 1), // purity
			new BiomeWeight((p) => p.position.Y > Main.worldSurface * 16.0, 2), //TBD
			new BiomeWeight((p) => p.ZoneSnow, 104),
			new BiomeWeight((p) => p.ZoneDesert, 102),
			new BiomeWeight((p) => p.ZoneJungle, 105),
			new BiomeWeight((p) => p.ZoneBeach, 103),
			new BiomeWeight((p) => p.ZoneHallow, 110),
			new BiomeWeight((p) => p.ZoneGlowshroom, 120),
			new BiomeWeight((p) => p.ZoneDungeon, 195),
			new BiomeWeight((p) => p.ZoneCorrupt, 131),
			new BiomeWeight((p) => p.ZoneCrimson, 130),
		};

		public static int GetPrimaryBiome(Player player) {
			int index = 0;
			byte weight = 0;
			for (int i = 0; i < vanillaPrimaryBiomes.Count; i++) {
				bool active = vanillaPrimaryBiomes[i].isActiveCondition(player);
				byte tst = vanillaPrimaryBiomes[i].weight;
				if (active && tst > weight) {
					index = i;
					weight = tst;
				}
			}

			for (int i = 0; i < biomes.Count; i++) {
				bool active = biomes[i].IsBiomeActive(player) && biomes[i].isPrimaryBiome;
				byte tst = biomes[i].GetBiomeStrength(player);
				if (active && tst > weight) {
					index = i + vanillaPrimaryBiomes.Count;
					weight = tst;
				}
			}

			return index;
		}
	}
}
