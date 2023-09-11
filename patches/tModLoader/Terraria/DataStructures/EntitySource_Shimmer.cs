using System;
using Terraria.ID;

#nullable enable

namespace Terraria.DataStructures;

// Added by TML.
/// <summary> //TODO: This
/// </summary>
public class EntitySource_Shimmer : EntitySource_Parent
{
	public Player AttachedPlayer { get; set; }
	public Entity ShimmeredEntity => Entity;

	public EntitySource_Shimmer(Entity shimmeredFrom, Player attachedPlayer, string? context = null) : base(shimmeredFrom, context)
	{
		AttachedPlayer = attachedPlayer;
	}
}