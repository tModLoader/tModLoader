using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.GameContent.Creative;

/// <summary>
/// Used by <see cref="ModLoader.ModSystem.ModifyCreativePowersMenu"/> to add custom categories and buttons to the Journey Mode powers menu.
/// </summary>
public sealed class CreativePowerMenuEntries
{
	private readonly List<CreativePowerMenuCategory> categories = new();
	private readonly List<Func<CreativePowerUIElementRequestInfo, UIElement>> mainButtons = new();

	public IReadOnlyList<CreativePowerMenuCategory> Categories => categories;
	public IReadOnlyList<Func<CreativePowerUIElementRequestInfo, UIElement>> MainButtons => mainButtons;

	/// <summary>
	/// Adds a top-level toggle button directly to the Journey Mode powers menu.
	/// </summary>
	/// <param name="nameKey">The localization key used for the tooltip. "Enabled", "Disabled", and "Description" suffixes will be used when present.</param>
	/// <param name="iconLocation">The icon frame in the Journey Mode powers icon sheet.</param>
	/// <param name="onClick">The action to run when the button is clicked.</param>
	/// <param name="isEnabled">Whether the button can currently be clicked. Defaults to true.</param>
	/// <param name="isSelected">Whether the button should currently appear selected.</param>
	public void AddToggle(string nameKey, Point iconLocation, Action onClick, Func<bool> isSelected, Func<bool> isEnabled = null)
	{
		ArgumentNullException.ThrowIfNull(nameKey);
		ArgumentNullException.ThrowIfNull(onClick);
		ArgumentNullException.ThrowIfNull(isSelected);

		mainButtons.Add(info => CreativePowerMenuCategory.CreateButton(info, nameKey, iconLocation, onClick, isEnabled, isSelected));
	}

	/// <summary>
	/// Adds a new parent category button to the Journey Mode powers menu.
	/// </summary>
	/// <param name="nameKey">The localization key used for the tooltip. "Opened", "Closed", and "Description" suffixes will be used when present.</param>
	/// <param name="iconLocation">The icon frame in the Journey Mode powers icon sheet.</param>
	/// <param name="sortOrder">Lower values appear first. Vanilla categories use their fixed positions, then modded categories are ordered by this value.</param>
	public CreativePowerMenuCategory AddCategory(string nameKey, Point iconLocation, int sortOrder = 0)
	{
		ArgumentNullException.ThrowIfNull(nameKey);

		CreativePowerMenuCategory category = new(nameKey, iconLocation, sortOrder);
		categories.Add(category);
		return category;
	}
}

/// <summary>
/// A custom parent category in the Journey Mode powers menu.
/// </summary>
public sealed class CreativePowerMenuCategory
{
	private readonly List<CreativePowerMenuEntry> elements = new();

	internal CreativePowerMenuCategory(string nameKey, Point iconLocation, int sortOrder)
	{
		NameKey = nameKey;
		IconLocation = iconLocation;
		SortOrder = sortOrder;
	}

	public string NameKey { get; }
	public Point IconLocation { get; }
	public int SortOrder { get; }
	public IReadOnlyList<CreativePowerMenuEntry> Elements => elements;

	/// <summary>
	/// Adds a child button to this category.
	/// </summary>
	/// <param name="nameKey">The localization key used for the tooltip. "Enabled", "Disabled", and "Description" suffixes will be used when present.</param>
	/// <param name="iconLocation">The icon frame in the Journey Mode powers icon sheet.</param>
	/// <param name="onClick">The action to run when the button is clicked.</param>
	/// <param name="isEnabled">Whether the button can currently be clicked. Defaults to true.</param>
	/// <param name="isSelected">Whether the button should currently appear selected. Defaults to false.</param>
	public CreativePowerMenuCategory AddButton(string nameKey, Point iconLocation, Action onClick, Func<bool> isEnabled = null, Func<bool> isSelected = null)
	{
		ArgumentNullException.ThrowIfNull(nameKey);
		ArgumentNullException.ThrowIfNull(onClick);

		elements.Add(new CreativePowerMenuEntry(nameKey, info => CreateButton(info, nameKey, iconLocation, onClick, isEnabled, isSelected), null));
		return this;
	}

