using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// <see cref="ExtraJump"/> is a singleton, defining the properties and behaviour of midair extra jumps.<br/>
/// Fields defining the state of a jump per player are stored in <see cref="ExtraJumpState"/>
/// </summary>
public abstract partial class ExtraJump : ModType
{
	public static ExtraJump Flipper { get; private set; } = new FlipperJump();

	public static ExtraJump BasiliskMount { get; private set; } = new BasiliskMountJump();

	public static ExtraJump GoatMount { get; private set; } = new GoatMountJump();

	public static ExtraJump SantankMount { get; private set; } = new SantankMountJump();

	public static ExtraJump UnicornMount { get; private set; } = new UnicornMountJump();

	public static ExtraJump SandstormInABottle { get; private set; } = new SandstormInABottleJump();

	public static ExtraJump BlizzardInABottle { get; private set; } = new BlizzardInABottleJump();

	public static ExtraJump FartInAJar { get; private set; } = new FartInAJarJump();

	public static ExtraJump TsunamiInABottle { get; private set; } = new TsunamiInABottleJump();

	public static ExtraJump CloudInABottle { get; private set; } = new CloudInABottleJump();

	/// <summary>
	/// The internal ID of this <see cref="ExtraJump"/>.
	/// </summary>
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		ModTypeLookup<ExtraJump>.Register(this);
		Type = ExtraJumpLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public override string ToString() => Name;

	/// <summary>
	/// Returns this extra jump's default position in regard to the vanilla extra jumps.  Make use of e.g. <see cref="Before"/>/<see cref="After"/>, and provide an extra jump.<br/><br/>
	///
	/// Recommended using one of: <c>BeforeMountJumps, MountJumpPosition, BeforeBottleJumps, AfterBottleJumps</c><br/><br/>
	/// 
	/// <b>NOTE:</b> The position must specify a vanilla <see cref="ExtraJump"/> otherwise an exception will be thrown.
	/// </summary>
	public abstract Position GetDefaultPosition();

	/// <summary>
	/// Modded jumps are placed between vanilla jumps via <see cref="GetDefaultPosition"/> and, by default, are sorted in load order.<br/>
	/// This hook allows you to sort this jump before/after other modded jumps that were placed between the same two vanilla jumps.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;SimpleExtraJump&gt;());</c>
	/// </para>
	/// By default, this hook returns <see langword="null"/>, which indicates that this jump has no modded ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position> GetModdedConstraints() => null;

	/// <summary>
	/// Spawn effects that should appear while the player is performing this jump here.<br/>
	/// Only runs while the jump is <see cref="ExtraJumpState.Active"/> <br/>
	/// For example, the Sandstorm in a Bottle's dusts are spawned here.
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	public virtual void ShowVisuals(Player player) { }

	/// <summary>
	/// Return <see langword="false"/> to prevent <see cref="ShowVisuals(Player)"/> from executing.<br/>
	/// By default, this hook returns whether the player is moving upwards with respect to <see cref="Player.gravDir"/>
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	public virtual bool CanShowVisuals(Player player) {
		return (player.gravDir == 1f && player.velocity.Y < 0f) || (player.gravDir == -1f && player.velocity.Y > 0f);
	}

	/// <summary>
	/// Vanilla's jumps use the following values:
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
	/// <param name="player">The player performing the jump</param>
	/// <returns>A modifier to the player's jump height, which when combined effectively acts as the duration for the jump</returns>
	public abstract float GetDurationMultiplier(Player player);

	/// <summary>
	/// An extra condition for whether this extra jump can be started.  Used by vanilla for flippers (<see cref="Entity.wet"/>).  Returns <see langword="true"/> by default.
	/// </summary>
	/// <param name="player">The player that would perform the jump</param>
	/// <returns><see langword="true"/> to let the jump be started, <see langword="false"/> otherwise.</returns>
	public virtual bool CanStart(Player player) => true;

	/// <summary>
	/// This hook runs when the player uses this jump via pressing the jump key<br/>
	/// Effects that should appear when the jump starts can be spawned here.<br/>
	/// For example, the Cloud in a Bottle's initial puff of smoke is spawned here.<br/>
	/// <br/>
	/// To make the jump re-usable, set <see cref="ExtraJumpState.Available"/> to  <see langword="true"/> <br/>
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	/// <param name="playSound">Whether the poof sound should play.  Set this parameter to <see langword="false"/> if you want to play a different sound.</param>
	public virtual void OnStarted(Player player, ref bool playSound) { }

	/// <summary>
	/// This hook runs before <see cref="ExtraJumpState.Active"/> is set from <see langword="true"/> to <see langword="false"/><br/>
	/// Jumps end when their duration expires or when <see cref="ExtraJumpState.Enabled"/> is no longer true. <br/>
	/// Jumps may end early via <see cref="Player.StopExtraJumpInProgress"/>, called when a grappling hook is thrown, the player grabs onto a rope, or when the player is frozen, turned to stone or webbed.
	/// </summary>
	/// <param name="player">The player that was performing the jump</param>
	public virtual void OnEnded(Player player) { }

	/// <summary>
	/// Modify the player's horizontal movement while performing this jump here.<br/>
	/// Only runs while the jump is <see cref="ExtraJumpState.Active"/> <br/>
	/// <br/>
	/// Vanilla's jumps use the following values:
	/// <para>
	/// Basilisk mount: runAcceleration *= 3; maxRunSpeed *= 1.5;<br/>
	/// Blizzard in a Bottle: runAcceleration *= 3; maxRunSpeed *= 1.5;<br/>
	/// Cloud in a Bottle: no change<br/>
	/// Fart in a Jar: runAcceleration *= 3; maxRunSpeed *= 1.75;<br/>
	/// Goat mount: runAcceleration *= 3; maxRunSpeed *= 1.5;<br/>
	/// Sandstorm in a Bottle: runAcceleration *= 1.5; maxRunSpeed *= 2;<br/>
	/// Santank mount: runAcceleration *= 3; maxRunSpeed *= 1.5;<br/>
	/// Tsunami in a Bottle: runAcceleration *= 1.5; maxRunSpeed *= 1.25;<br/>
	/// Unicorn mount: runAcceleration *= 3; maxRunSpeed *= 1.5;
	/// </para>
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	public virtual void UpdateHorizontalSpeeds(Player player) { }

	/// <summary>
	/// This hook runs before <see cref="ExtraJumpState.Available"/> is set to <see langword="true"/> in <see cref="Player.RefreshDoubleJumps"/><br/>
	/// This occurs at the start of the grounded jump and while the player is grounded, or when jumping off a grappling hook/rope.
	/// </summary>
	/// <param name="player">The player instance</param>
	public virtual void OnRefreshed(Player player) { }
}
