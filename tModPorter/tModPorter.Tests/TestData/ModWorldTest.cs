using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModWorld
{
	public override void Load(TagCompound tag) { /* Empty */ }

	public override TagCompound Save() => new TagCompound();

	public override void Initialize() { /* Empty */ }

	public override void PreUpdate() { /* Empty */ }

	public override void PostUpdate() { /* Empty */ }

	public override void TileCountsAvailable(int[] tileCounts) { /* Empty */ }
}