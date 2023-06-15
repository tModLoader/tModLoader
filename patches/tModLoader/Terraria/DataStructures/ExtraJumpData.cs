namespace Terraria.DataStructures;

/// <summary>
/// A structure containing fields used to manage extra jumps
/// </summary>
public struct ExtraJumpData
{
	/// <summary>
	/// Whether the extra jump can be used. This field is what should be set by equipment
	/// </summary>
	public bool Active;
	/// <summary>
	/// Whether the extra jump has been consumed
	/// </summary>
	public bool JumpAvailable;
	/// <summary>
	/// Whether any effects (e.g. spawning dusts) should be performed
	/// </summary>
	public bool PerformingJump;
}