	/// <summary>
	/// Adds a child button that opens a vertical slider panel to the right of this category.
	/// </summary>
	/// <param name="nameKey">The localization key used for the child button, tooltip, and labels. "Opened", "Closed", "Description", "Top", "Middle", and "Bottom" suffixes will be used when present.</param>
	/// <param name="iconLocation">The icon frame in the Journey Mode powers icon sheet.</param>
	/// <param name="getValue">Returns the current slider value, from 0f to 1f.</param>
	/// <param name="setValue">Sets the slider value, from 0f to 1f.</param>
	/// <param name="configure">Allows customizing the panel size, colors, hover text, and preset labels.</param>
	public CreativePowerMenuCategory AddSlider(string nameKey, Point iconLocation, Func<float> getValue, Action<float> setValue, Action<CreativePowerMenuSliderSettings> configure = null)
	{
		ArgumentNullException.ThrowIfNull(nameKey);
		ArgumentNullException.ThrowIfNull(getValue);
		ArgumentNullException.ThrowIfNull(setValue);

		CreativePowerMenuSliderSettings settings = CreativePowerMenuSliderSettings.Default(nameKey);
		configure?.Invoke(settings);

		elements.Add(new CreativePowerMenuEntry(
			nameKey,
			(info, optionValue) => CreateSliderButton(info, optionValue, nameKey, iconLocation),
			() => CreateSlider(settings, getValue, setValue)
		));
		return this;
	}

	/// <summary>
	/// Adds a custom child button to this category. Use this for controls that do not fit the simple button and slider helpers.
	/// </summary>
	public CreativePowerMenuCategory AddElement(string nameKey, Func<CreativePowerUIElementRequestInfo, UIElement> createElement, Func<UIElement> createPanel = null)
	{
		ArgumentNullException.ThrowIfNull(nameKey);
		ArgumentNullException.ThrowIfNull(createElement);

		elements.Add(new CreativePowerMenuEntry(nameKey, createElement, createPanel));
		return this;
	}

	internal static UIElement CreateButton(CreativePowerUIElementRequestInfo info, string nameKey, Point iconLocation, Action onClick, Func<bool> isEnabled, Func<bool> isSelected)
	{
		GroupOptionButton<bool> button = CreativePowersHelper.CreateSimpleButton(info);
		button.SetColorsBasedOnSelectionState(Main.OurFavoriteColor, Colors.InventoryDefaultColor, 1f, 0.7f);
		button.Append(CreativePowersHelper.GetIconImage(iconLocation));
		button.OnLeftClick += (_, _) => {
			if (isEnabled?.Invoke() ?? true) {
				SoundEngine.PlaySound(12);
				onClick();
			}
		};
		button.OnUpdate += element => {
			bool selected = isSelected?.Invoke() ?? false;
			button.SetCurrentOption(selected);
			button.SetBorderColor(isEnabled?.Invoke() ?? true ? Color.White : Color.DimGray);

			if (element.IsMouseHovering) {
				string suffix = selected ? "Enabled" : "Disabled";
				string text = GetText(nameKey, suffix, nameKey);

				AddDescriptionIfNeeded(ref text, nameKey, "Description");
				Main.instance.MouseTextNoOverride(text, 0, 0);
			}
		};
		return button;
	}

	private static UIElement CreateSliderButton(CreativePowerUIElementRequestInfo info, int optionValue, string nameKey, Point iconLocation)
	{
		GroupOptionButton<int> button = CreativePowersHelper.CreateCategoryButton(info, optionValue, 0);
		button.Append(CreativePowersHelper.GetIconImage(iconLocation));
		button.OnUpdate += element => {
			if (element.IsMouseHovering) {
				string text = GetText(nameKey, button.IsSelected ? "Opened" : "Closed", nameKey);
				AddDescriptionIfNeeded(ref text, nameKey, "Description");
				Main.instance.MouseTextNoOverride(text, 0, 0);
			}
		};
		return button;
	}

