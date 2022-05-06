#nullable enable

namespace Terraria.DataStructures
{
	/// <summary>
	/// Added by TML. Used during TorchGod-related projectile and item spawning.
	/// </summary>
	public record class EntitySource_TorchGod(Entity TargetedEntity, string? Context = null) : IEntitySource;
}
