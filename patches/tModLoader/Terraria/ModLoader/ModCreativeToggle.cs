using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A Journey Mode power that's turned on and off by clicking it, like vanilla's God Mode or Freeze Time. Its button is highlighted while <see cref="IsActive"/> is true.<para/>
/// See <see cref="ModCreativePower"/> for where it appears and how it works in multiplayer.
/// </summary>
public abstract class ModCreativeToggle : ModCreativePower
{
	/// <summary>
	/// Whether this power is currently on. Checked every frame while its button is shown.
	/// </summary>
	public abstract bool IsActive { get; }

	/// <summary>
	/// Shown when hovering the button while <see cref="IsActive"/> is true. Defaults to "<see cref="ModCreativePower.DisplayName"/> Enabled".
	/// </summary>
	public virtual LocalizedText EnabledTooltip => Language.GetText("tModLoader.CreativePowerEnabled").WithFormatArgs(DisplayName);

	/// <summary>
	/// Shown when hovering the button while <see cref="IsActive"/> is false. Defaults to "<see cref="ModCreativePower.DisplayName"/> Disabled".
	/// </summary>
	public virtual LocalizedText DisabledTooltip => Language.GetText("tModLoader.CreativePowerDisabled").WithFormatArgs(DisplayName);

	/// <summary>
	/// Called when the button is clicked and <see cref="ModCreativePower.CanUse"/> is true. Turn the power on or off here.
	/// </summary>
	public abstract void OnClick();

	internal UIElement CreateButton(CreativePowerUIElementRequestInfo request)
	{
		GroupOptionButton<bool> button = CreativePowersHelper.CreateToggleButton(request);
		AddIcon(button);
		button.OnLeftClick += (_, _) => {
			if (CanUse())
				OnClick();
		};
		button.OnUpdate += element => {
			button.SetCurrentOption(IsActive);
			UpdateButton(button);
			if (element.IsMouseHovering)
				ShowHoverText(IsActive ? EnabledTooltip : DisabledTooltip);
		};
		return button;
	}
}
