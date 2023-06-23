using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public static class ExtraJumpLoader
{
	public static int ExtraJumpCount => ExtraJumps.Count;

	// Order is the vanilla priority when consuming the extra jumps
	internal static readonly List<ExtraJump> ExtraJumps = new List<ExtraJump>()
	{
		ExtraJump.GoatMount,
		ExtraJump.SantankMount,
		ExtraJump.UnicornMount,
		ExtraJump.SandstormInABottle,
		ExtraJump.BlizzardInABottle,
		ExtraJump.FartInAJar,
		ExtraJump.TsunamiInABottle,
		ExtraJump.BasiliskMount,
		ExtraJump.CloudInABottle
	};

	internal static readonly int DefaultExtraJumpCount = ExtraJumps.Count;

	public static readonly ExtraJump FirstVanillaExtraJump = ExtraJumps[0];

	public static readonly ExtraJump LastVanillaExtraJump = ExtraJumps[^1];

	internal static IEnumerable<ExtraJump> ModdedExtraJumps => ExtraJumps.Skip(DefaultExtraJumpCount);

	private static ExtraJump[] orderedJumps;

	static ExtraJumpLoader()
	{
		RegisterDefaultJumps();
	}

	internal static int Add(ExtraJump jump)
	{
		ExtraJumps.Add(jump);
		return ExtraJumps.Count - 1;
	}

	public static ExtraJump Get(int index) => index < 0 || index >= ExtraJumpCount ? null : ExtraJumps[index];

	internal static void Unload()
	{
		ExtraJumps.RemoveRange(DefaultExtraJumpCount, ExtraJumpCount - DefaultExtraJumpCount);
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
		foreach (ExtraJump jump in ModdedExtraJumps) {
			var position = jump.GetDefaultPosition();

			switch (position) {
				case ExtraJump.After after:
					int afterParent = after.Parent?.Type is { } afterType ? afterType + 1 : 0;

					sortingSlots[afterParent].Add(jump.Type);
					break;
				case ExtraJump.Before before:
					int beforeParent = before.Parent?.Type is { } beforeType ? beforeType : sortingSlots.Length - 1;

					sortingSlots[beforeParent].Add(jump.Type);
					break;
				default:
					throw new ArgumentException($"ModExtraJump {jump} has unknown Position {position}");
			}
		}

		// Cache the information for which additional constraints each modded extra jump has
		var positions = ModdedExtraJumps.ToDictionary(j => j.Type, j => j.GetModdedConstraints()?.Select(p => VerifyPositionType(j, p)).ToList() ?? new());

		// Sort the modded jumps per slot
		List<ExtraJump> sorted = new();

		for (int i = 0; i < DefaultExtraJumpCount + 2; i++) {
			var sort = new TopoSort<ExtraJump>(sortingSlots[i].Select(static t => ExtraJumps[t]),
				j => positions[j.Type].OfType<ExtraJump.After>().Select(static a => a.Parent).OfType<ExtraJump>(),
				j => positions[j.Type].OfType<ExtraJump.Before>().Select(static b => b.Parent).OfType<ExtraJump>());

			foreach (ExtraJump jump in sort.Sort()) {
				sorted.Add(jump);
			}

			if (i < DefaultExtraJumpCount)
				sorted.Add(ExtraJumps[i]);
		}

		orderedJumps = sorted.ToArray();
	}

	private static ExtraJump.Position VerifyPositionType(ExtraJump jump, ExtraJump.Position position) {
		if (position is not ExtraJump.After and not ExtraJump.Before)
			throw new ArgumentException($"ModExtraJump {jump} has unknown Position {position}");

		return position;
	}

	internal static void RegisterDefaultJumps()
	{
		int i = 0;
		foreach (var jump in ExtraJumps) {
			jump.Type = i++;
			ContentInstance.Register(jump);
			ModTypeLookup<ExtraJump>.Register(jump);
		}
	}

	public static void UpdateHorizontalSpeeds(Player player)
	{
		foreach (ExtraJump moddedExtraJump in orderedJumps) {
			// Special case: Sandstorm in a Bottle uses a separate flag
			ref ExtraJumpState extraJump = ref player.GetJumpState(moddedExtraJump);
			if ((object.ReferenceEquals(moddedExtraJump, ExtraJump.SandstormInABottle) && player.sandStorm) || (extraJump.PerformingJump && extraJump.Enabled))
				moddedExtraJump.UpdateHorizontalSpeeds(player);
		}
	}

	public static void JumpVisuals(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.PerformingJump && state.Enabled && !state.JumpAvailable && jump.CanShowVisuals(player) && PlayerLoader.CanShowExtraJumpVisuals(jump, player)) {
				jump.Visuals(player);
				PlayerLoader.ExtraJumpVisuals(jump, player);
			}
		}
	}

	public static void ProcessJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.JumpAvailable && !state._disabled) {
				state.JumpAvailable = false;
				state.PerformingJump = true;
				jump.PerformJump(player);
				break;
			}
		}

		// The Basilisk mount's extra jump is always "consumed", even if a higher-priority jump was performed
		player.GetJumpState(ExtraJump.BasiliskMount).JumpAvailable = false;
	}

	public static void OnJumpEnded(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.PerformingJump) {
				jump.OnEnded(player);
				PlayerLoader.OnExtraJumpEnded(jump, player);
				state.PerformingJump = false;
			}
		}
	}

	public static void RefreshExtraJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Enabled) {
				jump.OnRefreshed(player);
				PlayerLoader.OnExtraJumpRefreshed(jump, player);
				state.JumpAvailable = true;
			}
		}
	}

	internal static void ResetActiveFlags(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			state.Enabled = false;
			state._disabled = false;
		}
	}

	internal static void ClearUnavailableExtraJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (!state.Enabled)
				state.JumpAvailable = false;
		}
	}

	internal static void ClearAllExtraJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			player.GetJumpState(jump).JumpAvailable = false;
		}
	}
}
