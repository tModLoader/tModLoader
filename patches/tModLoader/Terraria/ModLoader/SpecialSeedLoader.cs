using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

public static class SpecialSeedLoader
{
	public static bool ShouldSeedMenuScroll => false;

	internal static readonly IList<ModSpecialSeed> specialSeeds = new List<ModSpecialSeed>();

	internal static void Add(ModSpecialSeed modSpecialSeed)
	{
		specialSeeds.Add(modSpecialSeed);
	}

	internal static void Unload()
	{
		specialSeeds.Clear();
	}

	public static void SetEnabledFromUI()
	{
		foreach (ModSpecialSeed seed in specialSeeds) {
			seed.Enabled = seed.WorldGenerationOption.Enabled;
		}
	}

	public static void DisableAll()
	{
		foreach (ModSpecialSeed seed in specialSeeds) {
			seed.Enabled = false;
		}
	}

	/// <summary>
	/// Allows changing if a certain special seed is enabled or not for the loaded world
	/// </summary>
	public static void ChangeSpecialSeedFlag<T>(bool enable = true) where T : ModSpecialSeed
	{
		ModContent.GetInstance<T>().Enabled = enable;
	}

	public static void ModifyWorldGenTasks(List<GenPass> passes)
	{
		foreach (ModSpecialSeed seed in specialSeeds) {
			try {
				seed.ModifyWorldGenTasks(passes);
			}
			catch (Exception e) {
				string message = string.Join(
					"\n",
					seed.FullName + " : " + Language.GetTextValue("tModLoader.WorldGenError"),
					e
				);
				Utils.ShowFancyErrorMessage(message, 0);

				throw;
			}
		}
	}

	internal static void AddModdedSeedIcons(WorldFileData data, List<Asset<Texture2D>> list)
	{
		foreach (string seedName in data.ModSeeds) {
			if (ModContent.TryFind(seedName, out ModSpecialSeed seed)) {
				list.Add(seed.GetSeedIcon(data));
			}
		}
	}

	internal static bool CanEnableSeedsFromText(string seed)
	{
		int seedNum = WorldFileData.TranslateSeed(seed);
		string seedText = Regex.Replace(seed.ToLower(), "[^a-z0-9]+", "");
		bool enabledASeed = false;
		foreach (ModSpecialSeed seedCandidate in specialSeeds) {
			int[] candidateSeedValues = seedCandidate.WorldGenerationOption.SpecialSeedValues;
			foreach (int value in candidateSeedValues) {
				if (value != seedNum) {
					continue;
				}
				if (!enabledASeed) {
					WorldGenerationOptions.Reset();
				}
				seedCandidate.WorldGenerationOption.Enabled = true;
				enabledASeed = true;
			}

			string[] candidateSeedNames = seedCandidate.WorldGenerationOption.SpecialSeedNames;
			foreach (string seedName in candidateSeedNames) {
				if (seedName != seedText) {
					continue;
				}
				if (!enabledASeed) {
					WorldGenerationOptions.Reset();
				}
				seedCandidate.WorldGenerationOption.Enabled = true;
				enabledASeed = true;
			}
		}

		if (enabledASeed) {
			SoundEngine.PlaySound(24);
		}

		return enabledASeed;
	}
}