#nullable enable

namespace Terraria.DataStructures
{
	/// <summary>
	/// Used for when NPCs or other entities are caught by things like bug nets.
	/// </summary>
	public class EntitySource_CatchEntity : IEntitySource
	{
		/// <summary>
		/// The entity which performed the act of catching the <see cref="CaughtEntity"/>.<br></br>
		/// In the vast majority of cases, this will be a <see cref="Player"/>.
		/// </summary>
		public readonly Entity Entity;

		/// <summary>
		/// The entity which was caught by the <see cref="Entity"/>.<br></br>
		/// In the vast majority of cases, this will be an <see cref="NPC"/>.
		/// </summary>
		public readonly Entity CaughtEntity;

		public string? Context { get; }

		public EntitySource_CatchEntity(Entity entity, Entity caughtEntity, string? context = null) {
			Entity = entity;
			CaughtEntity = caughtEntity;
			Context = context;
		}
	}
}
