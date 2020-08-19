using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModExtraJump instance represents an addition to a Player instance that handles jumping logic. A ModExtraJump is tied to the player instance, use <see cref="Player.EnableExtraJump"/> to hasJumpOption a jump like you would hasJumpOption any effect through an accessory or armor
	/// </summary>
	public class ModExtraJump : ModType
	{
		/// <summary>
		/// The Player instance that this ModExtraJump instance is attached to.
		/// </summary>
		public Player player {
			get;
			internal set;
		}

		internal int index;

		internal bool hasJumpOption;

		internal bool canJumpAgain;

		public bool isPerformingJump;

		internal ModExtraJump CreateFor(Player newPlayer)
		{
			ModExtraJump ModExtraJump = (ModExtraJump)Activator.CreateInstance(GetType());
			ModExtraJump.Mod = Mod;
			ModExtraJump.player = newPlayer;
			ModExtraJump.index = index;
			return ModExtraJump;
		}

		protected sealed override void Register() {
			Mod.extraJumps[Name] = this;
			ModExtraJumpLoader.Add(this);
			ContentInstance.Register(this);
		}

		public bool TypeEquals(ModExtraJump other) => Mod == other.Mod && Name == other.Name;

		/// <summary>
		/// Allows you to set the jump height multiplier, disable default double jump sound, and add visuals when this jump is initiated.
		/// </summary>
		/// <param name="jumpHeight">Jump height multiplier, 1f by default</param>
		/// <param name="playSound">if left on true, will play the default double jump sound</param>
		public virtual void Jump(ref float jumpHeight, ref bool playSound)
		{

		}

		/// <summary>
		/// Allows you to create visuals while the jump is happening.
		/// </summary>
		public virtual void PerformingJump()
		{

		}

		/// <summary>
		/// Allows you to modify the horizontal acceleration and max speed during the jump (e.g. sandstorm uses 1.5f and 2f, blizzard uses 3f and 1.5f).
		/// </summary>
		/// <param name="runAccelerationMult">Horizontal acceleration multiplier</param>
		/// <param name="maxRunSpeedMult">Max speed multiplier</param>
		public virtual void HorizontalJumpSpeed(ref float runAccelerationMult, ref float maxRunSpeedMult)
		{

		}

		/// <summary>
		/// Allows you to decide if the same jump should be repeated.
		/// </summary>
		public virtual bool CanJumpAgain()
		{
			return false;
		}
	}
}
