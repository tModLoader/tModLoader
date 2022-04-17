namespace Terraria.DataStructures
{
	public class EntitySource_ByEntitySourceID : IEntitySource
	{
		public readonly Entity Entity;
		public readonly int SourceId;

		public EntitySource_ByEntitySourceID(Entity entity, int entitySourceId) {
			Entity = entity;
			SourceId = entitySourceId;
		}
	}
}
