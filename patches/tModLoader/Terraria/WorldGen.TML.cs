using Terraria.WorldBuilding;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses() => _generator?._passes.Clear();
}
