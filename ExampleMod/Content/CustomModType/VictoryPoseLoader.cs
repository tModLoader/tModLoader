using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md

	/// <summary>
	/// Manages loading and assigning content IDs for ModVictoryPose.
	/// </summary>
	public class VictoryPoseLoader : ILoadable
	{
		internal static readonly List<ModVictoryPose> VictoryPoses = [];

		internal static int Add(ModVictoryPose victoryPose) {
			int type = VictoryPoses.Count;
			VictoryPoses.Add(victoryPose);
			return type;
		}

		public void Load(Mod mod) {
		}

		public void Unload() {
		}
	}
}
