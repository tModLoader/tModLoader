using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

/// <summary>
/// <see cref="UIText"/> that will scroll text horizontally if it is too large to fit to avoid overflowing issues while keeping text readable.
/// </summary>
public class MarqueeText : UIElement
{
	// TODO: make SetText take an object, then set the text for a random UIText on recalculating
	// - avoid directly rendering text by myself
}