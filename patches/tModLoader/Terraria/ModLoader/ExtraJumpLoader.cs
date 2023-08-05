using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public static class ExtraJumpLoader
{
	public static int ExtraJumpCount => ExtraJumps.Count;

	// Order is the vanilla priority when consuming the extra jumps
	internal static readonly List<ExtraJump> ExtraJumps = new List<ExtraJump>()
	{
		ExtraJump.Flipper,
		ExtraJump.BasiliskMount,
		ExtraJump.GoatMount,
		ExtraJump.SantankMount,
		ExtraJump.UnicornMount,
		ExtraJump.SandstormInABottle,
		ExtraJump.BlizzardInABottle,
		ExtraJump.FartInAJar,
		ExtraJump.TsunamiInABottle,
		ExtraJump.CloudInABottle
	};

	private static readonly int DefaultExtraJumpCount = ExtraJumps.Count;

	private static IEnumerable<ExtraJump> ModdedExtraJumps => ExtraJumps.Skip(DefaultExtraJumpCount);

	private static ExtraJump[] orderedJumps;

	public static IReadOnlyList<ExtraJump> OrderedJumps => orderedJumps;

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
		var sortingSlots = new List<ExtraJump>[DefaultExtraJumpCount + 1];
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

					sortingSlots[afterParent].Add(jump);
					break;
				case ExtraJump.Before before:
					if (before.Target is not null and not VanillaExtraJump)
						throw new ArgumentException($"ExtraJump {jump} did not refer to a vanilla ExtraJump in GetDefaultPosition()");

					int beforeParent = before.Target?.Type is { } beforeType ? beforeType : sortingSlots.Length - 1;

					sortingSlots[beforeParent].Add(jump);
					break;
				default:
					throw new ArgumentException($"ExtraJump {jump} has unknown Position {position}");
			}
		}

		// Sort the modded jumps per slot
		List<ExtraJump> sorted = new();

		for (int i = 0; i < DefaultExtraJumpCount + 1; i++) {
			var elements = sortingSlots[i];
			var sort = new TopoSort<ExtraJump>(elements,
				j => j.GetModdedConstraints()?.OfType<ExtraJump.After>().Select(static a => a.Target).Where(elements.Contains) ?? Array.Empty<ExtraJump>(),
				j => j.GetModdedConstraints()?.OfType<ExtraJump.Before>().Select(static b => b.Target).Where(elements.Contains) ?? Array.Empty<ExtraJump>());

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
			if (state.Active && jump.CanShowVisuals(player) && PlayerLoader.CanShowExtraJumpVisuals(jump, player)) {
				jump.ShowVisuals(player);
				PlayerLoader.ExtraJumpVisuals(jump, player);
			}
		}
	}

	public static void ProcessJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Available && jump.CanStart(player) && PlayerLoader.CanStartExtraJump(jump, player)) {
				state.Start();
				PerformJump(jump, player);
				break;
			}
		}
	}

	public static void RefreshJumps(Player player)
	{
		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Enabled) {
				jump.OnRefreshed(player);
				PlayerLoader.OnExtraJumpRefreshed(jump, player);
				state.Available = true;
			}
		}
	}

	internal static void StopActiveJump(Player player, out bool anyJumpCancelled)
	{
		anyJumpCancelled = false;

		foreach (ExtraJump jump in orderedJumps) {
			ref ExtraJumpState state = ref player.GetJumpState(jump);
			if (state.Active) {
				StopJump(jump, player);
				anyJumpCancelled = true;
			}
		}
	}

	internal static void ResetEnableFlags(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			player.GetJumpState(jump).ResetEnabled();
		}
	}

	internal static void ConsumeAndStopUnavailableJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			player.GetJumpState(jump).CommitEnabledState(out bool jumpEnded);

			// Force the jump to stop early if unequipped or disabled
			if (jumpEnded) {
				StopJump(jump, player);
				player.jump = 0;
			}
		}
	}

	internal static void ConsumeAllJumps(Player player)
	{
		foreach (ExtraJump jump in ExtraJumps) {
			player.GetJumpState(jump).Available = false;
		}
	}

	private static void PerformJump(ExtraJump jump, Player player)
	{
		// Set velocity and jump duration
		float duration = jump.GetDurationMultiplier(player);
		PlayerLoader.ModifyExtraJumpDurationMultiplier(jump, player, ref duration);

		player.velocity.Y = -Player.jumpSpeed * player.gravDir;
		player.jump = (int)(Player.jumpHeight * duration);

		bool playSound = true;
		jump.OnStarted(player, ref playSound);
		PlayerLoader.OnExtraJumpStarted(jump, player, ref playSound);

		if (playSound)
			SoundEngine.PlaySound(16, (int)player.position.X, (int)player.position.Y);
	}

	private static void StopJump(ExtraJump jump, Player player)
	{
		jump.OnEnded(player);
		PlayerLoader.OnExtraJumpEnded(jump, player);
		player.GetJumpState(jump).Stop();
	}
}
