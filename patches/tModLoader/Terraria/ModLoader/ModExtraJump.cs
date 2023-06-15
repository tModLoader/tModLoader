﻿using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Graphics.Shaders;

namespace Terraria.ModLoader;

public abstract class ModExtraJump : ModType
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

	protected sealed override void Register()
	{
		ModTypeLookup<ModExtraJump>.Register(this);
		Type = ExtraJumpLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

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
	/// Vanilla jumps use the following values:
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

	// TODO: OnJumpEnded

	/// <summary>
	/// Modify the player's horizontal movement while performing this extra jump here.<br/>
	/// For example, the Sandstorm in a Bottle multiplies <see cref="Player.runAcceleration"/> by 1.5x and <see cref="Player.maxRunSpeed"/> by 2x.
	/// </summary>
	/// <param name="player">The player performing the jump</param>
	public virtual void ModifyHorizontalSpeeds(Player player) { }
}
