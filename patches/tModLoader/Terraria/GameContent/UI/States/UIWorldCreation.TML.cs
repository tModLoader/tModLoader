using System.Linq;
using System.Text.RegularExpressions;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent.UI.States;

public partial class UIWorldCreation : UIState, IHaveBackButtonCommand
{
	private void EnableAllHomonymSeeds(WorldGen.SecretSeed baseSeed)
	{
		string code = baseSeed.Code;
		var matchedSeeds = SeedLoader.SecretSeedsPlusModded().Where((WorldGen.SecretSeed s) => s.Code == code).ToList();
		if (matchedSeeds.Count == 1) {
			WorldGen.SecretSeed.Enable(baseSeed);
			return;
		}
		bool playSound = true;
		foreach (WorldGen.SecretSeed secretSeed in matchedSeeds) {
			WorldGen.SecretSeed.Enable(secretSeed, playSound);
			SecretSeedsTracker.AddSeedToTrackDirect(secretSeed);
			secretSeed.TextThatWasUsedToUnlock = baseSeed.TextThatWasUsedToUnlock;
			playSound = false;
		}
	}
}
