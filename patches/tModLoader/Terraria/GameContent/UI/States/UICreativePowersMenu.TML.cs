using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent.UI.States;

public partial class UICreativePowersMenu
{
	private readonly Dictionary<int, PowerStripUIElement> _modCategoryStrips = new Dictionary<int, PowerStripUIElement>();
	private readonly Dictionary<int, MenuTree<int>> _modCategoryTrees = new Dictionary<int, MenuTree<int>>();
	private readonly Dictionary<int, UIElement> _modCategoryContent = new Dictionary<int, UIElement>();

	internal int CurrentMainCategory => _mainCategory.CurrentOption;

	private void AddModCategoryButtons(CreativePowerUIElementRequestInfo request, List<UIElement> list)
	{
		foreach (ModCreativeCategory category in CreativeMenuLoader.categories) {
			GroupOptionButton<int> button = CreativePowersHelper.CreateCategoryButton(request, category.Type, 0);
			UIElement icon = category.CreateIcon();
			icon.IgnoresMouseInteraction = true;
			button.Append(icon);
			button.OnLeftClick += MainCategoryButtonClick;
			button.OnUpdate += affectedElement => ModCategoryButton_OnUpdate(affectedElement, category);
			_mainCategory.Buttons.Add(category.Type, button);
			list.Add(button);
			CreateModCategoryStrip(request, category);
		}

		AddModPowerButtons(CreativeCategoryID.None, request, list, _mainCategory, depth: 0);
	}

	private void CreateModCategoryStrip(CreativePowerUIElementRequestInfo request, ModCreativeCategory category)
	{
		MenuTree<int> tree = new MenuTree<int>(0);
		List<UIElement> buttons = new List<UIElement>();
		AddModPowerButtons(category.Type, request, buttons, tree, depth: 1);
		if (buttons.Count == 0)
			return;

		PowerStripUIElement strip = new PowerStripUIElement(STRIP_DEPTH_1, buttons) {
			HAlign = 0f,
			VAlign = 0.5f,
			Left = new StyleDimension(INITIAL_LEFT_PIXELS + LEFT_PIXELS_PER_STRIP_DEPTH, 0f)
		};
		strip.OnMouseOver += strip_OnMouseOver;
		strip.OnMouseOut += strip_OnMouseOut;
		_modCategoryStrips[category.Type] = strip;
		_modCategoryTrees[category.Type] = tree;
	}

	/// <summary>
	/// Adds the buttons of the modded powers in <paramref name="category"/> to a strip <paramref name="depth"/> strips from the main one.<br/>
	/// Sliders open in the next strip along, and are added to <paramref name="tree"/> after its existing options.
	/// </summary>
	private void AddModPowerButtons<TEnum>(int category, CreativePowerUIElementRequestInfo request, List<UIElement> list, MenuTree<TEnum> tree, int depth) where TEnum : struct, IConvertible
	{
		foreach (ModCreativePower power in CreativeMenuLoader.powers) {
			if (power.Category != category)
				continue;

			switch (power) {
				case ModCreativeToggle toggle:
					list.Add(toggle.CreateButton(request));
					break;
				case ModCreativeButton button:
					list.Add(button.CreateButton(request));
					break;
				case ModCreativeSlider slider:
					list.Add(CreateModSliderButton(slider, request, tree, depth));
					break;
			}
		}
	}

	private GroupOptionButton<int> CreateModSliderButton<TEnum>(ModCreativeSlider slider, CreativePowerUIElementRequestInfo request, MenuTree<TEnum> tree, int depth) where TEnum : struct, IConvertible
	{
		int option = tree.Buttons.Count == 0 ? 1 : tree.Buttons.Keys.Max() + 1;
		GroupOptionButton<int> button = slider.CreateButton(request, option, tree.CurrentOption);
		button.OnLeftClick += (_, _) => {
			if (!slider.CanUse())
				return;

			ToggleCategory(tree, option, default(TEnum));
			RefreshElementsOrder();
		};

		UIElement panel = slider.CreatePanel();
		panel.Left = new StyleDimension(INITIAL_LEFT_PIXELS + (depth + 1) * LEFT_PIXELS_PER_STRIP_DEPTH, 0f);
		panel.SetSnapPoint("strip " + (depth + 1), 0, new Vector2(0f, 0.5f), new Vector2(28f, 0f));
		tree.Buttons[option] = button;
		tree.Sliders[option] = panel;
		return button;
	}

	private void RemoveHiddenCategoryButtons(List<UIElement> list)
	{
		// Sliders on the main strip, such as Difficulty and modded ones, use the options after the categories.
		foreach (int category in CreativeMenuLoader.FindHiddenCategories(_mainCategory.Buttons.Keys.Where(option => option < CreativeMenuLoader.CategoryCount).ToList())) {
			list.Remove(_mainCategory.Buttons[category]);
			_mainCategory.Buttons.Remove(category);
		}
	}

	private static void ModCategoryButton_OnUpdate(UIElement affectedElement, ModCreativeCategory category)
	{
		if (!affectedElement.IsMouseHovering)
			return;

		bool isSelected = ((GroupOptionButton<int>)affectedElement).IsSelected;
		string text = (isSelected ? category.CloseTooltip : category.OpenTooltip).Value;
		string description = category.Description.Value;
		if (CreativePowerSettings.ShouldPowersBeElaborated && !string.IsNullOrEmpty(description))
			text += "\n" + description;

		Main.instance.MouseTextNoOverride(text);
	}

	private void RefreshModCategoryContent()
	{
		int option = _mainCategory.CurrentOption;
		if (CreativeMenuLoader.GetCategory(option) is not ModCreativeCategory category)
			return;

		if (_modCategoryStrips.TryGetValue(option, out PowerStripUIElement strip)) {
			_container.Append(strip);
			MenuTree<int> tree = _modCategoryTrees[option];
			if (tree.Sliders.TryGetValue(tree.CurrentOption, out UIElement slider))
				_container.Append(slider);
		}

		if (!_modCategoryContent.TryGetValue(option, out UIElement content))
			_modCategoryContent[option] = content = category.CreateContent();

		if (content != null)
			_container.Append(content);
	}
}
