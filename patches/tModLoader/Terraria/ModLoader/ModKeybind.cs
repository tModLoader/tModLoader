using System.Collections.Generic;
using Terraria.GameInput;

// Thanks to Yoraiz0r for the useful Properties approach.

namespace Terraria.ModLoader
{
	/// <summary>
	/// Represents a loaded input binding. It is suggested to access the keybind status only in ModPlayer.ProcessTriggers. 
	/// </summary>
	public class ModKeybind
	{
		internal readonly Mod mod;
		internal readonly string name; // name from modder: "Random Buff"
		internal readonly string uniqueName; // eg: "Example Mod: Random Buff" (currently also display name)
		internal readonly string defaultBinding; // from mod.Load

		internal ModKeybind(Mod mod, string name, string defaultBinding) {
			this.mod = mod;
			this.name = name;
			this.defaultBinding = defaultBinding;
			
			uniqueName = mod.Name + ": " + name;
		}

		/// <summary>
		/// Gets the currently assigned keybindings. Useful for prompts, tooltips, informing users.
		/// </summary>
		/// <param name="mode"> The InputMode. Choose between InputMode.Keyboard and InputMode.XBoxGamepad </param>
		public List<string> GetAssignedKeys(InputMode mode = InputMode.Keyboard) {
			return PlayerInput.CurrentProfile.InputModes[mode].KeyStatus[uniqueName];
		}

		public bool RetroCurrent {
			get {
				if (Main.drawingPlayerChat || Main.player[Main.myPlayer].talkNPC != -1 || Main.player[Main.myPlayer].sign != -1)
					return false;

				return Current;
			}
		}

		/// <summary>
		/// Returns true if this keybind is pressed currently. Useful for creating a behavior that relies on the keybind being held down.
		/// </summary>
		public bool Current => PlayerInput.Triggers.Current.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this keybind has just been pressed this update. This is a fire-once-per-press behavior.
		/// </summary>
		public bool JustPressed => PlayerInput.Triggers.JustPressed.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this keybind has just been released this update.
		/// </summary>
		public bool JustReleased => PlayerInput.Triggers.JustReleased.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this keybind has been pressed during the previous update.
		/// </summary>
		public bool Old => PlayerInput.Triggers.Old.KeyStatus[uniqueName];
	}
}
