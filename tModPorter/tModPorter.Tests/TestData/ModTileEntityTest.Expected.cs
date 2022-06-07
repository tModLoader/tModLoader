using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModTileEntityTest : ModTileEntity
{
	public override bool IsTileValidForEntity(int i, int j) { return true; } // Mandatory

	public override void LoadData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(TagCompound tag)/* Suggestion: Edit tag parameter rather than returning new TagCompound */ => new TagCompound();
#endif
}