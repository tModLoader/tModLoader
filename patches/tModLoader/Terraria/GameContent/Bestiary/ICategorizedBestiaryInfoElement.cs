using Terraria.GameContent.UI.Elements;

namespace Terraria.GameContent.Bestiary;

/// <summary>
/// Additional interface added by tML that allows modders to categorize their Bestiary UI Elements with vanilla's elements, instead of forcing modded ones to the very bottom of an given bestiary
/// entry.
/// </summary>
public interface ICategorizedBestiaryInfoElement
{
	/// <summary>
	/// The category to place this element inside of.
	/// </summary>
	public UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory {
		get;
	}
}