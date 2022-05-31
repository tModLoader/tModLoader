using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModTileEntityTest : ModTileEntity
{
	public override void Load(TagCompound tag) { /* Empty */ }

	public override bool ValidTile(int i, int j) { return true; } // Mandatory
}