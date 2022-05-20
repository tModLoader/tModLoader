#nullable enable

namespace Terraria.DataStructures
{
	/// <summary>
	/// Used for when NPCs or other entities are caught by things like bug nets.
	/// </summary>
	public class EntitySource_CatchEntity : IEntitySource
	{
		public readonly Entity Entity;
		public readonly Entity CaughtEntity;

		public string? Context { get; }

		public EntitySource_CatchEntity(Entity entity, Entity caughtEntity, string? context = null) {
			Entity = entity;
			CaughtEntity = caughtEntity;
			Context = context;
		}
	}
}
