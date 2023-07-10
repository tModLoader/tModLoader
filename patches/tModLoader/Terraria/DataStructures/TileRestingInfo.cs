using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace Terraria.DataStructures;

/// <summary>
/// Holds data required for offsetting an entity when it rests on a tile (sitting/sleeping).
/// </summary>
public struct TileRestingInfo
{
	/// <summary>
	/// The resting entity (Player or NPC). Can be null if not available from the context.
	/// </summary>
	public Entity RestingEntity;

	/// <summary>
	/// The bottom-most position of the resting tile in tile coordinates, affecting logic for resetting (invalid) resting state and used to align the hitbox.
	/// </summary>
	public Point AnchorTilePosition;

	/// <summary>
	/// The visual offset of the entity, not affecting any logic.
	/// </summary>
	public Vector2 VisualOffset;

	/// <summary>
	/// Direction the entity is facing while resting. Is 0 by default for beds.
	/// </summary>
	public int TargetDirection;

	/// <summary>
	/// Length of the entity position offset applied in the X direction based on targetDirection.
	/// </summary>
	public int DirectionOffset;

	/// <summary>
	/// Offset applied to the final anchor position. Use with caution, vanilla does not utilize it!
	/// </summary>
	public Vector2 FinalOffset;

	/// <summary>
	/// Contains additional information, such as <see cref="ExtraSeatInfo.IsAToilet"/>.
	/// </summary>
	public ExtraSeatInfo ExtraInfo;

	public TileRestingInfo(Entity restingEntity, Point anchorTilePosition, Vector2 visualOffset, int targetDirection, int directionOffset = 0, Vector2 finalOffset = default, ExtraSeatInfo extraInfo = default)
	{
		RestingEntity = restingEntity;
		AnchorTilePosition = anchorTilePosition;
		VisualOffset = visualOffset;
		TargetDirection = targetDirection;
		DirectionOffset = directionOffset;
		FinalOffset = finalOffset;
		ExtraInfo = extraInfo;
	}

	public void Deconstruct(out Entity restingEntity, out Point anchorTilePosition, out Vector2 visualOffset, out int targetDirection, out int directionOffset, out Vector2 finalOffset, out ExtraSeatInfo extraInfo)
	{
		restingEntity = RestingEntity;
		anchorTilePosition = AnchorTilePosition;
		visualOffset = VisualOffset;
		targetDirection = TargetDirection;
		directionOffset = DirectionOffset;
		finalOffset = FinalOffset;
		extraInfo = ExtraInfo;
	}
}
