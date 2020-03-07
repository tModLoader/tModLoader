using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModDoubleJump instance represents an addition to a Player instance that handles jumping logic. A ModDoubleJump is tied to the player instance, use <see cref="Player.EnableDoubleJump"/> to enable a jump like you would enable any effect through an accessory or armor
	/// </summary>
	public class ModDoubleJump
	{
		/// <summary>
		/// The mod that added this type of ModDoubleJump.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this ModDoubleJump. Used for distinguishing between multiple ModDoubleJump added by a single Mod, in addition to the argument passed to <see cref="Player.GetModDoubleJump"/>.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The Player instance that this ModDoubleJump instance is attached to.
		/// </summary>
		public Player player {
			get;
			internal set;
		}

		internal int index;

		internal bool enable;

		internal bool again;

		/// <summary>
		/// If this jump is the currently used one by the player
		/// </summary>
		public bool IsMidJump => enable && player.activeJump == this; // No '&& !again' for compatibility with JumpAgain()

		/// <summary>
		/// If this jump is enabled, but yet to be activated
		/// </summary>
		public bool CanJump => again;

		internal ModDoubleJump CreateFor(Player newPlayer)
		{
			ModDoubleJump modDoubleJump = (ModDoubleJump)Activator.CreateInstance(GetType());
			modDoubleJump.Name = Name;
			modDoubleJump.mod = mod;
			modDoubleJump.player = newPlayer;
			modDoubleJump.index = index;
			return modDoubleJump;
		}

		public bool TypeEquals(ModDoubleJump other)
		{
			return mod == other.mod && Name == other.Name;
		}

		/// <summary>
		/// Allows you to automatically add a ModDoubleJump instead of using Mod.AddDoubleJump. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this to either force or stop an autoload, or change the name that identifies this type of ModDoubleJump.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to set the jump height multiplier, disable default double jump sound, and add visuals when this jump is initiated
		/// </summary>
		/// <param name="playSound">if left on true, will play the default double jump sound</param>
		/// <returns>Jump height multiplier</returns>
		public virtual float Jump(ref bool playSound)
		{
			return 1f;
		}

		/// <summary>
		/// Allows you to create visuals while the jump is happening
		/// </summary>
		public virtual void MidJump()
		{

		}

		/// <summary>
		/// Allows you to modify the horizontal acceleration and max speed during the jump (sandstorm uses 1.5f and 2f, blizzard uses 3f and 1.5f)
		/// </summary>
		/// <param name="runAccelerationMult">Horizontal acceleration multiplier</param>
		/// <param name="maxRunSpeedMult">Max speed multiplier</param>
		public virtual void HorizontalJumpSpeed(ref float runAccelerationMult, ref float maxRunSpeedMult)
		{

		}

		/// <summary>
		/// Allows you to decide if the same jump should be repeated. Use a <see cref="ModPlayer"/> for custom conditions
		/// </summary>
		public virtual bool JumpAgain()
		{
			return false;
		}
	}
}
