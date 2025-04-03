using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser;

// The "UI" elements (View) of the browser
internal partial class UIModBrowser
{
	/* Layout */
	private UIElement _rootElement;
	private UIPanel _backgroundElement;
	public UIAsyncList_ModDownloadItem ModList;
	public UIText NoModsFoundText;
	public UITextPanel<LocalizedText> HeaderTextPanel;
	private UIElement _upperMenuContainer;
	internal readonly List<UICycleImage> CategoryButtons = new List<UICycleImage>();
	private UITextPanel<LocalizedText> _reloadButton;
	private UITextPanel<LocalizedText> _backButton;
	private UITextPanel<string> _clearButton;
	private UITextPanel<LocalizedText> _downloadAllButton;
	private UITextPanel<LocalizedText> _updateAllButton;
	private UIPanel _filterTextBoxBackground;
	internal UIInputTextField FilterTextBox;
	private UIBrowserStatus _browserStatus;
	private UIModTagFilterDropdown modTagFilterDropdown;

	/* Filters */
	public UIBrowserFilterToggle<ModBrowserSortMode> SortModeFilterToggle;
	public UIBrowserFilterToggle<ModBrowserTimePeriod> TimePeriodToggle;
	public UIBrowserFilterToggle<UpdateFilter> UpdateFilterToggle;
	public UIBrowserFilterToggle<SearchFilter> SearchFilterToggle;
	public UIBrowserFilterToggle<ModSideFilter> ModSideFilterToggle;
	public UICycleImage TagFilterToggle;

	internal void Reset()
	{
		ModList?.SetEnumerable(null);
		SearchFilterToggle?.SetCurrentState(SearchFilter.Name);
		TimePeriodToggle?.SetCurrentState(ModBrowserTimePeriod.OneWeek);
		UpdateFilterToggle?.SetCurrentState(UpdateFilter.All);
		ModSideFilterToggle?.SetCurrentState(ModSideFilter.All);
		SortModeFilterToggle?.SetCurrentState(ModBrowserSortMode.Hot);
		ResetTagFilters();
	}

	private void UpdateHandler(object sender, EventArgs e)
	{
		UpdateNeeded = true;
	}

	private void InitializeInteractions()
	{
		_reloadButton.OnLeftClick += ReloadList;
		_backButton.OnLeftClick += (_, _) => HandleBackButtonUsage();
		_clearButton.OnLeftClick += ClearTextFilters;
		_downloadAllButton.OnLeftClick += DownloadAllFilteredMods;
		_updateAllButton.OnLeftClick += UpdateAllMods;
		ModList.OnStartLoading += ModListStartLoading;
		ModList.OnFinished += ModListFinished;
		_filterTextBoxBackground.OnRightClick += (a, b) => FilterTextBox.Text = "";
		FilterTextBox.OnRightClick += (a, b) => FilterTextBox.Text = "";
		FilterTextBox.OnTextChange += UpdateHandler;
		foreach (var btn in CategoryButtons) {
			btn.OnStateChanged += UpdateHandler;
		}
	}

	public override void OnInitialize()
	{
		_rootElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f
		};

