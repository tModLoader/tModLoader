namespace Terraria.ModLoader.Default;

public abstract class ModLoaderModItem : ModItem
{
	// wish we could just use slashes everywhere, and use the standardised paths here too, but this will do for now - CB
	// Need to change how AssemblyResourcesContentSource works, and it's a pain to map forward and back with slashes and not stuff up extensions
	public override string Texture => "ModLoader/"+base.Texture.Substring("Terraria.ModLoader.Default.".Length).Replace('/', '.');
}
