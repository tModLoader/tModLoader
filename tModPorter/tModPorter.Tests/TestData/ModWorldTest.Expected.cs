using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModSystem
{
	public override void LoadWorldData(TagCompound tag) { /* Empty */ }

	public override void OnWorldLoad() /* Also concider overriding OnWorldUnload */ { /* Empty */ }

	public override void PreUpdateWorld() { /* Empty */ }

	public override void PostUpdateWorld() { /* Empty */ }
}