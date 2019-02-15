using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	internal class ModMemory
	{
		internal long autoload;
		internal long setupContent;
		internal long sounds;
		internal long textures;

		internal long total => autoload + setupContent + sounds + textures;
	}

	internal static class MemoryTracking
	{
		internal static Dictionary<string, ModMemory> modMemoryUsageEstimate = new Dictionary<string, ModMemory>();
		private static long previousMemory;
		private static long newMemory;

		internal static void Start() {
			if (!ModLoader.showMemoryEstimates) return;
			modMemoryUsageEstimate.Clear();
			previousMemory = GC.GetTotalMemory(false);
		}

		internal static void Load(Mod mod) {
			if (!ModLoader.showMemoryEstimates) return;
			modMemoryUsageEstimate.Add(mod.DisplayName, new ModMemory());
			newMemory = GC.GetTotalMemory(false);
			modMemoryUsageEstimate[mod.DisplayName].autoload = Math.Max(0, newMemory - previousMemory);
			previousMemory = newMemory;
			if (!Main.dedServ) {
				long textureMemory = 0;
				foreach (var item in mod.textures) {
					textureMemory += item.Value.Width * item.Value.Height * 4;
				}
				modMemoryUsageEstimate[mod.DisplayName].textures = textureMemory;
				long audioMemory = 0;
				foreach (var item in mod.sounds) {
					audioMemory += (long)(item.Value.Duration.TotalSeconds * 44100 * 2 * 2);
				}
				modMemoryUsageEstimate[mod.DisplayName].sounds = audioMemory;
			}
		}

		internal static void MidReset() {
			if (!ModLoader.showMemoryEstimates) return;
			previousMemory = newMemory = GC.GetTotalMemory(false);
		}

		internal static void Finish(Mod mod) {
			if (!ModLoader.showMemoryEstimates) return;
			newMemory = GC.GetTotalMemory(false);
			modMemoryUsageEstimate[mod.DisplayName].setupContent = newMemory - previousMemory;
			previousMemory = newMemory;
		}
	}
}
