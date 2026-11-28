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
	internal static List<string> UnloadedModSeeds = new List<string>();
	public static bool ProcessedConfig => _processedConfig;

	private static string ModSeedToString(ModSecretSeed seed)
	{
		return $"{seed.FullName}::{seed.SecretSeed.TextThatWasUsedToUnlock}";
	}

	internal static void AddSeedToTrackDirect(SecretSeed seed)
	{
		if (_seedsForInterface.Contains(seed))
			return;

		if (seed.ModSecretSeed == null)
			_seedsForConfig.Add(seed.TextThatWasUsedToUnlock);
		else
			_seedsForConfig.Add(ModSeedToString(seed.ModSecretSeed));

		_seedsForInterface.Add(seed);
		Main.SaveSettings();
	}

	public static void ResetConfig()
	{
		_processedConfig = false;
		SeedsForInterface.Clear();
		UnloadedModSeeds.Clear();
	}
}