		_backgroundElement = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -110, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
			PaddingTop = 0f
		};
		_rootElement.Append(_backgroundElement);

		ModList = new UIAsyncList_ModDownloadItem {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
			ListPadding = 5f
		};

		var listScrollbar = new UIScrollbar {
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		_backgroundElement.Append(listScrollbar);

		_backgroundElement.Append(ModList);
		ModList.SetScrollbar(listScrollbar);

		HeaderTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MenuModBrowser"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15f);
		_backgroundElement.Append(HeaderTextPanel);

		_reloadButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MBCancelLoading")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			VAlign = 1f,
			Top = { Pixels = -65 }
		}.WithFadedMouseOver();

		_backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			VAlign = 1f,
			Top = { Pixels = -20 }
		}.WithFadedMouseOver();

		_clearButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -65 },
			BackgroundColor = Color.Purple * 0.7f
		}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);

		_updateAllButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MBUpdateAll")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -20 },
			BackgroundColor = Color.Orange * 0.7f
		}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);

		_downloadAllButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MBDownloadAll")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -20 },
			BackgroundColor = Color.Azure * 0.7f
		}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);

		NoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
			HAlign = 0.5f
		}.WithPadding(15f);

		FilterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = { Pixels = 5 },
			Left = { Pixels = -161, Percent = 1f },
			Width = { Pixels = 100 },
			Height = { Pixels = 20 }
		};

		_upperMenuContainer = new UIElement {
			Width = { Percent = 1f },
			Height = { Pixels = 32 },
			Top = { Pixels = 10 }
		};

		_filterTextBoxBackground = new UIPanel {
			Top = { Percent = 0f },
			Left = { Pixels = -170, Percent = 1f },
			Width = { Pixels = 135 },
			Height = { Pixels = 40 }
		};

		SortModeFilterToggle = new UIBrowserFilterToggle<ModBrowserSortMode>(0, 0) {
			Left = new StyleDimension { Pixels = 0 * 36 }
		};
		TimePeriodToggle = new UIBrowserFilterToggle<ModBrowserTimePeriod>(34 * 8, 0) {
			Left = new StyleDimension { Pixels = 1 * 36 }
		};
		UpdateFilterToggle = new UIBrowserFilterToggle<UpdateFilter>(34, 0) {
			Left = new StyleDimension { Pixels = 2 * 36 }
		};
		SearchFilterToggle = new UIBrowserFilterToggle<SearchFilter>(34 * 2, 0) {
			Left = new StyleDimension { Pixels = 544f }
		};
		ModSideFilterToggle = new UIBrowserFilterToggle<ModSideFilter>(34 * 5, 0) {
			Left = new StyleDimension { Pixels = 3 * 36 }
		};
		TagFilterToggle = new UICycleImage(UICommon.ModBrowserIconsTexture, 2, 32, 32, 34 * 9, 0, 2) {
			Left = new StyleDimension { Pixels = 4 * 36 }
		};
		TagFilterToggle.OnLeftClick += OpenOrCloseTagFilterDropdown;
		TagFilterToggle.OnLeftClick += (a, b) => RefreshTagFilterState(); // Undo the automatic state cycle rather than modify existing public UIElement class.
		TagFilterToggle.OnRightClick += (a, b) => RefreshTagFilterState();

		Reset(); // Set filters to default states

		modTagFilterDropdown = new UIModTagFilterDropdown();
		modTagFilterDropdown.OnLeftClick += (a, b) => {
			if (a.Target == modTagFilterDropdown) {
				CloseTagFilterDropdown();
			}
		};
		OnLeftClick += (a, b) => {
			if (a.Target == this) {
				CloseTagFilterDropdown();
			}
		};
		modTagFilterDropdown.OnClickingTag += () => UpdateNeeded = true; // Triggers a workshop refresh

		_browserStatus = new UIBrowserStatus() {
			VAlign = 1f,
			Top = { Pixels = -65 + 25 - 32 }, // Align with _reloadButton
			Left = { Pixels = 545f } // Align with SearchFilterToggle
		};
		_rootElement.Append(_browserStatus);

		_rootElement.Append(_reloadButton);
		_rootElement.Append(_backButton);

		CategoryButtons.Add(SortModeFilterToggle);
		_upperMenuContainer.Append(SortModeFilterToggle);
		CategoryButtons.Add(TimePeriodToggle);
		_upperMenuContainer.Append(TimePeriodToggle);
		CategoryButtons.Add(UpdateFilterToggle);
		_upperMenuContainer.Append(UpdateFilterToggle);
		CategoryButtons.Add(ModSideFilterToggle);
		_upperMenuContainer.Append(ModSideFilterToggle);
		_upperMenuContainer.Append(TagFilterToggle);
		CategoryButtons.Add(SearchFilterToggle);
		_upperMenuContainer.Append(SearchFilterToggle);

		InitializeInteractions();

		_upperMenuContainer.Append(_filterTextBoxBackground);
		_upperMenuContainer.Append(FilterTextBox);
		_backgroundElement.Append(_upperMenuContainer);

		Append(_rootElement);
	}

	private void CloseTagFilterDropdown()
	{
		_backgroundElement.RemoveChild(modTagFilterDropdown);
		// We could do UpdateNeeded = true; here instead of in modTagFilterDropdown.OnClickingTag for responsiveness. It won't update until the drop down is closed. However, the responsiveness is only an issue in debug.
	}

	private void OpenOrCloseTagFilterDropdown(UIMouseEvent evt, UIElement listeningElement)
	{
		if (modTagFilterDropdown.Parent != null) {
			CloseTagFilterDropdown();
			return;
		}

		_backgroundElement.RemoveChild(modTagFilterDropdown);
		_backgroundElement.Append(modTagFilterDropdown);
	}

	internal void RefreshTagFilterState()
	{
		TagFilterToggle.SetCurrentState(CategoryTagsFilter.Any() || LanguageTagFilter != -1 ? 1 : 0);
	}
}
