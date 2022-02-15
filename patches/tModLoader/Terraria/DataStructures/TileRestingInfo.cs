using Microsoft.Xna.Framework;

namespace Terraria.DataStructures
{
	/// <summary>
	/// Holds data required for offsetting an entity when it rests on a tile (sitting/sleeping).
	/// </summary>
	public struct TileRestingInfo
	{
		/// <summary>
		/// The anchor position in tile coordinates, affecting logic for resetting (invalid) resting state.
		/// <br>After it's set, vanilla offsets Y by +1.</br>
		/// </summary>
		public Point anchorTilePosition;

		/// <summary>
		/// The visual offset, not affecting any logic.
		/// </summary>
		public Vector2 visualOffset;

		/// <summary>
		/// Direction the entity is facing while resting.
		/// </summary>
		public int targetDirection;

		/// <summary>
		/// Length of the entity position offset applied in the X direction based on targetDirection.
		/// </summary>
		public int directionOffset;

		/// <summary>
		/// Offset applied to the final anchor position. Use with caution, vanilla does not utilize it!
		/// </summary>
		public Vector2 finalOffset;

		public TileRestingInfo(Point anchorTilePosition, Vector2 visualOffset, int targetDirection, int directionOffset = 0, Vector2 finalOffset = default) {
			this.anchorTilePosition = anchorTilePosition;
			this.visualOffset = visualOffset;
			this.targetDirection = targetDirection;
			this.directionOffset = directionOffset;
			this.finalOffset = finalOffset;
		}

		public void Deconstruct(out Point anchorTilePosition, out Vector2 visualOffset, out int targetDirection, out int directionOffset, out Vector2 finalOffset) {
			anchorTilePosition = this.anchorTilePosition;
			visualOffset = this.visualOffset;
			targetDirection = this.targetDirection;
			directionOffset = this.directionOffset;
			finalOffset = this.finalOffset;
		}
	}
}
