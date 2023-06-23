﻿using Terraria.ModLoader;

namespace Terraria.DataStructures;

/// <summary>
/// A structure containing fields used to manage extra jumps
/// </summary>
public struct ExtraJumpState
{
	/// <summary>
	/// Whether the extra jump can be used. This field is what should be set by equipment in UpdateEquip or UpdateAccessory.<br/>
	/// This field is automatically set to <see langword="false"/> in ResetEffects.<br/>
	/// When set to <see langword="false"/>, this field does not cause <see cref="JumpAvailable"/> to be automatically set to <see langword="false"/> until the next game tick.<br/>
	/// If you want to forcibly disable the extra jump, use <see cref="Disable"/> instead of setting this field to <see langword="false"/>.<br/>
	/// If you want to forcibly disable <b>all</b> extra jumps, using <see cref="Player.blockExtraJumps"/> is preferred.
	/// </summary>
	public bool Enabled;
	/// <summary>
	/// <see langword="true"/> if the extra jump has not been consumed. Will be set to <see langword="false"/> when the extra jump starts.<br/>
	/// Setting this field to <see langword="false"/> will effectively make the game think that the player has already used this extra jump.<br/>
	/// When checking this field, make sure to check <see cref="Enabled"/> first.<br/>
	/// For a reusable jump (e.g. MultipleUseExtraJump from ExampleMod), this field should only be set to <see langword="true"/> in <see cref="ExtraJump.OnEnded(Player)"/> since <see cref="ExtraJump.Visuals(Player)"/> only runs when <see cref="Enabled"/> and <see cref="PerformingJump"/> are true and this field is <see langword="false"/>.
	/// </summary>
	public bool JumpAvailable;
	/// <summary>
	/// Whether any effects (e.g. spawning dusts) should be performed after consuming the extra jump, but before its duration runs out
	/// </summary>
	public bool PerformingJump;

	// Fields/properties for overriding state

	internal bool _disabled;

	/// <summary>
	/// Forces this extra jump to be disabled for this game tick without modifying the state of <see cref="Enabled"/><br/>
	/// If you want to disable all extra jumps, using <see cref="Player.blockExtraJumps"/> is preferred.
	/// </summary>
	public void Disable() => _disabled = true;
}
