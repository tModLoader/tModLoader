using static Terraria.WorldGen;

namespace Terraria.GameContent;

public static partial class SecretSeedsTracker
{
	public static bool ProcessedConfig => _processedConfig;

	public static void ResetConfig()
	{
		_processedConfig = false;
		SeedsForInterface.Clear();
	}
}