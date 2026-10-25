using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using static Terraria.WorldGen;

namespace Terraria.GameContent;

public static partial class SecretSeedsTracker
{
	public static bool ProcessedConfig => _processedConfig;

	internal static List<string> ModSeedsForConfig = new List<string>();
	internal static List<string> UnloadedModSeeds = new List<string>();

	private static string ModSeedToString(ModSecretSeed seed)
	{
		return $"{seed.FullName}::{seed.SecretSeed.TextThatWasUsedToUnlock}";
	}

	public static void ResetConfig()
	{
		_processedConfig = false;
		SeedsForInterface.Clear();
		UnloadedModSeeds.Clear();
	}

	internal static void RefreshModConfigSeeds()
	{
		ModSeedsForConfig.Clear();
		ModSeedsForConfig.AddRange(_seedsForInterface.Distinct().Where(s => s.ModSecretSeed != null).Select(x => ModSeedToString(x.ModSecretSeed)));
		ModSeedsForConfig.AddRange(UnloadedModSeeds);
		ModSeedsForConfig.Sort();
	}

	internal static void ParseModdedSeeds(JArray jsonList)
	{
		try {
			if (jsonList != null && jsonList.Count != 0)
				ModSeedsForConfig.AddRange(jsonList.ToObject<List<string>>());
		}
		catch (JsonReaderException arg) {
			Console.WriteLine("Failed to parse configuration entry for modded secret seed list. {0}", arg);
		}
	}
}