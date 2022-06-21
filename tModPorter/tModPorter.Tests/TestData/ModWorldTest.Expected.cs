using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModSystem
{
	public override void LoadWorldData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveWorldData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ => new TagCompound();
#endif

	public override void OnWorldLoad()/* tModPorter Suggestion: Also override OnWorldUnload, and mirror your worldgen-sensitive data initialization in PreWorldGen */ { /* Empty */ }

	public override void PreUpdateWorld() { /* Empty */ }

	public override void PostUpdateWorld() { /* Empty */ }

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) { /* Empty */ }
}