using Terraria.ModLoader;

namespace Terraria.DataStructures;

/// <summary>
/// A structure containing fields used to manage extra jumps<br/><br/>
///
/// Valid states for an extra jump are as follows:
/// <list type="bullet">
/// <item>Enabled = <see langword="false"/> | The extra jump cannot be used.  JumpAvailable and PerformingJump will be <see langword="false"/></item>
/// <item>Enabled = <see langword="true"/>, JumpAvailable = <see langword="true"/>, PerformingJump = <see langword="false"/> | The extra jump is ready to be consumed, but hasn't been consumed yet</item>
/// <item>Enabled = <see langword="true"/>, JumpAvailable = <see langword="false"/>, PerformingJump = <see langword="true"/> | The extra jump has been consumed and is currently in progress</item>
/// <item>Enabled = <see langword="true"/>, JumpAvailable = <see langword="true"/>, PerformingJump = <see langword="true"/> | The extra jump has been consumed and is currently in progress, but can be re-used again after it ends</item>
/// <item>Enabled = <see langword="true"/>, JumpAvailable = <see langword="false"/>, PerformingJump = <see langword="false"/> | The extra jump has been consumed and cannot be used again until extra jumps are refreshed</item>
/// </list>
/// </summary>
public struct ExtraJumpState
{
	internal bool _enabled;
	internal bool _jumpAvailable;
	internal bool _performingJump;
	internal bool _disabled;

	/// <summary>
	/// Whether the extra jump can be used. This property is set by <see cref="Enable"/> and <see cref="Disable"/>.<br/>
	/// This property is automatically set to <see langword="false"/> in ResetEffects.<br/>
	/// When <see langword="false"/>, this property automatically sets <see cref="JumpAvailable"/> to <see langword="false"/> as well.<br/>
	/// If you want to forcibly disable the extra jump, use <see cref="Disable"/>.<br/>
	/// If you want to forcibly disable <b>all</b> extra jumps, using <see cref="Player.blockExtraJumps"/> is preferred.
	/// </summary>
	public bool Enabled => _enabled && !_disabled;

	/// <summary>
	/// <see langword="true"/> if the extra jump has not been consumed. Will be set to <see langword="false"/> when the extra jump starts.<br/>
	/// Setting this field to <see langword="false"/> will effectively make the game think that the player has already used this extra jump.<br/>
	/// When checking this field, make sure to check <see cref="Enabled"/> first.<br/>
	/// For a reusable jump (e.g. MultipleUseExtraJump from ExampleMod), this field should only be set to <see langword="true"/> in <see cref="ExtraJump.OnEnded(Player)"/> since <see cref="ExtraJump.Visuals(Player)"/> only runs when <see cref="Enabled"/> and <see cref="PerformingJump"/> are <see langword="true"/> and this field is <see langword="false"/>.
	/// </summary>
	public bool JumpAvailable {
		get {
			// Ensure state validity
			if (!_enabled)
				_jumpAvailable = false;
			return _jumpAvailable;
		}
		set => _jumpAvailable = value;
	}

	/// <summary>
	/// Whether any effects (e.g. spawning dusts) should be performed after consuming the extra jump, but before its duration runs out
	/// </summary>
	public bool PerformingJump {
		get {
			// Ensure state validity
			if (!_enabled)
				_performingJump = false;
			return _performingJump;
		}
	}

	/// <summary>
	/// Sets this extra jump to usable for this game tick.<br/>
	/// If you want to disable this extra jump, use <see cref="Disable"/>
	/// </summary>
	public void Enable() => _enabled = true;

	/// <summary>
	/// Forces this extra jump to be disabled for this game tick without modifying the state of <see cref="Enabled"/><br/>
	/// If you want to disable all extra jumps, using <see cref="Player.blockExtraJumps"/> is preferred.
	/// </summary>
	public void Disable() => _disabled = true;
}
