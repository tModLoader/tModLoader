using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

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
	private UIImageButton clearSearchButton;
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
		clearSearchButton.OnLeftClick += (a, b) => FilterTextBox.Text = "";
		_filterTextBoxBackground.OnRightClick += (a, b) => FilterTextBox.Text = "";
		_filterTextBoxBackground.OnLeftClick += LeftClickTextBox;
		FilterTextBox.OnRightClick += (a, b) => FilterTextBox.Text = "";
		FilterTextBox.OnTextChange += UpdateHandler;
		foreach (var btn in CategoryButtons) {
			btn.OnStateChanged += UpdateHandler;
		}
	}

	private void LeftClickTextBox(UIMouseEvent evt, UIElement listeningElement)
	{
		if (!PlayerInput.UsingGamepadUI || evt.Target == clearSearchButton) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.clrInput();
		UIVirtualKeyboard uIVirtualKeyboard = new UIVirtualKeyboard(Language.GetTextValue("tModLoader.ModsTypeToSearch"), FilterTextBox.Text, OnFinishedNaming, OnCanceledNaming, 0, allowEmpty: true);
		uIVirtualKeyboard.SetMaxInputLength(20);
		Main.MenuUI.SetState(uIVirtualKeyboard);
	}

	private void OnFinishedNaming(string name)
	{
		FilterTextBox.Text = name.Trim();
		Main.MenuUI.SetState(this);
	}

	private void OnCanceledNaming()
	{
		Main.MenuUI.SetState(this);
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
		_reloadButton.SetSnapPoint("ReloadBrowser", 0);

		_backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			VAlign = 1f,
			Top = { Pixels = -20 }
		}.WithFadedMouseOver();
		_backButton.SetSnapPoint("Back", 0);

		_clearButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -65 },
			BackgroundColor = Color.Purple * 0.7f
		}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
		_clearButton.SetSnapPoint("ClearSpecialFilter", 0);

		_updateAllButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MBUpdateAll")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -20 },
			BackgroundColor = Color.Orange * 0.7f
		}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
		_updateAllButton.SetSnapPoint("UpdateAll", 0);

		_downloadAllButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.MBDownloadAll")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -20 },
			BackgroundColor = Color.Azure * 0.7f
		}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);
		_downloadAllButton.SetSnapPoint("DownloadAll", 0);

		NoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
			HAlign = 0.5f
		}.WithPadding(15f);

		FilterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = { Pixels = 5 },
			Height = { Percent = 1f },
			Width = { Percent = 1f },
			Left = { Pixels = 5 },
			VAlign = 0.5f,
		};
		FilterTextBox.SetSnapPoint("FilterTextBox", 0);

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
		_filterTextBoxBackground.SetPadding(0);
		_filterTextBoxBackground.WithFadedMouseOver();

		clearSearchButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
			VAlign = 0.5f,
			Left = new StyleDimension(-2f, 0f)
		};
		clearSearchButton.SetSnapPoint("ClearSearchButton", 0);
		
		SortModeFilterToggle = new UIBrowserFilterToggle<ModBrowserSortMode>(0, 0) {
			Left = new StyleDimension { Pixels = 0 * 36 }
		};
		SortModeFilterToggle.SetSnapPoint("SortModeFilterToggle", 0);
		TimePeriodToggle = new UIBrowserFilterToggle<ModBrowserTimePeriod>(34 * 8, 0) {
			Left = new StyleDimension { Pixels = 1 * 36 }
		};
		TimePeriodToggle.SetSnapPoint("TimePeriodToggle", 0);
		UpdateFilterToggle = new UIBrowserFilterToggle<UpdateFilter>(34, 0) {
			Left = new StyleDimension { Pixels = 2 * 36 }
		};
		UpdateFilterToggle.SetSnapPoint("UpdateFilterToggle", 0);
		SearchFilterToggle = new UIBrowserFilterToggle<SearchFilter>(34 * 2, 0) {
			Left = new StyleDimension { Pixels = 544f }
		};
		SearchFilterToggle.SetSnapPoint("SearchFilterToggle", 0);
		ModSideFilterToggle = new UIBrowserFilterToggle<ModSideFilter>(34 * 5, 0) {
			Left = new StyleDimension { Pixels = 3 * 36 }
		};
		ModSideFilterToggle.SetSnapPoint("ModSideFilterToggle", 0);
		TagFilterToggle = new UICycleImage(UICommon.ModBrowserIconsTexture, 2, 32, 32, 34 * 9, 0, 2) {
			Left = new StyleDimension { Pixels = 4 * 36 }
		};
		TagFilterToggle.SetSnapPoint("TagFilterToggle", 0);
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
		_filterTextBoxBackground.Append(FilterTextBox);
		_filterTextBoxBackground.Append(clearSearchButton);
		_backgroundElement.Append(_upperMenuContainer);

		Append(_rootElement);
	}

	private void CloseTagFilterDropdown()
	{
		_backgroundElement.RemoveChild(modTagFilterDropdown);
		// We could do UpdateNeeded = true; here instead of in modTagFilterDropdown.OnClickingTag for responsiveness. It won't update until the drop down is closed. However, the responsiveness is only an issue in debug.
		UILinkPointNavigator.ChangePoint(GamepadPointID.FancyUI0 + 6); // TagFilterToggle
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

	private void SetupGamepadPoints(SpriteBatch spriteBatch)
	{
		UIGamepadHelper helper;
		int startID = GamepadPointID.FancyUI0 + 2;
		int currentID = startID;

		int lastListButton = currentID;
		int above = currentID;
		var upperMenuSnapPoints = _upperMenuContainer.GetSnapPoints();
		upperMenuSnapPoints.Sort((SnapPoint x, SnapPoint y) => x.Position.X.CompareTo(y.Position.X));
		var upperMenuLinkPoints = helper.CreateUILinkStripHorizontal(ref currentID, upperMenuSnapPoints);
		foreach (var item in upperMenuLinkPoints) {
			item.Down = currentID;
		}

		if (modTagFilterDropdown.Parent != null) {
			var modTagFilterDropdownSnapPoints = modTagFilterDropdown.GetSnapPoints();
			List<SnapPoint> orderedPointsByCategoryName = helper.GetOrderedPointsByCategoryName(modTagFilterDropdownSnapPoints, "TagOption");

			UILinkPoint[,] linkPointGrid = CreateUILinkPointGridFromVerticalListData(ref currentID, orderedPointsByCategoryName, 2, UILinkPointNavigator.Points[above + 4], null, null, UILinkPointNavigator.Points[currentID + orderedPointsByCategoryName.Count]);
			var clearTagsSnapPoint = modTagFilterDropdownSnapPoints.Single(x => x.Name == "ClearTags");
			var clearTagsLinkPoint = helper.MakeLinkPointFromSnapPoint(currentID++, clearTagsSnapPoint);

			helper.PairUpDown(linkPointGrid[1, linkPointGrid.GetLength(1) - 1], clearTagsLinkPoint);
			helper.PairUpDown(linkPointGrid[0, linkPointGrid.GetLength(1) - 1], clearTagsLinkPoint);

			clearTagsLinkPoint.Down = currentID;
			lastListButton = clearTagsLinkPoint.ID;
		}
		else {
			List<(UIElement, List<SnapPoint>)> elementSnapPairs = ModList._items.Select(x => (x, x.GetSnapPoints())).ToList();
			foreach (var item in elementSnapPairs) {
				helper.CullPointsOutOfElementArea(spriteBatch, item.Item2, ModList);
			}
			elementSnapPairs.RemoveAll(x => x.Item2.Count == 0);
			foreach (var item in elementSnapPairs) {
				item.Item2.Sort((x, y) => x.Position.X.CompareTo(y.Position.X));
				lastListButton = currentID;
				var buttonLinkPoints = helper.CreateUILinkStripHorizontal(ref currentID, item.Item2);
				foreach (var buttonLinkPoint in buttonLinkPoints) {
					buttonLinkPoint.Up = above;
					buttonLinkPoint.Down = currentID;
				}
				above = buttonLinkPoints[0].ID;
			}
		}

		above = currentID;
		UILinkPoint linkPoint_ReloadBrowser = helper.GetLinkPoint(currentID++, _reloadButton);
		UILinkPoint linkPoint_Back = helper.GetLinkPoint(currentID++, _backButton);

		helper.PairUpDown(linkPoint_ReloadBrowser, linkPoint_Back);
		linkPoint_ReloadBrowser.Up = lastListButton;

		// These conditionally appear
		if (_clearButton.Parent != null) {
			UILinkPoint linkPoint_ClearFilter = helper.GetLinkPoint(currentID++, _clearButton);
			UILinkPoint linkPoint_DownloadAll = helper.GetLinkPoint(currentID++, _downloadAllButton);
			helper.PairUpDown(linkPoint_ClearFilter, linkPoint_DownloadAll);
			helper.PairLeftRight(linkPoint_ReloadBrowser, linkPoint_ClearFilter);
			helper.PairLeftRight(linkPoint_Back, linkPoint_DownloadAll);
			linkPoint_ClearFilter.Up = lastListButton;
		}
		if (_updateAllButton.Parent != null) {
			UILinkPoint linkPoint_UpdateAll = helper.GetLinkPoint(currentID++, _updateAllButton);
			helper.PairLeftRight(linkPoint_Back, linkPoint_UpdateAll);
			linkPoint_UpdateAll.Up = lastListButton;
		}
	}

	// Unlike UIGamepadHelper.CreateUILinkPointGrid, this takes in data arranged in columns instead of rows
	public UILinkPoint[,] CreateUILinkPointGridFromVerticalListData(ref int currentID, List<SnapPoint> pointsForGrid, int pointsPerLine, UILinkPoint topLinkPoint, UILinkPoint leftLinkPoint, UILinkPoint rightLinkPoint, UILinkPoint bottomLinkPoint)
	{
		UIGamepadHelper helper;
		int num = (int)Math.Ceiling((float)pointsForGrid.Count / (float)pointsPerLine);
		UILinkPoint[,] array = new UILinkPoint[pointsPerLine, num];
		for (int i = 0; i < pointsForGrid.Count; i++) {
			int num2 = i / num; // These 2 lines are changed from CreateUILinkPointGrid
			int num3 = i % num; 
			array[num2, num3] = helper.MakeLinkPointFromSnapPoint(currentID++, pointsForGrid[i]);
		}

		for (int j = 0; j < array.GetLength(0); j++) {
			for (int k = 0; k < array.GetLength(1); k++) {
				UILinkPoint uILinkPoint = array[j, k];
				if (uILinkPoint == null)
					continue;

				if (j < array.GetLength(0) - 1) {
					UILinkPoint uILinkPoint2 = array[j + 1, k];
					if (uILinkPoint2 != null)
						helper.PairLeftRight(uILinkPoint, uILinkPoint2);
				}

				if (k < array.GetLength(1) - 1) {
					UILinkPoint uILinkPoint3 = array[j, k + 1];
					if (uILinkPoint3 != null)
						helper.PairUpDown(uILinkPoint, uILinkPoint3);
				}

				if (leftLinkPoint != null && j == 0)
					uILinkPoint.Left = leftLinkPoint.ID;

				if (topLinkPoint != null && k == 0)
					uILinkPoint.Up = topLinkPoint.ID;

				if (rightLinkPoint != null && j == pointsPerLine - 1)
					uILinkPoint.Right = rightLinkPoint.ID;

				if (bottomLinkPoint != null && k == num - 1)
					uILinkPoint.Down = bottomLinkPoint.ID;
			}
		}

		return array;
	}
}
