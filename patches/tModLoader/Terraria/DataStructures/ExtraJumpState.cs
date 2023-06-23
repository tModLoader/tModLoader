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
	/// <see langword="true"/> if the extra jump has not been consumed. Will be set to <see langword="false"/> when a jump starts.<br/>
	/// When checking this field, make sure to check <see cref="Enabled"/> first.
	/// </summary>
	public bool JumpAvailable;
	/// <summary>
	/// Whether any effects (e.g. spawning dusts) should be performed after consuming the extra jump, but before its duration runs out
	/// </summary>
	public bool PerformingJump;
}
