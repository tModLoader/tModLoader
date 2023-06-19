using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public static class ExtraJumpLoader
{
	public static int ExtraJumpCount => ExtraJumps.Count;

	public static int GlobalExtraJumpCount => GlobalExtraJumps.Count;

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

	private static ModExtraJump[] orderedJumps;

	public static IReadOnlyList<ModExtraJump> OrderedExtraJumps => orderedJumps;

	internal static readonly List<GlobalExtraJump> GlobalExtraJumps = new List<GlobalExtraJump>();

	static ExtraJumpLoader()
	{
		RegisterDefaultJumps();
	}

	internal static int Add(ModExtraJump jump)
	{
		ExtraJumps.Add(jump);
		return ExtraJumps.Count - 1;
	}

	public static ModExtraJump Get(int index) => index < 0 || index >= ExtraJumpCount ? null : ExtraJumps[index];

	internal static int Add(GlobalExtraJump jump) {
		GlobalExtraJumps.Add(jump);
		return GlobalExtraJumps.Count - 1;
	}

	public static GlobalExtraJump GetGlobal(int index) => index < 0 || index >= GlobalExtraJumpCount ? null : GlobalExtraJumps[index];

	internal static void Unload()
	{
		ExtraJumps.RemoveRange(DefaultExtraJumpCount, ExtraJumpCount - DefaultExtraJumpCount);
		GlobalExtraJumps.Clear();
	}

	internal static void ResizeArrays()
	{
		if (!ModdedExtraJumps.Any()) {
			// Vanilla extra jumps are already sorted in the collection; any additional work would be a moot point
			orderedJumps = ExtraJumps.ToArray();
			return;
		}

		// Between each vanilla extra jump, before the first jump and after the last jump exists a "slot"
		// Modded jumps are added to a "slot", and then the slots are filled in load order by default
		// Modders can use "ModExtraJump::GetModdedConstraints()" to facilitate sorting within a slot
		List<int>[] sortingSlots = new List<int>[DefaultExtraJumpCount + 1];
		for (int i = 0; i < sortingSlots.Length; i++)
			sortingSlots[i] = new();

		// Initially put the modded extra jumps in load order
		foreach (ModExtraJump jump in ModdedExtraJumps) {
			var position = jump.GetDefaultPosition();

			switch (position) {
				case ModExtraJump.After after:
					int afterParent = after.Parent?.Type is { } afterType ? afterType + 1 : 0;

					sortingSlots[afterParent].Add(jump.Type);
					break;
				case ModExtraJump.Before before:
					int beforeParent = before.Parent?.Type is { } beforeType ? beforeType : sortingSlots.Length - 1;

					sortingSlots[beforeParent].Add(jump.Type);
					break;
				default:
					throw new ArgumentException($"ModExtraJump {jump} has unknown Position {position}");
			}
		}

		// Cache the information for which additional constraints each modded extra jump has
		var positions = ModdedExtraJumps.ToDictionary(j => j.Type, j => j.GetModdedConstraints()?.Select(p => VerifyPositionType(j, p)).ToList());

		// Sort the modded jumps per slot
		List<ModExtraJump> sorted = new();

		for (int i = 0; i < DefaultExtraJumpCount + 2; i++) {
			var sort = new TopoSort<ModExtraJump>(sortingSlots[i].Select(static t => ExtraJumps[t]),
				j => positions[j.Type].OfType<ModExtraJump.After>().Select(static a => a.Parent).OfType<ModExtraJump>(),
				j => positions[j.Type].OfType<ModExtraJump.Before>().Select(static b => b.Parent).OfType<ModExtraJump>());

			foreach (ModExtraJump jump in sort.Sort()) {
				sorted.Add(jump);
			}

			if (i < DefaultExtraJumpCount)
				sorted.Add(ExtraJumps[i]);
		}

		orderedJumps = sorted.ToArray();
	}

	private static ModExtraJump.Position VerifyPositionType(ModExtraJump jump, ModExtraJump.Position position) {
		if (position is not ModExtraJump.After and not ModExtraJump.Before)
			throw new ArgumentException($"ModExtraJump {jump} has unknown Position {position}");

		return position;
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
		foreach (ModExtraJump moddedExtraJump in orderedJumps) {
			// Special case: Sandstorm in a Bottle uses a separate flag
			ref ExtraJumpData extraJump = ref player.GetExtraJump(moddedExtraJump);
			if ((object.ReferenceEquals(moddedExtraJump, ModExtraJump.SandstormInABottle) && player.sandStorm) || (extraJump.PerformingJump && extraJump.Active))
				moddedExtraJump.ModifyHorizontalSpeeds(player);
		}
	}

	public static void JumpVisuals(Player player)
	{
		foreach (ModExtraJump jump in orderedJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (data.PerformingJump && data.Active && !data.JumpAvailable && jump.PreJumpVisuals(player))
				jump.JumpVisuals(player);
		}
	}

	public static void ProcessJumps(Player player, bool flipperOrSlimeMountSwimming)
	{
		foreach (ModExtraJump jump in orderedJumps) {
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
		foreach (ModExtraJump jump in orderedJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (data.PerformingJump) {
				jump.OnJumpEnded(player);

				foreach (GlobalExtraJump globalJump in GlobalExtraJumps)
					globalJump.OnJumpEnded(jump, player);

				data.PerformingJump = false;
			}
		}
	}

	public static void RefreshExtraJumps(Player player)
	{
		foreach (ModExtraJump jump in orderedJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (data.Active) {
				jump.OnJumpRefreshed(player);

				foreach (GlobalExtraJump globalJump in GlobalExtraJumps)
					globalJump.OnJumpRefreshed(jump, player);

				data.JumpAvailable = true;
			}
		}
	}

	internal static void ResetActiveFlags(Player player)
	{
		foreach (ModExtraJump jump in ExtraJumps)
			player.GetExtraJump(jump).Active = false;
	}

	internal static void ClearUnavailableExtraJumps(Player player)
	{
		foreach (ModExtraJump jump in ExtraJumps) {
			ref ExtraJumpData data = ref player.GetExtraJump(jump);
			if (!data.Active)
				data.JumpAvailable = false;
		}
	}

	internal static void ClearAllExtraJumps(Player player)
	{
		foreach (ModExtraJump jump in ExtraJumps)
			player.GetExtraJump(jump).JumpAvailable = false;
	}
}
