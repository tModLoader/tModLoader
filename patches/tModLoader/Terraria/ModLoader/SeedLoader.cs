using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.WorldBuilding;
using static Terraria.WorldGen;

namespace Terraria.ModLoader;

public static class SeedLoader
{
	internal static readonly IList<ModSpecialSeed> specialSeeds = new List<ModSpecialSeed>();

	internal static readonly IList<ModSecretSeed> secretSeeds = new List<ModSecretSeed>();

	internal static readonly IList<ModSeedType> allSeeds = new List<ModSeedType>();

	internal static void Add(ModSpecialSeed modSpecialSeed)
	{
		specialSeeds.Add(modSpecialSeed);
		allSeeds.Add(modSpecialSeed);
	}

	internal static void Add(ModSecretSeed modSecretSeed)
	{
		secretSeeds.Add(modSecretSeed);
		allSeeds.Add(modSecretSeed);
	}

	internal static void Unload()
	{
		foreach (ModSpecialSeed seed in specialSeeds) {
			seed.Unsubscribe();
		}
		specialSeeds.Clear();
		secretSeeds.Clear();
		allSeeds.Clear();
	}

	internal static void PostSetupContent()
	{
		if (allSeeds is List<ModSeedType> seedList) {
			seedList.Sort((left, right) => (right is ModSpecialSeed).CompareTo(left is ModSpecialSeed));
		}
		foreach (ModSpecialSeed seed in specialSeeds) {
			seed.FinalizeContent();
		}
	}

	/// <summary>
	/// Gets whether the specified special seed is enabled on this world.
	/// </summary>
	public static ref bool SeedEnabled<T>() where T : ModSeedType => ref ModContent.GetInstance<T>().Enabled;

	public static void SetEnabledFromUI()
	{
		foreach (ModSpecialSeed seed in specialSeeds) {
			seed.Enabled = seed.UIOption.Enabled;
		}
		foreach (ModSecretSeed seed in secretSeeds) {
			seed.Enabled = seed.SecretSeed.Enabled;
		}
	}

	public static ModMenu CurrentWorldGenMenu { get; private set; }

	public static void SetWorldGenMenu()
	{
		CurrentWorldGenMenu = null;
		float highestWeight = float.NegativeInfinity;
		foreach (ModSpecialSeed seed in specialSeeds) {
			if (!seed.UIOption.Enabled || seed.WorldGenMenu == null)
				continue;
			float menuWeight = Math.Clamp(seed.GetMenuWeight(),0f,1f);
			if (menuWeight < highestWeight) {
				continue;
			}
			highestWeight = menuWeight;
			CurrentWorldGenMenu = seed.WorldGenMenu;
		}
	}

	public static void DisableAll()
	{
		foreach (ModSeedType seed in allSeeds) {
			seed.Enabled = false;
		}
	}

	public static void DisableSecretSeedToggles()
	{
		foreach (ModSecretSeed secretSeed in secretSeeds) {
			SecretSeed.Disable(secretSeed.SecretSeed);
		}
	}

