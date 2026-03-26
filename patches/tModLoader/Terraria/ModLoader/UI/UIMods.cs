using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.ModLoader.Core;
using Terraria.Audio;
using Terraria.ID;
using System;
using Terraria.GameContent;

namespace Terraria.ModLoader.UI;

internal class UIMods : UIState, IHaveBackButtonCommand
{
	public UIState PreviousUIState { get; set; }
	private UIElement uIElement;
	private UIPanel uIPanel;
	private UILoaderAnimatedImage uiLoader;
	private bool needToRemoveLoading;
	private UIList modList;
	private UIScrollbar uIScrollbar;
	private float modListViewPosition;
	private readonly List<UIModItem> items = new List<UIModItem>();
	private Task<List<UIModItem>> modItemsTask;
	private bool updateNeeded;
	private bool saveNeeded;
	private UIMemoryBar ramUsage;
	private bool showRamUsage;
	public bool loading;
	private UIInputTextField filterTextBox;
	public UICycleImage SearchFilterToggle;
	public ModsMenuSortMode sortMode = ModsMenuSortMode.RecentlyUpdated;
	public EnabledFilter enabledFilterMode = EnabledFilter.All;
	public ModSideFilter modSideFilterMode = ModSideFilter.All;
	public SearchFilter searchFilterMode = SearchFilter.Name;
	internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();
	internal string filter;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonEA;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonDA;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonRM;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonB;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonOMF;
	private UIAutoScaleTextTextPanel<LocalizedText> buttonCL;
	// confirmation dialog
	private UIAutoScaleTextTextPanel<LocalizedText> _confirmDialogYesButton;
	private UIAutoScaleTextTextPanel<LocalizedText> _confirmDialogNoButton;
	private UIText _confirmDialogText;
	private UIImage _blockInput;
	private UIPanel _activeDialog;

	private CancellationTokenSource _cts;
	private bool forceReloadHidden => ModLoader.autoReloadRequiredModsLeavingModsScreen && !ModCompile.DeveloperMode;

	public override void OnInitialize()
	{
		uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f
		};

		uIPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -110, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
			PaddingTop = 0f
		};
		uIElement.Append(uIPanel);

		uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

		modList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
			ListPadding = 5f
		};
		uIPanel.Append(modList);

		ramUsage = new UIMemoryBar() {
			Top = { Pixels = 44 },
		};

		uIScrollbar = new UIScrollbar {
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		uIPanel.Append(uIScrollbar);

		modList.SetScrollbar(uIScrollbar);

		var uIHeaderTexTPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsModsList"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15f);
		uIElement.Append(uIHeaderTexTPanel);

		buttonEA = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModsEnableAll")) {
			TextColor = Color.Green,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = 1f,
			Top = { Pixels = -65 }
		}.WithFadedMouseOver();
		buttonEA.OnLeftClick += QuickEnableAll;
		uIElement.Append(buttonEA);

		// TODO CopyStyle doesn't capture all the duplication here, consider an inner method
		buttonDA = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModsDisableAll"));
		buttonDA.CopyStyle(buttonEA);
		buttonDA.TextColor = Color.Red;
		buttonDA.HAlign = 0.5f;
		buttonDA.WithFadedMouseOver();
		buttonDA.OnLeftClick += QuickDisableAll;
		uIElement.Append(buttonDA);

		buttonRM = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModsForceReload"));
		buttonRM.CopyStyle(buttonEA);
		buttonRM.Width = new StyleDimension(-10f, 1f / 3f);
		buttonRM.HAlign = 1f;
		buttonRM.WithFadedMouseOver();
		buttonRM.OnLeftClick += ReloadMods;
		uIElement.Append(buttonRM);

		UpdateTopRowButtons();

		buttonB = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = 1f,
			Top = { Pixels = -20 }
		}.WithFadedMouseOver();

		buttonB.OnLeftClick += (_, _) => HandleBackButtonUsage();

		uIElement.Append(buttonB);
		buttonOMF = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenModsFolders"));
		buttonOMF.CopyStyle(buttonB);
		buttonOMF.HAlign = 0.5f;
		buttonOMF.WithFadedMouseOver();
		buttonOMF.OnLeftClick += OpenModsFolder;
		uIElement.Append(buttonOMF);

		var texture = UICommon.ModBrowserIconsTexture;
		var upperMenuContainer = new UIElement {
			Width = { Percent = 1f },
			Height = { Pixels = 32 },
			Top = { Pixels = 10 }
		};

		UICycleImage toggleImage;
		for (int j = 0; j < 4; j++) {
			if (j == 0) { //TODO: ouch, at least there's a loop but these click events look quite similar
				toggleImage = new UICycleImage(texture, 3, 32, 32, 34 * 3, 0);
				toggleImage.SetCurrentState((int)sortMode);
				toggleImage.OnLeftClick += (a, b) => {
					sortMode = sortMode.NextEnum();
					saveNeeded = true;
					updateNeeded = true;
				};
				toggleImage.OnRightClick += (a, b) => {
					sortMode = sortMode.PreviousEnum();
					saveNeeded = true;
					updateNeeded = true;
				};
			}
			else if (j == 1) {
				toggleImage = new UICycleImage(texture, 3, 32, 32, 34 * 4, 0);
				toggleImage.SetCurrentState((int)enabledFilterMode);
				toggleImage.OnLeftClick += (a, b) => {
					enabledFilterMode = enabledFilterMode.NextEnum();
					updateNeeded = true;
				};
				toggleImage.OnRightClick += (a, b) => {
					enabledFilterMode = enabledFilterMode.PreviousEnum();
					updateNeeded = true;
				};
			}
			else if (j == 2) {
				toggleImage = new UICycleImage(texture, 5, 32, 32, 34 * 5, 0);
				toggleImage.SetCurrentState((int)modSideFilterMode);
				toggleImage.OnLeftClick += (a, b) => {
					modSideFilterMode = modSideFilterMode.NextEnum();
					updateNeeded = true;
				};
				toggleImage.OnRightClick += (a, b) => {
					modSideFilterMode = modSideFilterMode.PreviousEnum();
					updateNeeded = true;
				};
			}
			else {
				toggleImage = new UICycleImage(texture, 2, 32, 32, 34 * 7, 0);
				toggleImage.SetCurrentState(showRamUsage.ToInt());
				toggleImage.OnLeftClick += (a, b) => ToggleRamButtonAction();
				toggleImage.OnRightClick += (a, b) => ToggleRamButtonAction();
				void ToggleRamButtonAction()
				{
					showRamUsage = !showRamUsage;
					uIPanel.AddOrRemoveChild(ramUsage, showRamUsage);
					if (showRamUsage) {
						ramUsage.Show();
					}
					int ramUsageSpace = showRamUsage ? 72 : 50;
					modList.Height.Pixels = -ramUsageSpace;
					modList.Top.Pixels = ramUsageSpace;
					uIScrollbar.Height.Pixels = -ramUsageSpace;
					uIScrollbar.Top.Pixels = ramUsageSpace;
					uIScrollbar.Recalculate();
				}
			}
			toggleImage.Left.Pixels = j * 36;
			_categoryButtons.Add(toggleImage);
			upperMenuContainer.Append(toggleImage);
		}

		var filterTextBoxBackground = new UIPanel {
			Top = { Percent = 0f },
			Left = { Pixels = -186, Percent = 1f },
			Width = { Pixels = 150 },
			Height = { Pixels = 40 }
		};
		filterTextBoxBackground.SetPadding(0);
		filterTextBoxBackground.OnRightClick += ClearSearchField;
		upperMenuContainer.Append(filterTextBoxBackground);

		filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = { Pixels = 5 },
			Height = { Percent = 1f },
			Width = { Percent = 1f },
			Left = { Pixels = 5 },
			VAlign = 0.5f,
		};
		filterTextBox.OnTextChange += (a, b) => updateNeeded = true;
		filterTextBoxBackground.Append(filterTextBox);

		UIImageButton clearSearchButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
			VAlign = 0.5f,
			Left = new StyleDimension(-2f, 0f)
		};

		//clearSearchButton.OnMouseOver += searchCancelButton_OnMouseOver;
		clearSearchButton.OnLeftClick += ClearSearchField;
		filterTextBoxBackground.Append(clearSearchButton);

		SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 34 * 2, 0) {
			Left = { Pixels = 544 }
		};
		SearchFilterToggle.SetCurrentState((int)searchFilterMode);
		SearchFilterToggle.OnLeftClick += (a, b) => {
			searchFilterMode = searchFilterMode.NextEnum();
			updateNeeded = true;
		};
		SearchFilterToggle.OnRightClick += (a, b) => {
			searchFilterMode = searchFilterMode.PreviousEnum();
			updateNeeded = true;
		};
		_categoryButtons.Add(SearchFilterToggle);
		upperMenuContainer.Append(SearchFilterToggle);

		buttonCL = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfiguration"));
		buttonCL.CopyStyle(buttonOMF);
		buttonCL.HAlign = 1f;
		buttonCL.WithFadedMouseOver();
		buttonCL.OnLeftClick += GotoModConfigList;
		uIElement.Append(buttonCL);

		uIPanel.Append(upperMenuContainer);
		Append(uIElement);
	}

	private void ClearSearchField(UIMouseEvent evt, UIElement listeningElement) => filterTextBox.Text = "";

	// Adjusts sizing and placement of top row buttons according to whether or not
	// the Force Reload button is being shown.
	private void UpdateTopRowButtons()
	{
		var buttonWidth = new StyleDimension(-10f, 1f / (forceReloadHidden ? 2f : 3f));

		buttonEA.Width = buttonWidth;

		buttonDA.Width = buttonWidth;
		buttonDA.HAlign = forceReloadHidden ? 1f : 0.5f;

		uIElement.AddOrRemoveChild(buttonRM, ModCompile.DeveloperMode || !forceReloadHidden);
	}

	public void HandleBackButtonUsage()
	{
		if (_blockInput != null && HasChild(_blockInput)) {
			CloseConfirmDialog(null, null);
			return;
		}

		// To prevent entering the game with Configs that violate ReloadRequired
		if (ConfigManager.AnyModNeedsReload()) {
			Main.menuMode = Interface.reloadModsID;
			return;
		}

		// If auto reloading required mods is enabled, check if any mods need reloading and reload as required
		if (ModLoader.autoReloadRequiredModsLeavingModsScreen && items.Count(i => i.NeedsReload) > 0) {
			Main.menuMode = Interface.reloadModsID;
			return;
		}

		ConfigManager.OnChangedAll();

		IHaveBackButtonCommand.GoBackTo(PreviousUIState);
	}

	private void ReloadMods(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		if (items.Count > 0)
			ModLoader.Reload();
	}

	private static void OpenModsFolder(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		Directory.CreateDirectory(ModLoader.ModPath);
		Utils.OpenFolder(ModLoader.ModPath);

		if (ModOrganizer.WorkshopFileFinder.ModPaths.Any()) {
			string workshopFolderPath = Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString();
			Utils.OpenFolder(workshopFolderPath);
		}
	}

	internal void CloseConfirmDialog(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		_blockInput?.Remove();
		_activeDialog?.Remove();
	}

	internal void ShowConfirmDialog(UIPanel dialog)
	{
		_blockInput = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = Color.Black * 0.5f,
			ScaleToFit = true
		};
		_blockInput.Width = StyleDimension.Fill;
		_blockInput.Height = StyleDimension.Fill;
		_blockInput.OnLeftMouseDown += CloseConfirmDialog;
		Append(_blockInput);

		Append(_activeDialog = dialog);
	}

	private void QuickEnableAll(UIMouseEvent evt, UIElement listeningElement)
	{
		bool shiftPressed = Main.keyState.PressingShift();
		if (shiftPressed || !ModLoader.showConfirmationWindowWhenEnableDisableAllMods) {
			EnableAll(evt, listeningElement);
		}
		else {
			SoundEngine.PlaySound(10, -1, -1, 1);
			ShowConfirmationWindow(EnableAll, "tModLoader.ModsEnableAllConfirm");
		}
	}

	private void EnableAll(UIMouseEvent evt, UIElement listeningElement)
	{
		foreach (UIModItem modItem in items) {
			if (modItem.tMLUpdateRequired != null)
				continue;
			modItem.Enable();
		}
	}

	private void QuickDisableAll(UIMouseEvent evt, UIElement listeningElement)
	{
		bool shiftPressed = Main.keyState.PressingShift();
		if (shiftPressed || !ModLoader.showConfirmationWindowWhenEnableDisableAllMods) {
			DisableAll(evt, listeningElement);
		}
		else {
			SoundEngine.PlaySound(10, -1, -1, 1);
			ShowConfirmationWindow(DisableAll, "tModLoader.ModsDisableAllConfirm");
		}
	}

	private void ShowConfirmationWindow(MouseEvent yesAction, string confirmDialogTextKey)
	{
		var _toggleModsDialog = new UIPanel() {
			Width = { Percent = .30f },
			Height = { Percent = .30f },
			HAlign = .5f,
			VAlign = .5f,
			BackgroundColor = new Color(63, 82, 151),
			BorderColor = Color.Black
		};
		_toggleModsDialog.SetPadding(6f);
		ShowConfirmDialog(_toggleModsDialog);

		_confirmDialogYesButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.104")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = .6f,
			HAlign = .15f
		}.WithFadedMouseOver();
		_confirmDialogYesButton.OnLeftClick += yesAction;
		_confirmDialogYesButton.OnLeftClick += CloseConfirmDialog;
		_toggleModsDialog.Append(_confirmDialogYesButton);

		_confirmDialogNoButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.105")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = .6f,
			HAlign = .85f
		}.WithFadedMouseOver();
		_confirmDialogNoButton.OnLeftClick += CloseConfirmDialog;
		_toggleModsDialog.Append(_confirmDialogNoButton);

		var yesDontAskAgainButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.YesDontAskAgain")) {
			TextColor = Color.White,
			Width = new StyleDimension(0f, 2f / 3f),
			Height = { Pixels = 40 },
			VAlign = 0.95f,
			HAlign = .5f
		}.WithFadedMouseOver();
		yesDontAskAgainButton.OnLeftClick += (a, b) => ModLoader.showConfirmationWindowWhenEnableDisableAllMods = false;
		yesDontAskAgainButton.OnLeftClick += yesAction;
		yesDontAskAgainButton.OnLeftClick += CloseConfirmDialog;
		_toggleModsDialog.Append(yesDontAskAgainButton);

		_confirmDialogText = new UIText(Language.GetTextValue(confirmDialogTextKey)) {
			Width = { Percent = .75f },
			HAlign = .5f,
			VAlign = .2f,
			IsWrapped = true
		};
		_toggleModsDialog.Append(_confirmDialogText);
		Recalculate();
	}

	private void DisableAll(UIMouseEvent evt, UIElement listeningElement)
	{
		foreach (UIModItem modItem in items) {
			modItem.Disable();
		}
	}

	private void GotoModConfigList(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		Main.menuMode = Interface.modConfigListID;
	}

	public UIModItem FindUIModItem(string modName)
	{
		return items.SingleOrDefault(m => m.ModName == modName);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		UIModBrowser.PageUpDownSupport(modList);
		if (modItemsTask is { IsCompleted: true }) {
			var result = modItemsTask.Result;
			items.AddRange(result);
			foreach (var item in items) {
				item.Activate(); // Activate must happen after all UIModItem are in `items`
			}
			needToRemoveLoading = true;
			updateNeeded = true;
			loading = false;
			modItemsTask = null;
		}
		if (needToRemoveLoading) {
			needToRemoveLoading = false;
			uIPanel.RemoveChild(uiLoader);
		}
		if (!updateNeeded)
			return;
		updateNeeded = false;
		filter = filterTextBox.Text;
		modList.Clear();
		var filterResults = new UIModsFilterResults();
		var visibleItems = items.Where(item => item.PassFilters(filterResults)).ToList();
		if (filterResults.AnyFiltered) {
			var panel = new UIPanel();
			panel.Width.Set(0, 1f);
			modList.Add(panel);
			var filterMessages = new List<string>();
			if (filterResults.filteredByEnabled > 0)
				filterMessages.Add(Language.GetTextValue("tModLoader.ModsXModsFilteredByEnabled", filterResults.filteredByEnabled));
			if (filterResults.filteredByModSide > 0)
				filterMessages.Add(Language.GetTextValue("tModLoader.ModsXModsFilteredByModSide", filterResults.filteredByModSide));
			if (filterResults.filteredBySearch > 0)
				filterMessages.Add(Language.GetTextValue("tModLoader.ModsXModsFilteredBySearch", filterResults.filteredBySearch));
			string filterMessage = string.Join("\n", filterMessages);
			var text = new UIText(filterMessage);
			text.Width.Set(0, 1f);
			text.IsWrapped = true;
			text.WrappedTextBottomPadding = 0;
			text.TextOriginX = 0f;
			text.Recalculate();
			panel.Append(text);
			panel.Height.Set(text.MinHeight.Pixels + panel.PaddingTop, 0f);
		}
		modList.AddRange(visibleItems);
		Recalculate();
		modList.ViewPosition = modListViewPosition;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
		base.Draw(spriteBatch);
		for (int i = 0; i < _categoryButtons.Count; i++) {
			if (_categoryButtons[i].IsMouseHovering) {
				string text;
				switch (i) {
					case 0:
						text = sortMode.ToFriendlyString();
						break;
					case 1:
						text = enabledFilterMode.ToFriendlyString();
						break;
					case 2:
						text = modSideFilterMode.ToFriendlyString();
						break;
					case 3:
						text = Language.GetTextValue("tModLoader.ShowMemoryEstimates" + (showRamUsage ? "Yes" : "No"));
						break;
					case 4:
						text = searchFilterMode.ToFriendlyString();
						break;
					default:
						text = "None";
						break;
				}
				UICommon.TooltipMouseText(text);
				return;
			}
		}
		if (buttonOMF.IsMouseHovering)
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.ModsOpenModsFoldersTooltip"));
	}

	public override void OnActivate()
	{
		_cts = new CancellationTokenSource();
		Main.clrInput();
		modList.Clear();
		items.Clear();
		loading = true;
		uIPanel.Append(uiLoader);
		ConfigManager.LoadAll(); // Makes sure MP configs are cleared.
		Populate();
		UpdateTopRowButtons();
	}

	public override void OnDeactivate()
	{
		_cts?.Cancel(false);
		_cts?.Dispose();
		_cts = null;
		modListViewPosition = modList.ViewPosition;
		if (saveNeeded) {
			saveNeeded = false;
			Main.SaveSettings();
		}
	}

	internal void StoreCurrentScrollPosition()
	{
		modListViewPosition = modList.ViewPosition;
	}

	internal void Populate()
	{
		modItemsTask = Task.Run(() => {
			var mods = ModOrganizer.FindMods(logDuplicates: true);
			var pendingUIModItems = new List<UIModItem>();
			foreach (var mod in mods) {
				UIModItem modItem = new UIModItem(mod);
				pendingUIModItems.Add(modItem);
			}
			return pendingUIModItems;
		}, _cts.Token);
	}
}