	private static UIElement CreateSlider(CreativePowerMenuSliderSettings settings, Func<float> getValue, Action<float> setValue)
	{
		UIVerticalSlider slider = CreativePowersHelper.CreateSlider(
			getValue,
			setValue,
			() => {
				float sliderValue = getValue();
				float newValue = UILinksInitializer.HandleSliderVerticalInput(sliderValue, 0f, 1f, PlayerInput.CurrentProfile.InterfaceDeadzoneX, 0.35f);
				if (newValue != sliderValue)
					setValue(newValue);
			}
		);
		slider.OnUpdate += element => {
			if (settings.EmptyColor != null)
				slider.EmptyColor = settings.EmptyColor.Value;
			if (settings.FilledColor != null)
				slider.FilledColor = settings.FilledColor.Value;
			else if (settings.GetFilledColor != null)
				slider.FilledColor = settings.GetFilledColor(getValue());

			if (element.IsMouseHovering) {
				string text = settings.GetHoverText?.Invoke(getValue()) ?? GetText(settings.NameKey, null, settings.NameKey);
				AddDescriptionIfNeeded(ref text, settings.NameKey, "Description");
				Main.instance.MouseTextNoOverride(text, 0, 0);
			}
		};

		UIPanel panel = new UIPanel();
		panel.Width = new StyleDimension(settings.PanelWidth, 0f);
		panel.Height = new StyleDimension(settings.PanelHeight, 0f);
		panel.HAlign = 0f;
		panel.VAlign = 0.5f;
		panel.Append(slider);
		panel.OnUpdate += CreativePowersHelper.UpdateUseMouseInterface;

		foreach (CreativePowerMenuSliderLabel label in settings.Labels) {
			AddSliderLabel(panel, label, () => setValue(label.Value));
		}

		return panel;
	}

	private static void AddSliderLabel(UIPanel panel, CreativePowerMenuSliderLabel label, Action onClick)
	{
		UIElement element;
		if (label.Icon != null) {
			UIImage image = new UIImage(label.Icon) {
				HAlign = label.HAlign,
				VAlign = label.VAlign,
				Left = label.Offset,
				Top = new StyleDimension(2f, 0f),
				RemoveFloatingPointsFromDrawPosition = true
			};
			element = image;
		}
		else {
			UIText text = new UIText(label.Text) {
				HAlign = label.HAlign,
				VAlign = label.VAlign,
				Left = label.Offset
			};

			element = text;
		}

		element.OnMouseOver += (_, listeningElement) => {
			if (listeningElement is UIText uiText)
				uiText.ShadowColor = Main.OurFavoriteColor;

			SoundEngine.PlaySound(12);
		};
		element.OnMouseOut += (_, listeningElement) => {
			if (listeningElement is UIText uiText)
				uiText.ShadowColor = Color.Black;

			SoundEngine.PlaySound(12);
		};
		element.OnLeftClick += (_, _) => {
			onClick();
			SoundEngine.PlaySound(12);
		};
		panel.Append(element);
	}

	internal static string GetText(string key, string suffix, string fallback)
	{
		if (suffix == null)
			return Language.GetTextValue(key);

		string dottedKey = key + "." + suffix;
		string text = Language.GetTextValue(dottedKey);
		if (text != dottedKey)
			return text;

		string underscoredKey = key + "_" + suffix;
		text = Language.GetTextValue(underscoredKey);
		return text == underscoredKey ? fallback : text;
	}

	internal static void AddDescriptionIfNeeded(ref string text, string key, string suffix)
	{
		if (!CreativePowerSettings.ShouldPowersBeElaborated)
			return;

		string description = GetText(key, suffix, string.Empty);
		if (description.Length > 0)
			text += "\n" + description;
	}
}

public sealed class CreativePowerMenuEntry
{
	internal CreativePowerMenuEntry(string nameKey, Func<CreativePowerUIElementRequestInfo, int, UIElement> createButton, Func<UIElement> createPanel)
	{
		NameKey = nameKey;
		CreateButton = createButton;
		CreatePanel = createPanel;
	}

