using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedWall : ModWall
{
	public override string Texture => "ModLoader/UnloadedWall";

	public override void SetStaticDefaults()
	{
		TileIO.Walls.unloadedTypes.Add(Type);
	}
}