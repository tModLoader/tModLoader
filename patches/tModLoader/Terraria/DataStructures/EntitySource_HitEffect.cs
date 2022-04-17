#nullable enable

namespace Terraria.DataStructures
{
	/// <summary>
	/// Added by TML.
	/// <br/> To be used in not always authoritative contexts, like <see cref="NPC.HitEffect"/>'s,
	/// <br/> where the method may be called from a network packet, and not everything about the entity hit can be retrieved.
	/// </summary>
	public record class EntitySource_HitEffect(Entity Entity, string? Context = null) : IEntitySource;
}
