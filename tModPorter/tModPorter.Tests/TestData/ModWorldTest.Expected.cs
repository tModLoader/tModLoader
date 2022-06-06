using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModSystem
{
	public override void LoadWorldData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveWorldData(TagCompound tag)/* Suggestion: Edit tag parameter rather than returning new TagCompound */ => new TagCompound();
#endif

	public override void OnWorldLoad()/* Suggestion: Also concider overriding OnWorldUnload */ { /* Empty */ }

	public override void PreUpdateWorld() { /* Empty */ }

	public override void PostUpdateWorld() { /* Empty */ }

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) { /* Empty */ }
}