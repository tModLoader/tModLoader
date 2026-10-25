using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
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

	internal static AWorldGenerationOption FirstVanillaSeed = WorldGenerationOptions.Options.First(opt => opt != WorldGenerationOptions.Get<WorldSeedOption_Normal>());
	internal static AWorldGenerationOption LastVanillaSeed => WorldGenerationOptions.Options.Last();

	private static readonly List<AWorldGenerationOption> VanillaOptions = WorldGenerationOptions.Options.ToList();

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
			float menuWeight = seed.GetMenuWeight();
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

	internal static List<AWorldGenerationOption> SpecialSeedOptions = [];

	internal static void FinishSetup()
	{
		var sortingSlots = new List<AWorldGenerationOption>[VanillaOptions.Count + 1];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		foreach (ModSpecialSeed specialSeed in specialSeeds) {
			var position = specialSeed.GetDefaultPosition();

			switch (position) {
				case ModSpecialSeed.After after: {
					int afterIndex = VanillaOptions.IndexOf(after.Option);
					if (afterIndex >= VanillaOptions.Count || afterIndex == -1)
						throw BlameSpecialSeedException(new ArgumentException(
							$"ModSpecialSeed {specialSeed.FullName} did not refer to a vanilla achievement in GetDefaultPosition()"));

					sortingSlots[afterIndex + 1].Add(specialSeed.UIOption);
					break;
				}
				case ModSpecialSeed.Before before: {
					int beforeIndex = VanillaOptions.IndexOf(before.Option);
					if (beforeIndex >= VanillaOptions.Count || beforeIndex == -1)
						throw BlameSpecialSeedException(new ArgumentException(
							$"ModSpecialSeed {specialSeed.FullName} did not refer to a vanilla achievement in GetDefaultPosition()"));

					sortingSlots[beforeIndex].Add(specialSeed.UIOption);
					break;
				}
				default: {
					var ex = new ArgumentException($"ModSpecialSeed {specialSeed} has unknown Position {position}");
					throw BlameSpecialSeedException(ex);
				}
			}

			Exception BlameSpecialSeedException(Exception ex)
			{
				if (specialSeed is ModSpecialSeed moddedSeed)
					ex.Data["mod"] = moddedSeed.Mod.Name;
				return ex;
			}
		}

		SpecialSeedOptions.Clear();

		for (int i = 0; i < VanillaOptions.Count + 1; i++) {
			var elements = sortingSlots[i];
			var sort = new TopoSort<AWorldGenerationOption>(elements,
				l => (l as ModSpecialSeedUIOption)?.Parent.GetModdedConstraints()?.OfType<ModSpecialSeed.After>().Select(a => a.Option).Where(elements.Contains) ?? [],
				l => (l as ModSpecialSeedUIOption)?.Parent.GetModdedConstraints()?.OfType<ModSpecialSeed.Before>().Select(b => b.Option).Where(elements.Contains) ?? []);

			foreach (AWorldGenerationOption option in sort.Sort()) {
				SpecialSeedOptions.Add(option);
			}

			if(i < VanillaOptions.Count)
				SpecialSeedOptions.Add(VanillaOptions[i]);
		}


	}



	public static AWorldGenerationOption GetOptionFromSeedText(string seed)
	{
		int seedNum = WorldFileData.TranslateSeed(seed);
		string seedText = Regex.Replace(seed.ToLower(), "[^a-z0-9]+", "");
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
				if (string.IsNullOrEmpty(seedName)) {
					continue;
				}
				if (seedName != seedText) {
					continue;
				}
				return seedCandidate.UIOption;
			}
		}

		return null;
	}

	internal static void AddModdedSecretSeeds(List<SecretSeed> list)
	{
		foreach (string seedCode in SecretSeedsTracker.ModSeedsForConfig) {
			string[] components = seedCode.Split("::");
			string name = components[0];
			string unlockText = components[1];
			if (ModContent.TryFind(name, out ModSecretSeed seed)) {
				if (seed.AutoUnlock)
					continue; // Filter out auto-unlocked seeds from the saved seeds list.
				seed.SecretSeed.TextThatWasUsedToUnlock = unlockText;
				list.Add(seed.SecretSeed);
				continue;
			}
			string mod = name.Split('/')[0];
			if (ModLoader.TryGetMod(mod, out Mod loadedMod)) {
				continue; // If the mod is loaded and the seed isn't present, then there's no reason to track the seed.
			}
			SecretSeedsTracker.UnloadedModSeeds.Add(seedCode);
		}
		SecretSeedsTracker.RefreshModConfigSeeds(); // Do this before adding auto-unlocked seeds since they don't need to be saved.
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