using System;
using Terraria.ID;

#nullable enable

namespace Terraria.DataStructures
{
	// Added by TML.
	public class EntitySource_SetBonus : IEntitySource
	{
		public readonly Entity Entity;
		public readonly string SetName;
		
		public string? Context { get; init; }

		public EntitySource_SetBonus(Entity entity, string setName, string context = null) {
			Entity = entity;
			SetName = setName;
			Context = context;
		}
	}
}
