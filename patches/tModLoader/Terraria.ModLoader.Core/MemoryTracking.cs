using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Core
{
	internal class ModMemoryUsage
	{
		internal long managed;
		internal long sounds;
		internal long textures;
		internal long code;

		internal long total => managed + code + sounds + textures;
	}

	internal static class MemoryTracking
	{
		internal static Dictionary<string, ModMemoryUsage> modMemoryUsageEstimates = new Dictionary<string, ModMemoryUsage>();
		private static long previousMemory;

		internal static void Clear() {
			modMemoryUsageEstimates.Clear();
		}

		internal static ModMemoryUsage Update(string modName) {
			if (!modMemoryUsageEstimates.TryGetValue(modName, out var usage))
				modMemoryUsageEstimates[modName] = usage = new ModMemoryUsage();

			if (ModLoader.showMemoryEstimates) {
				var newMemory = GC.GetTotalMemory(true);
				usage.managed += Math.Max(0, newMemory - previousMemory);
				previousMemory = newMemory;
			}

			return usage;
		}

		internal static void Checkpoint() {
			if (ModLoader.showMemoryEstimates)
				previousMemory = GC.GetTotalMemory(true);
		}

		internal static void Finish() {
			foreach (var mod in ModLoader.Mods) {
				var usage = modMemoryUsageEstimates[mod.Name];
				usage.textures = mod.textures.Values.Sum(tex => tex.Width * tex.Height * 4);
				usage.sounds = mod.sounds.Values.Sum(sound => (long)sound.Duration.TotalSeconds * 44100 * 2 * 2);
			}
		}
	}
}
