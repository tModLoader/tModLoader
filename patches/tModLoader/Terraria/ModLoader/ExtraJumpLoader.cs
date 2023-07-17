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
					if (after.Target is not null and not VanillaExtraJump)
						throw new ArgumentException($"ExtraJump {jump} did not refer to a vanilla ExtraJump in GetDefaultPosition()");

					int afterParent = after.Target?.Type is { } afterType ? afterType + 1 : 0;

					sortingSlots[afterParent].Add(jump.Type);
					break;
				case ExtraJump.Before before:
					if (before.Target is not null and not VanillaExtraJump)
						throw new ArgumentException($"ExtraJump {jump} did not refer to a vanilla ExtraJump in GetDefaultPosition()");

					int beforeParent = before.Target?.Type is { } beforeType ? beforeType : sortingSlots.Length - 1;

					sortingSlots[beforeParent].Add(jump.Type);
					break;
				default:
					throw new ArgumentException($"ExtraJump {jump} has unknown Position {position}");
			}
		}

		// Cache the information for which additional constraints each modded extra jump has
		var positions = ModdedExtraJumps.ToDictionary(static j => j.Type, static j => j.GetModdedConstraints()?.ToList() ?? new());

		// Sort the modded jumps per slot
		List<ExtraJump> sorted = new();

		for (int i = 0; i < DefaultExtraJumpCount + 2; i++) {
			var sort = new TopoSort<ExtraJump>(sortingSlots[i].Select(static t => ExtraJumps[t]),
				j => positions[j.Type].OfType<ExtraJump.After>().Select(static a => a.Target).OfType<ExtraJump>(),
				j => positions[j.Type].OfType<ExtraJump.Before>().Select(static b => b.Target).OfType<ExtraJump>());

			foreach (ExtraJump jump in sort.Sort()) {
				sorted.Add(jump);
			}

			if (i < DefaultExtraJumpCount)
				sorted.Add(ExtraJumps[i]);
		}

		orderedJumps = sorted.ToArray();
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
			ref ExtraJumpState extraJump = ref player.GetJumpState(moddedExtraJump);
			if (extraJump.Active)
				moddedExtraJump.UpdateHorizontalSpeeds(player);
		}
	}

	public static void JumpVisuals(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);

			// Force the jump to stop early if unequipped or disabled
			if (!state.Enabled && state._active) {
				jump.OnEnded(player);
				PlayerLoader.OnExtraJumpEnded(jump, player);
				state._active = false;
				player.jump = 0;
			} else if (state.Active && jump.CanShowVisuals(player) && PlayerLoader.CanShowExtraJumpVisuals(jump, player)) {
				jump.Visuals(player);
				PlayerLoader.ExtraJumpVisuals(jump, player);
			}
		}
	}

	public static void ProcessJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Available) {
				state._available = false;
				state._active = true;
				jump.PerformJump(player);
				break;
			}
		}

		// The Basilisk mount's extra jump is always "consumed", even if a higher-priority jump was performed
		player.GetJumpState(ExtraJump.BasiliskMount).Available = false;
	}

	public static void RefreshJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state._enabled) {
				jump.OnRefreshed(player);
				PlayerLoader.OnExtraJumpRefreshed(jump, player);
				state._available = true;
			}
		}
	}

	internal static void StopActiveJump(Player player, out bool anyJumpCancelled)
	{
		anyJumpCancelled = false;

		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Active) {
				jump.OnEnded(player);
				PlayerLoader.OnExtraJumpEnded(jump, player);
				state._active = false;

				anyJumpCancelled = true;
			}
		}
	}

	internal static void ResetEnableFlags(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			state._enabled = false;
			state._disabled = false;
		}
	}

	internal static void ConsumeUnavailableJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (!state._enabled)
				state._available = false;
		}
	}

	internal static void ConsumeAllJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			player.GetJumpState(jump)._available = false;
		}
	}
}
