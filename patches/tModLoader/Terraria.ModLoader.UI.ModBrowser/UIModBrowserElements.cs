using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser
{
	// The "UI" elements (View) of the browser
	internal partial class UIModBrowser
	{
		public bool IsInitialized = false;
		
		/* Layout */
		private readonly UIElement _rootElement = new UIElement {
			Width = {Percent = 0.8f},
			MaxWidth = UICommon.MaxPanelWidth,
			Top = {Pixels = 220},
			Height = {Pixels = -220, Percent = 1f},
			HAlign = 0.5f
		};
		
		private readonly UIPanel _backgroundElement =  new UIPanel {
			Width = {Percent = 1f},
			Height = {Pixels = -110, Percent = 1f},
			BackgroundColor = UICommon.mainPanelBackground,
			PaddingTop = 0f
		};
		
		private readonly UILoaderAnimatedImage _loaderElement = new UILoaderAnimatedImage(0.5f, 0.5f);
		
		public UIList ModList = new UIList {
			Width = {Pixels = -25, Percent = 1f},
			Height = {Pixels = -50, Percent = 1f},
			Top = {Pixels = 50},
			ListPadding = 5f
		};
		
		public UIText NoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
			HAlign = 0.5f
		}.WithPadding(15f);

		public UITextPanel<string> HeaderTextPanel =  new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModBrowser"), 0.8f, true) {
			HAlign = 0.5f,
			Top = {Pixels = -35},
			BackgroundColor = UICommon.defaultUIBlue
		}.WithPadding(15f);
		
		private readonly UITextPanel<string> _reloadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBGettingData")) {
			Width = {Pixels = -10, Percent = 0.5f},
			Height = {Pixels = 25},
			VAlign = 1f,
			Top = {Pixels = -65}
		}.WithFadedMouseOver();
		
		private readonly UITextPanel<string> _backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
			Width = {Pixels = -10, Percent = 0.5f},
			Height = {Pixels = 25},
			VAlign = 1f,
			Top = {Pixels = -20}
		}.WithFadedMouseOver();
		
		private readonly UITextPanel<string> _clearButton =new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
			Width = {Pixels = -10, Percent = 0.5f},
			Height = {Pixels = 25},
			HAlign = 1f,
			VAlign = 1f,
			Top = {Pixels = -65},
			BackgroundColor = Color.Purple * 0.7f
		}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
		
		private readonly UITextPanel<string> _downloadAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBDownloadAll")) {
			Width = {Pixels = -10, Percent = 0.5f},
			Height = {Pixels = 25},
			HAlign = 1f,
			VAlign = 1f,
			Top = {Pixels = -20},
			BackgroundColor = Color.Azure * 0.7f
		}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);
		
		private readonly UITextPanel<string> _updateAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUpdateAll")) {
			Width = {Pixels = -10, Percent = 0.5f},
			Height = {Pixels = 25},
			HAlign = 1f,
			VAlign = 1f,
			Top = {Pixels = -20},
			BackgroundColor = Color.Orange * 0.7f
		}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
		
		private readonly UIElement _upperMenuContainer = new UIElement {
			Width = {Percent = 1f},
			Height = {Pixels = 32},
			Top = {Pixels = 10}
		};

		private readonly UIPanel _filterTextBoxBackground = new UIPanel {
			Top = {Percent = 0f},
			Left = {Pixels = -170, Percent = 1f},
			Width = {Pixels = 135},
			Height = {Pixels = 40}
		};
		
		internal readonly List<UICycleImage> CategoryButtons = new List<UICycleImage>();

		internal UIInputTextField FilterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = {Pixels = 5},
			Left = {Pixels = -160, Percent = 1f},
			Width = {Pixels = 100},
			Height = {Pixels = 10}
		};

		/* Filters */
		public readonly UIBrowserFilterToggle<ModBrowserSortMode> SortModeFilterToggle = new UIBrowserFilterToggle<ModBrowserSortMode>(0, 0) {
			Left = new StyleDimension {Pixels = 0 * 36 + 8}
		};

		public readonly UIBrowserFilterToggle<UpdateFilter> UpdateFilterToggle = new UIBrowserFilterToggle<UpdateFilter>(34, 0) {
			Left = new StyleDimension {Pixels = 1 * 36 + 8}
		};

		public readonly UIBrowserFilterToggle<SearchFilter> SearchFilterToggle = new UIBrowserFilterToggle<SearchFilter>(34 * 2, 0) {
			Left = new StyleDimension {Pixels = 545f}
		};

		public readonly UIBrowserFilterToggle<ModSideFilter> ModSideFilterToggle = new UIBrowserFilterToggle<ModSideFilter>(34 * 5, 0) {
			Left = new StyleDimension {Pixels = 2 * 36 + 8}
		};

		internal void Reset() {
			if (IsInitialized)
			{
				ModList?.Clear();
				_items?.Clear();
				_missingMods?.Clear();
				SearchFilterToggle.SetCurrentState(default);
				UpdateFilterToggle.SetCurrentState(default);
				ModSideFilterToggle.SetCurrentState(default);
				SortModeFilterToggle.SetCurrentState(default);
			}
			Loading = false;
			UpdateNeeded = true;
		}
		
		private void InitializeInteractions() {
			_reloadButton.OnClick += ReloadList;
			_backButton.OnClick += BackClick;
			_clearButton.OnClick += ClearFilters;
			_downloadAllButton.OnClick += DownloadAllFilteredMods;
			_updateAllButton.OnClick += UpdateAllMods;
			_filterTextBoxBackground.OnRightClick += (a, b) => FilterTextBox.Text = "";
			FilterTextBox.OnTextChange += (sender, e) => {
				UpdateNeeded = true;
			};
		}
		
		public override void OnInitialize() {
			if (!IsInitialized) {
				
				InitializeInteractions();
				
				_backgroundElement.Append(HeaderTextPanel);
				
				var listScrollbar = new UIScrollbar {
					Height = {Pixels = -50, Percent = 1f},
					Top = {Pixels = 50},
					HAlign = 1f
				}.WithView(100f, 1000f);
				
				ModList.SetScrollbar(listScrollbar);
				_backgroundElement.Append(ModList);
				_backgroundElement.Append(listScrollbar);

				_rootElement.Append(_reloadButton);
				_rootElement.Append(_backButton);
				
				CategoryButtons.Add(SortModeFilterToggle);
				_upperMenuContainer.Append(SortModeFilterToggle);
				CategoryButtons.Add(UpdateFilterToggle);
				_upperMenuContainer.Append(UpdateFilterToggle);
				CategoryButtons.Add(ModSideFilterToggle);
				_upperMenuContainer.Append(ModSideFilterToggle);
				CategoryButtons.Add(SearchFilterToggle);
				_upperMenuContainer.Append(SearchFilterToggle);
				
				_upperMenuContainer.Append(_filterTextBoxBackground);
				_upperMenuContainer.Append(FilterTextBox);
				_backgroundElement.Append(_upperMenuContainer);

				_rootElement.Append(_backgroundElement);
				Append(_rootElement);

				IsInitialized = true;
			}
		}
	}
}