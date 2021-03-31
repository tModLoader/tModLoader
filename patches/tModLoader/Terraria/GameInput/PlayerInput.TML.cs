using System.Collections.Generic;

namespace Terraria.GameInput
{
	public partial class PlayerInput
	{
		internal static List<string> MouseInModdedUI = new List<string>();
		/// <summary>
		/// Locks the vanilla scrollbar for the upcoming cycle when called. Autoclears in Player.
		/// Takes a string to denote that your UI has registered to lock vanilla scrolling. String does not need to be unique.
		/// </summary>
		public static void LockVanillaMouseScroll(string myUI) {
			if (!MouseInModdedUI.Contains(myUI))
				MouseInModdedUI.Add(myUI);
		}
	}
}
