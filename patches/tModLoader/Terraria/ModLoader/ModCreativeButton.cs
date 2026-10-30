using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A Journey Mode power that does something once each time it's clicked, like vanilla's Change Time to Dawn.<para/>
/// See <see cref="ModCreativePower"/> for where it appears and how it works in multiplayer.
/// </summary>
public abstract class ModCreativeButton : ModCreativePower
{
	/// <summary>
	/// Called when the button is clicked and <see cref="ModCreativePower.CanUse"/> is true.
	/// </summary>
	public abstract void OnClick();

	internal UIElement CreateButton(CreativePowerUIElementRequestInfo request)
	{
		GroupOptionButton<bool> button = CreativePowersHelper.CreateSimpleButton(request);
		AddIcon(button);
		button.OnLeftClick += (_, _) => {
			if (CanUse())
				OnClick();
		};
		button.OnUpdate += element => {
			UpdateButton(button);
			if (element.IsMouseHovering)
				ShowHoverText(DisplayName);
		};
		return button;
	}
}
