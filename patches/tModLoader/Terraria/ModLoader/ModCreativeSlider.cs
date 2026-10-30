using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A Journey Mode power with a vertical slider, like vanilla's Rain Control or Time Speed. Clicking its button opens the slider beside the strip, and opening another slider closes it.<para/>
/// See <see cref="ModCreativePower"/> for where it appears and how it works in multiplayer.
/// </summary>
public abstract class ModCreativeSlider : ModCreativePower
{
	/// <summary>
	/// The slider's position, from 0 at the bottom to 1 at the top. Set when the player moves the slider or clicks one of its labels, if <see cref="ModCreativePower.CanUse"/> is true.
	/// </summary>
	public abstract float Value { get; set; }

	/// <summary>
	/// Shown when hovering the slider. Defaults to <see cref="Value"/> as a percentage, such as "50%".
	/// </summary>
	public virtual string ValueText => Value.ToString("P0");

	/// <summary>
	/// Shown when hovering the button while the slider is closed. Defaults to "Open <see cref="ModCreativePower.DisplayName"/>".
	/// </summary>
	public virtual LocalizedText OpenTooltip => Language.GetText("tModLoader.CreativeMenuOpen").WithFormatArgs(DisplayName);

	/// <summary>
	/// Shown when hovering the button while the slider is open. Defaults to "Close <see cref="ModCreativePower.DisplayName"/>".
	/// </summary>
	public virtual LocalizedText CloseTooltip => Language.GetText("tModLoader.CreativeMenuClose").WithFormatArgs(DisplayName);

	/// <summary>
	/// Labels shown at the top, middle and bottom of the slider, like vanilla's "Monsoon", "Drizzle" and "Clear Sky". Clicking one sets <see cref="Value"/> to 1, 0.5 or 0. None by default.
	/// </summary>
	public virtual LocalizedText TopLabel => null;

	/// <inheritdoc cref="TopLabel"/>
	public virtual LocalizedText MiddleLabel => null;

	/// <inheritdoc cref="TopLabel"/>
	public virtual LocalizedText BottomLabel => null;

	/// <summary>
	/// Creates the panel shown while the slider is open. By default this is a panel like vanilla's, holding a slider for <see cref="Value"/> and the labels.<br/>
	/// Called once each time the power menu is built, which happens each time a world is entered. The panel is placed beside the strip automatically.
	/// </summary>
	public virtual UIElement CreatePanel()
	{
		UIVerticalSlider slider = CreativePowersHelper.CreateSlider(() => Value, SetValue, () => SetValue(UILinksInitializer.HandleSliderVerticalInput(Value, 0f, 1f, PlayerInput.CurrentProfile.InterfaceDeadzoneX, 0.35f)));
		slider.OnUpdate += element => {
			if (element.IsMouseHovering)
				Main.instance.MouseTextNoOverride(ValueText);
		};

		UIPanel panel = new UIPanel {
			Width = new StyleDimension(132f, 0f),
			Height = new StyleDimension(180f, 0f),
			HAlign = 0f,
			VAlign = 0.5f
		};
		panel.Append(slider);
		panel.OnUpdate += CreativePowersHelper.UpdateUseMouseInterface;
		AddLabel(panel, TopLabel, 1f, 0f);
		AddLabel(panel, MiddleLabel, 0.5f, 0.5f);
		AddLabel(panel, BottomLabel, 0f, 1f);
		return panel;
	}

	private void SetValue(float value)
	{
		if (CanUse() && value != Value)
			Value = value;
	}

	private void AddLabel(UIPanel panel, LocalizedText label, float value, float vAlign)
	{
		if (label == null)
			return;

		UIText text = new UIText(label) {
			HAlign = 1f,
			VAlign = vAlign
		};
		text.OnUpdate += element => {
			if (element.IsMouseHovering)
				Main.instance.MouseTextNoOverride(string.Empty);
		};
		text.OnMouseOver += (_, _) => {
			text.ShadowColor = Main.OurFavoriteColor;
			SoundEngine.PlaySound(SoundID.MenuTick);
		};
		text.OnMouseOut += (_, _) => {
			text.ShadowColor = Color.Black;
			SoundEngine.PlaySound(SoundID.MenuTick);
		};
		text.OnLeftClick += (_, _) => {
			SetValue(value);
			SoundEngine.PlaySound(SoundID.MenuTick);
		};
		panel.Append(text);
	}

	internal GroupOptionButton<int> CreateButton(CreativePowerUIElementRequestInfo request, int option, int currentOption)
	{
		GroupOptionButton<int> button = CreativePowersHelper.CreateCategoryButton(request, option, currentOption);
		AddIcon(button);
		button.OnUpdate += element => {
			UpdateButton(button);
			if (element.IsMouseHovering)
				ShowHoverText(button.IsSelected ? CloseTooltip : OpenTooltip);
		};
		return button;
	}
}
