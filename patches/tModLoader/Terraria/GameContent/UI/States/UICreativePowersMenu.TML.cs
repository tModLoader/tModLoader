using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent.UI.States;

public partial class UICreativePowersMenu
{
	private const int CUSTOM_CATEGORY_START = 1000;

	private CreativePowerMenuEntries _customEntries;
	private Dictionary<int, PowerStripUIElement> _customPowerStrips = new Dictionary<int, PowerStripUIElement>();
	private Dictionary<int, MenuTree<int>> _customCategories = new Dictionary<int, MenuTree<int>>();

	private void InitializeCustomPowerMenuEntries()
	{
		_customEntries = new CreativePowerMenuEntries();
		SystemLoader.ModifyCreativePowersMenu(_customEntries);
	}

	private void AddCustomMainButtons(CreativePowerUIElementRequestInfo request, List<UIElement> list)
	{
		foreach (var createMainButton in _customEntries.MainButtons) {
			list.Add(createMainButton(request));
		}
	}

	private void AddCustomCategoryButtons(CreativePowerUIElementRequestInfo request, List<UIElement> list)
	{
		int index = CUSTOM_CATEGORY_START;
		foreach (CreativePowerMenuCategory category in _customEntries.Categories.OrderBy(c => c.SortOrder)) {
			GroupOptionButton<int> button = CreativePowersHelper.CreateCategoryButton(request, index, 0);
			button.Append(CreativePowersHelper.GetIconImage(category.IconLocation));
			button.OnLeftClick += MainCategoryButtonClick;
			button.OnUpdate += element => CategoryButton_OnUpdate_DisplayTooltips(element, category.NameKey);
			_mainCategory.Buttons.Add(index, button);
			list.Add(button);
			index++;
		}
	}

	private void CreateCustomPowerStrips()
	{
		_customPowerStrips.Clear();
		_customCategories.Clear();

		CreativePowerUIElementRequestInfo request = default;
		request.PreferredButtonWidth = 40;
		request.PreferredButtonHeight = 40;

		int index = CUSTOM_CATEGORY_START;
		foreach (CreativePowerMenuCategory category in _customEntries.Categories.OrderBy(c => c.SortOrder)) {
			MenuTree<int> customTree = new MenuTree<int>(0);
			_customCategories[index] = customTree;
			List<UIElement> buttons = new List<UIElement>();

			int subcategoryIndex = 1;
			foreach (CreativePowerMenuEntry entry in category.Elements) {
				UIElement element = entry.CreateButton(request, subcategoryIndex);
				if (entry.HasPanel && element is GroupOptionButton<int> optionButton) {
					optionButton.SetCurrentOption(0);
					optionButton.OnLeftClick += CustomCategoryButtonClick;
					customTree.Buttons[subcategoryIndex] = optionButton;

					UIElement panel = entry.CreatePanel();
					panel.Left = new StyleDimension(140f, 0f);
					panel.SetSnapPoint(STRIP_DEPTH_2, 0, new Vector2(0f, 0.5f), new Vector2(28f, 0f));
					customTree.Sliders[subcategoryIndex] = panel;
				}

				buttons.Add(element);
				subcategoryIndex++;
			}

			PowerStripUIElement strip = new PowerStripUIElement(STRIP_DEPTH_1, buttons) {
				HAlign = 0f,
				VAlign = 0.5f,
				Left = new StyleDimension(INITIAL_LEFT_PIXELS + LEFT_PIXELS_PER_STRIP_DEPTH, 0f)
			};

			strip.OnMouseOver += strip_OnMouseOver;
			strip.OnMouseOut += strip_OnMouseOut;
			_customPowerStrips[index] = strip;
			index++;
		}
	}

	private void CustomCategoryButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{
		GroupOptionButton<int> groupOptionButton = listeningElement as GroupOptionButton<int>;
		if (_customCategories.TryGetValue(_mainCategory.CurrentOption, out MenuTree<int> customCategory)) {
			ToggleCategory(customCategory, groupOptionButton.OptionValue, 0);
			RefreshElementsOrder();
		}
	}

	private void AppendCustomCategoryElements(int currentOption)
	{
		if (_customPowerStrips.TryGetValue(currentOption, out PowerStripUIElement customStrip)) {
			_container.Append(customStrip);
			if (_customCategories.TryGetValue(currentOption, out MenuTree<int> customCategory) && customCategory.Sliders.TryGetValue(customCategory.CurrentOption, out UIElement value))
				_container.Append(value);
		}
	}
}
