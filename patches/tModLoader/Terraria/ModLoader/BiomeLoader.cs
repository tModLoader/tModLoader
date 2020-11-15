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

		internal static void CopyBiomesTo(Player newPlayer) {
			newPlayer.modBiomes = new ModBiome[biomes.Count];
			biomes.CopyTo(newPlayer.modBiomes, 0);
		}

		public static void UpdateBiomes(Player player) {
			foreach (var biome in player.modBiomes) {
				biome.Update();
			}
		}

		public static bool CustomBiomesMatch(Player player, Player other) {
			for (int i = 0; i < player.modBiomes.Length; i++) {
				if (player.modBiomes[i].Active ^ other.modBiomes[i].Active)
					return false;
			}
			return true;
		}

		public static void CopyCustomBiomesTo(Player player, Player other) {
			for (int i = 0; i < player.modBiomes.Length; i++) {
				other.modBiomes[i].Active = player.modBiomes[i].Active;
			}
		}

		public static void SendCustomBiomes(Player player, BinaryWriter writer) {
			ushort count = 0;
			byte[] data;
			using (MemoryStream stream = new MemoryStream()) {
				using BinaryWriter customWriter = new BinaryWriter(stream);
				for (int i = 0; i < player.modBiomes.Length; i++) {
					if (SendCustomBiomes(player.modBiomes[i], customWriter)) {
						count++;
					}
				}
				customWriter.Flush();
				data = stream.ToArray();
			}
			writer.Write(count);
			writer.Write(data);
		}

		private static bool SendCustomBiomes(ModBiome modBiome, BinaryWriter writer) {
			byte[] data;
			using (MemoryStream stream = new MemoryStream()) {
				using BinaryWriter customWriter = new BinaryWriter(stream);
				//modPlayer.SendCustomBiomes(writer);
				customWriter.Write(modBiome.Active);
				customWriter.Flush();
				data = stream.ToArray();
			}
			if (data.Length > 0) {
				writer.Write(modBiome.Mod.Name);
				writer.Write(modBiome.Name);
				writer.Write((byte)data.Length);
				writer.Write(data);
				return true;
			}
			return false;
		}

		public static void ReceiveCustomBiomes(Player player, BinaryReader reader) {
			int count = reader.ReadUInt16();

			for (int k = 0; k < count; k++) {
				string modName = reader.ReadString();
				string name = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadByte());

				if (ModContent.TryFind<ModBiome>(modName, name, out var modBiomeBase)) {
					var modBiome = player.GetModBiome(modBiomeBase);

					using MemoryStream stream = new MemoryStream(data);
					using BinaryReader customReader = new BinaryReader(stream);

					modBiome.Active = customReader.ReadBoolean();
					//try { modPlayer.ReceiveCustomBiomes(reader) }
				}
			}
		}

		private static HookList HookUpdateBiomeVisuals = AddHook<Action>(b => b.UpdateBiomeVisuals);

		public static void UpdateBiomeVisuals(Player player) {
			foreach (int index in HookUpdateBiomeVisuals.arr) {
				player.modBiomes[index].UpdateBiomeVisuals();
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
