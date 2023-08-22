#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Used when NPCs or other entities are caught by things like bug nets. Normally converting the caught entity into an item.
/// </summary>
public class EntitySource_Caught : EntitySource_Parent
{
	/// <summary>
	/// The entity which performed the act of catching.<br/><br/>
	/// In vanilla, this is a <see cref="Player"/>.
	/// </summary>
	public Entity Catcher { get; }

	public EntitySource_Caught(Entity catcher, Entity caught, string? context = null) : base(caught, context)
	{
		Catcher = catcher;
	}
}
