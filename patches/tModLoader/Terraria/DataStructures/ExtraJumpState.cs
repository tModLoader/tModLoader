namespace Terraria.DataStructures;

/// <summary>
/// A structure containing fields used to manage extra jumps
/// </summary>
public struct ExtraJumpState
{
	/// <summary>
	/// Whether the extra jump can be used. This field is what should be set by equipment in UpdateEquip or UpdateAccessory.<br/>
	/// This field is automatically cleared in ResetEffects.
	/// </summary>
	public bool Enabled;
	/// <summary>
	/// Whether the extra jump has been consumed
	/// </summary>
	public bool JumpAvailable;
	/// <summary>
	/// Whether any effects (e.g. spawning dusts) should be performed
	/// </summary>
	public bool PerformingJump;
}
