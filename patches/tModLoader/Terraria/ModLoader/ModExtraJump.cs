﻿using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// <see cref="ModExtraJump"/> is a singleton type used to facilitate the logic for extra midair jumps
/// </summary>
public abstract partial class ModExtraJump : ModType
{
	public static ModExtraJump GoatMount { get; private set; } = new GoatMountJump();

	public static ModExtraJump BasiliskMount { get; private set; } = new BasiliskMountJump();

	public static ModExtraJump SantankMount { get; private set; } = new SantankMountJump();

	public static ModExtraJump UnicornMount { get; private set; } = new UnicornMountJump();

	public static ModExtraJump SandstormInABottle { get; private set; } = new SandstormInABottleJump();

	public static ModExtraJump BlizzardInABottle { get; private set; } = new BlizzardInABottleJump();

	public static ModExtraJump FartInAJar { get; private set; } = new FartInAJarJump();

	public static ModExtraJump TsunamiInABottle { get; private set; } = new TsunamiInABottleJump();

	public static ModExtraJump CloudInABottle { get; private set; } = new CloudInABottleJump();

	/// <summary>
	/// The internal ID of this <see cref="ModExtraJump"/>.
	/// </summary>
	public int Type { get; internal set; }

	/// <summary>
	/// Whether this extra jump should ignore the vanilla checks for swimming with the Flippers equipped or swimming with the Slime mount
	/// </summary>
	public virtual bool IgnoresSwimmingChecks => false;

	protected sealed override void Register()
	{
		ModTypeLookup<ModExtraJump>.Register(this);
		Type = ExtraJumpLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public override string ToString() => Name;

	/// <summary>
	/// Returns this extra jump's default position in regard to the vanilla extra jumps.  Make use of e.g. <see cref="Before"/>/<see cref="After"/>, and provide an extra jump
	/// </summary>
	public abstract Position GetDefaultPosition();

	/// <summary>
	/// Returns additional constraints for this extra jump's position in regard to other modded extra jumps
	/// </summary>
	public virtual IEnumerable<Position> GetModdedConstraints()
	{
		yield break;
	}

	internal void PerformJump(Player player) {
		// Set velocity and jump duration
		player.velocity.Y = -Player.jumpSpeed * player.gravDir;
		player.jump = (int)(Player.jumpHeight * GetJumpDuration(player));

		bool playSound = true;
		OnJumpStarted(player, ref playSound);

		if (playSound)
			SoundEngine.PlaySound(16, (int)player.position.X, (int)player.position.Y);
	}

	/// <summary>
	/// Effects that should appear while the player is performing this extra jump should happen here.<br/>
	/// For example, the Sandstorm in a Bottle's dusts are spawned here.
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	public virtual void JumpVisuals(Player player) { }

	/// <summary>
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
	/// <param name="player">The player performing the jump</param>
	/// <returns>A modifier to the player's jump height, which when combined effectively acts as the duration for the extra jump</returns>
	public abstract float GetJumpDuration(Player player);

	/// <summary>
	/// Effects that should appear when the extra jump starts should happen here.<br/>
	/// For example, the Cloud in a Bottle's initial puff of smoke is spawned here.
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	/// <param name="playSound">Whether the poof sound should play.  Set this parameter to <see langword="false"/> if you want to play a different sound.</param>
	public virtual void OnJumpStarted(Player player, ref bool playSound) { }

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpData.PerformingJump"/> flag for this extra jump is set from <see langword="true"/> to <see langword="false"/> in <see cref="Player.CancelAllJumpVisualEffects"/><br/>
	/// This occurs when a grappling hook is thrown, the player grabs onto a rope, the jump's duration has finished and when the player's frozen, turned to stone or webbed.
	/// </summary>
	/// <param name="player">The player that was performing the jump</param>
	public virtual void OnJumpEnded(Player player) { }

	/// <summary>
	/// Modify the player's horizontal movement while performing this extra jump here.<br/>
	/// Vanilla's extra jumps use the following values:
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
	public virtual void ModifyHorizontalSpeeds(Player player) { }

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpData.JumpAvailable"/> flag for this extra jump is set from <see langword="false"/> to <see langword="true"/> in <see cref="Player.RefreshDoubleJumps"/><br/>
	/// This occurs at the start of the grounded jump and while the player is grounded.
	/// </summary>
	/// <param name="player">The player instance</param>
	public virtual void OnJumpRefreshed(Player player) { }
}
