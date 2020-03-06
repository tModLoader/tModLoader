using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all DoubleJumpHooks hooks are gathered and called.
	/// </summary>
	public static class DoubleJumpHooks
	{
		private static readonly IList<ModDoubleJump> doubleJumps = new List<ModDoubleJump>();

		/// <summary>
		/// This is the jump that will be executed next
		/// </summary>
		//It's okay to have it static here because all the jumping logic happens player-by-player which is reset to -1 when the next player jumping logic happens.
		//Also to reduce patch size having it here instead of as a local variable in Player.cs
		private static ModDoubleJump activeJump;

		private class HookList
		{
			public int[] arr = new int[0];
			public readonly MethodInfo method;

			public HookList(MethodInfo method)
			{
				this.method = method;
			}
		}

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModDoubleJump, F>> func)
		{
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		internal static void Add(ModDoubleJump doubleJump)
		{
			doubleJump.index = doubleJumps.Count;
			doubleJumps.Add(doubleJump);
		}

		internal static void RebuildHooks()
		{
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(doubleJumps, hook.method).Select(p => p.index).ToArray();
			}
		}

		internal static void Unload()
		{
			doubleJumps.Clear();
		}

		internal static void SetupPlayer(Player player)
		{
			player.modDoubleJumps = doubleJumps.Select(doubleJump => doubleJump.CreateFor(player)).ToArray();
		}

		private static HookList HookMidJump = AddHook<Action>(d => d.MidJump);

		/// <summary>
		/// Allows you to create visuals while the jump is happening
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void MidJump(Player player)
		{
			foreach (int index in HookMidJump.arr) {
				ModDoubleJump doubleJump = player.modDoubleJumps[index];
				if (doubleJump.IsMidJump) {
					player.modDoubleJumps[index].MidJump();
					return;
				}
			}
		}

		/// <summary>
		/// Do the jump if a valid <see cref="activeJump"/> is found in <see cref="SetNextJump(Player)"/>
		/// </summary>
		/// <param name="player">Jumping player</param>
		/// <param name="playSound">if left on true, will play the default double jump sound</param>
		/// <returns>Jump height multiplier</returns>
		internal static float Jump(ref bool playSound)
		{
			float mult = 1f;
			if (activeJump != null) {
				activeJump.effect = true;
				mult = activeJump.Jump(ref playSound);
				activeJump = null;
			}
			return mult;
		}

		/// <summary>
		/// Resets all modded double jump enable toggles
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllEnable(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				doubleJump.enable = false;
			}
		}

		/// <summary>
		/// Stops the ability to do modded double jumps
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllJumps(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				doubleJump.again = false;
			}
		}

		/// <summary>
		/// Resets all modded double jumps' effects while jumping
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllEffects(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				doubleJump.effect = false;
			}
		}

		/// <summary>
		/// Returns true if atleast one double jump is able to be used
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static bool AnyJumpAgain(Player player)
		{
			return player.modDoubleJumps.Any(doubleJump => doubleJump.again);
		}

		/// <summary>
		/// Allows the ability to double jump if it is enabled
		/// </summary>
		/// <param name="player">Jumping player</param>
		/// <param name="canJump">Additional condition</param>
		internal static void SetJumpState(Player player, bool canJump = true)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				if (canJump && doubleJump.enable) {
					doubleJump.again = true;
				}
			}
		}

		/// <summary>
		/// Toggles the ability to double jump if it's active, otherwise permits it
		/// </summary>
		/// <param name="player">Jumping player</param>
		/// <param name="canJump">Additional condition</param>
		internal static void ToggleJumpState(Player player, bool canJump)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				if (!doubleJump.enable) {
					doubleJump.again = false;
				}
				else if (canJump) {
					doubleJump.again = true;
				}
			}
		}

		/// <summary>
		/// Sets <see cref="activeJump"/> to the next jump that occurs, in order of modded jumps, alphabetically per mod, then disables the jump for the duration if not otherwise specified
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static bool SetNextJump(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				if (doubleJump.CanJump) {
					activeJump = doubleJump;
					doubleJump.again = doubleJump.JumpAgain();
					return true;
				}
			}
			return false;
		}
	}
}
