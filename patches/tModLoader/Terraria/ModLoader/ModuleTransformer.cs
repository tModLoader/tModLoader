using Mono.Cecil;

namespace Terraria.ModLoader;

/// <summary>
///		SUMMARY TODO HI MOM
/// </summary>
public abstract class ModuleTransformer
{
	public abstract bool Transform(ModuleDefinition module);
}
