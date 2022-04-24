#nullable enable

namespace Terraria.DataStructures
{
	public record class EntitySource_Death(Entity Entity, string? Context = null) : IEntitySource;
}
