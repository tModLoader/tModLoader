using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md

	/// <summary>
	/// Manages loading and assigning content IDs for ModVictoryPose.
	/// <para/> This is the also the main API exposed and intended to be used by other mods. For example, other mods could use the API to trigger a specific ModVictoryPose manually, such as when they craft a specific item.
	/// <para/> The internal methods are not part of the API.
	/// </summary>
	public class VictoryPoseLoader : ILoadable
	{
		internal static readonly List<ModVictoryPose> victoryPoses = [];

		// Expose victoryPoses as a ReadOnlyList to other mods to prevent accidental manipulations.
		public static IReadOnlyList<ModVictoryPose> VictoryPoses => victoryPoses;

		internal static int Add(ModVictoryPose victoryPose) {
			int type = victoryPoses.Count;
			victoryPoses.Add(victoryPose);
			return type;
		}

		public void Load(Mod mod) {
		}

		public void Unload() {
		}

		/// <summary>
		/// Attempts to start the specified <paramref name="pose"/> for the local player. The <paramref name="forced"/> parameter will interrupt an active pose, otherwise the pose won't start if an active pose is ongoing. The return value indicates if the pose was started or not.
		/// <para/> This method will also handle syncing the pose to other clients.
		/// </summary>
		public static bool StartPose(ModVictoryPose pose, bool forced = false) {
			if (Main.netMode == NetmodeID.Server) {
				return false;
			}
			VictoryPosePlayer victoryPosePlayer = Main.LocalPlayer.GetModPlayer<VictoryPosePlayer>();
			bool success = victoryPosePlayer.StartPose(pose);
			if (success) {
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// Inform other clients about the pose to sync the visuals.
					VictoryPosePlayer.SendStartVictoryPoseMessage(Main.myPlayer, victoryPosePlayer.activeVictoryPose);
				}
			}
			return success;
		}

		/// <summary>
		/// Ends the active pose for the local player immediately, if any. Syncs the change to other clients.
		/// </summary>
		public static void CancelPose() {
			VictoryPosePlayer victoryPosePlayer = Main.LocalPlayer.GetModPlayer<VictoryPosePlayer>();
			if (victoryPosePlayer.activeVictoryPose != null) {
				victoryPosePlayer.EndPose();

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// Inform other clients about the pose to sync the visuals.
					VictoryPosePlayer.SendCancelVictoryPoseMessage(Main.myPlayer);
				}
			}
		}

		public static ModVictoryPose GetActivePose(Player player) => player.GetModPlayer<VictoryPosePlayer>().activeVictoryPose;
	}
}
