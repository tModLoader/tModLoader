using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public static class ExtraJumpLoader
{
	public static int ExtraJumpCount => ExtraJumps.Count;

	// Order is the vanilla priority when consuming the extra jumps
	internal static readonly List<ModExtraJump> ExtraJumps = new List<ModExtraJump>()
	{
		ModExtraJump.GoatMount,
		ModExtraJump.SantankMount,
		ModExtraJump.UnicornMount,
		ModExtraJump.SandstormInABottle,
		ModExtraJump.BlizzardInABottle,
		ModExtraJump.FartInAJar,
		ModExtraJump.TsunamiInABottle,
		ModExtraJump.BasiliskMount,
		ModExtraJump.CloudInABottle
	};

	internal static readonly int DefaultExtraJumpCount = ExtraJumps.Count;

	public static readonly ModExtraJump FirstVanillaExtraJump = ExtraJumps[0];

	public static readonly ModExtraJump LastVanillaExtraJump = ExtraJumps[^1];

	internal static IEnumerable<ModExtraJump> ModdedExtraJumps => ExtraJumps.Skip(DefaultExtraJumpCount);

	static ExtraJumpLoader()
	{
		RegisterDefaultJumps();
	}

	internal static int Add(ModExtraJump jump)
	{
		ExtraJumps.Add(jump);
		return ExtraJumps.Count - 1;
	}

	internal static void Unload()
	{
		ExtraJumps.RemoveRange(DefaultExtraJumpCount, ExtraJumpCount - DefaultExtraJumpCount);
	}

	internal static void RegisterDefaultJumps()
	{
		int i = 0;
		foreach (var jump in ExtraJumps) {
			jump.Type = i++;
			ContentInstance.Register(jump);
			ModTypeLookup<ModExtraJump>.Register(jump);
		}
	}

	public static void ModifyPlayerHorizontalSpeeds(Player player)
	{
		foreach (ModExtraJump moddedExtraJump in ExtraJumps) {
			// Special case: Sandstorm in a Bottle uses a separate flag
			ref ExtraJumpData extraJump = ref player.GetExtraJump(moddedExtraJump);
			if ((object.ReferenceEquals(moddedExtraJump, ModExtraJump.SandstormInABottle) && player.sandStorm) || (extraJump.PerformingJump && extraJump.Active))
				moddedExtraJump.ModifyHorizontalSpeeds(player);
		}
	}

	public static void HandleJumpVisuals(Player player)
	{
		foreach (ModExtraJump jump in ExtraJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (data.PerformingJump && data.Active && !data.JumpAvailable)
				jump.JumpVisuals(player);
		}
	}

	public static void ProcessJumps(Player player, bool flipperOrSlimeMountSwimming)
	{
		foreach (ModExtraJump jump in GetOrderedJumps(player)) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);

			// The Cloud in a Bottle's extra jump ignores the "flipper swimming" check
			// "IgnoresSwimmingChecks" allows modders to mimic this behavior
			if ((jump.IgnoresSwimmingChecks || !flipperOrSlimeMountSwimming) && data.JumpAvailable) {
				data.JumpAvailable = false;
				data.PerformingJump = true;
				jump.PerformJump(player);
				break;
			}
		}

		if (!flipperOrSlimeMountSwimming) {
			// The Basilisk mount's extra jump is always "consumed", even if a higher-priority jump was performed
			player.GetExtraJump(ModExtraJump.BasiliskMount).JumpAvailable = false;
		}
	}

	public static void OnJumpEnded(Player player)
	{
		foreach (ModExtraJump jump in ExtraJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (data.PerformingJump) {
				jump.OnJumpEnded(player);
				data.PerformingJump = false;
			}
		}
	}

	private static IEnumerable<ModExtraJump> GetOrderedJumps(Player player)
	{
		var positions = ExtraJumps.ToDictionary(static j => j, j => j.GetOrder(player));

		var sort = new TopoSort<ModExtraJump>(positions.Keys,
			j => new[] { positions[j].Before }.Where(static j => j != null),
			j => new[] { positions[j].After }.Where(static j => j != null));

		return sort.Sort();
	}
}