	internal CreativePowerMenuEntry(string nameKey, Func<CreativePowerUIElementRequestInfo, UIElement> createButton, Func<UIElement> createPanel)
		: this(nameKey, (info, _) => createButton(info), createPanel)
	{
	}

	public string NameKey { get; }
	public Func<CreativePowerUIElementRequestInfo, int, UIElement> CreateButton { get; }
	public Func<UIElement> CreatePanel { get; }
	public bool HasPanel => CreatePanel != null;
}

public sealed class CreativePowerMenuSliderSettings
{
	public string NameKey { get; internal set; }
	public float PanelWidth { get; set; } = 132f;
	public float PanelHeight { get; set; } = 180f;
	public Color? EmptyColor { get; set; }
	public Color? FilledColor { get; set; } = Main.OurFavoriteColor;
	public Func<float, Color> GetFilledColor { get; set; }
	public Func<float, string> GetHoverText { get; set; }
	public List<CreativePowerMenuSliderLabel> Labels { get; } = new();

	public static CreativePowerMenuSliderSettings Default(string nameKey)
	{
		CreativePowerMenuSliderSettings settings = new CreativePowerMenuSliderSettings {
			NameKey = nameKey
		};

		settings.AddLabel(CreativePowerMenuCategory.GetText(nameKey, "Top", "Top"), 1f, 0f);
		settings.AddLabel(CreativePowerMenuCategory.GetText(nameKey, "Middle", "Middle"), 0.5f, 0.5f);
		settings.AddLabel(CreativePowerMenuCategory.GetText(nameKey, "Bottom", "Bottom"), 0f, 1f);
		return settings;
	}

	public CreativePowerMenuSliderSettings ClearLabels()
	{
		Labels.Clear();
		return this;
	}

	public CreativePowerMenuSliderSettings AddLabel(string text, float value, float vAlign, float hAlign = 1f, float offsetPixels = 0f)
	{
		Labels.Add(CreativePowerMenuSliderLabel.FromText(text, value, vAlign, hAlign, offsetPixels));
		return this;
	}

	public CreativePowerMenuSliderSettings AddIconLabel(Asset<Texture2D> icon, float value, float vAlign, float hAlign = 1f, float offsetPixels = 4f)
	{
		Labels.Add(CreativePowerMenuSliderLabel.FromIcon(icon, value, vAlign, hAlign, offsetPixels));
		return this;
	}

	public CreativePowerMenuSliderSettings UsePercentageHoverText()
	{
		GetHoverText = value => value.ToString("P0");
		return this;
	}

	public CreativePowerMenuSliderSettings UseMultiplierHoverText(float min, float max, string format = "0.##")
	{
		GetHoverText = value => "x" + MathHelper.Lerp(min, max, value).ToString(format);
		return this;
	}
}

public readonly struct CreativePowerMenuSliderLabel
{
	private CreativePowerMenuSliderLabel(string text, Asset<Texture2D> icon, float value, float vAlign, float hAlign, float offsetPixels)
	{
		Text = text;
		Icon = icon;
		Value = value;
		VAlign = vAlign;
		HAlign = hAlign;
		Offset = new StyleDimension(offsetPixels, 0f);
	}

	public string Text { get; }
	public Asset<Texture2D> Icon { get; }
	public float Value { get; }
	public float VAlign { get; }
	public float HAlign { get; }
	public StyleDimension Offset { get; }

	public static CreativePowerMenuSliderLabel FromText(string text, float value, float vAlign, float hAlign = 1f, float offsetPixels = 0f)
	{
		return new CreativePowerMenuSliderLabel(text, null, value, vAlign, hAlign, offsetPixels);
	}

	public static CreativePowerMenuSliderLabel FromIcon(Asset<Texture2D> icon, float value, float vAlign, float hAlign = 1f, float offsetPixels = 4f)
	{
		ArgumentNullException.ThrowIfNull(icon);
		return new CreativePowerMenuSliderLabel(null, icon, value, vAlign, hAlign, offsetPixels);
	}
}
