using System;
using Terraria.ID;

#nullable enable

namespace Terraria.DataStructures;

// Added by TML.
/// <summary>
/// To be used in cases where no entity is present. See <see cref="ItemSourceID"/> and <see cref="ProjectileSourceID"/> for vanilla values<para/>
/// <b>NOTE:</b> Unlike most other entity sources, this one requires <see cref="Context"/> to be specified.
/// </summary>
public class EntitySource_Misc : IEntitySource
{
	// Unlike every other Context implementation, this one wants non-null values.
	public string Context { get; }
	
	public EntitySource_Misc(string context)
	{
		Context = context ?? throw new ArgumentNullException(nameof(context), $"The {nameof(EntitySource_Misc)} type always expects a context string to be present.");
	}
}
