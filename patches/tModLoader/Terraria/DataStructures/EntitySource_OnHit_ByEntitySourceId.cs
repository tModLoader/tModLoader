using System;
using Terraria.ID;

namespace Terraria.DataStructures
{
	public class EntitySource_OnHit_ByEntitySourceID : EntitySource_OnHit
	{
		public readonly int SourceId;

		public EntitySource_OnHit_ByEntitySourceID(Entity entityStriking, Entity entityStruck, int entitySourceId) : base(entityStriking, entityStruck) {
			if (entitySourceId <= EntitySourceID.None || entitySourceId >= EntitySourceID.Count) {
				throw new ArgumentOutOfRangeException(nameof(entitySourceId));
			}

			SourceId = entitySourceId;
		}
	}
}