public class UIModsFilterResults
{
	public int filteredBySearch;
	public int filteredByModSide;
	public int filteredByEnabled;
	public bool AnyFiltered => filteredBySearch > 0 || filteredByModSide > 0 || filteredByEnabled > 0;
}

public static class ModsMenuSortModesExtensions
{
	public static string ToFriendlyString(this ModsMenuSortMode sortmode)
	{
		switch (sortmode) {
			case ModsMenuSortMode.RecentlyUpdated:
				return Language.GetTextValue("tModLoader.ModsSortRecently");
			case ModsMenuSortMode.DisplayNameAtoZ:
				return Language.GetTextValue("tModLoader.ModsSortNamesAlph");
			case ModsMenuSortMode.DisplayNameZtoA:
				return Language.GetTextValue("tModLoader.ModsSortNamesReverseAlph");
		}
		return "Unknown Sort";
	}
}

public static class EnabledFilterModesExtensions
{
	public static string ToFriendlyString(this EnabledFilter updateFilterMode)
	{
		switch (updateFilterMode) {
			case EnabledFilter.All:
				return Language.GetTextValue("tModLoader.ModsShowAllMods");
			case EnabledFilter.EnabledOnly:
				return Language.GetTextValue("tModLoader.ModsShowEnabledMods");
			case EnabledFilter.DisabledOnly:
				return Language.GetTextValue("tModLoader.ModsShowDisabledMods");
		}
		return "Unknown Sort";
	}
}

public enum ModsMenuSortMode
{
	RecentlyUpdated,
	DisplayNameAtoZ,
	DisplayNameZtoA,
}

public enum EnabledFilter
{
	All,
	EnabledOnly,
	DisabledOnly,
}