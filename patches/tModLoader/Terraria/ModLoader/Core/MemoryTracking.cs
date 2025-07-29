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
	private static long previousMemory; // Running total managed memory usage
	internal static bool accurate = false; // use -accuratememorytracking command line argument to set.
	internal static Stopwatch CheckRAMUsageTimer = new Stopwatch();
	private static long previousTotalMemory;

	internal static void InGameUpdate()
	{
		// Every 60 seconds, check if reach new GB RAM milestone
		if(CheckRAMUsageTimer.Elapsed.TotalSeconds > 60) {
			CheckRAMUsageTimer.Restart();

			Process process = Process.GetCurrentProcess();
			if (previousTotalMemory < process.PrivateMemorySize64 && previousTotalMemory >> 30 != process.PrivateMemorySize64 >> 30) {
				Logging.tML.Info($"tModLoader RAM usage has increased: {UIMemoryBar.SizeSuffix(process.PrivateMemorySize64)}");
			}
			previousTotalMemory = process.PrivateMemorySize64;
		}
	}

	internal static void Clear()
	{
		modMemoryUsageEstimates.Clear();
	}

	internal static ModMemoryUsage Update(string modName)
	{
		if (!modMemoryUsageEstimates.TryGetValue(modName, out var usage))
			modMemoryUsageEstimates[modName] = usage = new ModMemoryUsage();

		var newMemory = GC.GetTotalMemory(accurate);
		usage.managed += Math.Max(0, newMemory - previousMemory);
		previousMemory = newMemory;

		return usage;
	}

	internal static void Checkpoint()
	{
		// Sets new baseline prior to mod-specific loading
		previousMemory = GC.GetTotalMemory(accurate);
	}

	internal static void Finish()
	{
		CheckRAMUsageTimer.Restart();

		foreach (var mod in ModLoader.Mods) {
			var usage = modMemoryUsageEstimates[mod.Name];

			usage.textures = mod.Assets
				.GetLoadedAssets()
				.OfType<Asset<Texture2D>>()
				.Select(asset => asset.Value)
				.Where(val => val != null)
				.Sum(tex => (long)(tex.Width * tex.Height * 4));

			// Most mods don't load any sounds during mod loading, so this is usually 0.
			usage.sounds = mod.Assets
				.GetLoadedAssets()
				.OfType<Asset<SoundEffect>>()
				.Select(asset => asset.Value)
				.Where(val => val != null)
				.Sum(sound => (long)(sound.Duration.TotalSeconds * 44100 * 2 * 2));
		}
		if(ModLoader.Mods.Length > 1) {
			Logging.tML.Info("Mods using the most RAM: " + string.Join(", ", modMemoryUsageEstimates.OrderByDescending(x => x.Value.total).Where(x => x.Key != "ModLoader").Take(3).Select(x => $"{x.Key} {UIMemoryBar.SizeSuffix(x.Value.total)}")));
		}

		long totalRamUsage = -1;
		long totalCommit = -1;
		try {
			totalRamUsage = Process.GetProcesses().Sum(x => x.WorkingSet64); // Might throw UnauthorizedAccessException on locked down Linux systems. See https://github.com/tModLoader/tModLoader/issues/3689
			totalCommit = Process.GetProcesses().Sum(x => x.PrivateMemorySize64); // does this not account for shared?
		}
		catch {	}

		Process process = Process.GetCurrentProcess();
		process.Refresh();
		Logging.tML.Info($"RAM physical: tModLoader usage: {UIMemoryBar.SizeSuffix(process.WorkingSet64)}, All processes usage: {(totalRamUsage == -1 ? "Unknown" : UIMemoryBar.SizeSuffix(totalRamUsage))}, Available: {UIMemoryBar.SizeSuffix(UIMemoryBar.GetTotalMemory() - totalRamUsage)}, Total Installed: {UIMemoryBar.SizeSuffix(UIMemoryBar.GetTotalMemory())}");
		Logging.tML.Info($"RAM virtual: tModLoader usage: {UIMemoryBar.SizeSuffix(process.PrivateMemorySize64)}, All processes usage: {(totalCommit == -1 ? "Unknown" : UIMemoryBar.SizeSuffix(totalCommit))}");
		previousTotalMemory = process.PrivateMemorySize64;

		if (totalCommit > UIMemoryBar.GetTotalMemory()) {
			// No way to query page file size, but this warning should help identify if that is a potential issue.
			Logging.tML.Warn("Total system memory usage exceeds installed physical memory, tModLoader will likely experience performance issues due to frequent page file access.");
		}
	}
}
