using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md

	/// <summary>
	/// Manages content ID sets. We can use ID sets to store content-specific data.
	/// </summary>
	public class VictoryPoseID
	{
		[ReinitializeDuringResizeArrays]
		public static class Sets
		{
			public static SetFactory Factory = new SetFactory(VictoryPoseLoader.victoryPoses.Count, "ExampleMod/VictoryPoseID", Search);

			public static bool[] NonBoss = Factory.CreateNamedSet("NonBoss")
				.Description("Victory poses in this set are options to be chosen when defeating a regular enemy")
				.RegisterBoolSet(false);
		}

		public static IdDictionary Search = IdDictionary.Create<VictoryPoseID, int>();
	}
}
