using Terraria.WorldBuilding;

namespace Terraria
{
	public partial class WorldGen
	{
		private static double _timePass = 0.0; // Used to account for more precise time rates.

		internal static void ClearGenerationPasses() => _generator?._passes.Clear();
	}
}
