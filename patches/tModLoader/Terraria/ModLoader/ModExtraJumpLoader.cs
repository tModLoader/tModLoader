using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all ModExtraJump logic is handled and hooks called.
	/// </summary>
	public static class ModExtraJumpLoader
	{
		private static readonly IList<ModExtraJump> extraJumps = new List<ModExtraJump>();

		internal static void Add(ModExtraJump extraJump)
		{
			extraJump.index = extraJumps.Count;
			extraJumps.Add(extraJump);
		}

		internal static void Unload() => extraJumps.Clear();

		internal static void SetupPlayer(Player player) => player.modExtraJumps = extraJumps.Select(extraJump => extraJump.CreateFor(player)).ToArray();

		/// <summary>
		/// Allows you to create visuals while the jump in <see cref="Player.activeJump"/> is happening.
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void PerformingJump(Player player)
		{
			ModExtraJump jump = player.activeJump;
			if (jump != null && jump.isPerformingJump) // No '&& !canJumpAgain' for compatibility with CanJumpAgain()
				jump.PerformingJump();
		}

		/// <summary>
		/// Allows you to modify the horizontal acceleration and max speed during the jump (e.g. sandstorm uses 1.5f and 2f, blizzard uses 3f and 1.5f).
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void HorizontalJumpSpeed(Player player)
		{
			ModExtraJump jump = player.activeJump;
			if (jump != null && jump.hasJumpOption && jump.isPerformingJump) {
				float runAccelerationMult = 1f;
				float maxRunSpeedMult = 1f;
				jump.HorizontalJumpSpeed(ref runAccelerationMult, ref maxRunSpeedMult);
				player.runAcceleration *= runAccelerationMult;
				player.maxRunSpeed *= maxRunSpeedMult;
			}
		}

		/// <summary>
		/// Do the jump if a valid <see cref="Player.activeJump"/> is found in <see cref="SetNextJump(Player)"/>.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void Jump(Player player)
		{
			bool playSound = true;
			float jumpHeight = 1f;
			ModExtraJump jump = player.activeJump;
			jump.isPerformingJump = true;
			jump.Jump(ref jumpHeight, ref playSound);
			if (playSound) {
				SoundEngine.PlaySound(SoundID.DoubleJump, (int)player.position.X, (int)player.position.Y);
			}
			player.velocity.Y = -Player.jumpSpeed * player.gravDir;
			player.jump = (int)(Player.jumpHeight * jumpHeight);
		}

		/// <summary>
		/// Resets all hasJumpOption toggles.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllEnable(Player player)
		{
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				extraJump.hasJumpOption = false;
			}
		}

		/// <summary>
		/// Stops the ability to do modded extra jumps (finished jumping, on a rope, mounted, CCed or grappled).
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllJumps(Player player)
		{
			player.activeJump = null;
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				extraJump.canJumpAgain = false;
			}
		}

		/// <summary>
		/// Resets the jumping effect for all modded extra jumps.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllVisualEffects(Player player)	
		{
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				extraJump.isPerformingJump = false;
			}
		}

		/// <summary>
		/// Returns true if atleast one double jump is able to be used.
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static bool AnyJumpAgain(Player player) => player.modExtraJumps.Any(extraJump => extraJump.canJumpAgain);

		/// <summary>
		/// Allows the ability to double jump if it is enabled.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void SetJumpState(Player player)
		{
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				if (extraJump.hasJumpOption) {
					extraJump.canJumpAgain = true;
				}
			}
		}

		/// <summary>
		/// Toggles the ability to double jump if it's active, otherwise forbid it.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ToggleJumpState(Player player)
		{
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				if (!extraJump.hasJumpOption) {
					extraJump.canJumpAgain = false;
				}
			}
		}

		/// <summary>
		/// Sets <see cref="Player.activeJump"/> to the next modded jump that occurs, then disables the jump for the duration if not otherwise specified in <see cref="ModExtraJump.CanJumpAgain"/>.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static bool SetNextJump(Player player)
		{
			foreach (ModExtraJump extraJump in player.modExtraJumps) {
				if (extraJump.canJumpAgain) {
					player.activeJump = extraJump;
					extraJump.canJumpAgain = extraJump.CanJumpAgain(); // True to continue jumping, false to not allow additonal jumps after this one
					return true;
				}
			}
			return false;
		}
	}
}
