#nullable enable

namespace Terraria.DataStructures
{
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
