using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A class that is used to modify existing building toggles (i.e. the ruler line and mechanical ruler toggles).
	/// All vanilla toggles can be accessed using BuilderToggle.(name of item).
	/// </summary>
	public class GlobalBuilderToggle : ModType
	{
		protected sealed override void Register() => BuilderToggleLoader.AddGlobalBuilderToggles(this);

		public override void SetupContent() { }

		/// <summary>
		/// Allows you to modify whether or not a given BuilderToggle is active (displayed). Returns null (no change from default behavior) by default for all BuilderToggles.
		/// </summary>
		/// <param name="currentDisplay">The builder toggle you're deciding the active state for.</param>
		public virtual bool? Active(BuilderToggle builderToggle) => null;

		/// <summary>
		/// Allows you to modify the display value (the text displayed next to the icon) of an InfoDisplay.
		/// </summary>
		/// <param name="builderToggle">The builder toggle you're modifying the display value for.</param>
		/// <param name="displayValue">The value the builder toggle has</param>
		public virtual void ModifyDisplayValue(BuilderToggle builderToggle, ref string displayValue) { }
	}
}