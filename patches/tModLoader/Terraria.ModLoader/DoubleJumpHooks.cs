using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all ModDoubleJump logic is handled and hooks called.
	/// </summary>
	public static class DoubleJumpHooks
	{
		private static readonly IList<ModDoubleJump> doubleJumps = new List<ModDoubleJump>();

		internal static void Add(ModDoubleJump doubleJump)
		{
			doubleJump.index = doubleJumps.Count;
			doubleJumps.Add(doubleJump);
		}

		internal static void Unload()
		{
			doubleJumps.Clear();
		}

		internal static void SetupPlayer(Player player)
		{
			player.modDoubleJumps = doubleJumps.Select(doubleJump => doubleJump.CreateFor(player)).ToArray();
		}

		/// <summary>
		/// Allows you to create visuals while the jump in <see cref="Player.activeJump"/> is happening
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void MidJump(Player player)
		{
			player.activeJump?.MidJump();
		}

		/// <summary>
		/// Allows you to modify the horizontal acceleration and max speed during the jump (sandstorm uses 1.5f and 2f, blizzard uses 3f and 1.5f)
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void HorizontalJumpSpeed(Player player)
		{
			if (player.activeJump?.IsMidJump == true) {
				float runAccelerationMult = 1f;
				float maxRunSpeedMult = 1f;
				player.activeJump.HorizontalJumpSpeed(ref runAccelerationMult, ref maxRunSpeedMult);
				player.runAcceleration *= runAccelerationMult;
				player.maxRunSpeed *= maxRunSpeedMult;
			}
		}

		/// <summary>
		/// Do the jump if a valid <see cref="Player.activeJump"/> is found in <see cref="SetNextJump(Player)"/>
		/// </summary>
		/// <param name="player">Jumping player</param>
		/// <param name="playSound">If left on true, will play the default double jump sound</param>
		/// <returns>Jump height multiplier</returns>
		internal static float Jump(Player player, ref bool playSound)
		{
			return player.activeJump?.Jump(ref playSound) ?? 1f; // activeJump shouldn't be null here, but in case it is
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
		/// Stops the ability to do modded double jumps (if mounted and jumps are blocked on that mount)
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllJumps(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				doubleJump.again = false;
			}
			ResetActiveJump(player);
		}

		/// <summary>
		/// Resets active modded jump (finished jumping, on a rope, mounted, CCed or grappled)
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetActiveJump(Player player)
		{
			player.activeJump = null;
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
		/// Toggles the ability to double jump if it's active, otherwise forbit it
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
		/// Sets <see cref="Player.activeJump"/> to the next modded jump that occurs, alphabetically per mod and then per jump, then disables the jump for the duration if not otherwise specified in <see cref="ModDoubleJump.JumpAgain"/>
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static bool SetNextJump(Player player)
		{
			foreach (ModDoubleJump doubleJump in player.modDoubleJumps) {
				if (doubleJump.CanJump) {
					player.activeJump = doubleJump;
					doubleJump.again = doubleJump.JumpAgain(); // True to continue jumping, false to not allow additonal jumps after this one
					return true;
				}
			}
			return false;
		}
	}
}
