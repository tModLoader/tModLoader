using System;
using Terraria.ID;

namespace Terraria.DataStructures
{
	public class EntitySource_Death : IEntitySource
	{
		public readonly Entity Entity;

		public EntitySource_Death(Entity entity) {
			Entity = entity;
		}
	}
}
