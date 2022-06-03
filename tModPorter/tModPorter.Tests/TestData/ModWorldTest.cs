using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModWorldTest : ModWorld
{
	public override void Load(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override TagCompound Save() => new TagCompound();
#endif

	public override void Initialize() { /* Empty */ }

	public override void PreUpdate() { /* Empty */ }

	public override void PostUpdate() { /* Empty */ }
}