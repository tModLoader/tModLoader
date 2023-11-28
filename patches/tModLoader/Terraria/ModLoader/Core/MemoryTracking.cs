using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Core;

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

	internal static void Clear()
	{
		modMemoryUsageEstimates.Clear();
	}

	internal static ModMemoryUsage Update(string modName)
	{
		if (!modMemoryUsageEstimates.TryGetValue(modName, out var usage))
			modMemoryUsageEstimates[modName] = usage = new ModMemoryUsage();

		if (ModLoader.showMemoryEstimates) {
			var newMemory = GC.GetTotalMemory(true);
			usage.managed += Math.Max(0, newMemory - previousMemory);
			previousMemory = newMemory;
		}

		return usage;
	}

	internal static void Checkpoint()
	{
		if (ModLoader.showMemoryEstimates)
			previousMemory = GC.GetTotalMemory(true);
	}

	internal static void Finish()
	{
		foreach (var mod in ModLoader.Mods) {
			var usage = modMemoryUsageEstimates[mod.Name];

			usage.textures = mod.Assets
				.GetLoadedAssets()
				.OfType<Asset<Texture2D>>()
				.Select(asset => asset.Value)
				.Where(val => val != null)
				.Sum(tex => tex.Width * tex.Height * 4);

			usage.sounds = mod.Assets
				.GetLoadedAssets()
				.OfType<Asset<SoundEffect>>()
				.Select(asset => asset.Value)
				.Where(val => val != null)
				.Sum(sound => (long)sound.Duration.TotalSeconds * 44100 * 2 * 2);
		}
		long totalRamUsage = -1;
		try {
			totalRamUsage = Process.GetProcesses().Sum(x => x.WorkingSet64); // Might throw UnauthorizedAccessException on locked down Linux systems. See https://github.com/tModLoader/tModLoader/issues/3689
		}
		catch {	}
		Logging.tML.Info($"RAM: tModLoader usage: {UIMemoryBar.SizeSuffix(Process.GetCurrentProcess().WorkingSet64)}, All processes usage: {(totalRamUsage == -1 ? "Unknown" : UIMemoryBar.SizeSuffix(totalRamUsage))}, Available: {UIMemoryBar.SizeSuffix(UIMemoryBar.GetTotalMemory() - totalRamUsage)}, Total Installed: {UIMemoryBar.SizeSuffix(UIMemoryBar.GetTotalMemory())}");
	}
}
