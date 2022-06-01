using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModTileEntityTest : ModTileEntity
{
	public override void LoadData(TagCompound tag) { /* Empty */ }

	public override bool IsTileValidForEntity(int i, int j) { return true; } // Mandatory
}