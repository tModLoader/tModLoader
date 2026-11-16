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
}