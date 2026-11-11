using Terraria.GameContent.UI.Elements;

namespace Terraria.GameContent.Bestiary;

/// <summary>
/// Allows categorizing Bestiary UI Elements into existing categories. <see cref="IBestiaryInfoElement"/> that are not vanilla Types without this interface will be placed at the bottom in the <see cref="UIBestiaryEntryInfoPage.BestiaryInfoCategory.Misc"/> category.
/// </summary>
public interface ICategorizedBestiaryInfoElement
{
	/// <summary>
	/// The category to place this element inside of, which corresponds to the order of the bestiary elements.
	/// <para/> Use <see cref="IBestiaryPrioritizedElement.OrderPriority"/> to dictate a relative ordering within a category.
	/// </summary>
	public UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory {
		get;
	}
}