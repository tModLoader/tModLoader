using Mono.Cecil;

namespace Terraria.ModLoader;

/// <summary>
/// Base class that, when extended in a child class, facilitates the full strength of CoreMods.
/// Remember to set "hasCoreModTransformers" to <value>true</value> or else your child class will do nothing.
/// </summary>
/// <remarks>
/// <b> DO NOT USE THIS/COREMODS UNLESS YOU ARE ABSOLUTELY CERTAIN YOU CANNOT ACHIEVE YOUR FUNCTIONALITY OTHERWISE.</b>
/// CoreMods/Module Transformers are EXTREMELY powerful, and must be used with extreme care. <b> You have been warned. </b>
/// </remarks>
public abstract class ModuleTransformer
{

	public abstract void Transform(ModuleDefinition module);

}
