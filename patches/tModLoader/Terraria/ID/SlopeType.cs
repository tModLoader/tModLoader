namespace Terraria.ID;

public enum SlopeType
{
	Solid = 0,
	/// <summary> Only the bottom and left sides are solid. </summary>
	SlopeDownLeft = 1,
	/// <summary> Only the bottom and right sides are solid. </summary>
	SlopeDownRight = 2,
	/// <summary> Only the top and left sides are solid. </summary>
	SlopeUpLeft = 3,
	/// <summary> Only the top and right sides are solid. </summary>
	SlopeUpRight = 4,
}