	public static void ModifyWorldGenTasks(List<GenPass> passes)
	{
		foreach (ModSeedType seed in allSeeds) {
			if (!seed.Enabled) {
				continue;
			}
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

	public static void PostWorldGen()
	{
		foreach (ModSeedType seed in allSeeds) {
			if (!seed.Enabled) {
				continue;
			}
			seed.PostWorldGen();
		}
	}

	public static void ModifyLoadingTips(ref string text, ref Color drawColor)
	{
		if (!WorldGen.generatingWorld) {
			return;
		}
		foreach (ModSeedType seed in allSeeds) {
			if (!seed.Enabled) {
				continue;
			}
			seed.ModifyLoadingTips(ref text, ref drawColor);
		}
	}

	public static void ModifyWorldProgressText(ref string text, ref Color drawColor)
	{
		foreach (ModSeedType seed in allSeeds) {
			if (!seed.Enabled) {
				continue;
			}
			seed.ModifyWorldProgressText(ref text, ref drawColor);
		}
	}

	internal static void AddModSeedIcons(WorldFileData data, List<Asset<Texture2D>> icons, List<ModSpecialSeed> includedSeeds)
	{
		includedSeeds.Clear();
		foreach (string seedName in data.ModSeeds) {
			if (ModContent.TryFind(seedName, out ModSpecialSeed seed)) {
				includedSeeds.Add(seed);
			}
		}
		includedSeeds.RemoveAll(includedSeed => {
			return includedSeeds.Exists(otherIncludedSeed => otherIncludedSeed.Dependencies.Contains(includedSeed.UIOption));
		});
		foreach (ModSpecialSeed includedSeed in includedSeeds) {
			icons.Add(includedSeed.GetWorldIconTexture());
		}
	}

	internal static bool IsVanillaSeedDependency(string seedCode, WorldFileData data)
	{
		List<ModSpecialSeed> includedSeeds = new();
		foreach (string seedName in data.ModSeeds) {
			if (ModContent.TryFind(seedName, out ModSpecialSeed seed)) {
				includedSeeds.Add(seed);
			}
		}
		switch (seedCode) {
			case "CorruptionCrimson":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_Drunk>(includedSeeds);
			case "FTW":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_ForTheWorthy>(includedSeeds);
			case "NotTheBees":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_NotTheBees>(includedSeeds);
			case "Anniversary":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_Anniversary>(includedSeeds);
			case "DontStarve":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_DontStarve>(includedSeeds);
			case "Remix":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_Remix>(includedSeeds);
			case "Traps":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_NoTraps>(includedSeeds);
			case "Skyblock":
				return IsVanillaSeedDependency_Inner<WorldSeedOption_Skyblock>(includedSeeds);
		}

		return false;
	}

	private static bool IsVanillaSeedDependency_Inner<T>(List<ModSpecialSeed> modSeeds) where T : AWorldGenerationOption
	{
		return modSeeds.Exists(modSeed => modSeed.Dependencies.Contains(WorldGenerationOptions.Get<T>()));
	}

	internal static bool CanEnableSeedsFromText(string seed)
	{
		AWorldGenerationOption option = GetOptionFromSeedText(seed);

		if (option != null) {
			WorldGenerationOptions.Reset();
			option.Enabled = true;
			SoundEngine.PlaySound(24);
		}

		return option != null;
	}

	internal static List<AWorldGenerationOption> AddModSeedOptions(IEnumerable<AWorldGenerationOption> genOptions)
	{
		List<ModSpecialSeed> toBeAdded = new List<ModSpecialSeed>(specialSeeds);
		List<AWorldGenerationOption> genOptionsPlusModded = new List<AWorldGenerationOption>(genOptions);
		//Add seeds that have no sorting logic/invalid sorting logic first so that the sorter doesn't get stuck.
		for (int i = 0; i < toBeAdded.Count; i++) {
			if (toBeAdded[i].Ordering.target != null) {
				continue;
			}
			if (genOptionsPlusModded.Contains(toBeAdded[i].Ordering.target)) {
				continue;
			}
			if (specialSeeds.Any(seed => seed.UIOption == toBeAdded[i].Ordering.target)) {
				continue;
			}
			genOptionsPlusModded.Add(toBeAdded[i].UIOption);
			toBeAdded.RemoveAt(i);
			i -= 1;
		}
		while (toBeAdded.Count > 0) {
			for (int i = 0; i < toBeAdded.Count; i++) {
				int target = genOptionsPlusModded.FindIndex(sortTarget => sortTarget == toBeAdded[i].Ordering.target);
				if (target == -1) {
					continue;
				}
				genOptionsPlusModded.Insert(target + (toBeAdded[i].Ordering.after ? 1 : 0), toBeAdded[i].UIOption);
				toBeAdded.RemoveAt(i);
				i -= 1;
			}
		}

		return genOptionsPlusModded;
	}

	public static AWorldGenerationOption GetOptionFromSeedText(string seed)
	{
		int seedNum = WorldFileData.TranslateSeed(seed);
		string seedText = Regex.Replace(seed.ToLower(), "[^a-z0-9]+", "");
		bool enabledASeed = false;
		foreach (ModSpecialSeed seedCandidate in specialSeeds) {
			//We reuse the instance created for WorldGenerationOption instead of making a new one
			int[] candidateSeedValues = seedCandidate.UIOption.SpecialSeedValues;
			foreach (int value in candidateSeedValues) {
				if (value != seedNum) {
					continue;
				}
				return seedCandidate.UIOption;
			}

			string[] candidateSeedNames = seedCandidate.UIOption.SpecialSeedNames;
			foreach (string seedName in candidateSeedNames) {
				string formattedSeedName = Regex.Replace(seedName.ToLower(), "[^a-z0-9]+", "");
				if (string.IsNullOrEmpty(formattedSeedName)) {
					continue;
				}
				if (formattedSeedName != seedText) {
					continue;
				}
				return seedCandidate.UIOption;
			}
		}

		return null;
	}

	internal static void AddModdedSecretSeeds(List<SecretSeed> list)
	{
		foreach (ModSecretSeed secretSeed in secretSeeds) {
			if (!secretSeed.AutoUnlock)
				continue;
			list.Add(secretSeed.SecretSeed);
		}
	}

	internal static bool AnyAutoUnlock() => secretSeeds.Any(seed => seed.AutoUnlock);

	public static List<SecretSeed> SecretSeedsPlusModded()
	{
		List<SecretSeed> combinedList = new List<SecretSeed>(SecretSeed.AllSecretSeeds);
		combinedList.AddRange(secretSeeds.Select(secretSeed => secretSeed.SecretSeed));
		return combinedList;
	}
}