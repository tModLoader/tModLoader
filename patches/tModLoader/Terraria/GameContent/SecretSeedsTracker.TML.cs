using System.Collections.Generic;
using Terraria.ModLoader;
using static Terraria.WorldGen;

namespace Terraria.GameContent;

public static partial class SecretSeedsTracker
{
	public static bool ProcessedConfig => _processedConfig;

	internal static List<string> ModSeedsForConfig = new List<string>();

	private static string ModSeedToString(ModSecretSeed seed)
	{
		return $"{seed.FullName}::{seed.SecretSeed.TextThatWasUsedToUnlock}";
	}

	public static void ResetConfig()
	{
		_processedConfig = false;
		SeedsForInterface.Clear();
	}
}