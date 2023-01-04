namespace Terraria.ModLoader;

/// <summary>
/// The base type for most modded things with textures.
/// </summary>
public abstract class ModTexturedType : ModType
{
	/// <summary>
	/// The file name of this type's texture file in the mod loader's file space.
	/// </summary>
	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');//GetType().FullName.Replace('.', '/');
}
