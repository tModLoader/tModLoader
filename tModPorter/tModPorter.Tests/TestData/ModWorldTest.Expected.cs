using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModSystem
{
	public override void LoadWorldData(TagCompound tag) { /* Empty */ }
  
#if COMPILE_ERROR
	public override void SaveWorldData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ => new TagCompound();
#endif

	public override void OnWorldLoad() /* Also concider overriding OnWorldUnload */ { /* Empty */ }

	public override void PreUpdateWorld() { /* Empty */ }

	public override void PostUpdateWorld() { /* Empty */ }
}