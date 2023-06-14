#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Used when dropping coins and items when a player dies. <br/>
/// Recommended for use by mods when spawning gore.
/// </summary>
public class EntitySource_Death : EntitySource_Parent
{
	public EntitySource_Death(Entity entity, string? context = null) : base(entity, context) { }
}
