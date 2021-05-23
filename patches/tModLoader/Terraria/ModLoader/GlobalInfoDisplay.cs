using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A class that is used to modify existing info displays (i.e. the things that the Cell Phone uses to communicate information).
	/// All vanilla displays can be accessed using InfoDisplay.(name of item).
	/// </summary>
	public abstract class GlobalInfoDisplay : ModType
	{
		protected sealed override void Register() => InfoDisplayLoader.AddGlobalInfoDisplay(this);

		/// <summary>
		/// Allows you to modify whether or not a given InfoDisplay is active. Returns null (no change from default behavior) by default for all InfoDisplays.
		/// </summary>
		/// <param name="currentDisplay">The display you're deciding the active state for.</param>
		public virtual bool? Active(InfoDisplay currentDisplay) => null;

		/// <summary>
		/// Allows you to modify the display name of an InfoDisplay (shown when hovering over said display in-game).
		/// </summary>
		/// <param name="currentDisplay">The display you're modifying the display name for.</param>
		/// <param name="displayName">The display name of the current display.</param>
		public virtual void ModifyDisplayName(InfoDisplay currentDisplay, ref string displayName) { }

		/// <summary>
		/// Allows you to modify the display value (the text displayed next to the icon) of an InfoDisplay.
		/// </summary>
		/// <param name="currentDisplay">The display you're modifying the display value for.</param>
		/// <param name="displayValue">The</param>
		public virtual void ModifyDisplayValue(InfoDisplay currentDisplay, ref string displayValue) { }
	}
}
