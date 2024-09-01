using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.ModBrowser;

// This approach to implementing a dropdown is adapted from UIBestiarySortingOptionsGrid. This element is a semi-transparent panel covering the display area with the actual selection panel appended to that. The panel covering the display area intercepts clicks to handle deselecting/hiding the panel while fading out inactive content behind
internal class UIModTagFilterDropdown : UIPanel
{
	private List<GroupOptionButton<int>> tagButtons;
	private GroupOptionButton<int> clearTagsButton;
	private int indexOfLanguageTags;

	public event Action OnClickingTag;

	public UIModTagFilterDropdown()
	{
		tagButtons = new List<GroupOptionButton<int>>();
		Width = new StyleDimension(-25, 1f);
		Height = new StyleDimension(-50, 1f);
		Top = new StyleDimension(50, 0f);

		BackgroundColor = new Color(35, 40, 83) * 0.5f;
		BorderColor = new Color(35, 40, 83) * 0.5f;
		IgnoresMouseInteraction = false;
		SetPadding(0f);
		BuildGrid();
	}

	private void BuildGrid()
	{
		int labelWidth = 100;
		foreach (var tag in SteamedWraps.ModTags) {
			string tagName = Language.GetTextValue(tag.NameKey);
			labelWidth = Math.Max(labelWidth, (int)ChatManager.GetStringSize(FontAssets.MouseText.Value, tagName, new Vector2(0.8f)).X + 10);
		}

		int padding = 2;
		int buttonHeight = 26 + padding;
		int tagCount = SteamedWraps.ModTags.Count; // 17
		indexOfLanguageTags = SteamedWraps.ModTags.FindIndex(x => x.InternalNameForAPIs == "English"); // 8
		int maxColumnCount = Math.Max(indexOfLanguageTags, tagCount - indexOfLanguageTags);

		UIPanel dropdownPanel = new UIPanel {
			Width = new StyleDimension(15 + labelWidth * 2, 0f), // 5 padding, 150 wide elements: 5 + 150 + 5 + 150 + 5
			Height = new StyleDimension((maxColumnCount + 1) * buttonHeight + 5 + 3, 0f),
			Left = new StyleDimension(Math.Max(0, 4 * 36 + 16 - ((15 + labelWidth * 2) / 2)), 0f), // Center below button: 4 * 36 + 18 - 315/2
			Top = new StyleDimension(0f, 0f)
		};

		dropdownPanel.BorderColor = new Color(89, 116, 213, 255) * 0.9f;
		dropdownPanel.BackgroundColor = new Color(73, 94, 171) * 0.9f;
		dropdownPanel.SetPadding(0f);
		Append(dropdownPanel);
		for (int j = 0; j < tagCount; j++) {
			var tag = SteamedWraps.ModTags[j];

			int top = 5 + buttonHeight * j;
			int left = 5;
			if (j >= indexOfLanguageTags) {
				top = 5 + buttonHeight * (j - indexOfLanguageTags);
				left = 10 + labelWidth;
			}

			GroupOptionButton<int> groupOptionButton = new GroupOptionButton<int>(j, Language.GetText(tag.NameKey), null, Color.White, null, 0.8f) {
				Width = new StyleDimension(labelWidth, 0f),
				Height = new StyleDimension(buttonHeight - padding, 0f),
				Top = new StyleDimension(top, 0f),
				Left = new StyleDimension(left, 0f)
			};

			groupOptionButton.ShowHighlightWhenSelected = false;
			if (j >= indexOfLanguageTags)
				groupOptionButton.OnLeftClick += ClickLanguageTag;
			else
				groupOptionButton.OnLeftClick += ClickCategoryTag;
			// groupOptionButton.SetSnapPoint("SortSteps", j);
			dropdownPanel.Append(groupOptionButton);
			tagButtons.Add(groupOptionButton);
		}

		foreach (GroupOptionButton<int> item in tagButtons) {
			item.SetCurrentOption(-1);
		}

		clearTagsButton = new GroupOptionButton<int>(0, Language.GetText("tModLoader.ModConfigClear"), null, Color.White, null, 0.8f) {
			Width = new StyleDimension(labelWidth, 0f),
			Height = new StyleDimension(buttonHeight - padding, 0f),
			Top = new StyleDimension(5 + buttonHeight * maxColumnCount, 0f),
			Left = new StyleDimension(8 + labelWidth / 2, 0f)
		};
		clearTagsButton.SetColor(new Color(226, 49, 85), 1f);
		clearTagsButton.ShowHighlightWhenSelected = false;
		clearTagsButton.OnLeftClick += (a, b) => {
			Interface.modBrowser.ResetTagFilters();
			OnClickingTag?.Invoke();
		};
		dropdownPanel.Append(clearTagsButton);
	}

	private void ClickCategoryTag(UIMouseEvent evt, UIElement listeningElement)
	{
		int tagIndex = ((GroupOptionButton<int>)listeningElement).OptionValue;
		string tagName = SteamedWraps.ModTags[tagIndex].InternalNameForAPIs;

		if (Interface.modBrowser.CategoryTagsFilter.Contains(tagIndex))
			Interface.modBrowser.CategoryTagsFilter.Remove(tagIndex);
		else
			Interface.modBrowser.CategoryTagsFilter.Add(tagIndex);

		RefreshSelectionStates();
		OnClickingTag?.Invoke();
	}

	private void ClickLanguageTag(UIMouseEvent evt, UIElement listeningElement)
	{
		int tagIndex = ((GroupOptionButton<int>)listeningElement).OptionValue;
		if (Interface.modBrowser.LanguageTagFilter == tagIndex)
			Interface.modBrowser.LanguageTagFilter = -1;
		else
			Interface.modBrowser.LanguageTagFilter = tagIndex;

		RefreshSelectionStates();
		OnClickingTag?.Invoke();
	}

	internal void RefreshSelectionStates()
	{
		for (int i = 0; i < tagButtons.Count; i++) {
			var item = tagButtons[i];
			bool selected = i < indexOfLanguageTags ? Interface.modBrowser.CategoryTagsFilter.Contains(item.OptionValue) : Interface.modBrowser.LanguageTagFilter == item.OptionValue;

			item.SetCurrentOption(selected ? i : (-1));
			if (selected)
				item.SetColor(new Color(152, 175, 235), 1f);
			else
				item.SetColor(ID.Colors.InventoryDefaultColor, 0.7f);
		}

		Interface.modBrowser.RefreshTagFilterState();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		// Language descriptions are unnecessary, so skip their hover tooltip
		for (int i = 0; i < indexOfLanguageTags; i++) {
			var item = tagButtons[i];
			if (item.IsMouseHovering) {
				UICommon.TooltipMouseText(Language.GetTextValue(SteamedWraps.ModTags[item.OptionValue].NameKey + "Description"));
			}
		}
		if(clearTagsButton.IsMouseHovering)
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.MBTagsClear"));
	}
}
