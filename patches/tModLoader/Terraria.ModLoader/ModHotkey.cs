using System.Collections.Generic;
using Terraria.GameInput;

// Thanks to Yoraiz0r for the useful Properties approach.

namespace Terraria.ModLoader
{
	/// <summary>
	/// Represents a loaded hotkey. It is suggested to access the hotkey status only in ModPlayer.ProcessTriggers. 
	/// </summary>
	public class ModHotKey
	{
		internal readonly Mod mod;
		internal readonly string name; // name from modder: "Random Buff"
		internal readonly string uniqueName; // eg: "Example Mod: Random Buff" (currently also display name)
		internal readonly string defaultKey; // from mod.Load

		internal ModHotKey(Mod mod, string name, string defaultKey) {
			this.mod = mod;
			this.name = name;
			this.defaultKey = defaultKey;
			this.uniqueName = mod.Name + ": " + name;
		}

		/// <summary>
		/// Gets the currently assigned keybindings. Useful for prompts, tooltips, informing users.
		/// </summary>
		/// <param name="mode">The InputMode. Choose between InputMode.Keyboard and InputMode.XBoxGamepad</param>
		/// <returns></returns>
		public List<string> GetAssignedKeys(InputMode mode = InputMode.Keyboard) {
			return PlayerInput.CurrentProfile.InputModes[mode].KeyStatus[uniqueName];
		}

		public bool RetroCurrent {
			get {
				if (Main.drawingPlayerChat || Main.player[Main.myPlayer].talkNPC != -1 || Main.player[Main.myPlayer].sign != -1) return false;
				return Current;
			}
		}

		/// <summary>
		/// Returns true if this hotkey is pressed currently. Useful for createing a behavior that relies on the hotkey being held down.
		/// </summary>
		public bool Current => PlayerInput.Triggers.Current.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this hotkey was just released this update. This is a fire-once-per-press behavior.
		/// </summary>
		public bool JustPressed => PlayerInput.Triggers.JustPressed.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this hotkey was just released this update.
		/// </summary>
		public bool JustReleased => PlayerInput.Triggers.JustReleased.KeyStatus[uniqueName];

		/// <summary>
		/// Returns true if this hotkey was pressed the previous update.
		/// </summary>
		public bool Old => PlayerInput.Triggers.Old.KeyStatus[uniqueName];
	}
}
