using System;

#nullable enable

namespace Terraria.DataStructures
{
	// Added by TML.
	/// <summary>
	/// To be used in cases where no entity is present.<para/>
	/// <b>NOTE:</b> Unlike most other entity sources, this one requires <b>context</b> to be specified.
	/// </summary>
	public record class EntitySource_Misc : IEntitySource
	{
		// Unlike every other Context implementation, this one wants non-null values.
		public string Context { get; init; }
		
		public EntitySource_Misc(string context) {
			Context = context ?? throw new ArgumentNullException(nameof(context), $"The {nameof(EntitySource_Misc)} type always expects a context string to be present.");
		}
	}
}
