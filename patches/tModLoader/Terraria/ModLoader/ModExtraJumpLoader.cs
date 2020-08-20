using System;
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

		internal static void SetNextJump(Player player, ref bool basilisk, ref bool goat, ref bool santank, ref bool unicorn, ref bool sandstorm, ref bool blizzard, ref bool fart, ref bool sail, ref bool cloud, ref bool anyModExtraJump) {
			// This method is only called if atleast one modded jump exists
			foreach (var extraJump in player.modExtraJumps) {
				if (extraJump.index == VanillaExtraJump.Mounts.index) {
					if (player.canJumpAgain_Basilisk) {
						basilisk = true;
						player.canJumpAgain_Basilisk = false;
					}

					if (player.canJumpAgain_WallOfFleshGoat) {
						goat = true;
						player.canJumpAgain_WallOfFleshGoat = false;
						return;
					}
					else if (player.canJumpAgain_Santank) {
						santank = true;
						player.canJumpAgain_Santank = false;
						return;
					}
					else if (player.canJumpAgain_Unicorn) {
						unicorn = true;
						player.canJumpAgain_Unicorn = false;
						return;
					}
				}
				else if (extraJump.index == VanillaExtraJump.Sandstorm.index) {
					if (player.canJumpAgain_Sandstorm) {
						sandstorm = true;
						player.canJumpAgain_Sandstorm = false;
						return;
					}
				}
				else if (extraJump.index == VanillaExtraJump.Blizzard.index) {
					if (player.canJumpAgain_Blizzard) {
						blizzard = true;
						player.canJumpAgain_Blizzard = false;
						return;
					}
				}
				else if (extraJump.index == VanillaExtraJump.Fart.index) {
					if (player.canJumpAgain_Fart) {
						fart = true;
						player.canJumpAgain_Fart = false;
						return;
					}
				}
				else if (extraJump.index == VanillaExtraJump.Sail.index) {
					if (player.canJumpAgain_Sail) {
						sail = true;
						player.canJumpAgain_Sail = false;
						return;
					}
				}
				else if (extraJump.index == VanillaExtraJump.Cloud.index) {
					if (player.canJumpAgain_Cloud) {
						cloud = true;
						player.canJumpAgain_Cloud = false;
						return;
					}
				}
				// All vanilla cases done, only modded ones left to check
				else if (extraJump.canJumpAgain) {
					player.activeJump = extraJump;
					extraJump.canJumpAgain = extraJump.CanJumpAgain(); // True to continue jumping, false to not allow additonal jumps after this one
					anyModExtraJump = true;
					return;
				}
			}
		}

		internal static void Add(ModExtraJump extraJump) {
			if (extraJumps.Count == 0) {
				// Add vanilla jumps if not added already
				foreach (var vanillaJump in VanillaExtraJump.vanillaJumps) {
					vanillaJump.index = extraJumps.Count;
					extraJumps.Add(vanillaJump);
				}
			}

			var jumpAfter = extraJump.JumpAfter;
			if (jumpAfter == null || !VanillaExtraJump.vanillaJumps.Contains(jumpAfter)) {
				// Not a valid parent, default to last jump
				jumpAfter = VanillaExtraJump.Cloud;
			}

			// Insert after the parent
			int index = extraJumps.IndexOf(jumpAfter) + 1;
			extraJumps.Insert(index, extraJump);

			// Recalculate indexes due to new ones possibly being inserted
			for (int i = 0; i < extraJumps.Count; i++) {
				extraJumps[i].index = i;
			}
		}

		internal static void Unload() => extraJumps.Clear();

		internal static void SetupPlayer(Player player) => player.modExtraJumps = extraJumps.Select(extraJump => extraJump.CreateFor(player)).ToArray();

		/// <summary>
		/// Allows you to create visuals while the jump in <see cref="Player.activeJump"/> is happening.
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void PerformingJump(Player player) {
			ModExtraJump jump = player.activeJump;
			if (jump != null && jump.IsPerformingJump) // No '&& !canJumpAgain' for compatibility with CanJumpAgain()
				jump.PerformingJump();
		}

		/// <summary>
		/// Allows you to modify the horizontal acceleration and max speed during the jump (e.g. sandstorm uses 1.5f and 2f, blizzard uses 3f and 1.5f).
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static void HorizontalJumpSpeed(Player player) {
			ModExtraJump jump = player.activeJump;
			if (jump != null && jump.hasJumpOption && jump.IsPerformingJump) {
				float runAccelerationMult = 1f;
				float maxRunSpeedMult = 1f;
				jump.HorizontalJumpSpeed(ref runAccelerationMult, ref maxRunSpeedMult);
				player.runAcceleration *= runAccelerationMult;
				player.maxRunSpeed *= maxRunSpeedMult;
			}
		}

		/// <summary>
		/// Do the jump if a valid <see cref="Player.activeJump"/> is found in <see cref="SetNextJump"/>.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void Jump(Player player) {
			bool playSound = true;
			float jumpHeight = 1f;
			ModExtraJump jump = player.activeJump;
			jump.IsPerformingJump = true;
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
		internal static void ResetAllEnable(Player player) {
			foreach (ModExtraJump extraJump in player.modExtraJumps.Where(j => !(j is VanillaExtraJump))) {
				extraJump.hasJumpOption = false;
			}
		}

		/// <summary>
		/// Stops the ability to do modded extra jumps (finished jumping, on a rope, mounted, CCed or grappled).
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllJumps(Player player) {
			player.activeJump = null;
			foreach (ModExtraJump extraJump in player.modExtraJumps.Where(j => !(j is VanillaExtraJump))) {
				extraJump.canJumpAgain = false;
			}
		}

		/// <summary>
		/// Resets the jumping effect for all modded extra jumps.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ResetAllVisualEffects(Player player) {
			foreach (ModExtraJump extraJump in player.modExtraJumps.Where(j => !(j is VanillaExtraJump))) {
				extraJump.IsPerformingJump = false;
			}
		}

		/// <summary>
		/// Returns true if atleast one double jump is able to be used.
		/// </summary>
		/// <param name="player">Jumping player</param>
		public static bool AnyJumpAgain(Player player) => player.modExtraJumps.Any(j => !(j is VanillaExtraJump) && j.canJumpAgain);

		/// <summary>
		/// Allows the ability to double jump if it is enabled.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void SetJumpState(Player player) {
			foreach (ModExtraJump extraJump in player.modExtraJumps.Where(j => !(j is VanillaExtraJump))) {
				if (extraJump.hasJumpOption) {
					extraJump.canJumpAgain = true;
				}
			}
		}

		/// <summary>
		/// Toggles the ability to double jump if it's active, otherwise forbid it.
		/// </summary>
		/// <param name="player">Jumping player</param>
		internal static void ToggleJumpState(Player player) {
			foreach (ModExtraJump extraJump in player.modExtraJumps.Where(j => !(j is VanillaExtraJump))) {
				if (!extraJump.hasJumpOption) {
					extraJump.canJumpAgain = false;
				}
			}
		}
	}
}
