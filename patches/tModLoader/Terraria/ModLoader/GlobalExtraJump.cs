using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// <see cref="GlobalExtraJump"/> is a singleton type used to facilitate modifying or overwriting logic from <see cref="ExtraJump"/>
/// </summary>
public abstract class GlobalExtraJump : ModType
{
	/// <summary>
	/// The internal ID of this <see cref="GlobalExtraJump"/>.
	/// </summary>
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		ModTypeLookup<GlobalExtraJump>.Register(this);
		Type = ExtraJumpLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public override string ToString() => Name;

	/// <summary>
	/// Use this hook to modify the jump duration from an extra jump.<br/>
	/// Vanilla's extra jumps use the following values:
	/// <para>
	/// Basilisk mount: 0.75<br/>
	/// Blizzard in a Bottle: 1.5<br/>
	/// Cloud in a Bottle: 0.75<br/>
	/// Fart in a Jar: 2<br/>
	/// Goat mount: 2<br/>
	/// Sandstorm in a Bottle: 3<br/>
	/// Santank mount: 2<br/>
	/// Tsunami in a Bottle: 1.25<br/>
	/// Unicorn mount: 2
	/// </para>
	/// </summary>
	/// <param name="jump">The jump being performed</param>
	/// <param name="player">The player performing the jump</param>
	/// <param name="duration">A modifier to the player's jump height, which when combined effectively acts as the duration for the extra jump</param>
	public virtual void ModifyJumpDuration(ExtraJump jump, Player player, ref float duration) { }

	/// <summary>
	/// Effects that should appear when the extra jump starts should happen here.<br/>
	/// For example, the Cloud in a Bottle's initial puff of smoke is spawned here.
	/// </summary>
	/// <param name="jump">The jump being performed</param>
	/// <param name="player">The player performing the jump</param>
	/// <param name="playSound">Whether the poof sound should play.  Set this parameter to <see langword="false"/> if you want to play a different sound.</param>
	public virtual void OnJumpStarted(ExtraJump jump, Player player, ref bool playSound) { }

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpData.PerformingJump"/> flag for an extra jump is set from <see langword="true"/> to <see langword="false"/> in <see cref="Player.CancelAllJumpVisualEffects"/><br/>
	/// This occurs when a grappling hook is thrown, the player grabs onto a rope, the jump's duration has finished and when the player's frozen, turned to stone or webbed.
	/// </summary>
	/// <param name="jump">The jump that was performed</param>
	/// <param name="player">The player that was performing the jump</param>
	public virtual void OnJumpEnded(ExtraJump jump, Player player) { }

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpData.JumpAvailable"/> flag for an extra jump is set to <see langword="true"/> in <see cref="Player.RefreshDoubleJumps"/><br/>
	/// This occurs at the start of the grounded jump and while the player is grounded.
	/// </summary>
	/// <param name="jump">The jump instance</param>
	/// <param name="player">The player instance</param>
	public virtual void OnJumpRefreshed(ExtraJump jump, Player player) { }
}
